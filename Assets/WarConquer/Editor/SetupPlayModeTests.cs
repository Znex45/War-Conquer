#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class SetupPlayModeTests
    {
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Invoke(WarConquerController ui,string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Set(WarConquerController ui,string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        static void Click(WarConquerController ui,string name)
        {var button=ui.GetComponentsInChildren<Button>().First(b=>b.name==name);Check(button.interactable,"Botón deshabilitado: "+name);button.onClick.Invoke();}
        public static void Run(WarConquerController ui)
        {
            Set(ui,"humanPlayers",1);Invoke(ui,"ShowSetup");string before=GamePersistence.Serialize(ui.Game.State);
            var match=ui.GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Interfaz de partida");
            Check(!match.gameObject.activeInHierarchy&&!ui.GetComponentInChildren<Board3DScene>(true).gameObject.activeInHierarchy,"El setup deja visible la interfaz o el mundo de partida.");
            foreach(string label in new[]{"1 PERSONA · VS IA","2 PERSONAS","3 PERSONAS","4 PERSONAS"})
                Check(ui.GetComponentsInChildren<Button>().Any(b=>b.name==label),"Falta selector de personas.");
            foreach(int count in new[]{2,3,4,1})
            {
                Click(ui,count==1?"1 PERSONA · VS IA":count+" PERSONAS");
                Check(GamePersistence.Serialize(ui.Game.State)==before,"El selector avanza la partida durante la pausa.");
                int rows=ui.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Mazo de J"));
                Check(rows==(count==1?2:count),"Número de selectores de mazo incorrecto.");
            }
            var playerRow=ui.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Mazo de J1");
            playerRow.GetComponentsInChildren<Button>().First(b=>b.name.Contains("SAHRIA")).onClick.Invoke();
            var aiRow=ui.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Mazo de J2");
            aiRow.GetComponentsInChildren<Button>().First(b=>b.name.Contains("ZUKGROK")).onClick.Invoke();
            Check(GamePersistence.Serialize(ui.Game.State)==before,"Elegir mazos modifica la partida actual.");
            Click(ui,"COMENZAR PARTIDA");
            Check(match.gameObject.activeInHierarchy&&ui.GetComponentInChildren<Board3DScene>()!=null,"Comenzar no revela la interfaz y el tablero.");
            Check(ui.Game.State.players[0].leader=="SAHRIA"&&ui.Game.State.players[1].leader=="ZUKGROK"&&ui.Game.State.players[1].isAI,"No aplica los mazos elegidos.");
            ui.Game.AdvanceStage();ui.Game.AdvanceStage();ui.Game.EndTurn();
            Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="TURNO DE LA IA"),"Falta indicador de rival automático.");
            before=GamePersistence.Serialize(ui.Game.State);Invoke(ui,"ShowSetup");Set(ui,"nextAiAction",0f);Invoke(ui,"UpdateAI");
            Check(before==GamePersistence.Serialize(ui.Game.State),"La IA juega detrás de la pantalla de preparación.");
            Invoke(ui,"CloseModal");Set(ui,"nextAiAction",0f);Invoke(ui,"UpdateAI");
            Check(match.gameObject.activeInHierarchy,"Cerrar preparación no recupera la partida.");
            Check(before!=GamePersistence.Serialize(ui.Game.State),"La IA no reanuda automáticamente.");
            // Return the original deck defaults after checking the actual button callbacks.
            Set(ui,"leaders",new[]{"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"});
            Debug.Log("WAR_CONQUER_SETUP_UI_PASSED: selector 1/2/3/4, mazos humano/IA, pausa y reanudación automática.");
        }
    }
}
#endif
