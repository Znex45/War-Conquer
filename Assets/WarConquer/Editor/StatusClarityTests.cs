#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class StatusClarityTests
    {
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static void Invoke(WarConquerController ui,string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Set(WarConquerController ui,string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        public static void Run(WarConquerController ui)
        {
            Set(ui,"humanPlayers",4);Invoke(ui,"StartMatch",false);var g=ui.Game;
            var tile=g.State.tiles.First(t=>t.q==0&&t.r==0);tile.biome=Biome.Swamp;
            var p=g.Place("larva-micelial",0,tile.id);p.poison=1;p.poisonApplications=1;p.sleepUntilTurn=g.State.turn+4;p.slow=2;p.slowUntilTurn=g.State.turn+4;
            p.bonusAttack=2;p.temporaryMovement=4;p.movementExpiresTurn=g.State.turn;p.statBonuses.Add(new StatBonus{hp=1,attack=1,expiresTurn=g.State.turn+4});p.health++;p.evolved=true;
            tile.specialEffect=new TerrainEffect{threshold=5,damage=1,expiresRound=g.State.round+2};
            GenericCardRules.SyncAuras(g);ui.Render();
            string before=GamePersistence.Serialize(g.State);
            var effects=PieceStatusPresentation.Collect(g,p);
            Check(effects.Count>3&&effects.All(e=>!string.IsNullOrEmpty(e.description)&&!string.IsNullOrEmpty(e.duration)),"Faltan explicaciones o duración de estados.");
            var world=ui.GetComponentInChildren<Board3DScene>();
            var visual=world.UnitVisual(p.id).GetComponent<BoardStatusVisual>();
            Check(visual.StatusCount==effects.Count&&visual.VisibleBadgeCount==3,"El resumen compacto no limita los iconos o pierde efectos.");
            visual.SetDisplay(BoardEffectDisplay.Compact,true);
            Check(!visual.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf&&!visual.transform.Find("Sueño · burbujas y Zzz").gameObject.activeSelf&&!visual.GetComponentsInChildren<TextMesh>().Any(t=>t.name=="Disponibilidad de ataque"),"Compacto seleccionado sigue mostrando efectos o textos grandes.");
            Check(visual.GetComponentsInChildren<TextMesh>().Any(t=>t.name=="Más estados"&&t.text=="+"+(effects.Count-3)),"No aparece el contador de efectos adicionales.");
            visual.SetDisplay(BoardEffectDisplay.SelectedOnly,false);Check(visual.VisibleBadgeCount==0&&!visual.transform.Find("Estados compactos").gameObject.activeSelf,"Solo selección deja visibles otras cartas.");
            visual.SetDisplay(BoardEffectDisplay.SelectedOnly,true);Check(visual.VisibleBadgeCount==3,"Seleccionar no revela los estados.");
            visual.SetDisplay(BoardEffectDisplay.Hidden,true);Check(visual.VisibleBadgeCount==0&&!visual.transform.Find("Veneno · calavera y huesos").gameObject.activeSelf&&!visual.transform.Find("Sueño · burbujas y Zzz").gameObject.activeSelf,"Ocultar deja efectos persistentes.");
            world.EffectDisplay=BoardEffectDisplay.Hidden;Set(ui,"selectedPiece",p);Set(ui,"selectedCard",-1);Set(ui,"focus",tile.id);Set(ui,"mode","inspect");ui.Render();
            var stateButton=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Estados ("+effects.Count+")");stateButton.onClick.Invoke();
            var scroll=ui.GetComponentsInChildren<ScrollRect>().Single(s=>s.name=="Lista de estados");
            Check(scroll.content.rect.height>scroll.viewport.rect.height&&scroll.verticalScrollbar!=null,"Varios estados no pueden consultarse con desplazamiento.");
            foreach(var e in effects)Check(scroll.GetComponentsInChildren<Text>().Any(t=>t.text==e.description),"Falta detalle de "+e.id);
            Invoke(ui,"CloseModal");Check(GamePersistence.Serialize(g.State)==before,"Consultar u ocultar estados cambia la partida.");
            var structure=g.Place("oasis-de-cristal",0,tile.id);ui.Render();
            var detail=ui.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Detalle de selección");
            detail.GetComponentsInChildren<Button>().Single(b=>b.name=="Estructura").onClick.Invoke();
            Check(typeof(WarConquerController).GetField("selectedPiece",Flags).GetValue(ui)==structure,"No se puede inspeccionar la estructura bajo una unidad.");
            ui.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Detalle de selección").GetComponentsInChildren<Button>().Single(b=>b.name=="Unidad").onClick.Invoke();
            Check(typeof(WarConquerController).GetField("selectedPiece",Flags).GetValue(ui)==p,"No se recupera la selección de unidad.");
            world.EffectDisplay=BoardEffectDisplay.Compact;ui.Render();
            for(int i=0;i<3;i++)ui.GetComponentsInChildren<Button>().Single(b=>b.name.StartsWith("Efectos:")).onClick.Invoke();
            Check(world.EffectDisplay==BoardEffectDisplay.Compact,"El botón no recorre los tres modos.");
            Debug.Log("STATUS_CLARITY_PASSED: capped badges, overflow, selected/hidden modes, all descriptions/durations scrollable, co-occupant inspection and no game/RNG mutation.");
        }
    }
}
#endif
