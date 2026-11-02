#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace WarConquer.Editor
{
    public static class UnitMotionTests
    {
        const string Pending="UnitMotion.Pending";static bool running;static int step;static double due;
        static GameManager game;static Board3DScene world;static Piece unit;static Vector3 origin,destination;static string state;
        public static void BeginBatch(){SessionState.SetBool(Pending,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Sync()=>world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false)){SessionState.SetBool(Pending,false);running=true;step=0;due=EditorApplication.timeSinceStartup+2;}
            if(!running||EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    var cards=CardCatalog.Load();game=new GameManager(cards);game.NewGame(CardCatalog.Leaders,42,CardCatalog.LoadRules(),4);
                    var t=game.State.tiles.First(x=>x.q==0&&x.r==0);t.blocked=false;
                    unit=game.Place(cards.All.First(c=>c.category==Category.Unit&&c.movementType=="Ground").id,0,t.id);
                    world=new GameObject("Movement QA").AddComponent<Board3DScene>();Sync();origin=world.UnitVisual(unit.id).localPosition;
                    var next=game.State.tiles.First(x=>x.q==1&&x.r==0);var last=game.State.tiles.First(x=>x.q==2&&x.r==0);next.blocked=last.blocked=false;
                    MovementManager.Relocate(game,unit,next.id);MovementManager.Relocate(game,unit,last.id);Sync();destination=Board3DScene.Position(last)+Vector3.up*BoardMeshFactory.Surface;
                    Check(world.IsAnimating,"Movement not queued");Check(world.UnitVisual(unit.id).localPosition==origin,"Unit teleported");
                    state=GamePersistence.Serialize(game.State);for(int i=0;i<8;i++)Sync();
                    step=1;due=EditorApplication.timeSinceStartup+.12;return;
                }
                if(step==1)
                {
                    var pos=world.UnitVisual(unit.id).localPosition;Check(Vector3.Distance(pos,origin)>.01f&&Vector3.Distance(pos,destination)>.01f,"No intermediate movement");
                    Check(pos.y>origin.y,"Missing board-piece lift");step=2;due=EditorApplication.timeSinceStartup+3.2;return;
                }
                if(step==2)
                {
                    Check(!world.IsAnimating,"Repeated Sync restarted animation");Check(Vector3.Distance(world.UnitVisual(unit.id).localPosition,destination)<.001f,"Wrong landing");
                    Check(state==GamePersistence.Serialize(game.State),"Animation mutated game state");
                    var target=game.State.tiles.First(x=>x.q==3&&x.r==0);MovementManager.Relocate(game,unit,target.id);Sync();Check(world.IsAnimating,"Second move not animated");
                    game.Restore(GamePersistence.Deserialize(state));Sync();Check(!world.IsAnimating,"Restore retained stale movement");
                    unit=BoardManager.Pieces(game.State).First(x=>x.id==unit.id);game.State.stage=TurnStage.Assault;game.State.phase=Phase.Actions;unit.remainingMovement=3;unit.playedTurn=-1;game.State.pendingChoices.Clear();game.State.responsePlayer=-1;game.State.battle=null;target.blocked=false;game.State.tiles[target.id].blocked=false;Check(MovementManager.Move(game,unit,target.id),"Real movement failed");Sync();Check(world.IsAnimating,"Real move not animated");Check(game.Undo(),"Undo failed");Sync();Check(!world.IsAnimating,"Undo retained animation");Check(Vector3.Distance(world.UnitVisual(unit.id).localPosition,destination)<.001f,"Undo position wrong");
                    var cards=game.Catalog;var tile=game.State.tiles.First(x=>x.structure==null&&x.unit==null&&!x.blocked);
                    var building=game.Place(cards.All.First(c=>c.IsStructure).id,0,tile.id);Sync();Check(world.UnitVisual(building.id).GetComponent<BoardPieceMotion>()==null,"Structure has movement animation");
                    Debug.Log("UNIT_MOTION_PASSED: resolved route, intermediate frames, exact landing, stable refresh, state unchanged, restore cancellation, structures excluded.");running=false;EditorApplication.Exit(0);
                }
            }
            catch(Exception e){Debug.LogException(e);running=false;EditorApplication.Exit(1);}
        }
    }
}
#endif
