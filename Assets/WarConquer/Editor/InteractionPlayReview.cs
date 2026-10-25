#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    // Batch integration uses its own scene and never loads or saves a user's match.
    public static class InteractionPlayReview
    {
        const string Pending="WarConquer.InteractionPlayReview";
        const string Output="Library/InteractionQA";
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static WarConquerController ui;static Board3DScene world;
        static bool running;static int step;static double due,deadline;static string runtimeError,gameplayImage,detailImage;
        static int aId,bId,enemyId,structureId;

        public static void BeginBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool(Pending,true);EditorApplication.isPlaying=true;
        }
        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorApplication.update+=Tick;
            Application.logMessageReceived+=(message,stack,type)=>
            {if(running&&(type==LogType.Exception||type==LogType.Error)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))runtimeError=message;};
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static void Invoke(string method,params object[] args)=>typeof(WarConquerController).GetMethod(method,Flags).Invoke(ui,args);
        static void Set(string field,object value)=>typeof(WarConquerController).GetField(field,Flags).SetValue(ui,value);
        static T Get<T>(string field)=>(T)typeof(WarConquerController).GetField(field,Flags).GetValue(ui);
        static Piece Piece(int id)=>BoardManager.Pieces(ui.Game.State).Single(p=>p.id==id);
        static Button Button(string name)=>ui.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static void Click(string name)=>Button(name).onClick.Invoke();

        static void Fixture(bool statuses)
        {
            Set("humanPlayers",4);Set("leaders",(string[])CardCatalog.Leaders.Clone());Invoke("StartMatch",false);
            var g=ui.Game;
            foreach(var tile in g.State.tiles.Where(t=>Mathf.Abs(t.q)<=3&&Mathf.Abs(t.r)<=3))
            {tile.blocked=false;tile.biome=tile.q<1?Biome.Swamp:Biome.Desert;tile.owner=tile.q<1?0:1;}
            Func<int,int,int> tileId=(q,r)=>g.State.tiles.First(t=>t.q==q&&t.r==r).id;
            var a=g.Place("alligator-revengeful-bite",0,tileId(0,0));aId=a.id;
            var b=g.Place("lushroom",0,tileId(0,1));bId=b.id;
            var enemy=g.Place("old-mummy",1,tileId(1,0));enemyId=enemy.id;
            structureId=g.Place("oasis-de-cristal",0,a.tileId).id;
            g.State.stage=TurnStage.Assault;g.State.Active.currentEnergy=g.State.Active.maxEnergy=20;
            foreach(var p in BoardManager.Pieces(g.State)){p.playedTurn=-1;p.attacked=false;p.remainingMovement=3;}
            if(statuses)
            {
                a.poison=1;a.poisonApplications=1;a.sleepUntilTurn=g.State.turn+4;a.slow=2;a.slowUntilTurn=g.State.turn+4;
                a.bonusAttack=2;a.temporaryMovement=3;a.movementExpiresTurn=g.State.turn+4;
                a.statBonuses.Add(new StatBonus{hp=2,attack=1,expiresTurn=g.State.turn+4});a.health+=2;
                g.State.tiles[a.tileId].specialEffect=new TerrainEffect{threshold=5,damage=1,expiresRound=g.State.round+2};
                b.bonusAttack=1;b.statBonuses.Add(new StatBonus{hp=1,expiresTurn=g.State.turn+4});b.health++;
                enemy.slow=1;enemy.slowUntilTurn=g.State.turn+4;
            }
            GenericCardRules.SyncAuras(g);
            // Existing pieces are rebuilt from the restored state without a spawn animation.
            g.Restore(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)));
            world=ui.GetComponentInChildren<Board3DScene>();
        }

        static void ControllerChecks()
        {
            BoardSelectionTests.PointerGestures();
            Fixture(false);ui.enabled=false;
            var g=ui.Game;string before=GamePersistence.Serialize(g.State);
            var a=Piece(aId);var b=Piece(bId);
            Check(!world.IsAnimating,"Freshly restored fixture is still animating.");
            Check(CombatManager.Targets(g,a).Count>0&&CombatManager.Targets(g,b).Count>0,"Fixture needs two eligible attackers.");
            ui.TileClick(a.tileId);Invoke("Begin","attack");ui.TileClick(b.tileId);
            Check(Get<Piece>("selectedPiece")==b&&Get<string>("mode")=="attack","Switching a friendly attacker dropped the chosen action.");
            Check(GamePersistence.Serialize(g.State)==before,"Switching attackers consumed an action.");

            // A failed second target must not wipe the first target of a multi-target spell.
            var engage=PrototypeScenario.Take(g,0,"engage");g.State.Active.hand.Add(engage);
            Invoke("SelectCard",engage,g.State.Active);ui.TileClick(a.tileId);
            var targets=Get<List<int>>("targets");Check(targets.SequenceEqual(new[]{a.tileId}),"First spell target did not remain pending.");
            int invalid=g.State.tiles.First(t=>!t.IsOccupied&&t.baseOwner<0&&!t.blocked).id;
            ui.TileClick(invalid);
            Check(Get<string>("mode")=="card"&&Get<int>("selectedCard")==engage.instanceId&&targets.SequenceEqual(new[]{a.tileId}),"Invalid click erased partial spell selection.");
            Invoke("ClearAction");ui.Render();

            // Both occupants remain available without moving or activating either one.
            Invoke("InspectTile",a.tileId,false);string occupied=GamePersistence.Serialize(g.State);
            Click("Estructura");Check(Get<Piece>("selectedPiece").id==structureId,"Covered passable structure cannot be selected.");
            Click("Unidad");Check(Get<Piece>("selectedPiece").id==aId,"Unit selection cannot be restored.");
            Check(GamePersistence.Serialize(g.State)==occupied,"Occupant buttons changed the game.");

            g.State.Active.isAI=true;string ai=GamePersistence.Serialize(g.State);
            ui.TileClick(b.tileId);
            Check(Get<Piece>("selectedPiece")==b&&Get<string>("mode")=="inspect"&&GamePersistence.Serialize(g.State)==ai,"AI-turn inspection executed an action or failed.");
            g.State.Active.isAI=false;

            string rest=GamePersistence.Serialize(g.State);
            var moving=world.UnitVisual(aId).GetComponent<BoardPieceMotion>();
            moving.Enqueue(world.UnitVisual(aId).localPosition+Vector3.forward,a.tileId);
            Check(world.IsAnimating,"Movement fixture did not start.");ui.TileClick(b.tileId);
            Check(Get<Piece>("selectedPiece")==b&&Get<string>("mode")=="inspect","Cannot inspect a piece during animation.");
            Invoke("Begin","attack");
            Check(Get<string>("mode")=="inspect"&&GamePersistence.Serialize(g.State)==rest,"Animation-time inspection allowed an action.");
            g.Restore(GamePersistence.Deserialize(rest));Check(!world.IsAnimating,"Restore retained presentation input lock.");
            world.EffectDisplay=BoardEffectDisplay.Compact;ui.Render();string display=GamePersistence.Serialize(g.State);
            for(int i=0;i<3;i++)ui.GetComponentsInChildren<Button>().Single(x=>x.name.StartsWith("Efectos:")).onClick.Invoke();
            Check(world.EffectDisplay==BoardEffectDisplay.Compact&&GamePersistence.Serialize(g.State)==display,"Cycling effect visibility changed state.");
            ui.enabled=true;
            Debug.Log("INTERACTION_CONTROLLER_PASSED: attacker reassignment, partial spell targets, two occupants, AI/animation inspection, action guards, restore and pure display controls.");
        }

        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false))
            {SessionState.SetBool(Pending,false);running=true;step=0;due=EditorApplication.timeSinceStartup+.5;deadline=EditorApplication.timeSinceStartup+90;}
            if(!running)return;EditorApplication.QueuePlayerLoopUpdate();
            if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                Check(EditorApplication.timeSinceStartup<deadline,"Interaction review timed out.");
                Check(runtimeError==null,"Runtime error: "+runtimeError);
                if(step==0)
                {
                    Directory.CreateDirectory(Output);Screen.SetResolution(1600,1000,FullScreenMode.Windowed);
                    var gameView=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
                    if(gameView!=null){var view=EditorWindow.GetWindow(gameView);view.position=new Rect(30,30,1600,1038);view.Focus();}
                    var camera=new GameObject("Interaction review screen camera").AddComponent<Camera>();camera.tag="MainCamera";camera.cullingMask=0;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                    ui=new GameObject("Interaction review controller").AddComponent<WarConquerController>();
                    Invoke("StartMainGame");Set("humanPlayers",4);Invoke("StartMatch",false);
                    step=1;due=EditorApplication.timeSinceStartup+.8;return;
                }
                if(step==1)
                {
                    ControllerChecks();StatusClarityTests.Run(ui);FeedbackPlayTests.Run(ui);
                    Fixture(true);world.Zoom=5.2f;world.EffectDisplay=BoardEffectDisplay.Compact;
                    Invoke("InspectTile",Piece(aId).tileId,false);ui.Game.Notify("Selecciona una casilla o consulta Estados para ver todos sus efectos.");
                    Canvas.ForceUpdateCanvases();step=2;due=EditorApplication.timeSinceStartup+1.2;return;
                }
                if(step==2)
                {
                    Check(PieceStatusPresentation.Collect(ui.Game,Piece(aId)).Count>3,"Screenshot fixture lost stacked effects.");
                    gameplayImage=Path.GetFullPath(Output+"/partida-compacta.png");ScreenCapture.CaptureScreenshot(gameplayImage);
                    step=3;due=EditorApplication.timeSinceStartup+.8;return;
                }
                if(step==3)
                {
                    Check(File.Exists(gameplayImage)&&new FileInfo(gameplayImage).Length>10000,"Gameplay screenshot missing or empty.");
                    string before=GamePersistence.Serialize(ui.Game.State);Invoke("ShowPieceStates",Piece(aId));
                    Check(ui.GetComponentsInChildren<ScrollRect>().Any(x=>x.name=="Lista de estados"),"No scrollable effects detail view.");
                    Check(GamePersistence.Serialize(ui.Game.State)==before,"Opening effect details changed state.");
                    Canvas.ForceUpdateCanvases();step=4;due=EditorApplication.timeSinceStartup+.8;return;
                }
                if(step==4)
                {
                    detailImage=Path.GetFullPath(Output+"/detalle-estados.png");ScreenCapture.CaptureScreenshot(detailImage);
                    step=5;due=EditorApplication.timeSinceStartup+.8;return;
                }
                Check(File.Exists(detailImage)&&new FileInfo(detailImage).Length>10000,"Effects screenshot missing or empty.");
                File.WriteAllText(Output+"/interaction-play.txt","PASS: real Canvas startup; attacker switching; multi-target card retention; separate co-occupant inspection; read-only AI/animation inspection; action guards; restored animation cancellation; effect display cycle; status and legacy feedback integration; compact gameplay and scrollable effects screenshots. Unity "+Application.unityVersion+"; screen "+Screen.width+"x"+Screen.height);
                Debug.Log("INTERACTION_PLAY_REVIEW_PASSED");running=false;EditorApplication.Exit(0);
            }
            catch(Exception error)
            {running=false;Directory.CreateDirectory(Output);File.WriteAllText(Output+"/interaction-play.txt",error.ToString());Debug.LogException(error);EditorApplication.Exit(1);}
        }
    }
}
#endif
