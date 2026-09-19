using System.Linq;

namespace WarConquer
{
    public static class TerrainManager
    {
        public static bool Normal(HexTile t) => t.biome!=Biome.Neutral&&t.biome!=Biome.AshLand&&!t.permanentAsh;
        public static void Terraform(GameManager g,int tile,Biome biome,int owner)
        {
            var t=g.State.tiles[tile]; if(t.permanentAsh) return;
            t.biome=biome; t.owner=owner; t.specialEffect=null;
            if(t.unit!=null) t.unit.forestSinceTurn=biome==Biome.Forest?g.State.turn:-1;
            MaintainRoutes(g); g.State.Log("Hex "+(tile+1)+" → "+Names.Biomes[(int)biome]);
        }
        public static void DestroyBiome(GameManager g,HexTile t)
        {
            if(!Normal(t)) return;
            if(t.hiddenAsh&&g.State.rules.ashOnMarkedDestruction) { RevealAsh(g,t); return; }
            t.biome=Biome.Neutral; t.owner=t.unit!=null?t.unit.owner:t.structure!=null?t.structure.owner:t.baseOwner;
            t.specialEffect=null; if(t.unit!=null)t.unit.forestSinceTurn=-1; MaintainRoutes(g);
            g.State.Log("Hex "+(t.id+1)+": bioma destruido, casilla neutra.");
        }
        public static void RevealAsh(GameManager g,HexTile t)
        {
            t.biome=Biome.AshLand; t.hiddenAsh=false; t.specialEffect=null; if(t.unit!=null)t.unit.forestSinceTurn=-1;
            MaintainRoutes(g); g.State.Log("Hex "+(t.id+1)+": Tierra Ceniza revelada.");
        }
        public static bool CompatibleRoute(HexTile t,int owner) => t.owner==owner&&(t.biome==Biome.Forest||t.biome==Biome.Swamp);
        public static void MaintainRoutes(GameManager g)
        {
            g.State.fastRoutes.RemoveAll(f=>!f.protectedRoute && (!CompatibleRoute(g.State.tiles[f.a],f.owner)||!CompatibleRoute(g.State.tiles[f.b],f.owner)||!BoardManager.Pieces(g.State).Any(p=>p.id==f.sourceId)));
        }
        public static void CreateFastRoute(GameManager g,int a,int b,Piece source)
        {
            if(a==b||!CompatibleRoute(g.State.tiles[a],source.owner)||!CompatibleRoute(g.State.tiles[b],source.owner)) return;
            g.State.fastRoutes.RemoveAll(f=>f.sourceId==source.id&&!f.protectedRoute);
            bool eternal=g.Allies(source.owner).Any(p=>g.Data(p).Has("EternalForest")&&(g.State.tiles[p.tileId].neighbors.Contains(a)||g.State.tiles[p.tileId].neighbors.Contains(b)));
            g.State.fastRoutes.Add(new FastRoute {a=a,b=b,owner=source.owner,sourceId=source.id,protectedRoute=eternal});
            g.State.Log("Vía Rápida: "+(a+1)+" ↔ "+(b+1));
        }
        public static bool CheckUnstable(GameManager g,Piece p,HexTile tile,bool exiting)
        {
            var e=tile.specialEffect; var c=g.Data(p);
            if(e==null||(exiting&&!e.onExit)||c.movementType=="Flying"||c.Has("IgnoreUnstable")||c.Tag(e.compatibleTag)) return true;
            int die=g.State.Random(6)+1; bool passed=die>=e.threshold;
            g.State.Log(c.name+": d6="+die+" / "+e.threshold+"+ "+(exiting?"al salir":"al entrar")+(passed?" · supera":" · falla"));
            if(!passed)
            {
                p.remainingMovement=0;
                if(!exiting)
                {
                    CombatManager.Damage(g,p,e.damage+e.bonusDamage,false);
                    if(e.bonusGroup>0)foreach(var t in g.State.tiles.Where(t=>t.specialEffect!=null&&t.specialEffect.bonusGroup==e.bonusGroup))t.specialEffect.bonusDamage=0;
                    e.bonusDamage=0;
                }
            }
            return passed;
        }
    }
}
