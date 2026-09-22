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
            if(!c.IsStructure)attack+=g.Allies(p.owner).Count(a=>g.Data(a).Has("GlobalAttack"));
            return Math.Max(0,attack);
        }
        public static List<int> Targets(GameManager g,Piece p)
        {
            if(p==null||!g.CanTakeTurnAction(TurnStage.Assault)||p.owner!=g.State.activePlayer||p.attacked||g.IsSleeping(p)||g.Data(p).IsStructure) return new List<int>();
            return g.State.tiles.Where(t=>((t.unit!=null&&t.unit.owner!=p.owner)||(t.structure!=null&&t.structure.owner!=p.owner)||(t.baseOwner>=0&&t.baseOwner!=p.owner&&!g.State.players[t.baseOwner].eliminated))&&BoardManager.Distance(g.State,p.tileId,t.id,g.Data(p).range)<=g.Data(p).range).Select(t=>t.id).ToList();
        }
        public static bool Attack(GameManager g,Piece attacker,int target)
        {
            if(!Targets(g,attacker).Contains(target))return g.Fail("No puedes atacar: objetivo, alcance, ataque usado o Dormido.");
            if(!TerrainManager.CheckAction(g,attacker,"atacar")){attacker.attacked=true;g.Notify("Ataque interrumpido por el terreno.");return true;}
            BattleManager.Open(g,attacker,target);
            g.Notify(g.State.battle==null?"Ataque resuelto.":"Batalla abierta · responde J"+(g.ActingPlayerId+1)+".");return true;
        }
        internal static void ResolvePending(GameManager g,PendingBattle battle)
        {
            var attacker=BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==battle.attackerId);
            if(attacker==null||g.IsSleeping(attacker)||BoardManager.Distance(g.State,attacker.tileId,battle.targetTile)>g.Data(attacker).range)
            {g.State.Log("Ataque cancelado: el atacante ya no puede resolverlo.");return;}
            var tile=g.State.tiles[battle.targetTile];
            var victim=BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==battle.victimId&&p.tileId==battle.targetTile);
            if(battle.victimId>=0&&victim==null){g.State.Log("Ataque cancelado: el objetivo ya no está.");return;}
            int damage=AttackValue(g,attacker,victim);
            if(victim!=null)
            {
                Damage(g,victim,damage,true);GenericCardRules.OnKill(g,attacker,victim);
                if(victim.health>0&&g.Data(attacker).Has("PoisonAttack")&&(g.State.tiles[attacker.tileId].biome==Biome.Forest||g.State.tiles[attacker.tileId].biome==Biome.Swamp))EffectManager.Poison(g,victim,1,attacker.owner);
            }
            else
            {
                var leader=g.State.players[battle.defenderOwner];if(leader.eliminated)return;
                leader.leaderHealth=Math.Max(0,leader.leaderHealth-damage);
                g.State.Log("J"+(leader.id+1)+" pierde "+damage+" de vida de Líder.");
                if(leader.leaderHealth==0){Eliminate(g,leader);tile.owner=attacker.owner;g.State.Log("J"+(attacker.owner+1)+" conquista la base de J"+(leader.id+1)+".");}
            }
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
        public static void DirectDamage(GameManager g,Piece p,int amount,Piece source=null)
        {if(p==null||p.health<=0)return;p.health-=Math.Max(0,amount);g.State.Log(g.Data(p).name+" recibe "+amount+" daño directo.");if(p.health<=0){Remove(g,p);GenericCardRules.OnKill(g,source,p);}}
        public static void PoisonDamage(GameManager g,Piece p,int amount)
        {
            p.health-=Math.Min(p.health,amount);g.State.Log(g.Data(p).name+": pierde "+amount+" VIDA por veneno progresivo.");
            if(p.health<=0)Remove(g,p);
        }
        public static void Remove(GameManager g,Piece p)
        {
            if(p==null)return;var t=g.State.tiles[p.tileId];if(t.unit!=p&&t.structure!=p)return;p.health=0;if(t.unit==p)t.unit=null;if(t.structure==p)t.structure=null;
            if(t.specialEffect!=null&&t.specialEffect.sourceId==p.id)t.specialEffect=null;
            if(!p.token)g.State.players[p.owner].discardPile.Add(new CardInstance {instanceId=p.id,cardId=p.cardId});
            GenericCardRules.OnDestroyed(g,p);GenericCardRules.SyncAuras(g);TerrainManager.MaintainRoutes(g);ConquestManager.Refresh(g.State);g.State.Log(g.Data(p).name+" destruida"+(p.token?".":" → descarte."));
        }
        static void Eliminate(GameManager g,Player player)
        {
            player.eliminated=true;
            foreach(var p in g.Allies(player.id).ToList())Remove(g,p);
            foreach(var t in g.State.tiles.Where(t=>t.owner==player.id))t.owner=-1;
            ConquestManager.CheckLastLeader(g.State);
        }
    }
}
