#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class FeedbackVisualPreview
    {
        static WarConquerController ui;static bool batch;static int step,played;static double due;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static Piece Put(string id,int q,int r,int owner=0)
        {var g=ui.Game;return g.Place(id,owner,g.State.tiles.First(t=>t.q==q&&t.r==r).id);}
        public static void Begin(WarConquerController controller,bool exit)
        {
            ui=controller;batch=exit;step=0;
            typeof(WarConquerController).GetField("leaders",Flags).SetValue(ui,new[]{"ZUKGROK","SAHRIA","FAUNAR","ZUKGROK"});
            typeof(WarConquerController).GetField("humanPlayers",Flags).SetValue(ui,4);Invoke("StartMatch",false);
            var g=ui.Game;var poison=Put("old-mummy",-3,0);EffectManager.Poison(g,poison,1,1);
            var sleep=Put("dune-cammel",-1,0);EffectManager.Sleep(g,sleep,1);
            var slow=Put("sandy-ember",1,0);slow.slow=2;slow.slowUntilTurn=g.State.turn+4;g.State.tiles[slow.tileId].specialEffect=new TerrainEffect{threshold=5};
            var buff=Put("old-mummy",3,0);buff.bonusAttack=2;buff.temporaryMovement=4;buff.statBonuses.Add(new StatBonus{hp=2,attack=0,expiresTurn=g.State.turn+4});buff.health+=2;
            var evolve=Put("larva-micelial",-2,3);evolve.evolved=true;evolve.health=6;
            var guarded=Put("old-mummy",0,3);Put(g.Catalog.All.First(c=>c.Has("FirstAttackGuard")).id,0,4);
            var noHeal=Put("old-mummy",2,3);g.State.tiles[noHeal.tileId].biome=Biome.Swamp;EffectManager.Poison(g,noHeal,1,1);
            var lush=Put("lushroom",3,3,1);g.State.tiles[lush.tileId].biome=Biome.Swamp;
            var world=ui.GetComponentInChildren<Board3DScene>();world.Zoom=4;g.State.stage=TurnStage.Assault;g.Notify("Estados de cartas · efectos visuales");
            due=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;
        }
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/estados-cartas.png"));step++;due=EditorApplication.timeSinceStartup+1;return;
                }
                if(step==1)
                {
                    var g=ui.Game;var feedback=ui.GetComponent<ConquestAudioFeedback>();played=feedback.PlayedPoints;
                    foreach(var t in g.State.tiles.Where(ConquestManager.IsCenter))
                    {if(t.IsOccupied)continue;t.biome=Biome.Swamp;t.owner=0;g.Place("frog-token",0,t.id);}
                    ConquestManager.Refresh(g.State);g.State.turn+=4;ConquestManager.ScoreStart(g.State,0);g.Notify("Conquista · fanfarria por cada punto");
                    step++;due=EditorApplication.timeSinceStartup+5;return;
                }
                var audio=ui.GetComponent<ConquestAudioFeedback>();
                if(ui.Game.State.players[0].conquestPoints<1||audio.PlayedPoints-played!=ui.Game.State.players[0].conquestPoints||audio.PendingPoints!=0)throw new Exception("La cola de sonidos no resolvió los puntos reales.");
                EditorApplication.update-=Tick;Debug.Log("FEEDBACK_VISUAL_AUDIO_PASSED: estados renderizados y fanfarria reproducida una vez por punto real.");
                typeof(WarConquerController).GetField("humanPlayers",Flags).SetValue(ui,1);Invoke("StartMatch",false);Invoke("ShowSetup");if(batch)EditorApplication.Exit(0);
            }
            catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);if(batch)EditorApplication.Exit(1);}
        }
    }
}
#endif
