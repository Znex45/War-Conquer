#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer.Editor
{
    public static class XiltharPlayTests
    {
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(WarConquerController ui,string method,params object[] args)=>typeof(WarConquerController).GetMethod(method,Flags).Invoke(ui,args);
        static void Set(WarConquerController ui,string field,object value)=>typeof(WarConquerController).GetField(field,Flags).SetValue(ui,value);
        static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
        static void Click(WarConquerController ui,string label)=>ui.GetComponentsInChildren<Button>().First(b=>b.name==label&&b.interactable).onClick.Invoke();
        static Piece Put(GameManager g,string card,int q,int r)
        {var t=g.State.tiles.First(t=>t.q==q&&t.r==r);t.biome=Biome.Void;t.owner=0;t.blocked=false;return PrototypeScenario.Spawn(g,0,card,t.id);}
        public static void Run(WarConquerController ui)
        {
            var old=((string[])typeof(WarConquerController).GetField("leaders",Flags).GetValue(ui)).ToArray();
            try
            {
                Set(ui,"humanPlayers",4);Set(ui,"leaders",new[]{"FAUNAR","SAHRIA","ZUKGROK","FAUNAR"});Invoke(ui,"ShowSetup");
                var buttons=ui.GetComponentsInChildren<Button>().Where(b=>b.name=="XIL'THAR · 50 cartas").ToArray();Check(buttons.Length==4,"Xil'thar no disponible para los cuatro jugadores.");
                foreach(var b in buttons){var rect=(RectTransform)b.transform;Check(rect.anchoredPosition.x+rect.rect.width<=((RectTransform)rect.parent).rect.width,"Selector fuera del panel.");}
                buttons[0].onClick.Invoke();Click(ui,"COMENZAR PARTIDA");var g=ui.Game;Check(g.State.Active.leader=="XIL'THAR","No selecciona Xil'thar.");g.State.Active.currentEnergy=80;
                for(int i=0;i<4;i++){var t=g.State.tiles.First(t=>t.owner==0&&t.biome!=Biome.Void&&t.baseOwner<0);TerrainManager.Terraform(g,t.id,Biome.Void,0);}g.Notify("Xil'thar UI");
                Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="ENERGÍA GENERADA: +1"),"Falta contador fuera de la mano.");
                var dead=PrototypeScenario.Take(g,0,"crystal-minion");g.State.Active.discardPile.Add(dead);var spell=PrototypeScenario.Take(g,0,"under-the-darkness");g.State.Active.hand.Add(spell);
                Invoke(ui,"SelectCard",spell,g.State.Active);Check(g.State.pendingChoices.Single().kind=="DarknessPick","Falta selector de descarte.");
                var modal=(RectTransform)typeof(WarConquerController).GetField("modal",Flags).GetValue(ui);
                var recast=modal.GetComponentsInChildren<Button>().First(b=>b.name=="Crystal Minion");Check(recast.GetComponentsInChildren<Text>().Any(t=>t.text=="ENERGÍA · RECAST"),"No muestra coste alternativo.");
                recast.onClick.Invoke();Click(ui,"ELEGIR UNIT DEL DESCARTE");int landing=ChoiceManager.Targets(g,g.State.pendingChoices[0]).First(id=>g.State.tiles[id].biome==Biome.Void);ui.TileClick(landing);
                Check(g.State.tiles[landing].unit?.id==dead.instanceId&&g.State.tiles[landing].unit.underDarkness,"Recast no conserva carta/efecto.");
                Check(g.AdvanceStage(),g.LastMessage);var portal=Put(g,"portal",0,0);var crystal=Put(g,"crystal-minion",4,0);g.Notify("Portal UI");Set(ui,"selectedPiece",portal);Invoke(ui,"Begin","ability");ui.TileClick(crystal.tileId);
                Check(g.State.pendingChoices.Single().kind=="PortalLand","No solicita destino del Portal.");int destination=ChoiceManager.Targets(g,g.State.pendingChoices[0]).First();ui.TileClick(destination);Check(crystal.tileId==destination&&portal.abilityUsed,"Portal UI no mueve.");
                var beacon=Put(g,"void-beacon",-3,0);var victim=Put(g,"dark-fly",-3,1);CombatManager.Remove(g,victim);g.Notify("Beacon UI");ui.TileClick(beacon.tileId);Click(ui,"Pagar 5 Energía");Check(g.State.tiles[beacon.tileId].unit?.id==victim.id,"Beacon UI no recupera carta.");
                var hole=Put(g,"void-hole",0,3);CombatManager.DirectDamage(g,crystal,2);g.Notify("Void Hole UI");ui.TileClick(hole.tileId);Check(hole.health==2&&crystal.health==3,"No selecciona prevención.");
                Check(StateValidator.Validate(g.State,g.Catalog)=="","La UI duplicó o perdió cartas.");
                Debug.Log("XILTHAR_PLAY_PASSED: cuatro selectores, líder, coste recast, descarte real, Portal, Beacon y prevención por clic.");
            }
            finally{Invoke(ui,"CloseModal");Set(ui,"leaders",old);Set(ui,"humanPlayers",4);Invoke(ui,"StartMatch",false);}
        }
    }
}
#endif
