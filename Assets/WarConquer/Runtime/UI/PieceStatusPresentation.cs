using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public enum BoardEffectDisplay { Compact, SelectedOnly, Hidden }
    public readonly struct StatusIndicator
    {
        public readonly string id,label,description,duration;
        public readonly Color color;
        public StatusIndicator(string id,string label,Color color,string description="",string duration="")
        {this.id=id;this.label=label;this.color=color;this.description=description;this.duration=duration;}
    }
    public static class PieceStatusPresentation
    {
        // Presentation reads resolved game values; it never applies effects or changes the turn.
        public static List<StatusIndicator> Collect(GameManager g,Piece p)
        {
            var list=new List<StatusIndicator>();var c=g.Data(p);var t=g.State.tiles[p.tileId];
            if(p.poison>0)list.Add(new StatusIndicator("poison","VENENO "+FactionCardRules.PoisonDamage(g,p),new Color(.65f,.25f,.88f),"Pierde "+FactionCardRules.PoisonDamage(g,p)+" de vida al inicio de cada uno de sus turnos. El valor ya incluye los modificadores activos.","Persistente · hasta retirar el veneno"));
            if(g.IsSleeping(p))list.Add(new StatusIndicator("sleep","DORMIDO",new Color(.6f,.82f,1),"No puede moverse, atacar ni activar habilidades mientras duerme.",Until(p.sleepUntilTurn)));
            if(p.slow>0&&p.slowUntilTurn>=g.State.turn)list.Add(new StatusIndicator("slow","MOV -"+p.slow,new Color(.5f,.7f,1),"Reduce en "+p.slow+" los puntos de movimiento al preparar su turno.",Until(p.slowUntilTurn)));
            int terrain=TerrainManager.GetTerrainStatModifier(g,p);
            if(terrain>0)list.Add(new StatusIndicator("terrain","TERRENO +1 MOV / HP / ATK",new Color(.4f,.95f,.65f),"El bioma es compatible con su líder: +1 movimiento, +1 vida máxima y +1 ataque. Los valores de la carta seleccionada ya incluyen esta mejora.","Mientras permanezca en un bioma compatible"));
            int atk=CombatManager.AttackValue(g,p)-(p.evolved?g.State.rules.evolvedAttack:c.attack)-terrain;
            if(atk>0)list.Add(new StatusIndicator("attack","ATK +"+atk,new Color(1,.56f,.17f),"Ataque adicional por habilidades, auras y mejoras; no incluye el bono de terreno. "+(p.bonusAttack>0?"+"+p.bonusAttack+" de mejora hasta terminar su turno. ":"")+(p.permanentAttack>0?"+"+p.permanentAttack+" permanente. ":"")+BonusDetail(g,p,false),"Según la fuente · las auras se actualizan al moverse"));
            int hp=g.MaxHealth(p)-(p.evolved?g.State.rules.evolvedHealth:c.health)-terrain;
            if(hp>0)list.Add(new StatusIndicator("health","HP +"+hp,new Color(.35f,1,.55f),"Vida máxima adicional por habilidades y mejoras; no incluye el bono de terreno. "+BonusDetail(g,p,true),"Según la fuente · no es una curación"));
            int move=GenericCardRules.MovementAura(g,p)+p.temporaryMovement-terrain;
            move+=BoardManager.Nearby(g.State,p.tileId).Count(a=>a.owner==p.owner&&g.Data(a).Has("NomadAura")&&c.subtypes.Contains("Nómada")&&t.biome==Biome.Desert);
            if(!c.IsStructure&&move>0)list.Add(new StatusIndicator("movement","MOV +"+move,new Color(.25f,.94f,1),"Movimiento adicional por auras y habilidades, aparte del terreno. El movimiento disponible ya descuenta los pasos gastados."+(p.temporaryMovement>0?" +"+p.temporaryMovement+" temporal: "+Until(p.movementExpiresTurn)+".":""),"Las auras duran mientras se cumplan sus condiciones"));
            bool guard=BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).Has("FirstAttackGuard"))||c.Has("OasisGuard")&&t.structure!=null&&g.Data(t.structure).Has("Oasis");
            bool defense=c.Has("StructureGuard")&&BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).IsStructure)
                ||c.subtypes.Contains("Hongo")&&BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).Has("FungusDefenseAura"))
                ||c.IsStructure&&t.biome==Biome.Desert&&g.Allies(p.owner).Any(a=>g.Data(a).Has("SolarEngine")&&BoardManager.ConnectedBiome(g.State,a.tileId,p.owner,Biome.Desert).Contains(p.tileId));
            if(defense||guard&&p.protectionRound!=g.State.round)list.Add(new StatusIndicator("guard","ESCUDO",new Color(.95f,.83f,.35f),"Reduce el daño según las defensas y auras cercanas."+(guard&&p.protectionRound!=g.State.round?" Protección del primer impacto disponible en esta ronda.":""),"Condicional · protección de primer impacto una vez por ronda"));
            if(guard&&p.protectionRound==g.State.round)list.Add(new StatusIndicator("guard-used","ESCUDO USADO",new Color(.55f,.53f,.46f),"Ya gastó la protección del primer impacto en esta ronda. Otras defensas pueden seguir activas.","Se renueva al cambiar de ronda"));
            if(p.evolved)list.Add(new StatusIndicator("evolved","EVOLUCIÓN",new Color(.91f,.55f,1),"Usa sus estadísticas de evolución en lugar de los valores base.","Permanente mientras siga en el campo"));
            else if(c.Has("Evolve")&&p.forestSinceTurn>=0&&t.biome==Biome.Forest)list.Add(new StatusIndicator("growing","CRECIENDO",new Color(.58f,.95f,.3f),"Está preparando su evolución en Bosque. Moverla o abandonar el bioma interrumpe la preparación.","Progreso: "+Mathf.Min(4,g.State.turn-p.forestSinceTurn)+" / 4 turnos · se comprueba al iniciar su turno"));
            if(FactionCardRules.HealingBlocked(g,p)||c.Has("EternalEye")||c.Has("VoidHole"))list.Add(new StatusIndicator("no-heal","NO CURA",new Color(1,.3f,.38f),"No puede recuperar vida mediante curación.",c.Has("EternalEye")||c.Has("VoidHole")?"Rasgo propio de la carta":"Mientras persista el bloqueo de curación"));
            if(!XiltharRules.CanTarget(g,p))list.Add(new StatusIndicator("void-immune","INOBJETIVO",new Color(.4f,.7f,1),"No puede ser elegida como objetivo por los efectos que aplican esta restricción.","Mientras Eternal Eye permanezca en Void"));
            if(p.underDarkness)list.Add(new StatusIndicator("darkness","VOID O MUERTE",new Color(.65f,.45f,1),"Debe terminar en Void para sobrevivir al cierre del turno.","Se comprueba al finalizar el turno"));
            if(p.beaconDebt)list.Add(new StatusIndicator("beacon-debt","PAGO O MUERTE",new Color(1,.64f,.32f),"Tiene pendiente el mantenimiento de Beacon: pagar su coste o perder la unidad.","Se resuelve al finalizar el turno"));
            if(AbilityManager.HasActive(c)&&p.abilityUsed&&!c.Has("EternalEye"))list.Add(new StatusIndicator("ability-used","HABILIDAD USADA",new Color(.7f,.72f,.8f),"Ya utilizó su habilidad activa. Su movimiento y ataque se comprueban por separado.","Se renueva al iniciar su próximo turno"));
            if(t.specialEffect!=null&&c.movementType!="Flying"&&!c.Has("IgnoreUnstable")&&!c.Tag(t.specialEffect.compatibleTag))list.Add(new StatusIndicator("unstable","D6 "+t.specialEffect.threshold+"+",new Color(1,.72f,.2f),"El terreno exige superar un D6 con "+t.specialEffect.threshold+" o más. Fallar puede detener la acción e infligir "+t.specialEffect.damage+" de daño; depende de la acción y del terreno.",t.specialEffect.expiresTurn>=0?"Hasta el inicio del turno "+t.specialEffect.expiresTurn:t.specialEffect.expiresRound>=0?"Hasta el inicio de la ronda "+t.specialEffect.expiresRound:"Mientras exista el efecto del terreno"));
            return list;
        }
        static string Until(int turn)=>"Hasta el final del turno "+turn;
        static string BonusDetail(GameManager g,Piece p,bool health)
        {
            return string.Join(" ",p.statBonuses.Where(b=>(health?b.hp:b.attack)>0&&b.expiresTurn>g.State.turn).GroupBy(b=>b.expiresTurn).Select(group=>"+"+group.Sum(b=>health?b.hp:b.attack)+" hasta el inicio del turno "+group.Key+"."));
        }
    }
}
