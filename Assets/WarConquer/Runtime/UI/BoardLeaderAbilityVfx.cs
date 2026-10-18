using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // Uses the board's shared geometry/materials and never advances game simulation.
    public sealed class BoardLeaderAbilityVfx : MonoBehaviour
    {
        sealed class Burst
        {
            public Transform root,follow; public Transform[] motes; public Light light;
            public Vector3 from,to; public float age,duration,delay; public bool clouds; public Color color;
        }
        sealed class Leaves {public Transform root;public Transform[] leaves;public float amount,phase;public bool available;}
        sealed class Glow {public Transform target;public float age;public readonly Dictionary<Renderer,Material[]> originals=new Dictionary<Renderer,Material[]>();public readonly Dictionary<Material,Material> copies=new Dictionary<Material,Material>();}
        readonly List<Glow> glows=new List<Glow>();
        readonly Queue<LeaderAbilitySignal> pending=new Queue<LeaderAbilitySignal>();
        readonly List<Burst> bursts=new List<Burst>();
        readonly Dictionary<int,Leaves> foliage=new Dictionary<int,Leaves>();
        GameManager game;GameState state;BoardMeshFactory mesh;Board3DScene board;Material trailMaterial;
        public int ActivationCount {get;private set;}
        public int ActiveBursts=>bursts.Count;
        public int CloudCount=>bursts.Where(b=>b.clouds).Sum(b=>b.motes.Length);
        public int FoliageCount=>foliage.Count;
        public void Initialize(BoardMeshFactory shared,Board3DScene scene){mesh=shared;board=scene;}
        void Receive(LeaderAbilitySignal signal){if(isActiveAndEnabled&&pending.Count<64)pending.Enqueue(signal);}
        public void Sync(GameManager current)
        {
            if(game!=current){if(game!=null)game.LeaderAbilityResolved-=Receive;game=current;game.LeaderAbilityResolved+=Receive;Clear();state=null;}
            if(state!=game.State){Clear();state=game.State;}
            foreach(var p in state.players)
            {
                var leader=board.LeaderVisual(p.id);
                bool exists=p.leader=="FAUNAR"&&!p.eliminated&&!p.inactive&&leader!=null;
                if(!exists){if(foliage.TryGetValue(p.id,out var old)){Release(old.root);foliage.Remove(p.id);}continue;}
                if(!foliage.TryGetValue(p.id,out var leaves)||leaves.root==null)
                {
                    leaves=new Leaves{root=mesh.Group(leader,"FaunarAvailableVFX",Vector3.zero),leaves=new Transform[10]};foliage[p.id]=leaves;
                    for(int i=0;i<10;i++)leaves.leaves[i]=mesh.Part(leaves.root,"Hoja",mesh.Sphere,Vector3.zero,Vector3.zero,i%3==0?new Color(.65f,.65f,.12f):new Color(.16f,.65f,.19f)).transform;
                }
                leaves.available=!p.firstStructureUsed&&state.phase!=Phase.Finished;
            }
            while(pending.Count>0)
            {
                var signal=pending.Dequeue();if(signal.state!=state)continue;
                var leader=board.LeaderVisual(signal.owner);if(leader==null)continue;
                ActivationCount++;
                if(signal.leader=="XIL'THAR")Aura(leader,new Color(.7f,.08f,1),"XiltharAbilityVFX",1.05f);
                if(signal.leader=="SAHRIA")
                {
                    Aura(leader,new Color(1,.74f,.15f),"SahriaAbilityVFX",1.05f);
                    var unit=board.UnitVisual(signal.piece);if(unit!=null)Aura(unit,new Color(1,.74f,.15f),"SahriaUnitVFX",.95f,.18f);
                }
                if(signal.leader=="ZUKGROK"&&signal.tile>=0&&signal.tile<state.tiles.Count)
                {
                    FlashModel(leader);
                    Aura(leader,new Color(.35f,.035f,.6f),"ZukgrokAbilityVFX",.7f);
                    Clouds(leader.position+Vector3.up*1.4f,Board3DScene.Position(state.tiles[signal.tile])+Vector3.up*(BoardMeshFactory.Surface+.2f));
                }
            }
        }
        void Aura(Transform target,Color color,string name,float duration,float delay=0)
        {
            var b=new Burst{root=mesh.Group(transform,name,Vector3.zero),follow=target,color=color,duration=duration,delay=delay,motes=new Transform[22]};
            for(int i=0;i<b.motes.Length;i++)b.motes[i]=mesh.Part(b.root,"Partícula mágica",mesh.Sphere,Vector3.zero,Vector3.zero,color).transform;
            b.light=b.root.gameObject.AddComponent<Light>();b.light.type=LightType.Point;b.light.color=color;b.light.range=4;b.light.shadows=LightShadows.None;b.light.cullingMask=1<<BoardMeshFactory.Layer;b.light.intensity=0;
            bursts.Add(b);
        }
        void FlashModel(Transform target)
        {
            var active=glows.FirstOrDefault(x=>x.target==target);if(active!=null){active.age=0;return;}
            var glow=new Glow{target=target};
            foreach(var r in target.GetComponentsInChildren<Renderer>())
            {
                if(r is not MeshRenderer&&r is not SkinnedMeshRenderer||r.GetComponent<TextMesh>()!=null)continue;
                var originals=r.sharedMaterials;glow.originals[r]=originals;
                r.sharedMaterials=originals.Select(m=>{if(!glow.copies.TryGetValue(m,out var copy)){copy=new Material(m);glow.copies.Add(m,copy);}return copy;}).ToArray();
            }
            glows.Add(glow);
        }
        static void RestoreGlow(Glow glow){foreach(var r in glow.originals)if(r.Key!=null)r.Key.sharedMaterials=r.Value;foreach(var m in glow.copies.Values)BoardMeshFactory.Release(m);}
        void Clouds(Vector3 from,Vector3 to)
        {
            var b=new Burst{root=mesh.Group(transform,"ZukgrokPoisonCloudVFX",Vector3.zero),from=from,to=to,clouds=true,duration=1.15f,motes=new Transform[3]};
            if(trailMaterial==null){trailMaterial=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));trailMaterial.SetFloat("_Surface",1);trailMaterial.SetFloat("_Blend",0);trailMaterial.SetFloat("_SrcBlend",5);trailMaterial.SetFloat("_DstBlend",10);trailMaterial.SetFloat("_ZWrite",0);trailMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");trailMaterial.renderQueue=3000;}
            for(int i=0;i<3;i++)
            {
                var cloud=mesh.Group(b.root,"Nube "+(i+1),from);b.motes[i]=cloud;
                for(int puff=0;puff<4;puff++)mesh.Part(cloud,"Esporas",mesh.Sphere,new Vector3(Mathf.Sin(puff*2)*.1f,puff%2*.08f,Mathf.Cos(puff*2)*.1f),Vector3.one*.25f,new Color(.3f,.025f,.48f));
                var trail=cloud.gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=trailMaterial;trail.time=.25f;trail.startWidth=.11f;trail.endWidth=0;trail.minVertexDistance=.08f;trail.startColor=new Color(.5f,.08f,.75f,.8f);trail.endColor=new Color(.25f,0,.45f,0);trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            bursts.Add(b);
        }
        void Update()=>Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            for(int i=glows.Count-1;i>=0;i--)
            {
                var glow=glows[i];glow.age+=dt;if(glow.age>=.7f||glow.target==null){RestoreGlow(glow);glows.RemoveAt(i);continue;}
                float pulse=Mathf.Sin(glow.age/.7f*Mathf.PI);
                foreach(var pair in glow.copies)
                {
                    var originalEmission=pair.Key.IsKeywordEnabled("_EMISSION")?pair.Key.GetColor("_EmissionColor"):Color.black;
                    pair.Value.SetColor("_EmissionColor",originalEmission+new Color(.14f,.005f,.25f)*pulse);
                    pair.Value.SetColor("_BaseColor",Color.Lerp(pair.Key.GetColor("_BaseColor"),new Color(.23f,.025f,.38f),pulse*.65f));
                }
            }
            foreach(var leaves in foliage.Values)
            {
                if(leaves.root==null)continue;
                leaves.amount=Mathf.MoveTowards(leaves.amount,leaves.available?1:0,dt*1.35f);leaves.phase+=dt*(.15f+leaves.amount*1.8f);
                for(int i=0;i<leaves.leaves.Length;i++)
                {
                    float a=leaves.phase+i*Mathf.PI*2/10;
                    var t=leaves.leaves[i];t.localPosition=new Vector3(Mathf.Cos(a)*.72f,(.4f+i%4*.38f)*leaves.amount,Mathf.Sin(a)*.72f);
                    t.localRotation=Quaternion.Euler(20, -a*Mathf.Rad2Deg,Mathf.Sin(a*2)*25);t.localScale=new Vector3(.08f,.025f,.19f)*Mathf.Sqrt(leaves.amount);
                }
            }
            for(int i=bursts.Count-1;i>=0;i--)
            {
                var b=bursts[i];b.age+=dt;float time=b.age-b.delay;
                if(time<0)continue;
                if(time>b.duration||(!b.clouds&&b.follow==null)){Release(b.root);bursts.RemoveAt(i);continue;}
                float t=time/b.duration;
                if(b.clouds)
                {
                    for(int c=0;c<3;c++)
                    {
                        float u=Mathf.Clamp01((t-c*.06f)/.76f);float arc=Mathf.Sin(u*Mathf.PI);
                        b.motes[c].position=Vector3.Lerp(b.from,b.to,u)+new Vector3((c-1)*.8f*arc,(.5f+c*.2f)*arc,Mathf.Sin(c*2)*.7f*arc);
                        b.motes[c].localScale=Vector3.one*(u<.9f?1:Mathf.Max(0,(1-u)*10));
                    }
                }
                else
                {
                    b.root.position=b.follow.position+Vector3.up*.75f;
                    float envelope=Mathf.Sin(t*Mathf.PI);b.light.intensity=envelope*3;
                    for(int m=0;m<b.motes.Length;m++)
                    {
                        float angle=m*2.399f+t*2;float radius=.4f+t*.45f;
                        b.motes[m].localPosition=new Vector3(Mathf.Cos(angle)*radius,(m%7)*.18f-.3f+t*.25f,Mathf.Sin(angle)*radius);
                        b.motes[m].localScale=Vector3.one*((m%3==0?.07f:.035f)*envelope);
                    }
                }
            }
        }
        static void Release(Transform root){if(root!=null)BoardMeshFactory.Release(root.gameObject);}
        void Clear(){pending.Clear();foreach(var glow in glows)RestoreGlow(glow);glows.Clear();foreach(var b in bursts)Release(b.root);bursts.Clear();foreach(var l in foliage.Values)Release(l.root);foliage.Clear();}
        void OnDisable(){Clear();}
        void OnDestroy(){if(game!=null)game.LeaderAbilityResolved-=Receive;Clear();if(trailMaterial!=null)BoardMeshFactory.Release(trailMaterial);}
    }
}
