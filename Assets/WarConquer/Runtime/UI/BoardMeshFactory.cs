using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WarConquer
{
    public sealed class BoardMeshFactory : IDisposable
    {
        public const int Layer=30;
        public const float Surface=.36f;
        public readonly Mesh Hex, Ring, Cylinder, Cone, Cube, Sphere;
        public readonly Material Lit;
        readonly List<Mesh> owned=new List<Mesh>();
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        public BoardMeshFactory()
        {
            bool urp=GraphicsSettings.currentRenderPipeline!=null;
            var source=Resources.Load<Material>(urp?"WarConquer/Materials/BoardURP":"WarConquer/Materials/BoardStandard");
            Lit=source!=null?new Material(source):new Material(Shader.Find(urp?"Universal Render Pipeline/Lit":"Standard"));
            Lit.name="WarConquer 3D shared material";Lit.enableInstancing=true;
            Hex=Prism(.8f,Surface,.035f,6);Ring=MakeRing(.799f,.744f);
            Cylinder=Prism(.5f,1,0,16);Cone=Taper(.5f,0,1,8);
            Cube=PrimitiveMesh(PrimitiveType.Cube);Sphere=PrimitiveMesh(PrimitiveType.Sphere);
        }
        static Mesh PrimitiveMesh(PrimitiveType type)
        {
            var temp=GameObject.CreatePrimitive(type);temp.SetActive(false);var mesh=temp.GetComponent<MeshFilter>().sharedMesh;Release(temp);return mesh;
        }
        sealed class Builder
        {
            public readonly List<Vector3> vertices=new List<Vector3>();
            public readonly List<int> indices=new List<int>();
            public void Tri(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});indices.AddRange(new[]{n,n+1,n+2});}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(a,b,c);Tri(a,c,d);}
            public Mesh Build(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
        static Vector3 Point(int i,int count,float radius,float y){float a=i*2*Mathf.PI/count;return new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);}
        public Mesh Prism(float radius,float height,float bevel,int sides)
        {
            var b=new Builder();
            for(int i=0;i<sides;i++)
            {
                var a=Point(i,sides,radius-bevel,height);var n=Point(i+1,sides,radius-bevel,height);
                var o=Point(i,sides,radius,height-bevel);var p=Point(i+1,sides,radius,height-bevel);
                var c=Point(i,sides,radius,0);var d=Point(i+1,sides,radius,0);
                b.Tri(Vector3.up*height,n,a);if(bevel>0)b.Quad(a,n,p,o);b.Quad(o,p,d,c);b.Tri(Vector3.zero,c,d);
            }
            var mesh=b.Build("Hexagonal solid");owned.Add(mesh);return mesh;
        }
        Mesh Taper(float bottom,float top,float height,int sides)
        {
            var b=new Builder();for(int i=0;i<sides;i++)
            {
                var a=Point(i,sides,bottom,0);var n=Point(i+1,sides,bottom,0);var c=Point(i,sides,top,height);var d=Point(i+1,sides,top,height);
                if(top>0)b.Tri(Vector3.up*height,d,c);b.Quad(c,d,n,a);b.Tri(Vector3.zero,a,n);
            }
            var mesh=b.Build("Low poly cone");owned.Add(mesh);return mesh;
        }
        Mesh MakeRing(float outer,float inner)
        {
            var b=new Builder();for(int i=0;i<6;i++)b.Quad(Point(i,6,outer,0),Point(i,6,inner,0),Point(i+1,6,inner,0),Point(i+1,6,outer,0));
            var mesh=b.Build("Hex selection ring");owned.Add(mesh);return mesh;
        }
        public GameObject Part(Transform parent,string name,Mesh mesh,Vector3 position,Vector3 scale,Color color,bool collider=false)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.layer=Layer;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=Lit;renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;Paint(renderer,color);
            if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;return go;
        }
        public void Paint(Renderer renderer,Color color,float glow=0)
        {properties.Clear();properties.SetColor("_BaseColor",color);properties.SetColor("_Color",color);properties.SetColor("_EmissionColor",color*glow);renderer.SetPropertyBlock(properties);}
        public Transform Group(Transform parent,string name,Vector3 position)
        {var go=new GameObject(name);go.layer=Layer;go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
        public void Castle(Transform parent,Color faction)
        {
            Color stone=new Color(.48f,.53f,.61f);
            Part(parent,"Plataforma",Hex,Vector3.zero,new Vector3(.69f,.36f,.69f),faction);
            Part(parent,"Fortaleza",Cube,new Vector3(0,.35f,0),new Vector3(.59f,.55f,.53f),stone);
            for(int i=0;i<4;i++)
            {
                float x=i%2==0?-.35f:.35f,z=i<2?-.31f:.31f;
                Part(parent,"Torre de base",Cylinder,new Vector3(x,.1f,z),new Vector3(.23f,.55f,.23f),stone);
                Part(parent,"Techo de torre",Cone,new Vector3(x,.65f,z),new Vector3(.3f,.26f,.3f),faction);
            }
            Part(parent,"Cristal del Líder",Cone,new Vector3(0,.63f,0),new Vector3(.36f,.62f,.36f),faction);
        }
        public void PieceModel(Transform parent,CardData card,Color faction)
        {
            bool fungus=card.leader=="ZUKGROK";Color pale=new Color(.8f,.81f,.72f),dark=new Color(.16f,.2f,.27f);
            bool passable=card.Has("Passable");
            Part(parent,"Peana de facción",card.IsStructure?Cube:Cylinder,card.IsStructure?new Vector3(0,.055f,0):Vector3.zero,new Vector3(.77f,.1f,.77f),Color.Lerp(faction,dark,.3f));
            if(card.IsStructure)
            {
                if(passable)
                {
                    Part(parent,"Terreno transitable",Cylinder,new Vector3(0,.1f,0),new Vector3(.63f,.045f,.63f),card.Has("Oasis")?new Color(.2f,.72f,.81f):faction);
                    for(int i=0;i<3;i++)Part(parent,"Señal de terreno",Cone,Point(i,3,.48f,.1f),new Vector3(.17f,.24f,.17f),faction);
                }
                else if(fungus)
                {
                    for(int i=0;i<3;i++){float h=i==0?.88f:.54f;var pos=Point(i,3,.22f,.1f);Part(parent,"Columna micelial",Cylinder,pos,new Vector3(.2f,h,.2f),pale);Part(parent,"Cúpula micelial",Sphere,pos+Vector3.up*h,new Vector3(.59f,.26f,.59f),faction);}
                }
                else
                {
                    Part(parent,"Edificio solar",Cube,new Vector3(0,.34f,0),new Vector3(.6f,.5f,.6f),pale);
                    Part(parent,"Obelisco",Cube,new Vector3(0,.77f,0),new Vector3(.22f,.65f,.22f),faction);
                    Part(parent,"Cima",Cone,new Vector3(0,1.09f,0),new Vector3(.33f,.25f,.33f),faction);
                    for(int i=0;i<4;i++)Part(parent,"Pilar",Cylinder,new Vector3(i%2==0?-.3f:.3f,.1f,i<2?-.3f:.3f),new Vector3(.13f,.62f,.13f),faction);
                }
                return;
            }
            bool animal=Array.Exists(card.subtypes,s=>s=="Animal");
            bool beast=animal||Array.Exists(card.subtypes,s=>s=="Bestia"||s=="Criatura");
            float hover=card.movementType=="Flying"?.48f:0;var body=Group(parent,"Miniatura "+card.name,new Vector3(0,hover,0));
            if(beast)
            {
                for(int i=0;i<4;i++)Part(body,"Pata",Cube,new Vector3(i%2==0?-.2f:.2f,.25f,i<2?-.22f:.22f),new Vector3(.14f,.32f,.14f),dark);
                Part(body,"Cuerpo de criatura",Sphere,new Vector3(0,.52f,0),new Vector3(.65f,.46f,.73f),faction);
                Part(body,"Cabeza",Sphere,new Vector3(0,.61f,-.3f),new Vector3(.39f,.38f,.37f),pale);
            }
            else
            {
                Part(body,"Pie izquierdo",Cube,new Vector3(-.13f,.18f,0),new Vector3(.17f,.32f,.22f),dark);
                Part(body,"Pie derecho",Cube,new Vector3(.13f,.18f,0),new Vector3(.17f,.32f,.22f),dark);
                Part(body,"Cuerpo",fungus?Cylinder:Cube,new Vector3(0,fungus?.2f:.48f,0),new Vector3(.43f,fungus?.51f:.42f,.33f),pale);
                Part(body,"Cabeza",Sphere,new Vector3(0,.81f,0),new Vector3(.35f,.35f,.35f),pale);
                if(!fungus)
                {
                    Part(body,"Armadura",Cube,new Vector3(0,.49f,-.19f),new Vector3(.42f,.29f,.08f),faction);
                    Part(body,"Escudo",Cylinder,new Vector3(-.33f,.36f,0),new Vector3(.13f,.5f,.35f),faction);
                    Part(body,"Lanza",Cylinder,new Vector3(.31f,.13f,0),new Vector3(.045f,1.03f,.045f),dark);
                    Part(body,"Punta",Cone,new Vector3(.31f,1.16f,0),new Vector3(.15f,.22f,.15f),faction);
                }
            }
            if(fungus)
            {
                Part(body,"Sombrero de hongo",Sphere,new Vector3(0,.91f,0),new Vector3(.88f,.3f,.77f),faction);
                for(int i=0;i<3;i++)Part(body,"Espora del sombrero",Sphere,Point(i,3,.23f,1.03f),Vector3.one*.1f,pale);
            }
            else if(animal)
            {
                for(int i=0;i<2;i++)Part(body,"Oreja",Sphere,new Vector3(i==0?-.12f:.12f,.87f,-.3f),new Vector3(.12f,card.Has("Token")?.43f:.18f,.12f),faction);
                Part(body,"Cola",Sphere,new Vector3(0,.55f,.4f),new Vector3(.17f,.14f,card.Has("Token")?.17f:.42f),faction);
            }
            else Part(body,"Casco solar",Sphere,new Vector3(0,.93f,0),new Vector3(.41f,.2f,.39f),faction);
            for(int i=0;i<2;i++)Part(body,"Ojo",Sphere,new Vector3(i==0?-.085f:.085f,beast?.67f:.83f,beast?-.48f:-.16f),Vector3.one*.065f,dark);
            if(hover>0)for(int i=0;i<2;i++){var wing=Part(body,"Ala",Sphere,new Vector3(i==0?-.43f:.43f,.65f,.05f),new Vector3(.64f,.08f,.4f),faction);wing.transform.localRotation=Quaternion.Euler(0,0,i==0?-20:20);}
            if(card.health>=10)body.localScale=Vector3.one*1.13f;
        }
        public void BiomeModel(Transform parent,HexTile tile)
        {
            if(tile.biome==Biome.Neutral||tile.baseOwner>=0)return;
            var corner=new Vector3(.38f,Surface,.33f);
            if(tile.biome==Biome.Forest)
            {
                Part(parent,"Tronco",Cylinder,corner,new Vector3(.08f,.3f,.08f),new Color(.28f,.23f,.19f));
                Part(parent,"Árbol",Cone,corner+Vector3.up*.16f,new Vector3(.42f,.53f,.42f),new Color(.13f,.4f,.26f));
            }
            else if(tile.biome==Biome.Swamp)
                Part(parent,"Agua del pantano",Cylinder,new Vector3(.27f,Surface+.006f,.32f),new Vector3(.51f,.014f,.37f),new Color(.19f,.41f,.43f));
            else if(tile.biome==Biome.Desert)
                Part(parent,"Duna",Sphere,corner,new Vector3(.49f,.18f,.36f),new Color(.79f,.6f,.28f));
            else
            {
                Color color=tile.biome==Biome.Tundra?new Color(.74f,.89f,.95f):tile.biome==Biome.Volcanic?new Color(.29f,.19f,.19f):new Color(.37f,.36f,.41f);
                Part(parent,"Roca",Cone,corner,new Vector3(.37f,.31f,.34f),color);
            }
        }
        public TextMesh Label(Transform parent,string name,Vector3 position,float size,Color color)
        {
            var group=Group(parent,name,position);var text=group.gameObject.AddComponent<TextMesh>();text.font=GrayboxUI.Font;text.fontSize=64;text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=color;
            var renderer=text.GetComponent<MeshRenderer>();renderer.sharedMaterial=text.font.material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return text;
        }
        public static void Release(UnityEngine.Object value)
        {if(value==null)return;if(value is GameObject go)go.SetActive(false);if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
        public void Dispose(){foreach(var mesh in owned)Release(mesh);owned.Clear();Release(Lit);}
    }
    public sealed class Hex3DTarget : MonoBehaviour { public int tileId; }
}
