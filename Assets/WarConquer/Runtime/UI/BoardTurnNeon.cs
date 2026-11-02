using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WarConquer
{
    public sealed class BoardTurnNeon:MonoBehaviour
    {
        sealed class TileGlow { public Transform root;public int owner,baseOwner;public LineRenderer core,halo; }
        readonly Dictionary<int,TileGlow> tiles=new Dictionary<int,TileGlow>();
        readonly List<TileGlow> glowing=new List<TileGlow>();
        Material lineMaterial,sparkMaterial;Light baseLight,leaderLight;Transform aura,auraAnchor;LineRenderer auraCore,auraHalo;ParticleSystem sparks;
        Color color;int owner=-1;float nextFrame;
        public int PriorityPlayer=>owner;
        public Color PriorityColor=>color;
        public Light BaseLight=>baseLight;
        public int ActiveContourCount {get{int count=0;foreach(var tile in tiles.Values)if(tile.core!=null&&tile.core.enabled)count++;return count;}}
        public void Initialize()
        {
            if(lineMaterial!=null)return;
            var shader=Resources.Load<Shader>("WarConquer/Materials/NeonGlow");
            lineMaterial=new Material(shader){name="Shared neon contours"};lineMaterial.SetFloat("_SoftParticle",0);
            sparkMaterial=new Material(shader){name="Shared turn neon sparks"};sparkMaterial.SetFloat("_SoftParticle",1);
            baseLight=MakeLight("Active base neon light",6,2.5f);leaderLight=MakeLight("Active leader neon light",8,3);
            aura=new GameObject("Priority leader neon aura").transform;aura.SetParent(transform,false);aura.gameObject.layer=BoardMeshFactory.Layer;
            auraHalo=Contour(aura,"Soft neon halo",2.12f,.23f);auraCore=Contour(aura,"Neon core",2.1f,.045f);
            var particleObject=new GameObject("Priority neon particles");particleObject.layer=BoardMeshFactory.Layer;particleObject.transform.SetParent(aura,false);
            sparks=particleObject.AddComponent<ParticleSystem>();sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=sparks.main;main.loop=true;main.playOnAwake=false;main.useUnscaledTime=true;main.startLifetime=1.9f;main.startSpeed=.22f;main.startSize=.11f;main.maxParticles=32;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=sparks.emission;emission.rateOverTime=12;
            var shape=sparks.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=1.95f;shape.radiusThickness=.04f;shape.rotation=new Vector3(-90,0,0);
            var velocity=sparks.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;velocity.y=.25f;
            var fade=sparks.colorOverLifetime;fade.enabled=true;fade.color=Fade();
            var renderer=sparks.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=sparkMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            aura.gameObject.SetActive(false);
        }
        public static Gradient Fade()
        {
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.18f),new GradientAlphaKey(0,1)});return gradient;
        }
        Light MakeLight(string name,float range,float intensity)
        {
            var node=new GameObject(name);node.layer=BoardMeshFactory.Layer;node.transform.SetParent(transform,false);
            var light=node.AddComponent<Light>();light.type=LightType.Point;light.range=range;light.intensity=intensity;light.shadows=LightShadows.None;light.cullingMask=1<<BoardMeshFactory.Layer;light.enabled=false;return light;
        }
        LineRenderer Contour(Transform parent,string name,float radius,float width)
        {
            var node=new GameObject(name);node.layer=BoardMeshFactory.Layer;node.transform.SetParent(parent,false);var line=node.AddComponent<LineRenderer>();
            line.sharedMaterial=lineMaterial;line.useWorldSpace=false;line.loop=true;line.positionCount=6;line.widthMultiplier=width;line.numCornerVertices=3;line.alignment=LineAlignment.View;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;line.SetPosition(i,new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius));}
            return line;
        }
        public void Register(HexTile tile,Transform root)
        {
            Initialize();
            if(!tiles.TryGetValue(tile.id,out var glow)||glow.root!=root)
                tiles[tile.id]=glow=new TileGlow{root=root};
            glow.owner=tile.owner;glow.baseOwner=tile.baseOwner;
            if(tile.owner<0&&tile.baseOwner<0)return;
            if(glow.core==null)
            {
                var node=new GameObject("Turn neon contour").transform;node.SetParent(root,false);node.localPosition=Vector3.up*(BoardMeshFactory.Surface+.035f);node.gameObject.layer=BoardMeshFactory.Layer;
                glow.halo=Contour(node,"Turn halo",.787f,.12f);glow.core=Contour(node,"Turn core",.787f,.025f);
            }
        }
        public void Clear(){tiles.Clear();glowing.Clear();auraAnchor=null;if(aura!=null){aura.SetParent(transform,false);aura.gameObject.SetActive(false);}if(sparks!=null)sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);owner=-1;}
        public void Sync(GameManager game,Board3DScene world)
        {
            Initialize();int next=LeaderSkybox.PriorityOwner(game.State);
            if(next<0||next>=game.State.players.Count){owner=-1;baseLight.enabled=leaderLight.enabled=false;aura.gameObject.SetActive(false);return;}
            bool changed=owner!=next;owner=next;color=GrayboxUI.PlayerColor(owner);
            Transform platform=world.LeaderVisual(owner);Transform home=null;
            foreach(var tile in tiles.Values)if(tile.baseOwner==owner){home=tile.root;break;}
            if(home!=null){baseLight.transform.position=home.position+Vector3.up*2.2f;baseLight.enabled=true;}else baseLight.enabled=false;
            if(platform!=null)
            {
                auraAnchor=platform;aura.position=platform.position+Vector3.up*.09f;aura.gameObject.SetActive(true);
                leaderLight.transform.position=platform.position+Vector3.up*3;leaderLight.enabled=true;
                if(changed)sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=sparks.main;main.startColor=color;if(!sparks.isPlaying)sparks.Play();
            }
            else{aura.gameObject.SetActive(false);leaderLight.enabled=false;}
            glowing.Clear();
            foreach(var tile in tiles.Values)
            {
                if(tile.core==null)continue;
                bool active=tile.owner==owner||tile.baseOwner==owner;tile.core.enabled=tile.halo.enabled=active;
                if(active)glowing.Add(tile);
            }
            Animate(Time.unscaledTime);
        }
        public void Animate(float now)
        {
            if(lineMaterial==null||owner<0)return;
            if(auraAnchor!=null){aura.position=auraAnchor.position+Vector3.up*.09f;leaderLight.transform.position=auraAnchor.position+Vector3.up*3;}
            float pulse=.76f+.24f*Mathf.Sin(now*2.7f);
            foreach(var tile in glowing)
            {if(tile.core==null)continue;Tint(tile.core,color,.85f*pulse);Tint(tile.halo,color,.24f*pulse);}
            Tint(auraCore,color,pulse);Tint(auraHalo,color,.36f*pulse);
            baseLight.color=leaderLight.color=color;baseLight.intensity=2.6f*pulse;leaderLight.intensity=3.2f*pulse;
        }
        static void Tint(LineRenderer line,Color color,float alpha){color.a=alpha;line.startColor=line.endColor=color;}
        void Update(){float now=Time.unscaledTime;if(now<nextFrame)return;nextFrame=now+1f/30;Animate(now);}
        void OnDestroy(){if(aura!=null)BoardMeshFactory.Release(aura.gameObject);BoardMeshFactory.Release(lineMaterial);BoardMeshFactory.Release(sparkMaterial);}
    }
}
