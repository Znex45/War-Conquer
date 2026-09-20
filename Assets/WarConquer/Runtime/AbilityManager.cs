using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class AbilityManager
    {
        public static bool HasActive(CardData c) => new[]{"FastNetwork","StructureDiscount","CampStep","DestroyBiome","ExtendBuff"}.Any(c.Has);
        public static int TargetCount(CardData c) => c.Has("FastNetwork")?2:c.Has("StructureDiscount")?0:1;
        public static string Label(CardData c) => c.Has("FastNetwork")?"Conectar 2 casillas":c.Has("StructureDiscount")?"Descuento de estructura":c.Has("CampStep")?"Dar +1 MOV":c.Has("DestroyBiome")?"Destruir bioma":c.Has("ExtendBuff")?"Extender mejora (1 Espora)":"Sin habilidad activa";
        public static List<int> Targets(GameManager g,Piece source)
        {
            if(source==null||!TimingRules.AbilityAllowed(g,source)||source.abilityUsed||g.IsSleeping(source))return new List<int>();
            var c=g.Data(source);
            if(c.Has("FastNetwork"))return g.State.tiles.Where(t=>TerrainManager.CompatibleRoute(t,source.owner)).Select(t=>t.id).ToList();
            if(c.Has("DestroyBiome"))return g.State.tiles[source.tileId].neighbors.Where(n=>TerrainManager.Normal(g.State.tiles[n])).ToList();
            if(c.Has("CampStep")||c.Has("ExtendBuff"))return g.Allies(source.owner).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&(c.Has("CampStep")||g.State.tiles[source.tileId].neighbors.Contains(p.tileId))).Select(p=>p.tileId).ToList();
            return new List<int>();
        }
        public static bool CanActivate(GameManager g,Piece p)
        {
            if(p==null||!TimingRules.AbilityAllowed(g,p)||p.abilityUsed||g.IsSleeping(p)||!HasActive(g.Data(p)))return false;
            var c=g.Data(p);if(c.Has("ExtendBuff")&&(g.ActingPlayer.spores<1||!BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&a.bonusAttack>0)))return false;
            return TargetCount(c)==0||Targets(g,p).Count>=TargetCount(c);
        }
        public static bool Activate(GameManager g,Piece p,IList<int> targets)
        {
            if(!CanActivate(g,p))return g.Fail("La habilidad no está disponible.");
            var c=g.Data(p);if(targets.Count!=TargetCount(c)||targets.Distinct().Count()!=targets.Count||targets.Any(t=>!Targets(g,p).Contains(t)))return g.Fail("Selecciona los objetivos de la habilidad.");
            if(c.Has("ExtendBuff"))
            {
                int bonus=BoardManager.Nearby(g.State,p.tileId).Where(a=>a.owner==p.owner&&a.tileId!=targets[0]).Select(a=>a.bonusAttack).DefaultIfEmpty().Max();
                if(g.ActingPlayer.spores<1||bonus==0)return g.Fail("Requiere 1 Espora y otra unidad adyacente con una mejora temporal de ATQ.");
                g.ActingPlayer.spores--;g.State.tiles[targets[0]].unit.bonusAttack+=bonus;
            }
            if(c.Has("FastNetwork"))TerrainManager.CreateFastRoute(g,targets[0],targets[1],p);
            if(c.Has("StructureDiscount"))g.ActingPlayer.structureDiscount++;
            if(c.Has("CampStep"))g.State.tiles[targets[0]].unit.remainingMovement++;
            if(c.Has("DestroyBiome"))TerrainManager.DestroyBiome(g,g.State.tiles[targets[0]]);
            p.abilityUsed=true;BattleManager.AfterResponse(g);g.Notify("Habilidad de "+c.name+" resuelta.");return true;
        }
        public static List<int> LeaderTargets(GameManager g)
        {
            if(!TimingRules.LeaderAllowed(g)||g.ActingPlayer.currentEnergy<g.State.rules.leaderAbilityCost)return new List<int>();
            if(g.ActingPlayer.leader=="SAHRIA")return g.State.tiles.Where(t=>t.owner==g.ActingPlayerId&&t.biome==Biome.Desert).Select(t=>t.id).ToList();
            var connected=new HashSet<int>();
            foreach(var route in g.State.fastRoutes.Where(f=>f.owner==g.ActingPlayerId))
                foreach(int id in BoardManager.ConnectedBiome(g.State,route.a,g.ActingPlayerId,Biome.Forest,Biome.Swamp))connected.Add(id);
            return g.State.tiles.Where(t=>t.unit!=null&&t.unit.owner!=g.ActingPlayerId&&t.biome==Biome.Forest&&(connected.Contains(t.id)||t.neighbors.Any(connected.Contains))).Select(t=>t.id).ToList();
        }
        public static bool Leader(GameManager g,IList<int> targets)
        {
            int max=g.ActingPlayer.leader=="SAHRIA"?2:1;
            if(targets.Count<1||targets.Count>max||targets.Distinct().Count()!=targets.Count||targets.Any(t=>!LeaderTargets(g).Contains(t)))return g.Fail("Habilidad de Líder: energía o selección inválida.");
            g.ActingPlayer.currentEnergy-=g.State.rules.leaderAbilityCost;
            int bonusGroup=g.State.nextId++;
            foreach(int id in targets)
            {
                var t=g.State.tiles[id];
                if(g.ActingPlayer.leader=="ZUKGROK")
                {
                    EffectManager.Poison(g,t.unit,1,g.ActingPlayerId);
                    int free=t.neighbors.FirstOrDefault(n=>!g.State.tiles[n].IsOccupied&&!g.State.tiles[n].blocked&&g.State.tiles[n].baseOwner<0,-1);
                    if(free>=0){g.State.tiles[free].owner=g.ActingPlayerId;g.Place("espora",g.ActingPlayerId,free);}
                }
                else t.specialEffect=new TerrainEffect {threshold=4,damage=1,bonusDamage=1,bonusGroup=bonusGroup,expiresTurn=g.State.turn+4};
            }
            g.Notify("Habilidad de "+g.ActingPlayer.leader+" resuelta.");return true;
        }
    }
}
