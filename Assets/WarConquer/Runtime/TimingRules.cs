using System;
using System.Linq;

namespace WarConquer
{
    public static class TimingRules
    {
        public static readonly TurnStage[] Order={TurnStage.Deployment,TurnStage.Terraforming,TurnStage.Assault};
        public static string StageName(TurnStage stage)=>stage==TurnStage.Deployment?"DESPLIEGUE":stage==TurnStage.Assault?"ASALTO":"TERRAFORMACIÓN";
        public static string Description(CardData card)
        {
            string normal=string.Join(" / ",(card.allowedPhases??Array.Empty<TurnStage>()).Select(StageName));
            if(card.combatSpell)return "ASALTO · COMBATE / ATAQUE / DEFENSA / INTERVENCIÓN";
            return string.IsNullOrWhiteSpace(card.timingPermissionText)?"Tu turno · "+normal+". Sin permiso de intervención.":normal+" · "+card.timingPermissionText;
        }
        public static bool ExplicitWindow(CardData card,ActionTiming timing)
        {
            // An off-turn window requires a quoted permission from this very card, not an inferred combat trait.
            return (card.combatSpell&&timing!=ActionTiming.OtherTurn&&timing!=ActionTiming.OwnTurn)||!string.IsNullOrWhiteSpace(card.timingPermissionText)&&card.description.Contains(card.timingPermissionText)
                &&(card.allowedTiming??Array.Empty<ActionTiming>()).Contains(timing);
        }
        public static bool ResponseAllowed(GameManager g,CardData card,int owner)
        {
            var b=g.State.battle;if(b==null)return ExplicitWindow(card,ActionTiming.OtherTurn)&&(card.canInterrupt||card.canReact)&&owner!=g.State.activePlayer;
            bool attacker=owner==b.attackerOwner,defender=owner==b.defenderOwner;
            if(!attacker&&!defender&&!card.canInterrupt)return false;
            if((attacker||defender)&&!card.canRespond&&!card.canReact&&!card.canInterrupt)return false;
            return ExplicitWindow(card,ActionTiming.Battle)||ExplicitWindow(card,ActionTiming.Intervention)
                ||(attacker&&ExplicitWindow(card,ActionTiming.Attacking))||(defender&&ExplicitWindow(card,ActionTiming.Defending));
        }
        public static bool CardAllowed(GameManager g,CardData card)
        {
            if(!g.CanAct||g.State.pendingChoices.Count>0)return false;
            if(g.State.battle!=null||g.State.responsePlayer>=0||g.ActingPlayerId!=g.State.activePlayer)
                return ResponseAllowed(g,card,g.ActingPlayerId);
            return (card.allowedTiming??Array.Empty<ActionTiming>()).Contains(ActionTiming.OwnTurn)
                &&(card.allowedPhases??Array.Empty<TurnStage>()).Contains(g.State.stage);
        }
        public static TurnStage AbilityStage(CardData c)=>c.Has("StructureDiscount")?TurnStage.Deployment:c.Has("FastNetwork")||c.Has("DestroyBiome")?TurnStage.Terraforming:TurnStage.Assault;
        public static bool AbilityAllowed(GameManager g,Piece p)
        {
            if(g.State.pendingChoices.Count>0||!g.CanAct||p==null||p.owner!=g.ActingPlayerId)return false;
            if(g.State.battle!=null||g.State.responsePlayer>=0||g.ActingPlayerId!=g.State.activePlayer)return ResponseAllowed(g,g.Data(p),p.owner);
            return g.State.stage==AbilityStage(g.Data(p));
        }
        public static TurnStage LeaderStage(Player p)=>p.leader=="SAHRIA"?TurnStage.Terraforming:TurnStage.Assault;
        public static bool LeaderAllowed(GameManager g)=>false;
    }
}
