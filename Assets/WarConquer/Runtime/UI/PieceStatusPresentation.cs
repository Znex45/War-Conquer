using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public readonly struct StatusIndicator
    {
        public readonly string id,label;
        public readonly Color color;
        public StatusIndicator(string id,string label,Color color){this.id=id;this.label=label;this.color=color;}
    }
    public static class PieceStatusPresentation
    {
        // Presentation reads resolved game values; it never applies effects or changes the turn.
        public static List<StatusIndicator> Collect(GameManager g,Piece p)
        {
            var list=new List<StatusIndicator>();var c=g.Data(p);var t=g.State.tiles[p.tileId];
            if(p.poison>0)list.Add(new StatusIndicator("poison","VENENO "+FactionCardRules.PoisonDamage(g,p),new Color(.65f,.25f,.88f)));
            if(g.IsSleeping(p))list.Add(new StatusIndicator("sleep","DORMIDO",new Color(.6f,.82f,1)));
            if(p.slow>0&&p.slowUntilTurn>=g.State.turn)list.Add(new StatusIndicator("slow","MOV -"+p.slow,new Color(.5f,.7f,1)));
            int atk=CombatManager.AttackValue(g,p)-(p.evolved?g.State.rules.evolvedAttack:c.attack);
            if(atk>0)list.Add(new StatusIndicator("attack","ATK +"+atk,new Color(1,.56f,.17f)));
            int hp=g.MaxHealth(p)-(p.evolved?g.State.rules.evolvedHealth:c.health);
            if(hp>0)list.Add(new StatusIndicator("health","HP +"+hp,new Color(.35f,1,.55f)));
            int move=GenericCardRules.MovementAura(g,p)+p.temporaryMovement;
            move+=BoardManager.Nearby(g.State,p.tileId).Count(a=>a.owner==p.owner&&g.Data(a).Has("NomadAura")&&c.subtypes.Contains("Nómada")&&t.biome==Biome.Desert);
            if(!c.IsStructure&&move>0)list.Add(new StatusIndicator("movement","MOV +"+move,new Color(.25f,.94f,1)));
            bool guard=BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).Has("FirstAttackGuard"))||c.Has("OasisGuard")&&t.structure!=null&&g.Data(t.structure).Has("Oasis");
            bool defense=c.Has("StructureGuard")&&BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).IsStructure)
                ||c.subtypes.Contains("Hongo")&&BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&g.Data(a).Has("FungusDefenseAura"))
                ||c.IsStructure&&t.biome==Biome.Desert&&g.Allies(p.owner).Any(a=>g.Data(a).Has("SolarEngine")&&BoardManager.ConnectedBiome(g.State,a.tileId,p.owner,Biome.Desert).Contains(p.tileId));
            if(defense||guard&&p.protectionRound!=g.State.round)list.Add(new StatusIndicator("guard","ESCUDO",new Color(.95f,.83f,.35f)));
            if(guard&&p.protectionRound==g.State.round)list.Add(new StatusIndicator("guard-used","ESCUDO USADO",new Color(.55f,.53f,.46f)));
            if(p.evolved)list.Add(new StatusIndicator("evolved","EVOLUCIÓN",new Color(.91f,.55f,1)));
            else if(c.Has("Evolve")&&p.forestSinceTurn>=0&&t.biome==Biome.Forest)list.Add(new StatusIndicator("growing","CRECIENDO",new Color(.58f,.95f,.3f)));
            if(FactionCardRules.HealingBlocked(g,p))list.Add(new StatusIndicator("no-heal","NO CURA",new Color(1,.3f,.38f)));
            if(t.specialEffect!=null&&c.movementType!="Flying"&&!c.Has("IgnoreUnstable")&&!c.Tag(t.specialEffect.compatibleTag))list.Add(new StatusIndicator("unstable","D6 "+t.specialEffect.threshold+"+",new Color(1,.72f,.2f)));
            return list;
        }
    }
}
