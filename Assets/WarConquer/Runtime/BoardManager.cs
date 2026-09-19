using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class BoardManager
    {
        // Five separated islands reproduce the reference's compass layout. Bridges are graph edges, not extra tiles.
        public static void Create(GameState s)
        {
            float[] xs = { 0, 9, 0, -9, 0 }, ys = { 8.4f, 0, -8.4f, 0, 0 };
            for (int territory = 0; territory < 5; territory++)
            {
                for (int q = -2; q <= 2; q++) for (int r = -2; r <= 2; r++)
                {
                    if (Math.Abs(q + r) > 2) continue;
                    if (territory < 4 && ((q == -2 && r == 0) || (q == 2 && r == 0))) continue;
                    var t = new HexTile { id = s.tiles.Count, territory = territory, q = q, r = r,
                        x = xs[territory] + 1.2f * q, y = ys[territory] + 1.38564f * (r + q * .5f),
                        owner = territory < 4 ? territory : -1, biome = Biome.Neutral,
                        baseOwner = territory < 4 && q == 0 && r == 0 ? territory : -1,
                        resource = territory == 4 ? (q == 0 && r == 0 ? 2 : (Math.Abs(q + r) == 2 ? 1 : 0)) : 0,
                        hiddenAsh = q == 0 && r == 1, hiddenResource = territory == 4 && q == 1 && r == 0 };
                    s.tiles.Add(t);
                }
            }
            foreach (var a in s.tiles) foreach (var b in s.tiles.Where(b => b.id > a.id && b.territory == a.territory))
                if (AxialDistance(a, b) == 1) Connect(s, a.id, b.id, false);
            // Two separate entrances to each island pair prevent any single occupied gateway cutting a territory off.
            for (int i = 0; i < 4; i++) { BridgePair(s, i, 4); BridgePair(s, i, (i + 1) % 4); }
        }
        static void BridgePair(GameState s, int a, int b)
        {
            var candidates = (from x in s.tiles where x.territory == a && x.baseOwner < 0
                              from y in s.tiles where y.territory == b && y.baseOwner < 0
                              select new { x, y, d = (x.x-y.x)*(x.x-y.x)+(x.y-y.y)*(x.y-y.y) }).OrderBy(p => p.d).ToList();
            var first = candidates[0]; Connect(s, first.x.id, first.y.id, true);
            var second = candidates.First(p => p.x.id != first.x.id && p.y.id != first.y.id);
            Connect(s, second.x.id, second.y.id, true);
        }
        static int AxialDistance(HexTile a, HexTile b) => (Math.Abs(a.q-b.q)+Math.Abs(a.r-b.r)+Math.Abs(a.q+a.r-b.q-b.r))/2;
        static void Connect(GameState s, int a, int b, bool bridge)
        {
            s.tiles[a].neighbors.Add(b); s.tiles[b].neighbors.Add(a); s.connections.Add(new Connection { a=a,b=b,bridge=bridge });
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
