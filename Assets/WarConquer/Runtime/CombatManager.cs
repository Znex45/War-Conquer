using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class CombatManager
    {
        public static int AttackValue(GameManager g,Piece p,Piece defender=null)
        {
            var c=g.Data(p); var t=g.State.tiles[p.tileId]; int attack=(p.evolved?g.State.rules.evolvedAttack:c.attack)+p.bonusAttack+p.permanentAttack;
            if(c.Has("DesertAttack")&&t.biome==Biome.Desert&&t.owner==p.owner)attack+=2;
            if(c.Has("PoisonHunter")&&defender!=null&&defender.poison>0)attack++;
            foreach(var ally in BoardManager.Nearby(g.State,p.tileId).Where(a=>a.owner==p.owner))
            {
                var aura=g.Data(ally);
                if(aura.Has("FungusAttackAura")&&c.subtypes.Contains("Hongo"))attack++;
                if(aura.Has("SolarAttackAura")&&c.factionTag=="SOLAR")attack++;
                if(aura.Has("NomadAura")&&c.subtypes.Contains("Nómada")&&t.biome==Biome.Desert)attack++;
            }
            return Math.Max(0,attack);
        }
        public static List<int> Targets(GameManager g,Piece p)
        {
            if(p==null||!g.CanAct||p.owner!=g.State.activePlayer||p.attacked||g.IsSleeping(p)||g.Data(p).IsStructure) return new List<int>();
            return g.State.tiles.Where(t=>((t.unit!=null&&t.unit.owner!=p.owner)||(t.structure!=null&&t.structure.owner!=p.owner)||(t.baseOwner>=0&&t.baseOwner!=p.owner&&!g.State.players[t.baseOwner].eliminated))&&BoardManager.Distance(g.State,p.tileId,t.id,g.Data(p).range)<=g.Data(p).range).Select(t=>t.id).ToList();
        }
        public static bool Attack(GameManager g,Piece attacker,int target)
        {
            if(!Targets(g,attacker).Contains(target))return g.Fail("No puedes atacar: objetivo, alcance, ataque usado o Dormido.");
            var t=g.State.tiles[target]; var victim=t.unit!=null&&t.unit.owner!=attacker.owner?t.unit:t.structure!=null&&t.structure.owner!=attacker.owner?t.structure:null;
            attacker.attacked=true; int damage=AttackValue(g,attacker,victim);
            if(victim!=null)
            {
                Damage(g,victim,damage,true);
                if(victim.health>0&&g.Data(attacker).Has("PoisonAttack")&&(g.State.tiles[attacker.tileId].biome==Biome.Forest||g.State.tiles[attacker.tileId].biome==Biome.Swamp)) EffectManager.Poison(g,victim,1,attacker.owner);
            }
            else
            {
                var leader=g.State.players[t.baseOwner]; leader.leaderHealth=Math.Max(0,leader.leaderHealth-damage);
                g.State.Log("J"+(leader.id+1)+" pierde "+damage+" de vida de Líder.");
                if(leader.leaderHealth<=0)Eliminate(g,leader);
            }
            g.Notify("Ataque de "+g.Data(attacker).name+" resuelto.");return true;
        }
        public static void Damage(GameManager g,Piece p,int amount,bool attack)
        {
            var c=g.Data(p);var nearby=BoardManager.Nearby(g.State,p.tileId).Where(a=>a.owner==p.owner).ToList(); int defense=0;
            if(c.subtypes.Contains("Hongo"))defense+=nearby.Count(a=>g.Data(a).Has("FungusDefenseAura"));
            if(c.Has("StructureGuard")&&nearby.Any(a=>g.Data(a).IsStructure))defense++;
            if(c.IsStructure&&g.State.tiles[p.tileId].biome==Biome.Desert&&g.Allies(p.owner).Any(a=>g.Data(a).Has("SolarEngine")&&BoardManager.ConnectedBiome(g.State,a.tileId,p.owner,Biome.Desert).Contains(p.tileId)))defense++;
            bool guard=attack&&nearby.Any(a=>g.Data(a).Has("FirstAttackGuard"));
            var ownTile=g.State.tiles[p.tileId];
            bool oasis=c.Has("OasisGuard")&&ownTile.structure!=null&&g.Data(ownTile.structure).Has("Oasis");
            if((guard||oasis)&&p.protectionRound!=g.State.round){defense++;p.protectionRound=g.State.round;}
            int actual=Math.Max(0,amount-defense);p.health-=actual;
            g.State.Log(c.name+" recibe "+actual+" daño"+(defense>0?" (defensa "+defense+")":"")+".");
            if(p.health<=0)Remove(g,p);
        }
        public static void Remove(GameManager g,Piece p)
        {
            var t=g.State.tiles[p.tileId];if(t.unit==p)t.unit=null;if(t.structure==p)t.structure=null;
            if(t.specialEffect!=null&&t.specialEffect.sourceId==p.id)t.specialEffect=null;
            if(!p.token)g.State.players[p.owner].discardPile.Add(new CardInstance {instanceId=p.id,cardId=p.cardId});
            TerrainManager.MaintainRoutes(g);g.State.Log(g.Data(p).name+" destruida"+(p.token?".":" → descarte."));
        }
        static void Eliminate(GameManager g,Player player)
        {
            player.eliminated=true;
            foreach(var p in g.Allies(player.id).ToList())Remove(g,p);
            foreach(var t in g.State.tiles.Where(t=>t.owner==player.id))t.owner=-1;
            var alive=g.State.players.Where(p=>!p.eliminated).ToList();
            if(alive.Count==1){g.State.winner=alive[0].id;g.State.phase=Phase.Finished;g.State.Log("Gana J"+(alive[0].id+1)+".");}
        }
    }
}
