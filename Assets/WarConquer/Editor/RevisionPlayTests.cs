#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class RevisionPlayTests
    {
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(WarConquerController ui,string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static CardInstance Select(WarConquerController ui,string id)
        {
            var g=ui.Game;var c=PrototypeScenario.Take(g,g.State.activePlayer,id);g.State.Active.hand.Add(c);g.State.Active.currentEnergy=20;g.State.stage=g.Catalog[id].allowedPhases[0];
            Invoke(ui,"SelectCard",c,g.State.Active);return c;
        }
        public static void Run(WarConquerController ui)
        {
            Invoke(ui,"StartMatch",false);var g=ui.Game;var world=ui.GetComponentInChildren<Board3DScene>();
            var far=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0);far.biome=Biome.Forest;var enemy=PrototypeScenario.Spawn(g,1,"nomada-de-arena",far.id);
            var card=Select(ui,"espora-somnifera");ui.TileClick(far.id);
            Check(g.IsSleeping(enemy)&&g.State.Active.discardPile.Contains(card),"Clic de magia no aplica Sueño.");
            var model=world.GetComponentsInChildren<BoardStatusVisual>().Single(v=>v.GetComponent<Hex3DTarget>().tileId==far.id);
            Check(model.transform.Find("Sueño · burbujas y Zzz").gameObject.activeSelf&&model.GetComponentsInChildren<TextMesh>().Any(t=>t.text=="Zzz"),"Sueño invisible.");
            card=Select(ui,"nube-de-esporas");ui.TileClick(far.id);Check(enemy.poison==0,"La selección múltiple resuelve antes de confirmar.");Invoke(ui,"Confirm");
            Check(enemy.poison==1&&g.State.Active.discardPile.Contains(card)&&model.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf,"Veneno o confirmación de magia no funciona.");
            card=Select(ui,"crecimiento-descontrolado");var a=g.State.tiles.First(t=>g.TerraformTarget(t.id)&&t.neighbors.Any(g.TerraformTarget));int b=a.neighbors.First(g.TerraformTarget);
            typeof(WarConquerController).GetField("chosenBiome",Flags).SetValue(ui,Biome.Swamp);ui.TileClick(a.id);ui.TileClick(b);
            Check(a.biome==Biome.Swamp&&g.State.tiles[b].biome==Biome.Swamp&&g.State.Active.discardPile.Contains(card),"Terraformación múltiple no respeta la selección.");
            Check(StateValidator.Validate(g.State,g.Catalog)=="","Magias en Canvas rompen la partida.");
            Debug.Log("WAR_CONQUER_REVISION_UI_PASSED: clic de magias, selección múltiple, bioma elegido, calavera, burbujas y sueño.");
        }
    }
}
#endif
