using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    // Makes one decision at a time through the same public rules used by a human.
    // Planning reads the board and this player's hand, never an opponent's hand or draw order.
    public sealed class AiPlayer
    {
        public const bool UseResources=true;
        GameState previousState;
        int turn=-1,actor=-1,stage=-1,decisions,terraformActions;
        readonly HashSet<int> attemptedAbilities=new HashSet<int>();
        public bool Step(GameManager g)
        {
            if(g==null||!g.CanAct||!g.ActingPlayer.isAI||g.ActingPlayer.inactive)return false;
            var s=g.State;
            if(s.pendingChoices.Count>0)
            {
                var c=s.pendingChoices[0];var available=ChoiceManager.Targets(g,c);
                if(c.kind=="Discard")available=available.OrderBy(id=>g.Catalog[g.ActingPlayer.hand.First(x=>x.instanceId==id).cardId].energyCost).ToList();
                else if(c.kind=="DeathTerraform")available=available.OrderBy(id=>id==(int)PreferredBiome(g)?0:1).ToList();
                else if(c.kind=="Summon")available=available.OrderBy(id=>Distance(g,id,Goal(g))).ToList();
                else if(c.kind=="Cammel")available=available.Where(id=>s.tiles[id].unit?.owner==c.owner).ToList();
                else available=available.OrderBy(id=>(s.tiles[id].unit??s.tiles[id].structure)?.owner==c.owner?1:0).ToList();
                return ChoiceManager.Resolve(g,available.Take(c.count).ToArray(),c.optional&&available.Count==0);
            }
            if(previousState!=s||turn!=s.turn||actor!=g.ActingPlayerId||stage!=(int)s.stage)
            {previousState=s;turn=s.turn;actor=g.ActingPlayerId;stage=(int)s.stage;decisions=0;terraformActions=0;attemptedAbilities.Clear();}
            bool response=s.battle!=null||s.responsePlayer>=0;
            // A finite budget also guarantees progress with future cards that produce free actions.
            if(++decisions>32)return response?BattleManager.Pass(g,g.ActingPlayerId):g.AdvanceStage();
            if(response)
            {
                if(TryCard(g)||TryAbility(g))return true;
                return BattleManager.Pass(g,g.ActingPlayerId);
            }
            if(s.stage==TurnStage.Deployment)
            {
                if(TryAbility(g)||TryCard(g))return true;
            }
            else if(s.stage==TurnStage.Terraforming)
            {
                if(TryCard(g)||TryAbility(g)||TryLeader(g)||TryTerraform(g))return true;
            }
            else
            {
                // Buffs and control spells can improve an attack before damage is resolved.
                if(TryCard(g)||TryAbility(g)||TryLeader(g)||TryAttack(g)||TryMove(g)||TryFreeStep(g))return true;
            }
            return g.AdvanceStage();
        }
        static Biome PreferredBiome(GameManager g)=>g.ActingPlayer.leader=="SAHRIA"?Biome.Desert:g.ActingPlayer.leader=="ZUKGROK"?Biome.Swamp:Biome.Forest;
        static int Center(GameManager g)=>g.State.tiles.First(ConquestManager.IsCenter).id;
        static int Goal(GameManager g,Piece piece=null)
        {
            int center=Center(g);int from=piece?.tileId??g.State.tiles.First(t=>t.baseOwner==g.ActingPlayerId).id;
            var available=g.State.tiles.Where(ConquestManager.IsCenter).Where(t=>ConquestManager.EligibleOwner(g.State,t)!=g.ActingPlayerId).OrderBy(t=>BoardManager.Distance(g.State,from,t.id)).FirstOrDefault();
            if(available!=null)return available.id;
            return g.State.tiles.Where(t=>t.baseOwner>=0&&t.baseOwner!=g.ActingPlayerId&&!g.State.players[t.baseOwner].eliminated)
                .OrderBy(t=>BoardManager.AxialDistance(g.State.tiles[from],t)).Select(t=>t.id).DefaultIfEmpty(center).First();
        }
        static int Distance(GameManager g,int a,int b)=>BoardManager.AxialDistance(g.State.tiles[a],g.State.tiles[b]);
        static int EnemyDistance(GameManager g,int tile)=>BoardManager.Pieces(g.State).Where(p=>p.owner!=g.ActingPlayerId)
            .Select(p=>Distance(g,tile,p.tileId)).DefaultIfEmpty(99).Min();
        static float TerraformValue(GameManager g,int id,Biome biome)
        {
            var t=g.State.tiles[id];if(t.biome==biome)return -100;
            // Do not replace a useful friendly biome with one its occupant cannot use.
            var piece=t.unit??t.structure;
            if(piece!=null&&!g.Data(piece).biomes.Contains(biome)&&g.Data(piece).movementType!="Flying")return -100;
            return (t.owner!=g.ActingPlayerId?12:3)+(piece!=null?18:0)+(ConquestManager.IsCenter(t)?22:0)-Distance(g,id,Goal(g))*2;
        }
        static float TargetValue(GameManager g,CardData card,int id)
        {
            var t=g.State.tiles[id];var piece=t.unit??t.structure;
            if(card.category!=Category.Spell)
            {
                int nearby=g.Allies(g.ActingPlayerId).Count(p=>Distance(g,id,p.tileId)==1);
                return 45-Distance(g,id,Goal(g))*4+(card.IsStructure?nearby*3:0)-(EnemyDistance(g,id)<=1&&card.health<4?15:0);
            }
            var e=card.effects[0];
            if(e.operation=="TimeToMove")return BoardManager.UnitsWithinRadius(g.State,id,8).Count(p=>p.owner==g.ActingPlayerId)*5;
            if(e.operation=="GenericTerraform"||e.operation=="ClearingSpace"||e.operation=="CleansingConquest")return TerraformValue(g,id,e.operation=="GenericTerraform"?PreferredBiome(g):Biome.Wasteland);
            if(e.operation=="Terraform")
            {
                var biome=e.biome=="ChooseForestSwamp"?Biome.Forest:(Biome)Enum.Parse(typeof(Biome),e.biome);
                return TerraformValue(g,id,biome);
            }
            if(e.operation=="Buff")return piece!=null&&EnemyDistance(g,id)<=g.Data(piece).range+piece.remainingMovement?20+g.Data(piece).attack:-100;
            if(e.operation=="Heal")return piece!=null?(g.MaxHealth(piece)-piece.health)*8:-100;
            if(e.operation=="Unstable")return EnemyDistance(g,id)<=2&&t.specialEffect==null?12:-100;
            if(e.operation=="Reactivate")return t.structure!=null?8:-100;
            if(e.operation=="DestroyBiome")return t.owner!=g.ActingPlayerId?12+(piece!=null?8:0):-100;
            if(e.operation=="March")return 20-Distance(g,id,Goal(g));
            if(piece==null)return -100;
            if(e.operation=="Sleep"||e.operation=="SleepPoisonDamage")return g.IsSleeping(piece)?-100:20+g.Data(piece).attack*2;
            float danger=piece.owner!=g.ActingPlayerId?20+g.Data(piece).attack*2:0;
            if(g.State.battle!=null&&piece.id==g.State.battle.attackerId)danger+=25;
            if(e.operation=="Damage"||e.operation=="SporeDamage")danger+=(piece.health<=e.amount?22:0);
            return danger;
        }
        static List<int> ChooseTargets(GameManager g,CardData card)
        {
            var chosen=new List<int>();int max=GenericCardRules.MaxTargets(card);
            if(card.effects.Any(e=>e.operation=="SporeDamage"))max=Math.Min(max,g.ActingPlayer.spores);
            bool march=card.effects.Any(e=>e.operation=="March");
            for(int i=0;i<max;i++)
            {
                var candidates=g.CardTargets(card,chosen).OrderByDescending(id=>TargetValue(g,card,id)).ToList();
                if(candidates.Count==0||TargetValue(g,card,candidates[0])<=-50)break;
                int first=candidates[0];chosen.Add(first);
                if(march)
                {
                    var unit=g.State.tiles[first].unit;
                    var ends=g.CardTargets(card,chosen).OrderBy(id=>Distance(g,id,Goal(g,unit))).ToList();
                    if(ends.Count==0||Distance(g,ends[0],Goal(g,unit))>=Distance(g,first,Goal(g,unit))){chosen.RemoveAt(chosen.Count-1);break;}
                    chosen.Add(ends[0]);
                }
            }
            return chosen;
        }
        static bool TryCard(GameManager g)
        {
            int units=g.Allies(g.ActingPlayerId).Count(p=>!g.Data(p).IsStructure);
            var options=new List<(CardInstance card,List<int> targets,float score,bool sahria)>();
            foreach(var instance in g.ActingPlayer.hand)
            {
                g.ActingPlayer.sahriaDeployment=false;
                if(g.CardBlockReason(instance,UseResources)!=""&&g.ActingPlayer.leader=="SAHRIA"&&g.Catalog[instance.cardId].category==Category.Unit)g.ActingPlayer.sahriaDeployment=true;
                if(g.CardBlockReason(instance,UseResources)!="")continue;
                var c=g.Catalog[instance.cardId];var targets=ChooseTargets(g,c);if(targets.Count<GenericCardRules.MinTargets(c))continue;
                float value=GenericCardRules.NoBoardTarget(c)?12:c.category==Category.Spell?targets.Sum(id=>TargetValue(g,c,id)):
                    c.IsStructure?10+g.Allies(g.ActingPlayerId).Count()*2:20+c.attack*3+c.health*.5f+(units<3?15:0);
                value-=EnergyManager.Quote(g.ActingPlayer,c,UseResources).energy;
                options.Add((instance,targets,value,g.ActingPlayer.sahriaDeployment));
            }
            foreach(var option in options.OrderByDescending(o=>o.score))
                {g.ActingPlayer.sahriaDeployment=option.sahria;if(option.score>0&&g.Play(option.card.instanceId,option.targets,UseResources,g.Catalog[option.card.cardId].effects.Any(e=>e.operation=="GenericTerraform")?PreferredBiome(g):Biome.Forest))return true;}
            g.ActingPlayer.sahriaDeployment=false;
            return false;
        }
        bool TryAbility(GameManager g)
        {
            foreach(var p in g.Allies(g.ActingPlayerId).ToList())
            {
                if(attemptedAbilities.Contains(p.id)||!AbilityManager.CanActivate(g,p))continue;
                var c=g.Data(p);var choices=AbilityManager.Targets(g,p);
                if(c.Has("WanderingBeast")||c.Has("MushroomToken")||c.Has("FireflyToken")||c.Has("VigilantTower"))choices=choices.Where(id=>(g.State.tiles[id].unit??g.State.tiles[id].structure)?.owner!=p.owner).ToList();
                if(c.Has("DestroyBiome"))choices=choices.Where(id=>g.State.tiles[id].owner!=p.owner).ToList();
                if(c.Has("ExtendBuff"))choices=choices.Where(id=>BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&a.tileId!=id&&a.bonusAttack>0)).ToList();
                if(c.Has("FastNetwork"))choices=choices.OrderBy(id=>Distance(g,id,Goal(g))).ToList();
                else choices=choices.OrderBy(id=>EnemyDistance(g,id)).ToList();
                int count=AbilityManager.TargetCount(c);if(c.Has("MedCamp"))count=Math.Min(count,choices.Count);if(choices.Count<count)continue;
                attemptedAbilities.Add(p.id);
                if(AbilityManager.Activate(g,p,choices.Take(count).ToArray()))return true;
            }
            return false;
        }
        static bool TryLeader(GameManager g)
        {
            var choices=AbilityManager.LeaderTargets(g);
            if(g.ActingPlayer.leader=="SAHRIA")choices=choices.Where(id=>g.State.tiles[id].specialEffect==null&&EnemyDistance(g,id)<=2).ToList();
            else choices=choices.Where(id=>g.State.tiles[id].unit.poison<1).ToList();
            return choices.Count>0&&AbilityManager.Leader(g,choices.OrderBy(id=>EnemyDistance(g,id)).Take(g.ActingPlayer.leader=="SAHRIA"?2:1).ToArray());
        }
        bool TryTerraform(GameManager g)
        {
            if(terraformActions>=3)return false;
            var p=g.ActingPlayer;var biome=PreferredBiome(g);
            if(p.leader=="SAHRIA"&&p.hand.Any(x=>g.Catalog[x.cardId].Has("WanderingBeast"))&&!g.State.tiles.Any(t=>t.owner==p.id&&!t.IsOccupied&&t.biome==Biome.Wasteland))biome=Biome.Wasteland;
            if(p.hand.Any(c=>g.Catalog[c.cardId].requiresAshLand)&&!g.State.tiles.Any(t=>t.owner==p.id&&t.biome==Biome.AshLand&&!t.IsOccupied))
            {
                var ash=g.State.tiles.FirstOrDefault(t=>t.owner==p.id&&t.baseOwner<0&&!t.IsOccupied&&TerrainManager.Normal(t));
                if(ash!=null&&p.currentEnergy>=g.AshCost()){terraformActions++;return g.RevealAsh(ash.id);}
            }
            if(p.currentEnergy<g.TerraformCost(biome))return false;
            var candidates=g.State.tiles.Where(t=>g.TerraformTarget(t.id)&&TerraformValue(g,t.id,biome)>-30).OrderByDescending(t=>TerraformValue(g,t.id,biome)).ToList();
            if(candidates.Count==0)return false;
            terraformActions++;return g.Terraform(candidates[0].id,biome);
        }
        static bool TryAttack(GameManager g)
        {
            Piece attacker=null;int target=-1;float best=float.MinValue;
            foreach(var p in g.Allies(g.ActingPlayerId))foreach(int id in CombatManager.Targets(g,p))
            {
                var t=g.State.tiles[id];var victim=t.unit??t.structure;int damage=CombatManager.AttackValue(g,p,victim);
                if(damage<=0)continue;
                int hp=victim?.health??g.State.players[t.baseOwner].leaderHealth;
                float score=damage+(damage>=hp?30:0)+(victim==null?10:0)+(ConquestManager.IsCenter(t)?15:0);
                if(score>best){best=score;attacker=p;target=id;}
            }
            return attacker!=null&&CombatManager.Attack(g,attacker,target);
        }
        static float MoveValue(GameManager g,Piece p,int id)
        {
            if(id==p.tileId&&(ConquestManager.IsCenter(g.State.tiles[id])||ConquestManager.EligibleOwner(g.State,g.State.tiles[id])==p.owner))return 1000;
            var t=g.State.tiles[id];float value=-Distance(g,id,Goal(g,p))*12;
            if(ConquestManager.IsCenter(t))value+=t.owner==p.owner?5:35;
            if(t.baseOwner>=0&&t.baseOwner!=p.owner)value+=15;
            if(t.specialEffect!=null&&!g.Data(p).Tag(t.specialEffect.compatibleTag)&&g.Data(p).movementType!="Flying")value-=5;
            return value;
        }
        static bool TryMove(GameManager g)
        {
            Piece unit=null;int destination=-1;float best=0;
            foreach(var p in g.Allies(g.ActingPlayerId))
            {
                float current=MoveValue(g,p,p.tileId);
                foreach(int id in MovementManager.Paths(g,p).Keys)
                {
                    float gain=MoveValue(g,p,id)-current;
                    if(gain>best){best=gain;unit=p;destination=id;}
                }
            }
            return unit!=null&&MovementManager.Move(g,unit,destination);
        }
        static bool TryFreeStep(GameManager g)
        {
            if(g.ActingPlayer.freeSteps<=0)return false;
            foreach(var p in g.Allies(g.ActingPlayerId).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)))
            {
                var choices=g.State.tiles[p.tileId].neighbors.Where(id=>!g.State.tiles[id].IsOccupied&&!g.State.tiles[id].blocked&&g.State.tiles[id].baseOwner<0)
                    .OrderByDescending(id=>MoveValue(g,p,id));
                foreach(int id in choices)if(MoveValue(g,p,id)>MoveValue(g,p,p.tileId)&&MovementManager.FreeStep(g,p,id))return true;
            }
            return false;
        }
    }
}
