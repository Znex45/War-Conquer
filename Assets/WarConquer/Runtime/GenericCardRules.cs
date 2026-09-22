using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    // Additional card operations use the existing state, payment, terrain, combat and movement systems.
    public static class GenericCardRules
    {
        public static bool NoBoardTarget(CardData c)=>c.category==Category.Spell&&c.effects.FirstOrDefault()?.target=="Self";
        public static int MinTargets(CardData c)=>NoBoardTarget(c)?0:c.effects.Any(e=>e.operation=="Engage"||e.operation=="CleansingConquest")?2:1;
        public static int MaxTargets(CardData c)=>NoBoardTarget(c)?0:c.category==Category.Spell?c.effects[0].count*(c.effects[0].operation=="March"?2:1):1;
        public static bool CanLand(GameManager g,Piece p,HexTile t)=>p!=null&&!g.Data(p).IsStructure&&!t.blocked&&!t.BlocksMovement&&t.unit==null&&(t.baseOwner<0||t.baseOwner==p.owner||g.State.players[t.baseOwner].eliminated);
        public static bool BuildExtension(GameManager g,CardData card,HexTile tile)=>card.IsStructure&&g.Allies(g.ActingPlayerId).Any(p=>g.Data(p).Has("ForestBuild")&&g.State.tiles[p.tileId].biome==Biome.Forest&&BoardManager.AxialDistance(tile,g.State.tiles[p.tileId])<=2);
        public static List<int> SpecialTargets(GameManager g,CardData card,IList<int> selected)
        {
            string op=card.effects.FirstOrDefault()?.operation;
            if(op=="Engage")
            {
                if(selected.Count==0)return g.Allies(g.ActingPlayerId).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&BoardManager.UnitsWithinRadius(g.State,p.tileId,6).Any(x=>x.owner!=p.owner)).Select(p=>p.tileId).ToList();
                if(selected.Count!=1)return new List<int>();
                var ally=g.State.tiles[selected[0]].unit;
                return ally==null?new List<int>():BoardManager.UnitsWithinRadius(g.State,ally.tileId,6).Where(p=>p.owner!=ally.owner).Select(p=>p.tileId).ToList();
            }
            if(op=="CleansingConquest")
            {
                if(selected.Count==0)return g.State.tiles.Where(t=>g.TerraformTarget(t.id)&&!t.IsOccupied&&t.baseOwner<0&&g.Allies(g.ActingPlayerId).Any(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&BoardManager.AxialDistance(t,g.State.tiles[p.tileId])<=8)).Select(t=>t.id).ToList();
                if(selected.Count!=1)return new List<int>();
                return BoardManager.UnitsWithinRadius(g.State,selected[0],8).Where(p=>p.owner==g.ActingPlayerId&&!g.IsSleeping(p)).Select(p=>p.tileId).ToList();
            }
            return null;
        }
        public static string BlockReason(GameManager g,CardData c,CardInstance instance)
        {
            if(c.effects.Any(e=>e.operation=="DiscardDraw")&&g.ActingPlayer.hand.Count<2)return "Requiere otra carta en la mano para descartar.";
            return "";
        }
        public static bool Resolve(GameManager g,CardData card,IList<int> targets,Biome choice)
        {
            if(card.category!=Category.Spell)return false;
            var s=g.State;var player=g.ActingPlayer;int owner=player.id;
            string op=card.effects[0].operation;
            var t=targets.Count>0?s.tiles[targets[0]]:null;
            switch(op)
            {
                case "GenericTerraform": TerrainManager.Terraform(g,t.id,choice,owner);DeckManager.Draw(s,player,1);break;
                case "ClearingSpace": TerrainManager.Terraform(g,t.id,Biome.Wasteland,owner);DeckManager.Draw(s,player,2);break;
                case "CleansingConquest":
                    TerrainManager.Terraform(g,t.id,Biome.Wasteland,owner);SpecialMove(g,s.tiles[targets[1]].unit,t.id);break;
                case "KillUnit": CombatManager.Remove(g,t.unit);break;
                case "DiscardDraw": ChoiceManager.Enqueue(g,new PendingChoice{kind="Discard",owner=owner,count=1,drawAfter=2,prompt="One for the Team · descarta 1; después roba 2"});break;
                case "DrawDiscard":
                    DeckManager.Draw(s,player,3);ChoiceManager.Enqueue(g,new PendingChoice{kind="Discard",owner=owner,count=Math.Min(2,player.hand.Count),prompt="A Price to Pay · elige 2 cartas para descartar"});break;
                case "DestroyWasteland": TerrainManager.DestroyBiome(g,t);break;
                case "TokenDraw": DeckManager.Draw(s,player,g.Allies(owner).Count(p=>p.token));break;
                case "LastGift": int hp=t.unit.health;CombatManager.Remove(g,t.unit);DeckManager.Draw(s,player,hp);break;
                case "Engage":
                    var ally=t.unit;var enemy=s.tiles[targets[1]].unit;int destination=enemy.tileId;
                    if(!TerrainManager.CheckAction(g,ally,"Engage"))break;
                    CombatManager.DirectDamage(g,enemy,CombatManager.AttackValue(g,ally,enemy),ally);
                    if(enemy.health<=0&&CanLand(g,ally,s.tiles[destination]))SpecialMove(g,ally,destination);break;
                case "TimeToMove":
                    foreach(var unit in BoardManager.UnitsWithinRadius(s,t.id,8))
                    {unit.temporaryMovement+=4;unit.movementExpiresTurn=s.turn;if(!g.IsSleeping(unit))unit.remainingMovement+=4;}break;
                case "Bomb": CombatManager.DirectDamage(g,t.structure,3);break;
                default:return false;
            }
            return true;
        }
        public static bool SpecialMove(GameManager g,Piece p,int destination)
        {
            if(p==null||p.health<=0||g.IsSleeping(p)||!CanLand(g,p,g.State.tiles[destination]))return false;
            if(TerrainManager.CheckUnstable(g,p,g.State.tiles[p.tileId],true)&&TerrainManager.CheckUnstable(g,p,g.State.tiles[destination],false))MovementManager.Relocate(g,p,destination);
            return true;
        }
        public static void Heal(GameManager g,Piece p,int amount){if(p!=null&&p.health>0)p.health=Math.Min(g.MaxHealth(p),p.health+amount);}
        public static int ForestHealth(GameManager g,Piece p)=>g.Data(p).Has("ForestHealth")&&g.State.tiles[p.tileId].biome==Biome.Forest?2:0;
        public static int MovementAura(GameManager g,Piece p)=>g.Data(p).IsStructure?0:g.Allies(p.owner).Count(x=>g.Data(x).Has("GlobalMovement"));
        public static void SyncAuras(GameManager g)
        {
            foreach(var p in BoardManager.Pieces(g.State).ToList())
            {
                int health=ForestHealth(g,p);p.health+=health-p.terrainHealthBonus;p.terrainHealthBonus=health;
                int movement=MovementAura(g,p);
                if(!g.IsSleeping(p))p.remainingMovement=Math.Max(0,p.remainingMovement+movement-p.movementAura);
                p.movementAura=movement;
                if(p.health<=0)CombatManager.Remove(g,p);
            }
        }
        public static void OnPlayed(GameManager g,Piece p)
        {if(g.Data(p).Has("ForestDraw")&&g.State.tiles[p.tileId].biome==Biome.Forest)DeckManager.Draw(g.State,g.State.players[p.owner],1);}
        public static void OnMove(GameManager g,Piece p,Biome from)
        {
            if(!g.Data(p).Has("ForestPanther"))return;
            var to=g.State.tiles[p.tileId].biome;
            if(from!=Biome.Forest&&to==Biome.Forest)Heal(g,p,g.MaxHealth(p));
            if(from==Biome.Forest&&to!=Biome.Forest)ChoiceManager.Enqueue(g,new PendingChoice{owner=p.owner,kind="Panther",sourceId=p.id,tileId=p.tileId,count=1,optional=true,prompt="Panther · elige una unidad a 2rad para infligir 2 de daño"});
        }
        public static void OnKill(GameManager g,Piece source,Piece victim)
        {if(source!=null&&source.health>0&&!g.Data(victim).IsStructure&&victim.health<=0&&g.Data(source).Has("KillHeal"))Heal(g,source,g.MaxHealth(source));}
        public static void OnStart(GameManager g,Piece p)
        {
            var c=g.Data(p);var s=g.State;
            if(c.Has("ManaPool"))s.players[p.owner].currentEnergy++;
            if(c.Has("StructureHeal"))foreach(var a in g.Allies(p.owner).Where(a=>g.Data(a).IsStructure))Heal(g,a,1);
            if(c.Has("RabbitProducer"))
            {
                int id=s.tiles[p.tileId].neighbors.FirstOrDefault(n=>!s.tiles[n].IsOccupied&&!s.tiles[n].blocked&&s.tiles[n].baseOwner<0&&(s.tiles[n].owner<0||s.tiles[n].owner==p.owner),-1);
                if(id>=0){s.tiles[id].owner=p.owner;g.Place("rabbit-token",p.owner,id);s.Log("Berry Bush invoca Rabbit Token.");}
                else s.Log("Berry Bush: no hay Slab adyacente libre; no se invoca Token.");
            }
        }
        public static void OnDestroyed(GameManager g,Piece p)
        {
            if(g.Data(p).Has("EmergencySeal")&&!g.State.players[p.owner].eliminated)
                ChoiceManager.Enqueue(g,new PendingChoice{owner=p.owner,kind="Emergency",tileId=p.tileId,count=1,prompt="Emergency Seal · elige un aliado para moverlo al Slab destruido"});
        }
        public static bool IsActive(CardData c)=>new[]{"MedCamp","RestingCamp","VigilantTower","ForestLeap"}.Any(c.Has);
        public static List<int> AbilityTargets(GameManager g,Piece p)
        {
            var c=g.Data(p);var s=g.State;
            if(c.Has("MedCamp")||c.Has("RestingCamp"))return BoardManager.UnitsWithinRadius(s,p.tileId,c.Has("MedCamp")?5:4).Where(a=>a.owner==p.owner&&a.health<g.MaxHealth(a)).Select(a=>a.tileId).ToList();
            if(c.Has("VigilantTower"))return BoardManager.UnitsWithinRadius(s,p.tileId,6).Select(a=>a.tileId).ToList();
            if(c.Has("ForestLeap"))return BoardManager.StructuresWithinRadius(s,p.tileId,4).Where(a=>a.owner==p.owner&&(g.Data(a).Tag("BOSQUE")||s.tiles[a.tileId].biome==Biome.Forest)).SelectMany(a=>s.tiles[a.tileId].neighbors).Distinct().Where(id=>CanLand(g,p,s.tiles[id])).ToList();
            return new List<int>();
        }
        public static void Activate(GameManager g,Piece p,IList<int> targets)
        {
            var c=g.Data(p);
            if(c.Has("MedCamp"))foreach(int id in targets)Heal(g,g.State.tiles[id].unit,4);
            if(c.Has("RestingCamp"))foreach(int id in AbilityTargets(g,p))Heal(g,g.State.tiles[id].unit,2);
            if(c.Has("VigilantTower"))CombatManager.DirectDamage(g,g.State.tiles[targets[0]].unit,1,p);
            if(c.Has("ForestLeap"))SpecialMove(g,p,targets[0]);
        }
    }
}
