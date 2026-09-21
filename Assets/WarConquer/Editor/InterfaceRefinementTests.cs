#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class InterfaceRefinementTests
    {
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Invoke(WarConquerController ui,string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static void Click(WarConquerController ui,string name)=>ui.GetComponentsInChildren<Button>().First(b=>b.name==name).onClick.Invoke();
        public static void Run(WarConquerController ui)
        {
            typeof(WarConquerController).GetField("humanPlayers",Flags).SetValue(ui,4);Invoke(ui,"StartMatch",false);
            var g=ui.Game;string before=GamePersistence.Serialize(g.State);
            var hand=ui.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Hand");
            foreach(var instance in g.State.Active.hand.Take(6))
            {
                var card=g.Catalog[instance.cardId];var view=hand.Find(card.name);Check(view!=null,"Falta carta de la mano.");
                var dim=view.GetComponent<CanvasGroup>();bool blocked=g.CardBlockReason(instance,false)!="";
                Check(!blocked||dim!=null&&dim.alpha<.55f,"La carta no disponible no se oscurece.");
                var badge=view.Find("Etapa de uso");Check(badge!=null&&badge.GetComponentInChildren<Text>().text.Contains(TimingRules.StageName(card.allowedPhases[0])),"Falta la etapa legible.");
                Check(view.GetComponent<Button>().interactable&&view.GetComponent<CardHover>()!=null,"La carta oscura perdió su consulta.");
            }
            Click(ui,"Ver datos");Check(ui.GetComponentInChildren<Board3DScene>().ShowLabels,"No se recuperan los datos del tablero.");Click(ui,"Ocultar datos");
            Click(ui,"Detalles");Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("N:")),"Falta el control detallado por zona.");Click(ui,"Menos");
            Click(ui,"Ver habilidad");Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Envenena 1")),"Falta el texto completo del Líder.");Click(ui,"Cerrar");
            Click(ui,"Menú");foreach(string action in new[]{"Nueva partida","Guardar","Cargar","Registro","Ayuda","Continuar partida"})Check(ui.GetComponentsInChildren<Button>().Any(b=>b.name==action),"Se perdió una función: "+action);
            Click(ui,"Ayuda");Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("último Líder")),"No se conservan las reglas de victoria.");Click(ui,"Volver al menú");Click(ui,"Nueva partida");
            Check(!hand.gameObject.activeInHierarchy,"Menú de preparación deja visible la mano.");Click(ui,"Cerrar");
            Check(hand.gameObject.activeInHierarchy&&before==GamePersistence.Serialize(g.State),"Consultar la interfaz cambia la partida o no la restaura.");
            Debug.Log("WAR_CONQUER_INTERFACE_REFINEMENT_PASSED: cartas oscuras consultables, etapas visibles, funciones conservadas y setup aislado.");
        }
    }
}
#endif
