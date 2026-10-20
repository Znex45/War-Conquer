#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer.Editor
{
    public static class FactionPlayTests
    {
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(WarConquerController ui,string method,params object[] args)=>typeof(WarConquerController).GetMethod(method,Flags).Invoke(ui,args);
        static void Set(WarConquerController ui,string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static void Click(WarConquerController ui,string label)=>ui.GetComponentsInChildren<Button>().First(b=>b.name==label&&b.interactable).onClick.Invoke();
        static void Select(WarConquerController ui,string id)
        {var c=PrototypeScenario.Take(ui.Game,ui.Game.ActingPlayerId,id);ui.Game.ActingPlayer.hand.Add(c);Invoke(ui,"SelectCard",c,ui.Game.ActingPlayer);}
        public static void Run(WarConquerController ui)
        {
            var old=(string[])typeof(WarConquerController).GetField("leaders",Flags).GetValue(ui);
            try
            {
                Set(ui,"humanPlayers",4);Set(ui,"leaders",new[]{"SAHRIA","ZUKGROK","FAUNAR","ZUKGROK"});Invoke(ui,"StartMatch",false);var g=ui.Game;
                var tile=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0);TerrainManager.Terraform(g,tile.id,Biome.Swamp,0);g.State.Active.currentEnergy=3;g.Notify("Sahria UI");
                Click(ui,"Usar SAHRIA +1 E");Select(ui,"dune-cammel");Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="2 → 3"),"Recargo no visible en carta.");
                ui.TileClick(tile.id);Check(tile.unit?.cardId=="dune-cammel"&&g.State.Active.currentEnergy==0,"Sahria UI no cobra o despliega.");
                g.AdvanceStage();g.AdvanceStage();g.EndTurn();g.State.Active.currentEnergy=20;
                tile=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0&&!t.IsOccupied);Select(ui,"firefly-nest");ui.TileClick(tile.id);
                Check(g.State.pendingChoices.Single().kind=="Summon","No permite escoger invocación.");
                int first=ChoiceManager.Targets(g,g.State.pendingChoices[0]).First();ui.TileClick(first);int second=ChoiceManager.Targets(g,g.State.pendingChoices[0]).First();ui.TileClick(second);
                Check(g.State.tiles[first].unit?.cardId=="firefly-token"&&g.State.tiles[second].unit?.cardId=="firefly-token","Firefly Nest no invoca dos por UI.");
                var firefly=g.State.tiles[first].unit;var enemyTile=g.State.tiles.First(t=>!t.IsOccupied&&!t.blocked&&t.baseOwner<0&&BoardManager.AxialDistance(t,g.State.tiles[first])<=2);
                var enemy=PrototypeScenario.Spawn(g,0,"old-mummy",enemyTile.id);g.State.stage=TurnStage.Assault;g.Notify("Firefly UI");
                Set(ui,"selectedPiece",firefly);Invoke(ui,"Begin","ability");ui.TileClick(enemyTile.id);
                Check(g.State.pendingChoices.Single().kind=="DeathTerraform"&&enemy.poison==0,"Muerte no abre elección de bioma.");
                Check(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Pantano"),"Falta selector de bioma de muerte.");Click(ui,"Pantano");
                Check(g.State.tiles[first].biome==Biome.Swamp&&enemy.poison==1&&g.State.pendingChoices.Count==0,"Secuencia de Firefly UI incorrecta.");
                var model=ui.GetComponentInChildren<Board3DScene>().GetComponentsInChildren<BoardStatusVisual>().First(v=>v.GetComponent<Hex3DTarget>().tileId==enemyTile.id);
                Check(model.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf,"Poison sin calavera.");
                Check(StateValidator.Validate(g.State,g.Catalog)=="","UI alteró la conservación de los mazos.");
                Debug.Log("FACTION_PLAY_PASSED: Sahria +1, coste visible, Firefly Nest, dos Tokens elegidos, habilidad, muerte, bioma gratuito y Poison 3D.");
            }
            finally{Invoke(ui,"CloseModal");Set(ui,"leaders",old);Set(ui,"humanPlayers",4);Invoke(ui,"StartMatch",false);}
        }
    }
}
#endif
