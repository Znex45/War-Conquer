using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    // Local visual offsets only. Tile coordinates, ownership and movement costs never change.
    public sealed class BoardLandingWave : MonoBehaviour
    {
        sealed class Slab {public HexTile tile;public Transform surface;}
        struct Pulse {public HexTile center;public float started;}
        readonly Dictionary<int,Slab> slabs=new Dictionary<int,Slab>();
        readonly List<Pulse> pulses=new List<Pulse>();
        public bool IsActive=>pulses.Count>0;
        public void Register(HexTile tile,Transform surface)=>slabs[tile.id]=new Slab{tile=tile,surface=surface};
        public void Impact(int tile)
        {
            if(slabs.TryGetValue(tile,out var slab))pulses.Add(new Pulse{center=slab.tile,started=Time.unscaledTime});
        }
        public void Clear()
        {
            foreach(var slab in slabs.Values)if(slab.surface!=null)slab.surface.localPosition=Vector3.zero;
            pulses.Clear();slabs.Clear();
        }
        void LateUpdate()
        {
            if(pulses.Count==0)return;
            float now=Time.unscaledTime;
            foreach(var slab in slabs.Values)
            {
                if(slab.surface==null)continue;
                float height=0;
                foreach(var pulse in pulses)
                {
                    int distance=BoardManager.AxialDistance(pulse.center,slab.tile);
                    if(distance>6)continue;
                    float phase=(now-pulse.started-distance*.085f)/.42f;
                    if(phase>=0&&phase<1)height-=Mathf.Sin(phase*Mathf.PI*2)*.24f*(1-distance/8f)*(1-phase);
                }
                slab.surface.localPosition=Vector3.up*Mathf.Clamp(height,-.26f,.26f);
            }
            pulses.RemoveAll(p=>now-p.started>=.94f);
            if(pulses.Count==0)foreach(var slab in slabs.Values)if(slab.surface!=null)slab.surface.localPosition=Vector3.zero;
        }
    }
}
