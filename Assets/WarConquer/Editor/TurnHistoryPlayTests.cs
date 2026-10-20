#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class TurnHistoryPlayTests
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static void Invoke(WarConquerController ui,string method,params object[] args)
        {typeof(WarConquerController).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,args);}
        static Button Button(WarConquerController ui)=>ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Rewind / Deshacer");
        public static void Run(WarConquerController ui)
        {
            string saved=GamePersistence.Serialize(ui.Game.State);
            try
            {
                Invoke(ui,"StartMatch",false);var g=ui.Game;var button=Button(ui);
                Check(!button.interactable&&button.gameObject.activeInHierarchy,"Deshacer inicial no visible/deshabilitado.");
                foreach(var text in ui.GetComponentsInChildren<Text>().Where(t=>t.transform.parent.name=="Etapa de uso"&&t.text.Contains("\n")))
                    Check(text.preferredHeight<=text.rectTransform.rect.height+.1f&&text.text.Contains("TERRAFORMACIÓN"),"Indicador de tres etapas recortado.");
                Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="IR A ASALTO"),"Botón inicial incorrecto.");
                g.AdvanceStage();Check(button.interactable&&g.State.stage==TurnStage.Assault,"Deshacer en Asalto.");
                g.AdvanceStage();Check(button.interactable&&g.State.stage==TurnStage.Terraforming,"Deshacer en Terraformación.");
                button.onClick.Invoke();Check(g.State.stage==TurnStage.Assault&&button.interactable,"Botón no vuelve a Asalto.");
                button.onClick.Invoke();Check(g.State.stage==TurnStage.Deployment&&!button.interactable,"Botón no vuelve al inicio.");
                var spell=PrototypeScenario.Take(g,0,"a-price-to-pay");g.State.Active.hand.Add(spell);g.State.Active.currentEnergy=20;
                string before=GamePersistence.Serialize(g.State);Check(g.Play(spell.instanceId,Array.Empty<int>()),g.LastMessage);
                Check(g.State.pendingChoices.Count>0&&button.interactable,"Deshacer elección pendiente.");
                Check(button.transform.GetSiblingIndex()==button.transform.parent.childCount-1,"Diálogo tapa Deshacer.");
                button.onClick.Invoke();Check(g.State.pendingChoices.Count==0&&GamePersistence.Serialize(g.State)==before,"UI no cancela magia completa.");
                Check(!ui.GetComponentsInChildren<Text>().Any(t=>t.text=="CONFIRMAR DESCARTE"),"Diálogo obsoleto tras deshacer.");
                g.AdvanceStage();g.AdvanceStage();g.EndTurn();Check(!button.interactable,"Botón conserva turno previo.");
                Invoke(ui,"ShowSetup");Check(!button.gameObject.activeInHierarchy,"Deshacer aparece en setup.");
                Debug.Log("TURN_HISTORY_UI_PASSED: botón real, tres etapas, varios retrocesos, elección modal, fin de turno y setup.");
            }
            finally{Invoke(ui,"CloseModal");ui.Game.Restore(GamePersistence.Deserialize(saved));}
        }
    }
}
#endif
