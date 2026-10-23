#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace WarConquer.Editor
{
    public static class HexBiomeCrop
    {
        public const float Radius=.795f;
        struct Vertex
        {
            public Vector3 p,n;public Vector2 uv;public Color color;
            public static Vertex Mix(Vertex a,Vertex b,float t)=>new Vertex{p=Vector3.LerpUnclamped(a.p,b.p,t),n=Vector3.LerpUnclamped(a.n,b.n,t).normalized,uv=Vector2.LerpUnclamped(a.uv,b.uv,t),color=Color.LerpUnclamped(a.color,b.color,t)};
        }
        static float Distance(Vector3 p,int edge)
        {
            float angle=(edge+.5f)*Mathf.PI/3;
            return Mathf.Cos(angle)*p.x+Mathf.Sin(angle)*p.z-Radius*Mathf.Cos(Mathf.PI/6);
        }
        static List<Vertex> Clip(List<Vertex> input,int edge)
        {
            var output=new List<Vertex>();if(input.Count==0)return output;
            var a=input[input.Count-1];float da=Distance(a.p,edge);
            foreach(var b in input)
            {
                float db=Distance(b.p,edge);
                if((da<=0)!=(db<=0))output.Add(Vertex.Mix(a,b,da/(da-db)));
                if(db<=0)output.Add(b);a=b;da=db;
            }
            return output;
        }
        static Mesh SaveMesh(string path,List<Vertex> vertices,List<int>[] submeshes)
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.indexFormat=IndexFormat.UInt32;mesh.name=Path.GetFileNameWithoutExtension(path);
            mesh.SetVertices(vertices.Select(v=>v.p).ToList());mesh.SetNormals(vertices.Select(v=>v.n).ToList());mesh.SetUVs(0,vertices.Select(v=>v.uv).ToList());mesh.SetColors(vertices.Select(v=>v.color).ToList());
            mesh.subMeshCount=submeshes.Length;for(int i=0;i<submeshes.Length;i++)mesh.SetTriangles(submeshes[i],i);
            mesh.RecalculateBounds();mesh.RecalculateTangents();EditorUtility.SetDirty(mesh);return mesh;
        }
        public static void Build(GameObject root,GameObject source,Bounds bounds,string biome,string output)
        {
            string folder=output+"/Meshes/"+biome;Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            // Cover the hex footprint with a central crop; long rectangles retain their proportions.
            float horizontal=Mathf.Max(1.6f/Mathf.Max(.001f,bounds.size.x),1.385641f/Mathf.Max(.001f,bounds.size.z))*1.08f;
            float vertical=Mathf.Min(horizontal,.45f/Mathf.Max(.001f,bounds.size.y));
            Material ground=null;double largest=0;int index=0;
            foreach(var filter in source.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<MeshRenderer>();var mesh=filter.sharedMesh;if(renderer==null||mesh==null)continue;
                var positions=mesh.vertices;var normals=mesh.normals;var uvs=mesh.uv;var colors=mesh.colors;
                var transformed=new Vertex[positions.Length];
                for(int i=0;i<positions.Length;i++)
                {
                    var p=filter.transform.TransformPoint(positions[i]);p=new Vector3((p.x-bounds.center.x)*horizontal,BoardMeshFactory.Surface+.008f+(p.y-bounds.min.y)*vertical,(p.z-bounds.center.z)*horizontal);
                    var n=normals.Length==positions.Length?filter.transform.localToWorldMatrix.inverse.transpose.MultiplyVector(normals[i]):Vector3.up;
                    n=new Vector3(n.x/horizontal,n.y/vertical,n.z/horizontal).normalized;
                    transformed[i]=new Vertex{p=p,n=n,uv=uvs.Length==positions.Length?uvs[i]:Vector2.zero,color=colors.Length==positions.Length?colors[i]:Color.white};
                }
                var vertices=new List<Vertex>();var triangles=new List<int>[mesh.subMeshCount];var materials=renderer.sharedMaterials;
                for(int sub=0;sub<mesh.subMeshCount;sub++)
                {
                    triangles[sub]=new List<int>();var sourceIndices=mesh.GetTriangles(sub);double area=0;
                    for(int i=0;i<sourceIndices.Length;i+=3)
                    {
                        var a=transformed[sourceIndices[i]];var b=transformed[sourceIndices[i+1]];var c=transformed[sourceIndices[i+2]];
                        // Prefer broad, low ground surfaces when choosing the continuation material.
                        if(Mathf.Max(a.p.y,Mathf.Max(b.p.y,c.p.y))<BoardMeshFactory.Surface+.18f)
                            area+=Math.Abs(Vector3.Cross(b.p-a.p,c.p-a.p).y);
                        var polygon=new List<Vertex>{a,b,c};for(int edge=0;edge<6&&polygon.Count>0;edge++)polygon=Clip(polygon,edge);
                        for(int j=1;j+1<polygon.Count;j++)
                        {int offset=vertices.Count;vertices.Add(polygon[0]);vertices.Add(polygon[j]);vertices.Add(polygon[j+1]);triangles[sub].AddRange(new[]{offset,offset+1,offset+2});}
                    }
                    if(sub<materials.Length&&area>largest&&materials[sub]!=null){largest=area;ground=materials[sub];}
                }
                if(vertices.Count==0)continue;
                var part=new GameObject("Hex crop "+index);part.transform.SetParent(root.transform,false);part.layer=BoardMeshFactory.Layer;
                part.AddComponent<MeshFilter>().sharedMesh=SaveMesh(folder+"/Surface_"+index+".asset",vertices,triangles);
                var r=part.AddComponent<MeshRenderer>();r.sharedMaterials=materials;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;index++;
            }
            if(index==0)throw new Exception("Crop has no geometry: "+biome);
            if(ground==null)ground=root.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            // A matching textured hex closes gaps in irregular/round dioramas right up to all six edges.
            var cap=new List<Vertex>();var indices=new List<int>();float y=BoardMeshFactory.Surface+.006f;
            for(int edge=0;edge<6;edge++)
            {
                Vector3 a=new Vector3(Mathf.Cos(edge*Mathf.PI/3)*Radius,y,Mathf.Sin(edge*Mathf.PI/3)*Radius);
                Vector3 b=new Vector3(Mathf.Cos((edge+1)*Mathf.PI/3)*Radius,y,Mathf.Sin((edge+1)*Mathf.PI/3)*Radius);
                foreach(var p in new[]{new Vector3(0,y,0),b,a}){indices.Add(cap.Count);cap.Add(new Vertex{p=p,n=Vector3.up,uv=new Vector2(p.x/1.6f+.5f,p.z/1.385641f+.5f),color=Color.white});}
            }
            var foundation=new GameObject("Full hex terrain");foundation.transform.SetParent(root.transform,false);foundation.layer=BoardMeshFactory.Layer;
            foundation.AddComponent<MeshFilter>().sharedMesh=SaveMesh(folder+"/Foundation.asset",cap,new[]{indices});
            var floor=foundation.AddComponent<MeshRenderer>();floor.sharedMaterial=ground;floor.shadowCastingMode=ShadowCastingMode.Off;
            UnityEngine.Object.DestroyImmediate(source);
        }
        [MenuItem("War & Conquer/Entornos/Ajustar terrenos al hexagono %&h")]
        public static void Rebuild(){EnvironmentAssets.Build();EnvironmentReview.Run();}
    }
}
#endif
