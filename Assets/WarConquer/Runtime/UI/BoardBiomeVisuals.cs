using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WarConquer
{
    // One animation driver for the board. Slabs keep their existing identities/biomes.
    // Geometry, particles and materials are created on biome changes, never per frame.
    public sealed class BoardBiomeVisuals : MonoBehaviour
    {
        public sealed class Habitat
        {
            public Transform root, moving, water, fauna;
            public Vector3 faunaOrigin;
            public ParticleSystem dust;
            public Renderer ground;
            public Color from, to;
            public float started, offset;
            public Biome biome;
            public bool emerging;
        }
        readonly Dictionary<int,Habitat> active=new Dictionary<int,Habitat>();
        BoardMeshFactory mesh;
        Material particles,waterMaterial;
        float nextFrame;
        public int HabitatCount=>active.Count;
        public void Initialize(BoardMeshFactory factory)
        {
            mesh=factory;
            // Shared transparent material: no per-slab material instances or lights.
            particles=new Material(Resources.Load<Shader>("WarConquer/Materials/TerrainTransparent"));
            particles.name="Partículas de terreno compartidas";particles.SetFloat("_SoftParticle",1);
            waterMaterial=new Material(particles);waterMaterial.name="Agua turbia compartida";waterMaterial.SetFloat("_SoftParticle",0);
            waterMaterial.SetColor("_BaseColor",new Color(.16f,.34f,.27f,.58f));
        }
        public void Clear(){active.Clear();}
        public void Sync(HexTile tile,Transform root,Renderer ground,bool animate,Color previous)
        {
            active.Remove(tile.id);
            var h=mesh.BiomeModel(root,tile);h.ground=ground;h.from=previous;
            h.to=tile.biome==Biome.Neutral?GrayboxUI.TerritoryColor(tile.territory):GrayboxUI.BiomeColor(tile.biome);
            h.started=Time.unscaledTime;h.emerging=animate;h.offset=tile.id*.73f;
            if(h.water!=null){var renderer=h.water.GetComponent<Renderer>();renderer.sharedMaterial=waterMaterial;renderer.SetPropertyBlock(null);}
            if(animate)
            {
                h.dust=MakeParticles(root,"TerraformDustVFX",new Color(.72f,.67f,.54f,.45f),false,tile.id);
                h.dust.Emit(22);h.root.localScale=new Vector3(1,.03f,1);h.root.localPosition=Vector3.up*BoardMeshFactory.Surface*.97f;
            }
            if(tile.biome!=Biome.Neutral)
            {
                var color=tile.biome==Biome.Tundra?new Color(.85f,.95f,1,.6f):tile.biome==Biome.Void?new Color(.6f,.22f,.9f,.55f):tile.biome==Biome.Volcanic?new Color(1,.32f,.06f,.55f):tile.biome==Biome.Swamp?new Color(.4f,.65f,.48f,.3f):new Color(.65f,.6f,.43f,.25f);
                MakeParticles(root,"Ambiente "+tile.biome,color,true,tile.id,tile.biome);
            }
            active[tile.id]=h;
        }
        ParticleSystem MakeParticles(Transform root,string name,Color color,bool loop,int seed,Biome biome=Biome.Neutral)
        {
            var obj=mesh.Group(root,name,Vector3.up*(BoardMeshFactory.Surface+(loop&&biome==Biome.Tundra?.5f:.04f)));var ps=obj.gameObject.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.useAutoRandomSeed=false;ps.randomSeed=(uint)(seed+923);
            var main=ps.main;main.loop=loop;main.playOnAwake=false;main.duration=1;
            main.startLifetime=loop?1.8f:.85f;main.startSpeed=loop?.06f:.24f;main.startSize=loop?.035f:.12f;main.startColor=color;
            main.maxParticles=loop?8:22;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=ps.emission;emission.rateOverTime=loop?3:0;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=.52f;shape.rotation=new Vector3(-90,0,0);
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;velocity.y=loop?(biome==Biome.Tundra?-.12f:.06f):.18f;velocity.x=loop&&biome==Biome.Desert?.12f:0;
            var fade=ps.colorOverLifetime;fade.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.18f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particles;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            ps.Play();return ps;
        }
        void Update()
        {
            float now=Time.unscaledTime;if(now<nextFrame)return;nextFrame=now+1f/30;
            foreach(var h in active.Values)
            {
                if(h.root==null)continue;
                if(h.emerging)
                {
                    float t=Mathf.Clamp01((now-h.started)/1.05f),ease=t*t*(3-2*t);
                    float height=Mathf.Lerp(.03f,1,ease);h.root.localScale=new Vector3(1,height,1);h.root.localPosition=Vector3.up*BoardMeshFactory.Surface*(1-height);mesh.Paint(h.ground,Color.Lerp(h.from,h.to,ease));
                    if(t>=1){h.emerging=false;if(h.dust!=null){BoardMeshFactory.Release(h.dust.gameObject);h.dust=null;}}
                }
                float wave=Mathf.Sin(now*1.5f+h.offset);
                if(h.moving!=null)
                {
                    if(h.biome==Biome.Void){h.moving.localRotation=Quaternion.Euler(0,now*24+h.offset,0);h.moving.localScale=Vector3.one*(1+.055f*wave);}
                    else {float height=1+.12f*wave;h.moving.localScale=new Vector3(1,height,1);h.moving.localPosition=Vector3.up*BoardMeshFactory.Surface*(1-height);}
                }
                if(h.water!=null)h.water.localPosition=new Vector3(.22f,BoardMeshFactory.Surface+.012f+wave*.004f,.22f);
                if(h.fauna!=null){h.fauna.localPosition=h.faunaOrigin+new Vector3(Mathf.Sin(now*.5f+h.offset)*.07f,0,Mathf.Cos(now*.5f+h.offset)*.03f);h.fauna.localRotation=Quaternion.Euler(0,Mathf.Sin(now*.7f+h.offset)*16,0);}
            }
        }
        void OnDestroy(){BoardMeshFactory.Release(particles);BoardMeshFactory.Release(waterMaterial);}

        // Small peripheral scenery keeps the center free for units and selection markers.
        public static Habitat Build(BoardMeshFactory m,Transform root,HexTile t)
        {
            var h=new Habitat{root=root,biome=t.biome};
            if(t.biome==Biome.Neutral)return h;
            float surface=BoardMeshFactory.Surface;Color bark=new Color(.28f,.2f,.13f);
            GameObject Part(Transform parent,string name,Mesh shape,Vector3 pos,Vector3 scale,Color color)
            {var p=m.Part(parent,name,shape,pos,scale,color);var r=p.GetComponent<Renderer>();r.shadowCastingMode=ShadowCastingMode.Off;return p;}
            void Tree(Vector3 p,Color foliage,bool dry=false,bool cold=false)
            {
                Part(root,"Tronco",m.Cylinder,p,new Vector3(.045f,.23f,.045f),bark);
                if(dry)
                {for(int i=0;i<2;i++){var b=Part(root,"Rama seca",m.Cylinder,p+Vector3.up*.12f,new Vector3(.025f,.17f,.025f),bark);b.transform.localRotation=Quaternion.Euler(0,0,i==0?40:-40);}}
                else {Part(root,cold?"Pino helado":"Copa estacional",cold?m.Cone:m.Sphere,p+Vector3.up*.24f,new Vector3(.24f,cold?.3f:.25f,.24f),foliage);
                    Part(root,cold?"Rama nevada":"Copa secundaria",cold?m.Cone:m.Sphere,p+new Vector3(.07f,.17f,.035f),new Vector3(.19f,.18f,.19f),Color.Lerp(foliage,Color.white,cold?.35f:.12f));
                    Part(root,cold?"Cima helada":"Copa iluminada",cold?m.Cone:m.Sphere,p+new Vector3(-.055f,.25f,-.035f),new Vector3(.17f,.18f,.17f),Color.Lerp(foliage,Color.white,.2f));}
            }
            Vector3 corner=new Vector3(.42f,surface,-.19f);
            switch(t.biome)
            {
                case Biome.Forest:
                    Color[] seasons={new Color(.26f,.63f,.25f),new Color(.1f,.37f,.19f),new Color(.85f,.43f,.12f),new Color(.71f,.67f,.23f)};
                    Tree(corner,seasons[t.id%4]);Tree(new Vector3(-.38f,surface,.28f),seasons[(t.id+1)%4]);
                    Part(root,"Vegetación",m.Sphere,new Vector3(.25f,surface,.4f),new Vector3(.3f,.11f,.18f),seasons[(t.id+2)%4]);break;
                case Biome.Wasteland:case Biome.AshLand:
                    Tree(corner,Color.gray,true);Tree(new Vector3(-.4f,surface,.25f),Color.gray,true);
                    Part(root,"Ceniza",m.Sphere,new Vector3(.24f,surface,.36f),new Vector3(.34f,.055f,.2f),new Color(.38f,.36f,.31f));break;
                case Biome.Volcanic:
                    Part(root,"Cráter basáltico",m.Cone,corner,new Vector3(.38f,.16f,.36f),new Color(.18f,.13f,.13f));
                    h.moving=m.Group(root,"Lava viva",Vector3.zero);
                    Part(h.moving,"Lava del cráter",m.Cylinder,corner+Vector3.up*.13f,new Vector3(.14f,.016f,.14f),new Color(1,.34f,.025f));
                    for(int i=0;i<3;i++){var crack=Part(h.moving,"Grieta de lava",m.Cube,new Vector3(-.4f+i*.1f,surface+.013f,.23f+i*.055f),new Vector3(.18f,.014f,.025f),new Color(1,.25f,.015f));crack.transform.localRotation=Quaternion.Euler(0,i%2==0?25:-35,0);}break;
                case Biome.Swamp:
                    Part(root,"Lecho y musgo",m.Sphere,new Vector3(.22f,surface-.005f,.22f),new Vector3(.65f,.016f,.5f),new Color(.15f,.26f,.13f));
                    h.water=Part(root,"Agua turbia translúcida",m.Cylinder,new Vector3(.22f,surface+.012f,.22f),new Vector3(.62f,.012f,.47f),new Color(.15f,.35f,.3f,.55f)).transform;
                    for(int i=0;i<2;i++){var p=new Vector3(-.43f+i*.12f,surface,.16f);Part(root,"Tallo húmedo",m.Cylinder,p,new Vector3(.035f,.13f,.035f),new Color(.55f,.58f,.3f));Part(root,"Hongo",m.Sphere,p+Vector3.up*.13f,new Vector3(.13f,.07f,.13f),new Color(.6f,.36f,.55f));}
                    Part(root,"Juncos",m.Cone,new Vector3(.45f,surface,-.28f),new Vector3(.12f,.24f,.12f),new Color(.25f,.4f,.15f));
                    if(t.id%3==0){h.fauna=m.Group(root,"Fauna ambiental · tortuga",new Vector3(.26f,surface+.025f,.28f));Part(h.fauna,"Caparazón",m.Sphere,Vector3.zero,new Vector3(.13f,.055f,.1f),new Color(.3f,.4f,.17f));Part(h.fauna,"Cabeza",m.Sphere,new Vector3(.08f,0,0),Vector3.one*.045f,new Color(.5f,.55f,.27f));}
                    else if(t.id%3==1){h.fauna=m.Group(root,"Fauna ambiental · pez",new Vector3(.2f,surface+.008f,.26f));Part(h.fauna,"Pez sumergido",m.Sphere,Vector3.zero,new Vector3(.13f,.012f,.045f),new Color(.52f,.4f,.17f));Part(h.fauna,"Cola",m.Cone,new Vector3(-.08f,0,0),new Vector3(.05f,.045f,.05f),new Color(.5f,.43f,.17f));}break;
                case Biome.Desert:
                    Part(root,"Duna de arena",m.Sphere,corner,new Vector3(.4f,.13f,.3f),new Color(.79f,.62f,.32f));
                    Part(root,"Duna menor",m.Sphere,new Vector3(-.4f,surface,.28f),new Vector3(.3f,.08f,.2f),new Color(.84f,.68f,.39f));
                    if(t.id%4==0){h.fauna=m.Group(root,"Fauna ambiental · serpiente",new Vector3(.38f,surface+.02f,.26f));for(int i=0;i<4;i++)Part(h.fauna,"Curva de serpiente",m.Sphere,new Vector3(i*.045f,0,Mathf.Sin(i*2)*.025f),new Vector3(.065f,.028f,.035f),new Color(.4f,.3f,.15f));}
                    else if(t.id%4==1){h.fauna=m.Group(root,"Fauna ambiental · escorpión",new Vector3(.38f,surface+.02f,.26f));Part(h.fauna,"Cuerpo",m.Sphere,Vector3.zero,new Vector3(.1f,.03f,.06f),new Color(.32f,.23f,.13f));for(int i=0;i<2;i++)Part(h.fauna,"Pinza",m.Sphere,new Vector3(.06f,0,i==0?-.04f:.04f),new Vector3(.05f,.025f,.04f),bark);Part(h.fauna,"Cola alzada",m.Cone,new Vector3(-.05f,0,0),new Vector3(.025f,.09f,.025f),bark);}break;
                case Biome.Void:
                    // The dark surface covers the entire slab; only the inner accretion disk rotates.
                    Part(root,"Horizonte negro hexagonal",m.Hex,new Vector3(0,surface+.001f,0),new Vector3(.98f,.025f,.98f),new Color(.012f,.005f,.025f));
                    Part(root,"Límite violeta de Void",m.Ring,new Vector3(0,surface+.016f,0),Vector3.one*.97f,new Color(.32f,.08f,.57f));
                    h.moving=m.Group(root,"Distorsión cósmica",new Vector3(0,surface+.024f,0));
                    Part(h.moving,"Disco de acreción",m.Vortex,Vector3.zero,Vector3.one,new Color(.52f,.15f,.85f));
                    Part(h.moving,"Filamentos luminosos",m.Vortex,Vector3.up*.004f,new Vector3(.86f,1,.86f),new Color(.8f,.43f,1));
                    Part(h.moving,"Núcleo oscuro",m.Cylinder,Vector3.up*.006f,new Vector3(.45f,.014f,.45f),new Color(.002f,.001f,.008f));
                    for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;Part(h.moving,"Energía orbital",m.Sphere,new Vector3(Mathf.Cos(angle)*.57f,.025f,Mathf.Sin(angle)*.57f),new Vector3(.045f,.025f,.045f),new Color(.8f,.44f,1));}break;
                case Biome.Tundra:
                    Part(root,"Nieve",m.Sphere,corner,new Vector3(.4f,.065f,.3f),new Color(.9f,.96f,1));
                    Tree(new Vector3(-.4f,surface,.25f),new Color(.58f,.77f,.78f),false,true);
                    Part(root,"Roca congelada",m.Cone,new Vector3(.38f,surface,.28f),new Vector3(.2f,.26f,.18f),new Color(.5f,.83f,.94f));break;
            }
            // Deterministic detail placement never consumes the gameplay RNG.
            for(int i=0;i<5;i++)
            {
                float angle=(i*1.256637f+t.id*.31f),radius=.47f+(i%2)*.09f;
                var p=new Vector3(Mathf.Cos(angle)*radius,surface+.008f,Mathf.Sin(angle)*radius);
                if(t.biome==Biome.Forest)
                {
                    Part(root,"Hierba",m.Cone,p,new Vector3(.05f,.075f,.05f),new Color(.32f,.51f,.18f));
                    if(i%2==0)Part(root,"Flor silvestre",m.Sphere,p+Vector3.up*.055f,new Vector3(.045f,.025f,.045f),i==0?new Color(.96f,.74f,.34f):new Color(.7f,.47f,.76f));
                }
                else if(t.biome==Biome.Wasteland||t.biome==Biome.AshLand)
                {
                    var crack=Part(root,"Grieta seca",m.Cube,p,new Vector3(.15f,.007f,.016f),new Color(.19f,.16f,.14f));crack.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg+25,0);
                    Part(root,"Piedra erosionada",m.Sphere,p+new Vector3(.03f,.012f,.035f),new Vector3(.06f,.045f,.05f),new Color(.47f,.44f,.37f));
                }
                else if(t.biome==Biome.Volcanic)
                {
                    Part(root,"Escoria basáltica",m.Cone,p,new Vector3(.08f,.06f,.07f),new Color(.18f,.15f,.16f));
                    var vein=Part(h.moving,"Vena incandescente",m.Cube,p+Vector3.up*.009f,new Vector3(.16f,.012f,.019f),new Color(1,.45f,.045f));vein.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
                }
                else if(t.biome==Biome.Desert)
                {
                    var ridge=Part(root,"Ondulación de arena",m.Sphere,p,new Vector3(.18f,.024f,.045f),new Color(.85f,.69f,.42f));ridge.transform.localRotation=Quaternion.Euler(0,35,0);
                    if(i<2)Part(root,"Guijarro desértico",m.Sphere,p+Vector3.up*.018f,new Vector3(.045f,.04f,.04f),new Color(.54f,.38f,.21f));
                }
                else if(t.biome==Biome.Swamp)
                {
                    Part(root,"Musgo ribereño",m.Sphere,p,new Vector3(.12f,.025f,.08f),new Color(.24f,.39f,.16f));
                    if(i<3){Part(root,"Junco fino",m.Cylinder,p,new Vector3(.012f,.14f,.012f),new Color(.39f,.49f,.18f));Part(root,"Espiga",m.Sphere,p+Vector3.up*.13f,new Vector3(.022f,.055f,.022f),new Color(.39f,.25f,.12f));}
                }
                else if(t.biome==Biome.Tundra)
                {
                    Part(root,"Cristal de hielo",m.Cone,p,new Vector3(.055f,.1f+i*.015f,.055f),new Color(.65f,.88f,1));
                    Part(root,"Nieve acumulada",m.Sphere,p,new Vector3(.12f,.025f,.1f),new Color(.88f,.95f,1));
                }
            }
            if(h.fauna!=null)h.faunaOrigin=h.fauna.localPosition;
            return h;
        }
    }
}
