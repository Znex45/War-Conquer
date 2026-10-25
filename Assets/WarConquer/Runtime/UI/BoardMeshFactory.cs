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
        public readonly Mesh Hex, Ring, Cylinder, Cone, Cube, Sphere, Vortex;
        public readonly Mesh Carved, Crystal, Petal, MushroomCap, Bell;
        public readonly Material Lit;
        readonly List<Mesh> owned=new List<Mesh>();
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        public BoardMeshFactory()
        {
            bool urp=GraphicsSettings.currentRenderPipeline!=null;
            var source=Resources.Load<Material>(urp?"WarConquer/Materials/BoardURP":"WarConquer/Materials/BoardStandard");
            Lit=source!=null?new Material(source):new Material(Shader.Find(urp?"Universal Render Pipeline/Lit":"Standard"));
            Lit.name="WarConquer 3D shared material";Lit.enableInstancing=true;
            Hex=Prism(.8f,Surface,.035f,6);Ring=MakeRing(.799f,.744f);Vortex=MakeVortex();
            Cylinder=Prism(.5f,1,0,16);Cone=Taper(.5f,0,1,8);
            Cube=PrimitiveMesh(PrimitiveType.Cube);Sphere=PrimitiveMesh(PrimitiveType.Sphere);
            Carved=Profile("Faceted miniature",new[]{new Vector2(0,-.5f),new Vector2(.32f,-.4f),new Vector2(.48f,-.18f),new Vector2(.5f,.12f),new Vector2(.36f,.4f),new Vector2(0,.5f)},10);
            Crystal=Profile("Cut crystal",new[]{new Vector2(0,-.5f),new Vector2(.32f,-.2f),new Vector2(.36f,.18f),new Vector2(0,.5f)},6);
            Petal=Profile("Carved leaf",new[]{new Vector2(0,-.5f),new Vector2(.3f,-.15f),new Vector2(.22f,.18f),new Vector2(0,.5f)},4);
            MushroomCap=Profile("Organic mushroom cap",new[]{new Vector2(0,-.08f),new Vector2(.42f,-.08f),new Vector2(.5f,0),new Vector2(.4f,.15f),new Vector2(.2f,.29f),new Vector2(0,.34f)},12);
            Bell=Profile("Sculpted tunic",new[]{new Vector2(.48f,0),new Vector2(.5f,.08f),new Vector2(.29f,.74f),new Vector2(.32f,1)},10);
        }
        Mesh Profile(string name,Vector2[] profile,int sides)
        {
            var b=new Builder();
            for(int row=0;row<profile.Length-1;row++)for(int i=0;i<sides;i++)
            {
                var a=profile[row];var c=profile[row+1];
                b.Quad(Point(i,sides,a.x,a.y),Point(i,sides,c.x,c.y),Point(i+1,sides,c.x,c.y),Point(i+1,sides,a.x,a.y));
            }
            // Open profile ends are capped, so tunics have no visible holes from below.
            if(profile[0].x>0)for(int i=0;i<sides;i++)b.Tri(Vector3.up*profile[0].y,Point(i,sides,profile[0].x,profile[0].y),Point(i+1,sides,profile[0].x,profile[0].y));
            var last=profile[profile.Length-1];if(last.x>0)for(int i=0;i<sides;i++)b.Tri(Vector3.up*last.y,Point(i+1,sides,last.x,last.y),Point(i,sides,last.x,last.y));
            var result=b.Build(name);owned.Add(result);return result;
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
        Mesh MakeVortex()
        {
            var b=new Builder();
            for(int arm=0;arm<3;arm++)for(int i=0;i<32;i++)
            {
                Vector3 Edge(float t,float side)
                {
                    float angle=arm*Mathf.PI*2/3+t*4.4f;
                    float radius=.12f+t*.51f+side*(.012f+.025f*t);
                    return new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
                }
                float a=i/32f,n=(i+1)/32f;b.Quad(Edge(a,1),Edge(a,-1),Edge(n,-1),Edge(n,1));
            }
            var mesh=b.Build("Espiral de acreción compartida");owned.Add(mesh);return mesh;
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
            CardPieceModels.Build(this,parent,card,faction);
        }
        public BoardBiomeVisuals.Habitat BiomeModel(Transform parent,HexTile tile)
        {return ImportedBiomeVisuals.TryBuild(parent,tile,out var imported)?imported:BoardBiomeVisuals.Build(this,parent,tile);}
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
