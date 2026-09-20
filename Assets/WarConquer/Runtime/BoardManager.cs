using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class BoardManager
    {
        // One flat-top axial lattice: every connection crosses a shared hexagon edge.
        public static void Create(GameState s)
        {
            int[] halfHeights = { 10, 11, 10, 5, 4, 3 };
            int[] baseQ = { 0, 4, 0, -4 }, baseR = { 4, -2, -4, 2 };
            for (int territory = 0; territory < 5; territory++)
            {
                for (int q = -5; q <= 5; q++) for (int r = -8; r <= 8; r++)
                {
                    int height = 2 * r + q;
                    if (Math.Abs(height) > halfHeights[Math.Abs(q)]) continue;
                    int region = Math.Max(Math.Abs(q), Math.Max(Math.Abs(r), Math.Abs(q+r))) <= 2 ? 4
                        : Math.Abs(q) >= 3 || (Math.Abs(q) == 2 && Math.Abs(height) == 4) ? (q > 0 ? 1 : 3)
                        : height > 0 ? 0 : 2;
                    if (region != territory) continue;
                    bool home = territory < 4;
                    var t = new HexTile { id = s.tiles.Count, territory = territory, q = q, r = r,
                        x = 1.2f * q, y = 1.385640646f * (r + q * .5f),
                        owner = home ? territory : -1, biome = Biome.Neutral,
                        baseOwner = home && q == baseQ[territory] && r == baseR[territory] ? territory : -1,
                        resource = territory == 4 ? (q == 0 && r == 0 ? 2 : (Math.Abs(q + r) == 2 ? 1 : 0)) : 0,
                        hiddenAsh = home ? q == baseQ[territory] && r == baseR[territory]+1 : q == 0 && r == 1,
                        hiddenResource = territory == 4 && q == 1 && r == 0 };
                    s.tiles.Add(t);
                }
            }
            foreach (var a in s.tiles) foreach (var b in s.tiles.Where(b => b.id > a.id))
                if (AxialDistance(a, b) == 1) Connect(s, a.id, b.id);
        }
        public static int AxialDistance(HexTile a, HexTile b) => (Math.Abs(a.q-b.q)+Math.Abs(a.r-b.r)+Math.Abs(a.q+a.r-b.q-b.r))/2;
        static void Connect(GameState s, int a, int b)
        {
            s.tiles[a].neighbors.Add(b); s.tiles[b].neighbors.Add(a); s.connections.Add(new Connection { a=a,b=b });
        }
        public static int Distance(GameState s, int a, int b, int limit = 100)
        {
            if (a == b) return 0;
            var seen = new HashSet<int> { a }; var queue = new Queue<(int,int)>(); queue.Enqueue((a,0));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue(); if (item.Item2 >= limit) continue;
                foreach (var n in s.tiles[item.Item1].neighbors)
                { if (n == b) return item.Item2+1; if (seen.Add(n)) queue.Enqueue((n,item.Item2+1)); }
            }
            return 999;
        }
        public static List<int> ConnectedBiome(GameState s, int start, int owner, params Biome[] biomes)
        {
            var result = new List<int>(); var queue = new Queue<int>(); queue.Enqueue(start);
            while(queue.Count>0)
            {
                int id=queue.Dequeue(); var t=s.tiles[id];
                if(result.Contains(id) || t.owner!=owner || !biomes.Contains(t.biome)) continue;
                result.Add(id); foreach(int n in t.neighbors) queue.Enqueue(n);
                foreach(var route in s.fastRoutes.Where(f=>f.owner==owner && (f.a==id || f.b==id))) queue.Enqueue(route.a==id?route.b:route.a);
            }
            return result;
        }
        public static IEnumerable<Piece> Pieces(GameState s) => s.tiles.SelectMany(t => new[] { t.unit, t.structure }).Where(p => p != null);
        public static IEnumerable<Piece> Nearby(GameState s, int tile) => s.tiles[tile].neighbors.SelectMany(n => new[]{s.tiles[n].unit,s.tiles[n].structure}).Where(p=>p!=null);
    }
}
