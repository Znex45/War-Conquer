using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    // All faction triggers share the existing board, death, choices and payment systems.
    public static class FactionCardRules
    {
        public static bool CanDeploy(GameManager g,CardData c,HexTile t)
        {
            if(t.IsOccupied||t.blocked||t.baseOwner>=0)return false;
            if(c.Has("WanderingBeast")&&t.biome!=Biome.Wasteland)return false;
            bool special=g.ActingPlayer.leader=="SAHRIA"&&g.ActingPlayer.sahriaDeployment&&c.category==Category.Unit;
            if(special)return t.setBy==g.ActingPlayerId&&!t.permanentAsh&&t.biome!=Biome.AshLand;
            return (t.owner==g.ActingPlayerId||GenericCardRules.BuildExtension(g,c,t))&&
                (c.requiresAshLand?t.biome==Biome.AshLand:(t.biome==Biome.Neutral&&g.State.rules.allowNeutralDeployment)||c.biomes.Contains(t.biome));
        }
        public static bool CanAttack(GameManager g,Piece p)=>!g.Data(p).Has("BabyPhoenix")||p.playedTurn!=g.State.turn||p.playedInWasteland;
        public static int TokenStats(GameManager g,Piece p)=>g.Data(p).Has("Alligator")?g.Allies(p.owner).Count(x=>x.token&&x.health>0):0;
        public static int SwampMovement(GameManager g,Piece p)=>g.State.tiles[p.tileId].biome!=Biome.Swamp?0:g.Data(p).Has("FrogToken")?2:g.Data(p).Has("AlligatorToken")?3:0;
        public static int AttackBonus(GameManager g,Piece p)=>TokenStats(g,p)+(g.Data(p).Has("AntTogether")&&BoardManager.Pieces(g.State).Any(x=>x.id!=p.id&&x.health>0&&g.Data(x).subtypes.Contains("Ant")&&BoardManager.AxialDistance(g.State.tiles[x.tileId],g.State.tiles[p.tileId])<=3)?1:0);
        static bool EnemyLushroom(GameManager g,Piece p)=>BoardManager.Pieces(g.State).Any(x=>x.owner!=p.owner&&x.health>0&&g.Data(x).Has("Lushroom")&&g.State.tiles[x.tileId].biome==Biome.Swamp);
        public static int PoisonDamage(GameManager g,Piece p)=>p.poison<=0?0:1+(EnemyLushroom(g,p)?1:0);
        public static bool HealingBlocked(GameManager g,Piece p)=>p.poison>0&&g.State.tiles[p.tileId].biome==Biome.Swamp&&EnemyLushroom(g,p);
        public static void OnPlayed(GameManager g,Piece p)
        {
            var c=g.Data(p);
            if(c.Has("SandyEmber"))
            {
                if(g.State.tiles.Any(t=>t.structure!=null&&t.biome==Biome.Desert))
                    ChoiceManager.Enqueue(g,new PendingChoice{owner=p.owner,kind="Ember",sourceId=p.id,tileId=p.tileId,prompt="Sandy Ember · elige cualquier Structure en Desierto: 1 daño"});
                else ChoiceManager.Enqueue(g,new PendingChoice{owner=p.owner,kind="Discard",prompt="Sandy Ember · no hay Structures en Desierto: descarta 1 carta"});
            }
            string token=c.Has("AntProducer")?"battle-ant-token":c.Has("FrogProducer")?"frog-token":c.Has("MushroomProducer")?"mushroom-token":c.Has("FireflyProducer")?"firefly-token":null;
            if(token!=null)Summon(g,p.owner,p.tileId,token,c.Has("FireflyProducer")?2:1,true,false);
        }
        public static void Summon(GameManager g,int owner,int slab,string token,int count,bool adjacent,bool optional)
        {
            ChoiceManager.Enqueue(g,new PendingChoice{kind="Summon",owner=owner,tileId=slab,tokenId=token,summonsLeft=count,adjacentOnly=adjacent,optional=optional,prompt="Invocar "+g.Catalog[token].name+" · elige un Slab "+(adjacent?"adyacente ":"propio ")+"libre"});
        }
        public static void OnStart(GameManager g,Piece p)
        {
            if(g.Data(p).Has("OldMummy")&&g.State.tiles[p.tileId].biome==Biome.Desert)
            {
                GenericCardRules.Heal(g,p,g.MaxHealth(p));
                p.temporaryMovement++;p.movementExpiresTurn=g.State.turn;if(!g.IsSleeping(p))p.remainingMovement++;
            }
        }
        public static void OnMove(GameManager g,Piece p,HexTile from,bool carried)
        {
            if(!g.Data(p).Has("DuneCammel"))return;
            var to=g.State.tiles[p.tileId];
            if(from.biome==Biome.Desert&&to.biome!=Biome.Desert)
                foreach(var ally in BoardManager.Pieces(g.State).Where(a=>a.owner==p.owner&&a.health>0&&BoardManager.AxialDistance(from,g.State.tiles[a.tileId])<=1).ToArray())
                {
                    ally.statBonuses.Add(new StatBonus{hp=1,attack=1,expiresTurn=g.NextTurnOf(p.owner)});ally.health++;
                }
            // A carried Cammel can apply its terrain trigger, but cannot start another carry chain.
            if(!carried)ChoiceManager.Enqueue(g,new PendingChoice{kind="Cammel",owner=p.owner,sourceId=p.id,tileId=from.id,dq=to.q-from.q,dr=to.r-from.r,optional=true,prompt="Dune Cammel · mueve una Unit a 1rad del origen en la misma dirección"});
        }
        public static void ExpireBonuses(GameManager g)
        {
            foreach(var p in BoardManager.Pieces(g.State).ToArray())
            {
                int lost=p.statBonuses.Where(b=>b.expiresTurn<=g.State.turn).Sum(b=>b.hp);
                p.statBonuses.RemoveAll(b=>b.expiresTurn<=g.State.turn);p.health-=lost;
                if(p.health<=0)CombatManager.Remove(g,p);
            }
        }
        public static int MushroomRange(GameManager g,Piece p)=>2+g.Allies(p.owner).Count(x=>x.id!=p.id&&x.token&&g.Data(x).Has("MushroomToken"));
        public static List<int> AbilityTargets(GameManager g,Piece p)
        {
            var c=g.Data(p);int rad=c.Has("WanderingBeast")?3:c.Has("MushroomToken")?MushroomRange(g,p):c.Has("FireflyToken")?2:0;
            if(rad==0)return null;
            return g.State.tiles.Where(t=>BoardManager.AxialDistance(g.State.tiles[p.tileId],t)<=rad&&(t.unit!=null||c.Has("WanderingBeast")&&t.structure!=null)).Select(t=>t.id).ToList();
        }
        public static void Activate(GameManager g,Piece p,IList<int> targets)
        {
            if(targets.Count==0)return;var c=g.Data(p);var t=g.State.tiles[targets[0]];
            if(c.Has("WanderingBeast"))CombatManager.DirectDamage(g,t.unit??t.structure,2,p);
            if(c.Has("MushroomToken"))EffectManager.Poison(g,t.unit,1,p.owner);
            if(c.Has("FireflyToken"))CombatManager.Remove(g,p,t.unit.id);
        }
        public static void OnKill(GameManager g,Piece source,Piece victim)
        {
            if(source!=null&&source.health>0&&victim.health<=0&&!g.Data(victim).IsStructure&&!victim.token&&g.Data(source).Has("Alligator"))
                Summon(g,source.owner,source.tileId,"alligator-token",1,false,true);
        }
        public static bool CanDeathTerraform(GameManager g,PendingChoice c)
        {
            var t=g.State.tiles[c.tileId];
            return !t.blocked&&!t.permanentAsh&&t.biome!=Biome.AshLand&&(t.unit==null||t.unit.owner==c.owner)&&(t.structure==null||t.structure.owner==c.owner)&&(t.baseOwner<0||t.baseOwner==c.owner||g.State.players[t.baseOwner].eliminated);
        }
        public static void CompleteDeathChoice(GameManager g,PendingChoice c)
        {
            if(!string.IsNullOrEmpty(c.returnCardId))
                g.State.players[c.owner].hand.Add(new CardInstance{instanceId=c.sourceId,cardId=c.returnCardId});
            if(c.poisonAfterId>=0)
            {
                var target=BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==c.poisonAfterId);
                EffectManager.Poison(g,target,1,c.owner);
            }
        }
        public static void OnDestroyed(GameManager g,Piece p,int poisonAfterId)
        {
            var c=g.Data(p);var player=g.State.players[p.owner];
            if(!p.token&&!c.IsStructure)
                foreach(var alligator in BoardManager.Pieces(g.State).Where(x=>g.Data(x).Has("Alligator")).ToArray())GenericCardRules.Heal(g,alligator,1);
            if(c.Has("WanderingBeast"))DeckManager.Draw(g.State,player,4);
            bool phoenix=c.Has("BabyPhoenix")&&!p.token;
            if(phoenix||p.token&&player.leader=="ZUKGROK")
            {
                var choice=new PendingChoice{kind="DeathTerraform",owner=p.owner,tileId=p.tileId,sourceId=p.id,returnCardId=phoenix?p.cardId:null,poisonAfterId=poisonAfterId,prompt=(phoenix?"Baby Phoenix":"ZUKGROK · Token destruido")+" · elige el bioma del Slab de muerte · 0 Energía"};
                if(!player.eliminated&&CanDeathTerraform(g,choice))ChoiceManager.Enqueue(g,choice);
                else{g.State.Log("Terraformación de muerte impedida por el estado del Slab.");CompleteDeathChoice(g,choice);}
            }
            else if(poisonAfterId>=0)CompleteDeathChoice(g,new PendingChoice{owner=p.owner,poisonAfterId=poisonAfterId});
        }
    }
}
