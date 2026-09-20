#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
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
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(PendingKey,false))return;
                SessionState.SetBool(PendingKey,false);
                EditorApplication.delayCall+=()=>{Run();EditorApplication.ExecuteMenuItem("Window/General/Game");};
            };
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
            try
            {
                Invoke(ui,"StartMatch",false);var g=ui.Game;
                Check(Has(ui,"ETAPAS · J1")&&Has(ui,"ENERGÍA 3 / 3"),"Estado inicial no reflejado en Canvas.");
                for(int i=0;i<4;i++)Check(Has(ui,"J"+(i+1)+" "+g.State.players[i].leader),"Falta un jugador en la puntuación.");
                Check(Has(ui,"INFLUENCIA MICELIAL")&&Has(ui,"COSTE 3 E"),"Falta la habilidad o coste del Líder.");
                g.AdvanceStage();Check(Has(ui,"IR A ASALTO")&&Has(ui,"Terraformar casilla"),"Acciones de Terraformación ausentes.");
                g.AdvanceStage();Check(Has(ui,"FINALIZAR TURNO"),"Asalto ausente.");
                g.EndTurn();Check(Has(ui,"ETAPAS · J2")&&Has(ui,"DOMINIO DE LAS ARENAS"),"Interfaz no sigue el turno del jugador.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                g.State.players[0].conquestPoints=9;g.State.tiles.First(ConquestManager.IsCenter).owner=0;
                ConquestManager.RoundCompleted(g.State);g.Notify("Comprobación de victoria.");
                Check(Has(ui,"JUGADOR 1 HA GANADO")&&Has(ui,"10 PUNTOS DE CONQUISTA")&&!g.CanAct,"Pantalla de victoria de Conquista incorrecta.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                g.State.players[0].eliminated=g.State.players[1].eliminated=g.State.players[3].eliminated=true;
                ConquestManager.CheckLastLeader(g.State);g.Notify("Comprobación de victoria.");
                Check(Has(ui,"JUGADOR 3 HA GANADO")&&Has(ui,"ÚLTIMO LÍDER EN PIE")&&!g.CanAct,"Pantalla de último Líder incorrecta.");
                Invoke(ui,"StartMatch",false);g=ui.Game;
                string before=GamePersistence.Serialize(g.State);
                Invoke(ui,"ShowCardModal",g.Catalog["bestia-micelial"],g.State.Active);
                Check(Has(ui,"VENTANAS DE USO")&&before==GamePersistence.Serialize(g.State),"La consulta de carta modifica la partida.");
                Debug.Log("WAR_CONQUER_UI_PASSED: Canvas, cuatro jugadores, tres etapas, Líderes, dos pantallas de victoria y consulta sin mutación.");
            }
            finally
            {
                Invoke(ui,"CloseModal");ui.Game.Restore(GamePersistence.Deserialize(previous));
            }
        }
    }
}
#endif
