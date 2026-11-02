#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class CombatPresentationReview
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("War & Conquer/Pruebas/Animaciones de combate")]
        public static void Verify()
        {
            VerifySignals();
            var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Combat presentation review");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
            try
            {
                var game=new GameManager(CardCatalog.Load());game.NewGame(CardCatalog.Leaders,42,CardCatalog.LoadRules(),4);
                foreach(var player in game.State.players)player.hand.Clear();
                game.State.stage=TurnStage.Assault;
                var first=game.State.tiles.First(t=>!t.blocked&&t.baseOwner<0&&t.neighbors.Any(n=>!game.State.tiles[n].blocked&&game.State.tiles[n].baseOwner<0));
                var second=game.State.tiles[first.neighbors.First(n=>!game.State.tiles[n].blocked&&game.State.tiles[n].baseOwner<0)];
                first.specialEffect=second.specialEffect=null;
                var attacker=game.Place("nomada-de-arena",0,first.id);var target=game.Place("nomada-de-arena",1,second.id);
                attacker.attacked=false;target.health=20;
                var signals=new List<CombatVisualSignal>();game.CombatVisual+=signals.Add;
                var world=root.AddComponent<Board3DScene>();
                Action sync=()=>world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);sync();
                var animation=root.GetComponent<BoardCombatPresentation>();Check(animation!=null,"Scene has no combat presentation");
                Check(!animation.IsAnimating&&signals.Count==0,"Initial load generated a combat event");
                var attackerRoot=world.UnitVisual(attacker.id);var origin=attackerRoot.position;
                Check(CombatManager.Attack(game,attacker,second.id),"Test attack was refused");sync();
                Check(signals.Count==2&&signals[0].kind==CombatVisualKind.Attack&&signals[1].kind==CombatVisualKind.Damage,"Attack/hit signal order is wrong");
                Check(animation.IsAnimating&&world.IsAnimating,"Combat not exposed to the board input gate");
                var logic=GamePersistence.Serialize(game.State);int random=game.State.randomState;
                animation.Tick(.15f);
                Check(attackerRoot.Find("Reacción de combate").localPosition.sqrMagnitude>.00001f,"Attacker miniature did not lunge");
                Check(attackerRoot.position==origin,"Attack moved the logical piece root");
                Capture(world,game,first.id,second.id,"attack");
                animation.Tick(.22f);
                Check(world.UnitVisual(target.id).Find("Reacción de combate").localPosition.sqrMagnitude>.00001f,"Victim did not flinch");
                Capture(world,game,first.id,second.id,"hit");
                animation.Tick(1f);Check(!animation.IsAnimating,"Attack never finished");
                Check(attackerRoot.Find("Reacción de combate").localPosition==Vector3.zero,"Attack pose accumulated drift");
                Check(logic==GamePersistence.Serialize(game.State)&&random==game.State.randomState,"Animation mutated rules or random stream");
                signals.Clear();Check(game.Undo(),"Attack was not undoable");sync();
                Check(signals.Count==0&&!animation.IsAnimating,"Undo replayed combat");
                attacker=BoardManager.Pieces(game.State).First(p=>p.id==attacker.id);target=BoardManager.Pieces(game.State).First(p=>p.id==target.id);
                target.health=1;var dyingRoot=world.UnitVisual(target.id);
                CombatManager.DirectDamage(game,target,2,attacker);sync();
                Check(signals.Count==2&&signals[0].kind==CombatVisualKind.Damage&&signals[1].kind==CombatVisualKind.Death,"Direct lethal damage must emit one hit and one death");
                Check(world.UnitVisual(target.id)==null&&dyingRoot!=null&&animation.RetiredCount==1,"Dead miniature was destroyed before its animation");
                Check(dyingRoot.GetComponentsInChildren<Collider>().All(c=>!c.enabled),"Dead miniature still blocks tile clicks");
                logic=GamePersistence.Serialize(game.State);animation.Tick(.36f);
                Check(dyingRoot.Find("Reacción de combate").localScale.x<.99f,"Death did not collapse the miniature");
                Capture(world,game,first.id,second.id,"death");
                animation.Tick(1f);Check(dyingRoot==null&&!animation.IsAnimating&&animation.RetiredCount==0,"Death retained resources or blocked input");
                Check(logic==GamePersistence.Serialize(game.State),"Death VFX changed discard or death triggers");
                signals.Clear();CombatManager.Remove(game,target);Check(signals.Count==0,"Repeated death generated duplicate effects");
                CombatManager.PoisonDamage(game,attacker,1);sync();Check(signals.Count==1&&signals[0].kind==CombatVisualKind.Damage&&animation.IsAnimating,"Poison damage has no hit feedback");
                var restored=GamePersistence.Deserialize(GamePersistence.Serialize(game.State));signals.Clear();game.Restore(restored);sync();
                Check(signals.Count==0&&!animation.IsAnimating,"Restore retained/replayed a transient effect");
                attacker=BoardManager.Pieces(game.State).First(p=>p.id==attacker.id);
                target=game.Place("nomada-de-arena",1,second.id);target.health=1;attacker.attacked=false;
                sync();var arriving=world.UnitVisual(target.id).GetComponent<CardUnitPresentation>();arriving.enabled=false;
                var retiring=world.UnitVisual(target.id);signals.Clear();Check(CombatManager.Attack(game,attacker,second.id),"Lethal attack was refused");sync();
                Check(animation.RetiredCount==1,"Lethal attack did not retain its victim");
                Check(signals.Count==3&&signals[0].kind==CombatVisualKind.Attack&&signals[1].kind==CombatVisualKind.Damage&&signals[2].kind==CombatVisualKind.Death,"Lethal attack event order is wrong");
                Check(game.Undo(),"Lethal attack was not undoable");sync();
                Check(retiring==null&&animation.RetiredCount==0&&!animation.IsAnimating&&world.UnitVisual(target.id)!=null,"Undo during death left a ghost or replayed the restored unit");
                game=Fixture(out attacker,out target);signals.Clear();game.CombatVisual+=signals.Add;
                var distant=game.State.tiles.First(t=>!t.blocked&&t.baseOwner<0&&t.unit==null&&BoardManager.AxialDistance(t,game.State.tiles[attacker.tileId])==2);
                distant.specialEffect=null;MovementManager.Relocate(game,target,distant.id);target.health=1;sync();
                var engagingRoot=world.UnitVisual(attacker.id);var engagingOrigin=engagingRoot.position;int sourceTile=attacker.tileId;
                Check(GenericCardRules.Resolve(game,game.Catalog["engage"],new[]{sourceTile,target.tileId},Biome.Neutral),"Engage did not resolve");sync();
                var movement=engagingRoot.GetComponent<BoardPieceMotion>();
                Check(signals.Count==3&&signals[0].kind==CombatVisualKind.Attack&&signals[0].range==2,"Engage did not report its actual ranged strike");
                Check(movement.IsMoving&&movement.SuspendForCombat,"Engage relocation was not queued behind its attack");
                logic=GamePersistence.Serialize(game.State);movement.Tick(.3f);Check(engagingRoot.position==engagingOrigin,"Engage moved before striking");
                animation.Tick(.31f);movement.Tick(.2f);Check(engagingRoot.position==engagingOrigin&&movement.SuspendForCombat,"Engage moved during attack recoil");
                animation.Tick(.31f);Check(!movement.SuspendForCombat,"Engage never released its movement queue");
                movement.Tick(.2f);Check(Vector3.Distance(engagingRoot.position,engagingOrigin)>.001f,"Engage movement did not resume");
                movement.Tick(2f);animation.Tick(2f);Check(!movement.IsMoving&&!animation.IsAnimating,"Engage never settled");
                Check(logic==GamePersistence.Serialize(game.State),"Engage animation changed gameplay or random state");
                animation.Clear();Check(!movement.SuspendForCombat,"Cancel retained a presentation movement lock");
                var buildTile=game.State.tiles.First(t=>!t.blocked&&!t.IsOccupied&&t.baseOwner<0);
                var structure=game.Place("obelisco-solar",0,buildTile.id);sync();
                var structureRoot=world.UnitVisual(structure.id);var assembly=structureRoot.GetComponent<BoardStructureAssembly>();
                var assembling=structureRoot.GetComponentsInChildren<MeshRenderer>().Where(r=>r.GetComponent<TextMesh>()==null&&r.transform.localPosition.y>5).Select(r=>r.transform).ToArray();
                var seated=assembling.Select(t=>t.localPosition-Vector3.up*6).ToArray();
                Check(assembly.IsBuilding&&assembling.Length>0,"Structure assembly fixture was not staged");
                CombatManager.DirectDamage(game,structure,1000);sync();
                Check(!assembly.IsBuilding&&!assembly.enabled,"Destroyed structure continued its construction");
                for(int i=0;i<assembling.Length;i++)Check(Vector3.Distance(assembling[i].localPosition,seated[i])<.001f,"Dead structure retained a floating assembly fragment");
                animation.Tick(2f);Check(structureRoot==null&&!animation.IsAnimating,"Interrupted structure death did not clean up");
                Debug.Log("COMBAT_PRESENTATION_PASSED: ordered attack/hit/death signals, direct/poison damage, retained nonselectable deaths, miniature lunge/flinch/collapse, fixed logical roots, bounded cleanup, no state/RNG changes, undo/load cancellation.");
            }
            finally{BoardMeshFactory.Release(root);EditorSceneManager.ClosePreviewScene(scene);}
        }
        static void Capture(Board3DScene world,GameManager game,int first,int second,string name)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var camera=world.BoardCamera;var position=camera.transform.position;var rotation=camera.transform.rotation;
            float size=camera.orthographicSize,aspect=camera.aspect;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var previousScene=camera.scene;var previousFlags=camera.clearFlags;var previousBackground=camera.backgroundColor;
            var target=new RenderTexture(900,600,24);var pixels=new Texture2D(900,600,TextureFormat.RGB24,false);
            try
            {
                var center=world.transform.TransformPoint((Board3DScene.Position(game.State.tiles[first])+Board3DScene.Position(game.State.tiles[second]))*.5f+Vector3.up*.8f);
                camera.scene=world.gameObject.scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.035f,.05f);
                camera.orthographicSize=2.8f;camera.aspect=1.5f;camera.transform.position=center+new Vector3(4,6,-5);camera.transform.LookAt(center);
                target.Create();camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,900,600),0,0);pixels.Apply();Directory.CreateDirectory("Library/InteractionQA");File.WriteAllBytes("Library/InteractionQA/"+name+".png",pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previousActive;camera.targetTexture=previousTarget;camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;camera.aspect=aspect;
                camera.scene=previousScene;camera.clearFlags=previousFlags;camera.backgroundColor=previousBackground;
                target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
        static GameManager Fixture(out Piece attacker,out Piece victim)
        {
            var g=new GameManager(CardCatalog.Load());g.NewGame(CardCatalog.Leaders,42,CardCatalog.LoadRules(),4);
            foreach(var player in g.State.players)player.hand.Clear();g.State.stage=TurnStage.Assault;
            var a=g.State.tiles.First(t=>!t.blocked&&t.baseOwner<0&&t.neighbors.Any(n=>!g.State.tiles[n].blocked&&g.State.tiles[n].baseOwner<0));
            var b=g.State.tiles[a.neighbors.First(n=>!g.State.tiles[n].blocked&&g.State.tiles[n].baseOwner<0)];a.specialEffect=b.specialEffect=null;
            attacker=g.Place("nomada-de-arena",0,a.id);victim=g.Place("nomada-de-arena",1,b.id);attacker.attacked=false;victim.health=20;return g;
        }
        static void VerifySignals()
        {
            var g=Fixture(out var a,out var v);var signals=new List<CombatVisualSignal>();g.CombatVisual+=signals.Add;
            a.sleepUntilTurn=g.State.turn;Check(!CombatManager.Attack(g,a,v.tileId)&&signals.Count==0,"Canceled attack emitted an attack/hit");a.sleepUntilTurn=0;
            v.cardId="guardian-del-obelisco";g.Place("obelisco-solar",v.owner,a.tileId);
            CombatManager.Damage(g,v,1,false,a);Check(signals.Count==0,"Fully defended damage flinched as if health were lost");
            v.health=1;CombatManager.PoisonDamage(g,v,1);
            Check(signals.Count==2&&signals[0].kind==CombatVisualKind.Damage&&signals[1].kind==CombatVisualKind.Death,"Lethal poison event sequence is wrong");
            g=Fixture(out a,out v);signals.Clear();g.CombatVisual+=signals.Add;
            int tile=v.tileId;CombatManager.Remove(g,v);signals.Clear();g.State.tiles[tile].baseOwner=1;g.State.players[1].leaderHealth=20;
            Check(CombatManager.Attack(g,a,tile),"Leader hit was refused");
            Check(signals.Count==2&&signals[0].kind==CombatVisualKind.Attack&&signals[1].kind==CombatVisualKind.LeaderDamage&&signals[1].amount>0,"Leader hit has no feedback");
        }
        public static void Batch(){try{Verify();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
    }
}
#endif
