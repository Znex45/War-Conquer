#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
namespace WarConquer.Editor
{
    // Frozen pre-optimization algorithm for differential regression checks only.
    static class ReferenceMovementPaths
    {
        static bool Current(GameManager g,Piece p)=>p!=null&&p.tileId>=0&&p.tileId<g.State.tiles.Count&&(g.State.tiles[p.tileId].unit==p||g.State.tiles[p.tileId].structure==p);
        class Node { public int tile,cost; public bool route; public List<int> path; }
        static bool CanStop(GameManager g,Piece p,int id)
        {
            var t=g.State.tiles[id]; return !t.blocked&&t.unit==null&&!t.BlocksMovement&&(t.baseOwner<0||t.baseOwner==p.owner||g.State.players[t.baseOwner].eliminated);
        }
        public static Dictionary<int,List<int>> Paths(GameManager g,Piece p)
        {
            var result=new Dictionary<int,List<int>>();
            if(!Current(g,p)||p.owner!=g.State.activePlayer||g.IsSleeping(p)||g.Data(p).IsStructure||!g.CanTakeTurnAction(TurnStage.Assault)||(p.remainingMovement<=0||p.darkFlyExtraAttack)) return result;
            var c=g.Data(p); var open=new List<Node> { new Node {tile=p.tileId,cost=0,path=new List<int>(),route=false} };
            var seen=new Dictionary<(int,bool),int>(); var best=new Dictionary<int,int>();
            while(open.Count>0)
            {
                var cur=open.OrderBy(n=>n.cost).First(); open.Remove(cur);
                if(seen.TryGetValue((cur.tile,cur.route),out int old)&&old<=cur.cost) continue;
                seen[(cur.tile,cur.route)]=cur.cost;
                if(cur.tile!=p.tileId&&CanStop(g,p,cur.tile)&&(!best.ContainsKey(cur.tile)||cur.cost<best[cur.tile])) { result[cur.tile]=cur.path; best[cur.tile]=cur.cost; }
                var edges=g.State.tiles[cur.tile].neighbors.Select(n=>(id:n,route:false)).ToList();
                edges.AddRange(g.State.fastRoutes.Where(f=>f.owner==p.owner&&(f.a==cur.tile||f.b==cur.tile)).Select(f=>(id:f.a==cur.tile?f.b:f.a,route:true)));
                foreach(var edge in edges)
                {
                    var t=g.State.tiles[edge.id]; bool via=cur.route||edge.route;
                    int cost=cur.cost+(edge.route&&c.Has("FreeRoute")?0:1);
                    // A ground unit uses the bonus of the slab it is leaving; flying
                    // retains its starting bonus until landing. Account for spent movement.
                    int terrainDelta=c.movementType=="Flying"?0:(TerrainManager.Compatible(g.State.players[p.owner].leader,g.State.tiles[cur.tile].biome)?1:0)-TerrainManager.GetTerrainStatModifier(g,p);
                    if(g.State.rules.summoningSickness&&p.playedTurn==g.State.turn)terrainDelta=0;
                    int budget=p.remainingMovement-p.movementAuraDebt+terrainDelta+(via&&c.Has("FastBonus")&&!p.fastBonusUsed?1:0);
                    if(cost>budget||cur.path.Contains(edge.id)||edge.id==p.tileId) continue;
                    if(c.movementType!="Flying"&&(t.BlocksMovement||t.unit!=null||(t.baseOwner>=0&&t.baseOwner!=p.owner&&!g.State.players[t.baseOwner].eliminated))) continue;
                    var path=new List<int>(cur.path){edge.id}; open.Add(new Node {tile=edge.id,cost=cost,route=via,path=path});
                }
            }
            if(g.Data(p).Has("CrystalMinion")&&g.State.tiles[p.tileId].biome==Biome.Void)
                foreach(var t in g.State.tiles.Where(t=>t.id!=p.tileId&&t.biome==Biome.Void&&CanStop(g,p,t.id)&&BoardManager.AxialDistance(g.State.tiles[p.tileId],t)<=3))result[t.id]=new List<int>{t.id};
            return result;
        }

    }
}
#endif
