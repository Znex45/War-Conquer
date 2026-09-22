using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class AbilityManager
    {
        public static bool HasActive(CardData c) => new[]{"FastNetwork","StructureDiscount","CampStep","DestroyBiome","ExtendBuff","MedCamp","RestingCamp","VigilantTower","ForestLeap","WanderingBeast","MushroomToken","FireflyToken"}.Any(c.Has);
        public static int TargetCount(CardData c) => c.Has("MedCamp")?4:c.Has("FastNetwork")?2:c.Has("StructureDiscount")||c.Has("RestingCamp")?0:1;
        public static string Label(CardData c) => c.Has("WanderingBeast")?"Infligir 2 a 3rad":c.Has("MushroomToken")?"Envenenar (radio por Tokens)":c.Has("FireflyToken")?"Sacrificar y envenenar a 2rad": c.Has("MedCamp")?"Curar hasta 4 aliados":c.Has("RestingCamp")?"Curar aliados a 4rad":c.Has("VigilantTower")?"Infligir 1 a 6rad":c.Has("ForestLeap")?"Saltar junto a Bosque": c.Has("FastNetwork")?"Conectar 2 casillas":c.Has("StructureDiscount")?"Descuento de estructura":c.Has("CampStep")?"Dar +1 MOV":c.Has("DestroyBiome")?"Destruir bioma":c.Has("ExtendBuff")?"Extender mejora (1 Espora)":"Sin habilidad activa";
        public static List<int> Targets(GameManager g,Piece source)
        {
            if(source==null||!TimingRules.AbilityAllowed(g,source)||source.abilityUsed||g.IsSleeping(source))return new List<int>();
            var c=g.Data(source);if(GenericCardRules.IsActive(c))return GenericCardRules.AbilityTargets(g,source);
            if(c.Has("FastNetwork"))return g.State.tiles.Where(t=>TerrainManager.CompatibleRoute(t,source.owner)).Select(t=>t.id).ToList();
            if(c.Has("DestroyBiome"))return g.State.tiles[source.tileId].neighbors.Where(n=>TerrainManager.Normal(g.State.tiles[n])).ToList();
            if(c.Has("CampStep")||c.Has("ExtendBuff"))return g.Allies(source.owner).Where(p=>!g.Data(p).IsStructure&&!g.IsSleeping(p)&&(c.Has("CampStep")||g.State.tiles[source.tileId].neighbors.Contains(p.tileId))).Select(p=>p.tileId).ToList();
            return new List<int>();
        }
        public static bool CanActivate(GameManager g,Piece p)
        {
            if(p==null||!TimingRules.AbilityAllowed(g,p)||p.abilityUsed||g.IsSleeping(p)||!HasActive(g.Data(p)))return false;
            var c=g.Data(p);if(c.Has("ExtendBuff")&&(g.ActingPlayer.spores<1||!BoardManager.Nearby(g.State,p.tileId).Any(a=>a.owner==p.owner&&a.bonusAttack>0)))return false;
            if(c.Has("RestingCamp"))return GenericCardRules.AbilityTargets(g,p).Count>0;
            return TargetCount(c)==0||Targets(g,p).Count>=(c.Has("MedCamp")?1:TargetCount(c));
        }
        public static bool Activate(GameManager g,Piece p,IList<int> targets)
        {
            if(!CanActivate(g,p))return g.Fail("La habilidad no está disponible.");
            var c=g.Data(p);if((c.Has("MedCamp")?(targets.Count<1||targets.Count>4):targets.Count!=TargetCount(c))||targets.Distinct().Count()!=targets.Count||targets.Any(t=>!Targets(g,p).Contains(t)))return g.Fail("Selecciona los objetivos de la habilidad.");
            int bonus=0;
            if(c.Has("ExtendBuff"))
            {
                bonus=BoardManager.Nearby(g.State,p.tileId).Where(a=>a.owner==p.owner&&a.tileId!=targets[0]).Select(a=>a.bonusAttack).DefaultIfEmpty().Max();
                if(g.ActingPlayer.spores<1||bonus==0)return g.Fail("Requiere 1 Espora y otra unidad adyacente con una mejora temporal de ATQ.");
            }
            if(!TerrainManager.CheckAction(g,p,"habilidad")){p.abilityUsed=true;g.Notify("Habilidad interrumpida por el terreno.");return true;}
            if(GenericCardRules.IsActive(c))GenericCardRules.Activate(g,p,targets);
            if(c.Has("ExtendBuff")){g.ActingPlayer.spores--;g.State.tiles[targets[0]].unit.bonusAttack+=bonus;}
            if(c.Has("FastNetwork"))TerrainManager.CreateFastRoute(g,targets[0],targets[1],p);
            if(c.Has("StructureDiscount"))g.ActingPlayer.structureDiscount++;
            if(c.Has("CampStep"))g.State.tiles[targets[0]].unit.remainingMovement++;
            if(c.Has("DestroyBiome"))TerrainManager.DestroyBiome(g,g.State.tiles[targets[0]]);
            p.abilityUsed=true;BattleManager.AfterResponse(g);g.Notify("Habilidad de "+c.name+" resuelta.");return true;
        }
        public static List<int> LeaderTargets(GameManager g)=>new List<int>();
        public static bool Leader(GameManager g,IList<int> targets)=>g.Fail("La habilidad del Líder se resuelve con su condición, no como ataque activo.");
    }
}
