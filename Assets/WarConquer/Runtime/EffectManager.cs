using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class EffectManager
    {
        public static void ResolveSpell(GameManager g,CardData card,IList<int> targets,Biome choice)
        {
            var s=g.State;
            foreach(var effect in card.effects)
            {
                if(effect.operation=="March")
                {
                    for(int i=0;i<targets.Count;i+=2)
                    {
                        var p=s.tiles[targets[i]].unit;
                        if(p!=null&&TerrainManager.CheckUnstable(g,p,s.tiles[p.tileId],true)&&TerrainManager.CheckUnstable(g,p,s.tiles[targets[i+1]],false))MovementManager.Relocate(g,p,targets[i+1]);
                    }
                    continue;
                }
                if(effect.operation=="Spore") {g.ActingPlayer.spores+=effect.amount;continue;}
                foreach(int id in targets)
                {
                    var t=s.tiles[id];var p=effect.target=="EnemyPiece"?(t.unit!=null&&t.unit.owner!=g.ActingPlayerId?t.unit:t.structure):t.unit??t.structure;
                    switch(effect.operation)
                    {
                        case "Terraform": TerrainManager.Terraform(g,id,effect.biome=="ChooseForestSwamp"?choice:(Biome)Enum.Parse(typeof(Biome),effect.biome),g.ActingPlayerId);break;
                        case "Buff": if(p!=null)p.bonusAttack+=effect.amount;break;
                        case "Poison": if(p!=null)Poison(g,p,effect.amount,g.ActingPlayerId);g.ActingPlayer.towerUsed=true;break;
                        case "Sleep": if(p!=null)Sleep(g,p,g.ActingPlayerId);break;
                        case "SleepPoisonDamage": if(p!=null){bool poisoned=p.poison>0;Sleep(g,p,g.ActingPlayerId);if(poisoned)CombatManager.Damage(g,p,1,false);}break;
                        case "SporeDamage": g.ActingPlayer.spores--;if(p!=null)CombatManager.Damage(g,p,1,false);break;
                        case "Damage": if(p!=null)CombatManager.Damage(g,p,effect.amount,false);break;
                        case "Heal": if(p!=null)p.health=Math.Min(g.MaxHealth(p),p.health+effect.amount);break;
                        case "Slow": if(p!=null){p.slow=effect.amount;p.slowUntilTurn=g.NextTurnOf(p.owner);Negative(g,p);}break;
                        case "DestroyBiome": TerrainManager.DestroyBiome(g,t);break;
                        case "Reactivate": if(t.structure!=null)t.structure.abilityUsed=false;break;
                        case "Unstable": t.specialEffect=new TerrainEffect {threshold=5,damage=1,onExit=true,expiresRound=s.round+effect.duration};break;
                    }
                }
            }
        }
        public static void Poison(GameManager g,Piece p,int amount,int source)
        {
            if(p==null||g.Data(p).IsStructure)return;
            if(p.poison==0){p.poison=1;p.poisonTurns=0;}p.poisonApplications++;
            Negative(g,p);
            if(g.HasTrait(source,"PoisonSpore"))g.State.players[source].spores++;
            g.State.Log(g.Data(p).name+": veneno progresivo · próximo daño "+p.poison+" (se duplica cada turno propio).");
        }
        public static void Sleep(GameManager g,Piece p,int source)
        {
            p.sleepUntilTurn=g.NextTurnOf(p.owner);p.remainingMovement=0;Negative(g,p);
            var player=g.State.players[source];
            if(g.HasTrait(source,"SleepStep")&&player.dreamRound!=g.State.round){player.freeSteps++;player.dreamRound=g.State.round;}
            g.State.Log(g.Data(p).name+": Dormido durante su próximo turno.");
        }
        static void Negative(GameManager g,Piece p)
        {
            if(g.HasTrait(p.owner,"NegativeSpore"))g.State.players[p.owner].spores++;
        }
        public static void OnTileEnter(GameManager g,Piece p)
        {
            var c=g.Data(p);var t=g.State.tiles[p.tileId];
            if(c.Has("EnterPoison")&&t.biome==Biome.Forest&&t.owner==p.owner)
            {
                var enemy=BoardManager.Nearby(g.State,t.id).FirstOrDefault(a=>a.owner!=p.owner&&!g.Data(a).IsStructure);
                if(enemy!=null)Poison(g,enemy,1,p.owner);
            }
            if(c.Has("RevealResource")&&t.territory==4&&t.hiddenResource){t.hiddenResource=false;t.resource=2;g.State.Log("Recurso oculto revelado: +2 en hex "+(t.id+1));}
            if(c.Has("ExtraResource")&&t.resource>0&&!p.abilityUsed){EnergyManager.AddResource(g.State,g.State.players[p.owner],Biome.Desert,1);p.abilityUsed=true;}
        }
        public static void OnEnter(GameManager g,Piece p)
        {
            var c=g.Data(p);var s=g.State;var t=s.tiles[p.tileId];OnTileEnter(g,p);
            if(c.Has("EnterForest")||c.Has("EnterDesert"))
            {
                var target=t.neighbors.Select(n=>s.tiles[n]).FirstOrDefault(n=>g.TerraformTarget(n.id));
                if(target!=null)TerrainManager.Terraform(g,target.id,c.Has("EnterForest")?Biome.Forest:Biome.Desert,p.owner);
            }
            if(c.Has("DeepSand")||c.Has("MovingDunes"))
            {
                // Trap structures occupy a structure slot while allowing a unit to enter the same terrain.
                t.specialEffect=new TerrainEffect {threshold=4,damage=c.Has("DeepSand")?2:1,onExit=c.Has("MovingDunes"),sourceId=p.id};
            }
            if(c.Has("FastNetwork"))
            {
                var tiles=s.tiles.Where(n=>TerrainManager.CompatibleRoute(n,p.owner)).OrderBy(n=>BoardManager.Distance(s,t.id,n.id)).Take(2).ToList();
                if(tiles.Count==2)TerrainManager.CreateFastRoute(g,tiles[0].id,tiles[1].id,p);
            }
            if(c.Has("EnterKing"))
            {
                foreach(var ally in g.Allies(p.owner).Where(a=>g.Data(a).subtypes.Contains("Hongo")))ally.permanentAttack++;
                foreach(var enemy in BoardManager.Pieces(s).Where(a=>a.owner!=p.owner&&!g.Data(a).IsStructure&&s.tiles[a.tileId].biome==Biome.Forest).ToList())Poison(g,enemy,1,p.owner);
            }
            if(c.Has("EternalForest"))
            {
                foreach(int id in t.neighbors.Where(n=>g.TerraformTarget(n)).Take(3).ToList())TerrainManager.Terraform(g,id,Biome.Forest,p.owner);
                foreach(var route in s.fastRoutes.Where(f=>f.owner==p.owner&&(t.neighbors.Contains(f.a)||t.neighbors.Contains(f.b))))route.protectedRoute=true;
            }
            if(c.Has("EnterTitan"))
                foreach(var enemy in BoardManager.Nearby(s,t.id).Where(a=>a.owner!=p.owner&&g.Data(a).movementType=="Ground").ToList())
                {int roll=g.RollDie(enemy,s.tiles[enemy.tileId],5,"Titán de Ceniza");if(roll<5)CombatManager.Damage(g,enemy,3,false);}
            if(c.Has("BuriedCity"))
            {
                t.permanentAsh=true;t.biome=Biome.AshLand;
                foreach(int id in t.neighbors.Where(n=>!s.tiles[n].IsOccupied&&!s.tiles[n].blocked&&s.tiles[n].baseOwner<0).Take(2).ToList())
                {s.tiles[id].owner=p.owner;g.Place("obelisco-de-arena",p.owner,id);}
            }
        }
        public static void OnStart(GameManager g,Piece p)
        {
            if(g.IsSleeping(p))return;
            var c=g.Data(p);var s=g.State;var t=s.tiles[p.tileId];var player=s.players[p.owner];
            if(c.Has("ForestSpore")&&t.biome==Biome.Forest)player.spores++;
            if(c.Has("ConnectedSpores")&&BoardManager.ConnectedBiome(s,t.id,p.owner,Biome.Forest).Count>=3)player.spores+=2;
            if(c.Has("SporeToken"))
            {
                int tile=t.neighbors.FirstOrDefault(n=>!s.tiles[n].IsOccupied&&!s.tiles[n].blocked&&s.tiles[n].baseOwner<0&&s.tiles[n].owner==p.owner,-1);
                if(tile>=0) {g.Place("espora",p.owner,tile);s.Log("Semillero crea ficha Espora.");}
            }
            if(c.Has("SolarResource"))EnergyManager.AddResource(s,player,Biome.Desert,1);
            if(c.Has("ExtraResource")&&t.resource>0){EnergyManager.AddResource(s,player,Biome.Desert,1);p.abilityUsed=true;}
            if(c.Has("SolarEngine"))
            {
                var connected=BoardManager.ConnectedBiome(s,t.id,p.owner,Biome.Desert);
                int energy=g.Allies(p.owner).Count(a=>g.Data(a).IsStructure&&connected.Contains(a.tileId));
                player.currentEnergy+=energy;g.State.Log("Trono Solar: +"+energy+" Energía adicional.");
            }
        }
    }
    public static class EnumerableCompat
    {
        public static T FirstOrDefault<T>(this System.Collections.Generic.IEnumerable<T> source,Func<T,bool> predicate,T fallback)
        {foreach(var item in source)if(predicate(item))return item;return fallback;}
    }
}
