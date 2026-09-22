using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    [Serializable] public class PendingChoice
    {
        public string kind,prompt,tokenId,returnCardId;
        public int dq,dr,summonsLeft=1,poisonAfterId=-1;
        public bool adjacentOnly;
        public int owner,sourceId=-1,tileId=-1,count=1,drawAfter;
        public bool optional;
    }
    public static class ChoiceManager
    {
        public static List<int> Targets(GameManager g,PendingChoice c)
        {
            if(c.kind=="DeathTerraform")return FactionCardRules.CanDeathTerraform(g,c)?new List<int>{1,2,3,4,5,7}:new List<int>();
            if(c.kind=="Ember")return g.State.tiles.Where(t=>t.structure!=null&&t.biome==Biome.Desert).Select(t=>t.id).ToList();
            if(c.kind=="Summon")return g.State.tiles.Where(t=>!t.IsOccupied&&!t.blocked&&t.baseOwner<0&&(c.adjacentOnly?g.State.tiles[c.tileId].neighbors.Contains(t.id):t.owner==c.owner)).Select(t=>t.id).ToList();
            if(c.kind=="Cammel")return BoardManager.UnitsWithinRadius(g.State,c.tileId,1).Where(p=>p.id!=c.sourceId&&!g.IsSleeping(p)&&CarryDestination(g,p,c)>=0).Select(p=>p.tileId).ToList();
            if(c.kind=="Discard")return g.State.players[c.owner].hand.Select(x=>x.instanceId).ToList();
            if(c.kind=="Panther")return BoardManager.UnitsWithinRadius(g.State,c.tileId,2).Select(p=>p.tileId).ToList();
            if(c.kind=="Emergency")return g.Allies(c.owner).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&GenericCardRules.CanLand(g,p,g.State.tiles[c.tileId])).Select(p=>p.tileId).ToList();
            return new List<int>();
        }
        public static int CarryDestination(GameManager g,Piece p,PendingChoice c)
        {
            var t=g.State.tiles[p.tileId];
            return g.State.tiles.Where(x=>x.q==t.q+c.dq&&x.r==t.r+c.dr&&GenericCardRules.CanLand(g,p,x)).Select(x=>x.id).DefaultIfEmpty(-1).First();
        }
        public static bool BoardChoice(PendingChoice c)=>c.kind!="Discard"&&c.kind!="DeathTerraform";
        public static bool KnownKind(string kind)=>new[]{"Discard","Panther","Emergency","DeathTerraform","Ember","Summon","Cammel"}.Contains(kind);
        public static void Enqueue(GameManager g,PendingChoice choice)
        {
            if(g.State.players[choice.owner].eliminated)return;
            choice.count=Math.Min(choice.count,Targets(g,choice).Count);
            if(choice.count==0){if(choice.kind=="DeathTerraform")FactionCardRules.CompleteDeathChoice(g,choice);if(choice.drawAfter>0)DeckManager.Draw(g.State,g.State.players[choice.owner],choice.drawAfter);return;}
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
                if(c.kind=="DeathTerraform")
                {if(targets.Count>0)TerrainManager.Terraform(g,c.tileId,(Biome)targets[0],c.owner);FactionCardRules.CompleteDeathChoice(g,c);}
                if(c.kind=="Ember"&&targets.Count>0)CombatManager.DirectDamage(g,g.State.tiles[targets[0]].structure,1,BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==c.sourceId));
                if(c.kind=="Summon"&&targets.Count>0)
                {int id=targets[0];g.State.tiles[id].owner=c.owner;g.Place(c.tokenId,c.owner,id);if(c.summonsLeft>1)FactionCardRules.Summon(g,c.owner,c.tileId,c.tokenId,c.summonsLeft-1,c.adjacentOnly,false);}
                if(c.kind=="Cammel"&&targets.Count>0)
                {
                    var p=g.State.tiles[targets[0]].unit;int dest=CarryDestination(g,p,c);
                    if(dest>=0&&TerrainManager.CheckUnstable(g,p,g.State.tiles[p.tileId],true)&&TerrainManager.CheckUnstable(g,p,g.State.tiles[dest],false))MovementManager.Relocate(g,p,dest,true);
                }
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
