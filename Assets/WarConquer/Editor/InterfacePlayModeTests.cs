#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    // Integration checks against the live Canvas. Never writes the player's saved game.
    public static class InterfacePlayModeTests
    {
        const string PendingKey="WarConquer.PendingInterfaceCheck";
        // Unity CLI entry point for the same Play Mode integration test.
        public static void Begin()
        {
            if(EditorApplication.isPlaying){Run();return;}
            SessionState.SetBool(PendingKey,true);EditorApplication.isPlaying=true;
        }
        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorApplication.delayCall+=RunRequestedCheck;
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(PendingKey,false))return;
                SessionState.SetBool(PendingKey,false);
                EditorApplication.delayCall+=()=>
                {
                    Run();
                    var view=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
                    view.maximized=true;view.Focus();
                };
            };
        }
        static void RunRequestedCheck()
        {
            if(File.Exists("Temp/WarConquer.3DCheck.request"))return;
            // Optional local test request, consumed once after an editor script import.
            const string request="Temp/WarConquer.InterfaceCheck.request";
            if(!File.Exists(request))return;
            if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=RunRequestedCheck;return;}
            File.Delete(request);Begin();
        }
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        static void Invoke(WarConquerController ui,string method,params object[] args)
        {typeof(WarConquerController).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,args);}
        static string[] Labels(WarConquerController ui)=>ui.GetComponentsInChildren<Text>().Select(t=>t.text).ToArray();
        static bool Has(WarConquerController ui,string value)=>Labels(ui).Any(t=>t.Contains(value));
        [MenuItem("War & Conquer/Comprobar interfaz en Play")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Inicia Play antes de comprobar la interfaz.");
            var ui=UnityEngine.Object.FindAnyObjectByType<WarConquerController>();
            if(ui==null)throw new Exception("No hay un Graybox activo.");
            string previous=GamePersistence.Serialize(ui.Game.State);
            var humanField=typeof(WarConquerController).GetField("humanPlayers",BindingFlags.Instance|BindingFlags.NonPublic);
            int previousHumans=(int)humanField.GetValue(ui);
            try
            {
                humanField.SetValue(ui,4);
                Invoke(ui,"StartMatch",false);var g=ui.Game;
                Check(Has(ui,"ETAPAS · J1")&&Has(ui,"ENERGÍA 3 / 3"),"Estado inicial no reflejado en Canvas.");
                for(int i=0;i<4;i++)Check(Has(ui,"J"+(i+1)+" "+g.State.players[i].leader),"Falta un jugador en la puntuación.");
                Check(Has(ui,"INFLUENCIA MICELIAL")&&Has(ui,"COSTE 3 E"),"Falta la habilidad o coste del Líder.");
                g.AdvanceStage();Check(Has(ui,"IR A TERRAFORMACIÓN"),"Ataque ausente.");
                g.AdvanceStage();Check(Has(ui,"FINALIZAR TURNO")&&Has(ui,"Terraformar casilla"),"Terraformación ausente.");
                g.EndTurn();Check(Has(ui,"ETAPAS · J2")&&Has(ui,"DOMINIO DE LAS ARENAS"),"Interfaz no sigue el turno del jugador.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                g.State.players[0].conquestPoints=9;var site=g.State.tiles.First(ConquestManager.IsCenter);site.biome=Biome.Forest;PrototypeScenario.Spawn(g,0,"bestia-micelial",site.id);ConquestManager.Refresh(g.State);g.State.turn+=4;
                ConquestManager.ScoreStart(g.State,0);g.Notify("Comprobación de victoria.");
                Check(Has(ui,"JUGADOR 1 HA GANADO")&&Has(ui,"10 PUNTOS DE CONQUISTA")&&!g.CanAct,"Pantalla de victoria de Conquista incorrecta.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                g.State.players[0].eliminated=g.State.players[1].eliminated=g.State.players[3].eliminated=true;
                ConquestManager.CheckLastLeader(g.State);g.Notify("Comprobación de victoria.");
                Check(Has(ui,"JUGADOR 3 HA GANADO")&&Has(ui,"ÚLTIMO LÍDER EN PIE")&&!g.CanAct,"Pantalla de último Líder incorrecta.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                string before=GamePersistence.Serialize(g.State);
                Invoke(ui,"ShowCardModal",g.Catalog["bestia-micelial"],g.State.Active);
                Check(Has(ui,"VENTANAS DE USO")&&before==GamePersistence.Serialize(g.State),"La consulta de carta modifica la partida.");
                Invoke(ui,"CloseModal");SetupPlayModeTests.Run(ui);InterfaceRefinementTests.Run(ui);
                Debug.Log("WAR_CONQUER_UI_PASSED: Canvas, cuatro jugadores, tres etapas, Líderes, dos pantallas de victoria y consulta sin mutación.");
            }
            finally
            {
                Invoke(ui,"CloseModal");humanField.SetValue(ui,previousHumans);ui.Game.Restore(GamePersistence.Deserialize(previous));
                if(ui.Game.State.phase==Phase.Setup)Invoke(ui,"ShowSetup");
            }
        }
    }
}
#endif
