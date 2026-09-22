#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer.Editor
{
    public static class FeedbackPlayTests
    {
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Invoke(object instance,string method,params object[] args)=>instance.GetType().GetMethod(method,Flags).Invoke(instance,args);
        static void Check(bool condition,string reason){if(!condition)throw new Exception(reason);}
        static Piece Put(GameManager g,string id,int owner,int q,int r)
        {var t=g.State.tiles.First(t=>t.q==q&&t.r==r);return g.Place(id,owner,t.id);}
        public static void Run(WarConquerController ui)
        {
            Invoke(ui,"StartMatch",false);Invoke(ui,"ShowSetup");
            Check(!ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Cambiar semilla"),"Persiste Cambiar semilla.");
            Check(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Escenario de pruebas"),"Se perdió escenario de pruebas.");Invoke(ui,"StartMatch",false);
            var g=ui.Game;var p=Put(g,"larva-micelial",0,0,0);var t=g.State.tiles[p.tileId];t.biome=Biome.Swamp;
            Put(g,"lushroom",1,1,0);g.State.tiles[g.State.tiles.First(x=>x.q==1&&x.r==0).id].biome=Biome.Swamp;
            p.poison=1;p.poisonApplications=1;p.sleepUntilTurn=g.State.turn+4;p.slow=2;p.slowUntilTurn=g.State.turn+4;
            p.bonusAttack=2;p.temporaryMovement=4;p.movementExpiresTurn=g.State.turn;p.statBonuses.Add(new StatBonus{hp=1,attack=1,expiresTurn=g.State.turn+4});p.health++;p.evolved=true;
            t.specialEffect=new TerrainEffect{threshold=5,damage=1};
            string before=GamePersistence.Serialize(g.State);g.Notify("Estados de prueba");
            var world=ui.GetComponentInChildren<Board3DScene>();var visual=world.GetComponentsInChildren<BoardStatusVisual>().First(v=>v.GetComponent<Hex3DTarget>().tileId==p.tileId);
            foreach(string id in new[]{"slow","attack","health","movement","evolved","no-heal","unstable"})Check(visual.transform.Find("Estado · "+id)?.gameObject.activeSelf==true,"Falta estado: "+id);
            Check(visual.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf&&visual.transform.Find("Sueño · burbujas y Zzz").gameObject.activeSelf,"Faltan veneno/sueño.");
            Check(world.GetComponentsInChildren<BoardTerrainStatusVisual>().Length==1,"Terreno inestable invisible.");
            Check(GamePersistence.Serialize(g.State)==before,"Los efectos visuales mutan la partida.");
            p.poison=0;p.sleepUntilTurn=0;p.slow=0;p.bonusAttack=0;p.temporaryMovement=0;p.statBonuses.Clear();p.forestSinceTurn=-1;t.specialEffect=null;
            g.Notify("Retirar estados");Check(!visual.transform.Find("Estado · slow").gameObject.activeSelf&&!visual.transform.Find("Estado · no-heal").gameObject.activeSelf&&!visual.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf,"Estados no desaparecen.");
            Check(world.GetComponentsInChildren<BoardTerrainStatusVisual>().Length==0,"Aviso de terreno no expira.");
            p.health=1;g.Notify("Daño");Check(visual.transform.Find("Cambio de vida · partículas").gameObject.activeSelf,"Daño sin efecto.");GenericCardRules.Heal(g,p,1);g.Notify("Curación");Check(visual.GetComponentsInChildren<TextMesh>().Any(l=>l.text=="+1 HP"),"Curación sin efecto.");
            // Shield readiness follows the combat rules rather than a permanent cosmetic tag.
            var protectedUnit=Put(g,"bestia-micelial",0,-2,0);var guardCard=g.Catalog.All.First(c=>c.Has("FirstAttackGuard"));Put(g,guardCard.id,0,-2,1);g.Notify("Protección");
            Check(PieceStatusPresentation.Collect(g,protectedUnit).Any(s=>s.id=="guard"),"Escudo ausente.");protectedUnit.protectionRound=g.State.round;
            Check(PieceStatusPresentation.Collect(g,protectedUnit).Any(s=>s.id=="guard-used")&&!PieceStatusPresentation.Collect(g,protectedUnit).Any(s=>s.id=="guard"),"Escudo usado no cambia.");
            Invoke(ui,"StartMatch",false);g=ui.Game;var feedback=ui.GetComponent<ConquestAudioFeedback>();
            Check(feedback!=null&&feedback.Trumpet!=null&&feedback.Trumpet.length>.5f&&feedback.Trumpet.length<2,"Fanfarria no cargada.");
            var samples=new float[feedback.Trumpet.samples*feedback.Trumpet.channels];Check(feedback.Trumpet.GetData(samples,0)&&samples.Any(s=>Math.Abs(s)>.1f)&&samples.All(s=>Math.Abs(s)<1),"Audio vacío o saturado.");
            foreach(var point in g.State.tiles.Where(ConquestManager.IsCenter)){point.biome=Biome.Swamp;PrototypeScenario.Spawn(g,0,"alligator-revengeful-bite",point.id);}
            ConquestManager.Refresh(g.State);g.State.turn+=4;ConquestManager.ScoreStart(g.State,0);g.Notify("Tres puntos reales");
            Check(g.State.Active.conquestPoints==3&&feedback.PendingPoints==3,"Un sonido por punto real.");g.Notify("Refrescar sin puntuar");Check(feedback.PendingPoints==3,"Sonido duplicado por Render.");
            int played=feedback.PlayedPoints;Invoke(feedback,"Update");Check(feedback.PlayedPoints==played+1&&feedback.PendingPoints==2,"No reproduce fanfarria.");Invoke(feedback,"Update");Check(feedback.PlayedPoints==played+1,"Fanfarria superpuesta.");
            g.Restore(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)));Check(feedback.PendingPoints==0,"Cargar reproduce puntos antiguos.");Invoke(ui,"StartMatch",false);Check(feedback.PendingPoints==0,"Nueva partida conserva cola.");
            Debug.Log("FEEDBACK_PLAY_PASSED: sin Cambiar semilla, estados animados/retiro, curación/daño, escudos, WAV real, puntos reales sin duplicación y carga silenciosa.");
        }
    }
}
#endif
