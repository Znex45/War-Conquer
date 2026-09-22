using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class MovementManager
    {
        class Node { public int tile,cost; public bool route; public List<int> path; }
        static bool CanStop(GameManager g,Piece p,int id)
        {
            var t=g.State.tiles[id]; return !t.blocked&&t.unit==null&&!t.BlocksMovement&&(t.baseOwner<0||t.baseOwner==p.owner||g.State.players[t.baseOwner].eliminated);
        }
        public static Dictionary<int,List<int>> Paths(GameManager g,Piece p)
        {
            var result=new Dictionary<int,List<int>>();
            if(p==null||p.owner!=g.State.activePlayer||g.IsSleeping(p)||g.Data(p).IsStructure||!g.CanTakeTurnAction(TurnStage.Assault)||p.remainingMovement<=0) return result;
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
                    int budget=p.remainingMovement+(via&&c.Has("FastBonus")&&!p.fastBonusUsed?1:0);
                    if(cost>budget||cur.path.Contains(edge.id)||edge.id==p.tileId) continue;
                    if(c.movementType!="Flying"&&(t.BlocksMovement||t.unit!=null||(t.baseOwner>=0&&t.baseOwner!=p.owner&&!g.State.players[t.baseOwner].eliminated))) continue;
                    var path=new List<int>(cur.path){edge.id}; open.Add(new Node {tile=edge.id,cost=cost,route=via,path=path});
                }
            }
            return result;
        }
        public static List<int> RouteDestinations(GameManager g,Piece p)
        {
            if(g.IsSleeping(p))return new List<int>();
            return g.State.fastRoutes.Where(f=>f.owner==p.owner&&(f.a==p.tileId||f.b==p.tileId)).Select(f=>f.a==p.tileId?f.b:f.a).Where(n=>CanStop(g,p,n)).Distinct().ToList();
        }
        public static bool Move(GameManager g,Piece p,int destination)
        {
            var paths=Paths(g,p); if(!paths.TryGetValue(destination,out var path)) return g.Fail("Movimiento inválido: alcance, ocupación o estado Dormido.");
            bool fastBonusUsed=p.fastBonusUsed;
            foreach(int next in path)
            {
                var origin=g.State.tiles[p.tileId];
                if(!TerrainManager.CheckUnstable(g,p,origin,true)) break;
                bool fast=g.State.fastRoutes.Any(f=>f.owner==p.owner&&((f.a==origin.id&&f.b==next)||(f.b==origin.id&&f.a==next)));
                int cost=fast&&g.Data(p).Has("FreeRoute")?0:1;
                if(fast&&!fastBonusUsed&&g.Data(p).Has("FastBonus")) {p.remainingMovement++;fastBonusUsed=true;p.fastBonusUsed=true;}
                // Flying may pass above occupied/blocked tiles; it only lands at the chosen destination.
                bool landing=g.Data(p).movementType!="Flying"||next==destination;
                p.remainingMovement=Math.Max(0,p.remainingMovement-cost);
                if(!landing)continue;
                if(!TerrainManager.CheckUnstable(g,p,g.State.tiles[next],false))break;
                Relocate(g,p,next);if(p.health<=0||g.State.pendingChoices.Count>0)break;
            }
            g.Notify(g.Data(p).name+" · movimiento resuelto. Revisa el registro si hubo tiradas."); return true;
        }
        public static void Relocate(GameManager g,Piece p,int destination,bool carried=false)
        {
            var from=g.State.tiles[p.tileId]; if(from.unit==p)from.unit=null;
            ConquestManager.Refresh(g.State);
            var to=g.State.tiles[destination]; p.tileId=destination; p.moved=true; to.unit=p; to.owner=p.owner;
            p.forestSinceTurn=-1;
            TerrainManager.MaintainRoutes(g);GenericCardRules.SyncAuras(g);if(p.health>0){EffectManager.OnTileEnter(g,p);GenericCardRules.OnMove(g,p,from.biome);FactionCardRules.OnMove(g,p,from,carried);}ConquestManager.Refresh(g.State);
            g.State.Log(g.Data(p).name+" → hex "+(destination+1));
        }
        public static bool FreeStep(GameManager g,Piece p,int destination)
        {
            if(!g.CanTakeTurnAction(TurnStage.Assault)||p==null||p.owner!=g.State.activePlayer||g.IsSleeping(p)||g.State.Active.freeSteps<=0||!g.State.tiles[p.tileId].neighbors.Contains(destination)||!CanStop(g,p,destination)) return g.Fail("No hay un paso adicional válido.");
            g.State.Active.freeSteps--;
            if(TerrainManager.CheckUnstable(g,p,g.State.tiles[p.tileId],true)&&TerrainManager.CheckUnstable(g,p,g.State.tiles[destination],false)) Relocate(g,p,destination);
            g.Notify("Paso adicional resuelto.");return true;
        }
    }
}
