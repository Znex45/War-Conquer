using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class BoardManager
    {
        // One flat-top axial lattice: every connection crosses a shared hexagon edge.
        public static void Create(GameState s,int participants=4)
        {
            if(participants<2||participants>4)throw new ArgumentOutOfRangeException(nameof(participants));
            s.mapPlayers=participants;s.tiles.Clear();s.connections.Clear();
            var bases=participants==2?new[]{(-8,4),(8,-4)}:participants==3?new[]{(0,9),(-9,0),(9,-9)}:new[]{(-12,12),(12,0),(12,-12),(-12,0)};
            // Three-player objectives face the gaps between bases, keeping all starts equally distant.
            var sites=participants==3?new[]{(0,-2),(2,0),(-2,2)}:new[]{(0,-4),(0,0),(0,4)};
            int Dist(int q,int r,int x,int y)=>(Math.Abs(q-x)+Math.Abs(r-y)+Math.Abs(q+r-x-y))/2;
            for(int q=-14;q<=14;q++)for(int r=-18;r<=18;r++)
            {
                int h=2*r+q;
                bool inside=participants==2?Math.Min(Dist(q,r,-4,2),Math.Min(Dist(q,r,0,0),Dist(q,r,4,-2)))<=6:
                    participants==3?Dist(q,r,0,0)<=10:
                    Math.Abs(q)<=14&&Math.Abs(h)<=(Math.Abs(q)<=4?12:Math.Abs(q)<=10?18:Math.Abs(q)<=12?16:14);
                if(!inside)continue;
                int owner=Array.FindIndex(bases,b=>Dist(q,r,b.Item1,b.Item2)<=1);
                int baseOwner=Array.FindIndex(bases,b=>q==b.Item1&&r==b.Item2);
                bool site=sites.Contains((q,r));
                s.tiles.Add(new HexTile{q=q,r=r,x=1.2f*q,y=1.385640646f*(r+q*.5f),owner=owner,territory=owner<0?4:owner,
                    baseOwner=baseOwner,biome=Biome.Neutral,conquestSite=site,resource=site?1:0,hiddenAsh=baseOwner>=0,hiddenResource=site});
            }
            s.tiles=s.tiles.OrderBy(t=>t.territory).ThenBy(t=>t.q).ThenBy(t=>t.r).ToList();
            for(int i=0;i<s.tiles.Count;i++)s.tiles[i].id=i;
            var lattice=s.tiles.ToDictionary(t=>(t.q,t.r));
            var directions=new[]{(1,0),(1,-1),(0,-1),(-1,0),(-1,1),(0,1)};
            foreach(var a in s.tiles)foreach(var d in directions)
                if(lattice.TryGetValue((a.q+d.Item1,a.r+d.Item2),out var b)&&b.id>a.id)Connect(s,a.id,b.id);
            foreach(var tile in s.tiles)tile.neighbors.Sort();
            s.connections=s.connections.OrderBy(c=>c.a).ThenBy(c=>c.b).ToList();
        }
        public static int AxialDistance(HexTile a, HexTile b) => (Math.Abs(a.q-b.q)+Math.Abs(a.r-b.r)+Math.Abs(a.q+a.r-b.q-b.r))/2;
        static void Connect(GameState s, int a, int b)
        {
            s.tiles[a].neighbors.Add(b); s.tiles[b].neighbors.Add(a); s.connections.Add(new Connection { a=a,b=b });
        }
        public static int Distance(GameState s, int a, int b, int limit = 100)
        {
            if (a == b) return 0;
            if(limit<=0)return 999;
            int count=s.tiles.Count;var distances=ArrayPool<int>.Shared.Rent(count);var queue=ArrayPool<int>.Shared.Rent(count);
            try
            {
                Array.Clear(distances,0,count);int head=0,tail=0;distances[a]=1;queue[tail++]=a;
                while(head<tail)
                {
                    int current=queue[head++],distance=distances[current]-1;if(distance>=limit)continue;
                    foreach(int neighbor in s.tiles[current].neighbors)
                    {
                        if(neighbor==b)return distance+1;
                        if(distances[neighbor]!=0)continue;
                        distances[neighbor]=distance+2;queue[tail++]=neighbor;
                    }
                }
                return 999;
            }
            finally{ArrayPool<int>.Shared.Return(distances);ArrayPool<int>.Shared.Return(queue);}
        }
        public static List<int> ConnectedBiome(GameState s, int start, int owner, params Biome[] biomes)
        {
            var result = new List<int>(); var visited=new HashSet<int>();var queue = new Queue<int>(); queue.Enqueue(start);
            while(queue.Count>0)
            {
                int id=queue.Dequeue(); var t=s.tiles[id];
                if(visited.Contains(id) || t.owner!=owner || !biomes.Contains(t.biome)) continue;
                visited.Add(id);
                result.Add(id); foreach(int n in t.neighbors) queue.Enqueue(n);
                foreach(var route in s.fastRoutes.Where(f=>f.owner==owner && (f.a==id || f.b==id))) queue.Enqueue(route.a==id?route.b:route.a);
            }
            return result;
        }
        public static IEnumerable<HexTile> SlabsWithinRadius(GameState s,int center,int radius)
        {if(center<0||center>=s.tiles.Count||radius<0)return Enumerable.Empty<HexTile>();return s.tiles.Where(t=>AxialDistance(s.tiles[center],t)<=radius);}
        public static IEnumerable<Piece> UnitsWithinRadius(GameState s,int center,int radius)=>SlabsWithinRadius(s,center,radius).Where(t=>t.unit!=null).Select(t=>t.unit);
        public static IEnumerable<Piece> StructuresWithinRadius(GameState s,int center,int radius)=>SlabsWithinRadius(s,center,radius).Where(t=>t.structure!=null).Select(t=>t.structure);
        public static IEnumerable<Piece> Pieces(GameState s)
        {
            foreach(var tile in s.tiles){var unit=tile.unit;var structure=tile.structure;if(unit!=null)yield return unit;if(structure!=null)yield return structure;}
        }
        public static IEnumerable<Piece> Nearby(GameState s,int tile)
        {
            foreach(int id in s.tiles[tile].neighbors){var unit=s.tiles[id].unit;var structure=s.tiles[id].structure;if(unit!=null)yield return unit;if(structure!=null)yield return structure;}
        }
    }
}
