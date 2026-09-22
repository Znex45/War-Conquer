using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    [Serializable] public class PendingChoice
    {
        public string kind,prompt;
        public int owner,sourceId=-1,tileId=-1,count=1,drawAfter;
        public bool optional;
    }
    public static class ChoiceManager
    {
        public static List<int> Targets(GameManager g,PendingChoice c)
        {
            if(c.kind=="Discard")return g.State.players[c.owner].hand.Select(x=>x.instanceId).ToList();
            if(c.kind=="Panther")return BoardManager.UnitsWithinRadius(g.State,c.tileId,2).Select(p=>p.tileId).ToList();
            if(c.kind=="Emergency")return g.Allies(c.owner).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&GenericCardRules.CanLand(g,p,g.State.tiles[c.tileId])).Select(p=>p.tileId).ToList();
            return new List<int>();
        }
        public static void Enqueue(GameManager g,PendingChoice choice)
        {
            if(g.State.players[choice.owner].eliminated)return;
            choice.count=Math.Min(choice.count,Targets(g,choice).Count);
            if(choice.count==0){if(choice.drawAfter>0)DeckManager.Draw(g.State,g.State.players[choice.owner],choice.drawAfter);return;}
            g.State.pendingChoices.Add(choice);
        }
        public static bool Resolve(GameManager g,IList<int> targets,bool skip=false)
        {
            var c=g.State.pendingChoices.FirstOrDefault();
            if(c==null||!g.CanAct||g.ActingPlayerId!=c.owner)return g.Fail("No hay una elección pendiente.");
            var valid=Targets(g,c);int count=Math.Min(c.count,valid.Count);
            if(skip&&!c.optional)return g.Fail("Debes resolver esta elección.");
            if(!skip&&(targets==null||targets.Count!=count||targets.Distinct().Count()!=count||targets.Any(t=>!valid.Contains(t))))return g.Fail("Selecciona exactamente "+count+" objetivos válidos.");
            // Remove before resolving so any secondary trigger is queued after this choice.
            g.State.pendingChoices.RemoveAt(0);
            if(!skip)
            {
                var player=g.State.players[c.owner];
                if(c.kind=="Discard")
                {foreach(int id in targets){var card=player.hand.Find(x=>x.instanceId==id);player.hand.Remove(card);player.discardPile.Add(card);}DeckManager.Draw(g.State,player,c.drawAfter);}
                if(c.kind=="Panther"&&targets.Count>0)
                {var source=BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==c.sourceId);CombatManager.DirectDamage(g,g.State.tiles[targets[0]].unit,2,source);}
                if(c.kind=="Emergency"&&targets.Count>0)GenericCardRules.SpecialMove(g,g.State.tiles[targets[0]].unit,c.tileId);
            }
            BattleManager.AfterResponse(g);g.Notify("Elección resuelta.");return true;
        }
    }
}
