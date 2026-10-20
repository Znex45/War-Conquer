#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer.Editor
{
    public static class FaunarPlayTests
    {
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(WarConquerController ui,string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Set(WarConquerController ui,string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static CardInstance Select(WarConquerController ui,string id)
        {var g=ui.Game;var card=PrototypeScenario.Take(g,g.ActingPlayerId,id);g.ActingPlayer.hand.Add(card);g.ActingPlayer.currentEnergy=40;Invoke(ui,"SelectCard",card,g.ActingPlayer);return card;}
        public static void Run(WarConquerController ui)
        {
            var oldLeaders=(string[])typeof(WarConquerController).GetField("leaders",Flags).GetValue(ui);
            try
            {
                Set(ui,"humanPlayers",4);Set(ui,"leaders",new[]{"FAUNAR","SAHRIA","ZUKGROK","FAUNAR"});Invoke(ui,"ShowSetup");
                Check(ui.GetComponentsInChildren<Button>().Count(b=>b.name.Contains("FAUNAR · 50 cartas"))==4,"No ofrece Faunar a cada participante.");
                Check(!ui.GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Interfaz de partida").gameObject.activeInHierarchy,"Setup no oculta partida.");
                Invoke(ui,"StartMatch",false);var g=ui.Game;
                Check(g.State.Active.leader=="FAUNAR"&&ui.GetComponentsInChildren<Text>().Any(t=>t.text=="AVAILABLE"),"Faunar no muestra pasiva disponible.");
                var tile=g.State.tiles.First(t=>t.owner==0&&!t.IsOccupied&&t.baseOwner<0);Select(ui,"terraform");ui.TileClick(tile.id);
                Check(tile.biome==Biome.Forest,"Clic Terraform no cambia bioma.");
                Select(ui,"berry-bush");ui.TileClick(tile.id);Check(tile.structure?.cardId=="berry-bush"&&g.State.Active.currentEnergy==39,"UI coste de Faunar incorrecto.");
                Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="USED"),"No actualiza estado de Faunar.");
                var spell=Select(ui,"one-for-the-team");Check(g.State.pendingChoices.Count==1,"No abre elección de descarte desde mano.");
                var choice=g.State.pendingChoices[0];int chosen=g.State.Active.hand[0].instanceId;string name=g.Catalog[g.State.Active.hand[0].cardId].name;
                var body=ui.GetComponentsInChildren<RectTransform>().First(r=>r.name==choice.prompt);body.GetComponentsInChildren<Button>().First(b=>b.name==name).onClick.Invoke();
                body=ui.GetComponentsInChildren<RectTransform>().First(r=>r.name==choice.prompt);var confirm=body.GetComponentsInChildren<Button>().First(b=>b.name=="CONFIRMAR DESCARTE");
                Check(confirm.interactable,"Confirmación de descarte deshabilitada.");confirm.onClick.Invoke();
                Check(g.State.pendingChoices.Count==0&&g.State.Active.discardPile.Any(c=>c.instanceId==chosen),"UI no descarta la selección.");
                Check(StateValidator.Validate(g.State,g.Catalog)=="","Interfaz alteró el mazo.");
                var center=g.State.tiles.First(t=>t.q==0&&t.r==0);var enemy=g.Place("med-camp",1,center.id);g.State.stage=TurnStage.Assault;
                Select(ui,"bomb");ui.TileClick(center.id);Check(enemy.health==2,"Bomb UI no hace tres daño a estructura.");
                var adjacent=g.State.tiles[center.neighbors.First()];var unit=g.Place("rabbit-token",0,adjacent.id);int mov=unit.remainingMovement;
                Select(ui,"time-to-move");ui.TileClick(adjacent.id);Check(unit.remainingMovement==mov+4,"UI no aplica +4 MOV.");
                var view=CardPresentation.Draw(ui.transform,g,g.Catalog["ferret-eluding-hunter"],g.State.Active,0,0,388,624,false,true);
                var labels=view.GetComponentsInChildren<Text>();var movement=labels.Single(t=>t.text=="MOV").rectTransform;var attack=labels.Single(t=>t.text=="ATK").rectTransform;
                Check(movement.anchoredPosition.x<attack.anchoredPosition.x,"MOV/ATK invertidos en carta.");UnityEngine.Object.Destroy(view.gameObject);
                Debug.Log("FAUNAR_PLAY_PASSED: setup, líder, coste, cartas, descarte elegido, Bomb, radio MOV y posiciones de estadísticas.");
            }
            finally{Invoke(ui,"CloseModal");Set(ui,"leaders",oldLeaders);Set(ui,"humanPlayers",4);Invoke(ui,"StartMatch",false);}
        }
    }
}
#endif
