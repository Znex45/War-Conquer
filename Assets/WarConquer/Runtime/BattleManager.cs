using System.Linq;

namespace WarConquer
{
    public static class BattleManager
    {
        public static bool HasResponse(GameManager g,int owner)
        {
            if(owner<0||owner>3||g.State.players[owner].eliminated)return false;
            return g.ForPlayer(owner,()=>g.ActingPlayer.hand.Any(c=>TimingRules.ResponseAllowed(g,g.Catalog[c.cardId],owner)&&g.CardBlockReason(c,true)=="")
                ||g.Allies(owner).Any(p=>AbilityManager.HasActive(g.Data(p))&&TimingRules.ResponseAllowed(g,g.Data(p),owner)&&AbilityManager.CanActivate(g,p)));
        }
        public static void Open(GameManager g,Piece attacker,int target)
        {
            var tile=g.State.tiles[target];var victim=tile.unit!=null&&tile.unit.owner!=attacker.owner?tile.unit:tile.structure!=null&&tile.structure.owner!=attacker.owner?tile.structure:null;
            var battle=new PendingBattle{attackerId=attacker.id,attackerOwner=attacker.owner,targetTile=target,victimId=victim?.id??-1,defenderOwner=victim?.owner??tile.baseOwner};
            battle.order.Add(attacker.owner);battle.order.Add(battle.defenderOwner);
            for(int n=1;n<4;n++){int id=(attacker.owner+n)%4;if(!battle.order.Contains(id))battle.order.Add(id);}
            g.State.battle=battle;attacker.attacked=true;
            SeekPriority(g);
        }
        static void SeekPriority(GameManager g)
        {
            var b=g.State.battle;if(b==null)return;
            while(b.priorityIndex<b.order.Count)
            {
                b.priorityPlayer=b.order[b.priorityIndex];
                if(HasResponse(g,b.priorityPlayer))
                {g.State.Log("Intervención de J"+(b.priorityPlayer+1)+": solo cartas o habilidades con permiso expreso.");return;}
                b.priorityIndex++;
            }
            g.State.battle=null;CombatManager.ResolvePending(g,b);
        }
        public static bool Pass(GameManager g,int player)
        {
            if(g.State.phase!=Phase.Actions)return g.Fail("La partida no admite respuestas.");
            var b=g.State.battle;
            if(b==null)
            {
                if(g.State.responsePlayer!=player)return g.Fail("No tienes una ventana abierta.");
                g.State.responsePlayer=-1;g.Notify("Intervención finalizada.");return true;
            }
            if(b.priorityPlayer!=player)return g.Fail("Otro jugador tiene la prioridad.");
            b.priorityIndex++;SeekPriority(g);g.Notify(g.State.battle==null?"Batalla resuelta. Continúa Asalto.":"Responde J"+(g.ActingPlayerId+1)+".");return true;
        }
        public static void AfterResponse(GameManager g)
        {
            var b=g.State.battle;
            if(b!=null&&!HasResponse(g,b.priorityPlayer)){b.priorityIndex++;SeekPriority(g);}
            else if(b==null&&g.State.responsePlayer>=0&&!HasResponse(g,g.State.responsePlayer))g.State.responsePlayer=-1;
        }
        public static bool RequestOutsideTurn(GameManager g,int player)
        {
            if(!g.CanAct||g.State.battle!=null||g.State.responsePlayer>=0||player==g.State.activePlayer||!HasResponse(g,player))return g.Fail("No hay una carta o habilidad que permita intervenir ahora.");
            g.State.responsePlayer=player;g.Notify("Intervención de J"+(player+1)+" durante el turno de J"+(g.State.activePlayer+1)+".");return true;
        }
    }
}
