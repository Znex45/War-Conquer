using System;
using System.Linq;

namespace WarConquer
{
    public readonly struct CostQuote
    {
        public readonly int original, discount, resources, energy, surcharge;
        public CostQuote(int original,int discount,int resources,int surcharge=0){this.original=original;this.discount=discount;this.resources=resources;this.surcharge=surcharge;energy=original-discount-resources+surcharge;}
        public string EnergyLabel => original==energy?energy.ToString():original+" → "+energy;
        public string Detail => energy+" Energía"+(surcharge>0?" · SAHRIA +1 E":"")+(resources>0?" + "+resources+" recursos compatibles":"")+(discount>0?" · descuento de habilidad: "+discount:"");
    }
    public static class EnergyManager
    {
        public static int CompatibleResources(Player p,CardData card) => p.resources.Where(r=>card.Tag(r.requiredTag)).Sum(r=>r.amount);
        public static CostQuote Quote(Player p,CardData card,bool useResources=false)
        {
            int discount=card.IsStructure?Math.Min(card.energyCost,Math.Max(0,p.structureDiscount)+(p.leader=="FAUNAR"&&!p.firstStructureUsed?1:0)):0;
            int resources=useResources?Math.Min(card.energyCost-discount,CompatibleResources(p,card)):0;
            return new CostQuote(card.energyCost,discount,resources,p.leader=="SAHRIA"&&p.sahriaDeployment&&card.category==Category.Unit?1:0);
        }
        public static int Cost(Player p, CardData card) => Quote(p,card).energy;
        public static bool CanPay(Player p, CardData card, bool useResources=false) => p.currentEnergy>=Quote(p,card,useResources).energy;
        public static void Pay(Player p, CardData card, bool useResources)
        {
            var quote=Quote(p,card,useResources);
            if(p.currentEnergy<quote.energy)throw new InvalidOperationException("Energía insuficiente.");
            int remaining=quote.resources;
            foreach(var pool in p.resources.Where(r=>card.Tag(r.requiredTag))) {int paid=Math.Min(pool.amount,remaining);pool.amount-=paid;remaining-=paid;}
            p.currentEnergy-=quote.energy;if(card.IsStructure){p.structureDiscount=0;p.firstStructureUsed=true;}
        }
        public static void AddResource(GameState s, Player p, Biome biome,int amount)
        {
            string tag=Names.BiomeTag(biome); if(tag.Length==0) return;
            var pool=p.resources.Find(r=>r.biome==biome); if(pool==null) { pool=new ResourcePool { biome=biome,requiredTag=tag }; p.resources.Add(pool); }
            pool.amount=Math.Min(s.rules.maxResourcesPerBiome,pool.amount+amount);
        }
    }
}
