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
            var far=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0);far.biome=Biome.Desert;var enemy=PrototypeScenario.Spawn(g,1,"old-mummy",far.id);
            EffectManager.Sleep(g,enemy,0);g.Notify("Comprobación de Sueño");
            var model=world.GetComponentsInChildren<BoardStatusVisual>().Single(v=>v.GetComponent<Hex3DTarget>().tileId==far.id);
            Check(model.transform.Find("Sueño · burbujas y Zzz").gameObject.activeSelf&&model.GetComponentsInChildren<TextMesh>().Any(t=>t.text=="Zzz"),"Sueño invisible.");
            EffectManager.Poison(g,enemy,1,0);g.Notify("Comprobación de Poison");
            Check(enemy.poison==1&&model.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf,"Veneno invisible.");
            var card=Select(ui,"terraform");var a=g.State.tiles.First(t=>g.TerraformTarget(t.id)&&!t.IsOccupied&&t.baseOwner<0);
            typeof(WarConquerController).GetField("chosenBiome",Flags).SetValue(ui,Biome.Swamp);ui.TileClick(a.id);
            Check(a.biome==Biome.Swamp&&g.State.Active.discardPile.Contains(card),"Terraformación no respeta bioma elegido.");
            var ally=PrototypeScenario.Spawn(g,0,"alligator-revengeful-bite",g.State.tiles.First(t=>t.q==0&&t.r==0).id);
            var victim=PrototypeScenario.Spawn(g,1,"sandy-ember",g.State.tiles.First(t=>t.q==2&&t.r==0).id);victim.health=1;
            card=Select(ui,"engage");ui.TileClick(ally.tileId);Check(victim.health==1,"Engage resolvió con un solo objetivo.");ui.TileClick(victim.tileId);
            Check(victim.health==0&&ally.tileId==victim.tileId&&g.State.Active.discardPile.Contains(card),"Engage no resuelve selección y movimiento.");
            while(g.State.pendingChoices.Count>0){var choice=g.State.pendingChoices[0];ChoiceManager.Resolve(g,ChoiceManager.Targets(g,choice).Take(choice.count).ToArray());}
            Check(StateValidator.Validate(g.State,g.Catalog)=="","Magias en Canvas rompen la partida.");
            Debug.Log("WAR_CONQUER_REVISION_UI_PASSED: clic de magias, selección múltiple, bioma elegido, calavera, burbujas y sueño.");
        }
    }
}
#endif
