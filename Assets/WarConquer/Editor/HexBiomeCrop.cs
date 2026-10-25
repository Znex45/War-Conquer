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
        static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
        public static float Alignment(Vector3[] points,out Quaternion rotation,out Bounds oriented)
        {
            var ordered=points.Select(p=>new Vector2(p.x,p.z)).Distinct().OrderBy(p=>p.x).ThenBy(p=>p.y).ToArray();
            var hull=new List<Vector2>();
            foreach(var point in ordered){while(hull.Count>1&&Cross(hull[hull.Count-2],hull[hull.Count-1],point)<=0)hull.RemoveAt(hull.Count-1);hull.Add(point);}
            int lower=hull.Count;
            for(int i=ordered.Length-2;i>=0;i--){var point=ordered[i];while(hull.Count>lower&&Cross(hull[hull.Count-2],hull[hull.Count-1],point)<=0)hull.RemoveAt(hull.Count-1);hull.Add(point);}
            float best=float.MaxValue;rotation=Quaternion.identity;oriented=new Bounds();
            for(int sample=0;sample<600;sample++)
            {
                var turn=Quaternion.Euler(0,sample*.1f,0);var rotated=hull.Select(p=>turn*new Vector3(p.x,0,p.y)).ToArray();
                var box=new Bounds(rotated[0],Vector3.zero);foreach(var point in rotated)box.Encapsulate(point);
                float extent=0;
                foreach(var point in rotated)for(int edge=0;edge<6;edge++){float angle=(edge+.5f)*Mathf.PI/3;var delta=point-box.center;extent=Mathf.Max(extent,Mathf.Cos(angle)*delta.x+Mathf.Sin(angle)*delta.z);}
                if(extent<best){best=extent;rotation=turn;oriented=box;}
            }
            var full=new Bounds(rotation*points[0],Vector3.zero);foreach(var point in points)full.Encapsulate(rotation*point);oriented=full;
            // Enlarge uniformly until the source footprint covers the target corners.
            // Clip the perimeter to six straight sides; never stretch either axis.
            float cover=0;
            var finalRotation=rotation;var footprint=hull.Select(p=>finalRotation*new Vector3(p.x,0,p.y)-new Vector3(full.center.x,0,full.center.z)).ToArray();
            for(int i=0;i<footprint.Length-1;i++)
            {
                var a=footprint[i];var b=footprint[i+1];var normal=new Vector3(b.z-a.z,0,a.x-b.x).normalized;
                float distance=Vector3.Dot(normal,a);if(distance<.00001f)continue;
                for(int corner=0;corner<6;corner++){float angle=corner*Mathf.PI/3;cover=Mathf.Max(cover,Vector3.Dot(normal,new Vector3(Mathf.Cos(angle)*Radius,0,Mathf.Sin(angle)*Radius))/distance);}
            }
            return Mathf.Max(cover*1.002f,Radius*Mathf.Cos(Mathf.PI/6)/Mathf.Max(.001f,best));
        }
        public static void Build(GameObject root,GameObject source,Bounds bounds,string biome,string output)
        {
            string folder=output+"/Meshes/"+biome;Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            // Align the authored footprint, resize uniformly, then trim its perimeter to the tile.
            var sourcePoints=source.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v))).ToArray();
            float horizontal=Alignment(sourcePoints,out var rotation,out var alignedBounds),vertical=horizontal;
            bounds=alignedBounds;
            Debug.Log("BIOME_ALIGNED "+biome+" yaw="+rotation.eulerAngles.y+" uniform="+horizontal+" fitted="+(bounds.size*horizontal));
            Material ground=null;double largest=0;int index=0;
            foreach(var filter in source.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<MeshRenderer>();var mesh=filter.sharedMesh;if(renderer==null||mesh==null)continue;
                var positions=mesh.vertices;var normals=mesh.normals;var uvs=mesh.uv;var colors=mesh.colors;
                var transformed=new Vertex[positions.Length];
                for(int i=0;i<positions.Length;i++)
                {
                    var p=rotation*filter.transform.TransformPoint(positions[i]);p=new Vector3((p.x-bounds.center.x)*horizontal,BoardMeshFactory.Surface+.008f+(p.y-bounds.min.y)*vertical,(p.z-bounds.center.z)*horizontal);
                    var n=normals.Length==positions.Length?filter.transform.localToWorldMatrix.inverse.transpose.MultiplyVector(normals[i]):Vector3.up;
                    n=(rotation*n).normalized;
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
            // A neutral hex continuation closes gaps without stretching any source geometry or texture.
            var cap=new List<Vertex>();var indices=new List<int>();float y=BoardMeshFactory.Surface+.006f;
            for(int edge=0;edge<6;edge++)
            {
                Vector3 a=new Vector3(Mathf.Cos(edge*Mathf.PI/3)*Radius,y,Mathf.Sin(edge*Mathf.PI/3)*Radius);
                Vector3 b=new Vector3(Mathf.Cos((edge+1)*Mathf.PI/3)*Radius,y,Mathf.Sin((edge+1)*Mathf.PI/3)*Radius);
                foreach(var p in new[]{new Vector3(0,y,0),b,a}){indices.Add(cap.Count);cap.Add(new Vertex{p=p,n=Vector3.up,uv=new Vector2(p.x/1.6f+.5f,p.z/1.385641f+.5f),color=Color.white});}
            }
            var foundation=new GameObject("Full hex terrain");foundation.transform.SetParent(root.transform,false);foundation.layer=BoardMeshFactory.Layer;
            foundation.AddComponent<MeshFilter>().sharedMesh=SaveMesh(folder+"/Foundation.asset",cap,new[]{indices});
            var floor=foundation.AddComponent<MeshRenderer>();
            string foundationPath=output+"/Materials/"+biome+"_Foundation.mat";
            var foundationMaterial=AssetDatabase.LoadAssetAtPath<Material>(foundationPath);
            if(foundationMaterial==null){foundationMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(foundationMaterial,foundationPath);}
            // A plain continuation closes the perimeter without projecting an atlas over it.
            foundationMaterial.SetTexture("_BaseMap",null);foundationMaterial.SetTexture("_BumpMap",null);foundationMaterial.DisableKeyword("_NORMALMAP");foundationMaterial.DisableKeyword("_EMISSION");
            foundationMaterial.SetColor("_BaseColor",Color.Lerp(GrayboxUI.BiomeColor((Biome)Enum.Parse(typeof(Biome),biome)),new Color(.14f,.15f,.17f),.6f));foundationMaterial.SetFloat("_Smoothness",.12f);foundationMaterial.enableInstancing=false;EditorUtility.SetDirty(foundationMaterial);
            floor.sharedMaterial=foundationMaterial;floor.shadowCastingMode=ShadowCastingMode.Off;
            UnityEngine.Object.DestroyImmediate(source);
            SeatAndSeal(root,folder,foundationMaterial);
        }
        struct RimSegment {public float a,b,ya,yb;public float Height(float t)=>Mathf.Lerp(ya,yb,(t-a)/(b-a));}
        static void SeatAndSeal(GameObject root,string folder,Material material)
        {
            var filters=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Hex crop ",StringComparison.Ordinal)).ToArray();
            float floor=BoardMeshFactory.Surface-.012f;
            float lowest=filters.Min(f=>f.sharedMesh.vertices.Min(v=>v.y));
            float offset=lowest-floor;
            foreach(var filter in filters)
            {
                var mesh=filter.sharedMesh;var points=mesh.vertices;
                for(int i=0;i<points.Length;i++)points[i].y-=offset;
                mesh.vertices=points;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            }
            var rims=new List<RimSegment>[6];for(int edge=0;edge<6;edge++)rims[edge]=new List<RimSegment>();
            foreach(var filter in filters)
            {
                var mesh=filter.sharedMesh;var points=mesh.vertices;var indices=mesh.triangles;
                for(int i=0;i<indices.Length;i+=3)for(int side=0;side<3;side++)
                {
                    if(Vector3.Cross(points[indices[i+1]]-points[indices[i]],points[indices[i+2]]-points[indices[i]]).y<=.00000001f)continue;
                    var a=points[indices[i+side]];var b=points[indices[i+(side+1)%3]];
                    for(int edge=0;edge<6;edge++)
                    {
                        if(Mathf.Abs(Distance(a,edge))>.00003f||Mathf.Abs(Distance(b,edge))>.00003f)continue;
                        float angle=(edge+.5f)*Mathf.PI/3;var tangent=new Vector3(-Mathf.Sin(angle),0,Mathf.Cos(angle));
                        float ta=Vector3.Dot(a,tangent),tb=Vector3.Dot(b,tangent);
                        if(Mathf.Abs(tb-ta)<.00001f)continue;
                        rims[edge].Add(ta<tb?new RimSegment{a=ta,b=tb,ya=a.y,yb=b.y}:new RimSegment{a=tb,b=ta,ya=b.y,yb=a.y});
                    }
                }
            }
            var vertices=new List<Vertex>();var triangles=new List<int>();
            for(int edge=0;edge<6;edge++)
            {
                var lines=rims[edge];var stops=lines.SelectMany(v=>new[]{v.a,v.b}).OrderBy(v=>v).ToArray();
                float angle=(edge+.5f)*Mathf.PI/3;var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var tangent=new Vector3(-normal.z,0,normal.x);
                for(int i=1;i<stops.Length;i++)
                {
                    float a=stops[i-1],b=stops[i],mid=(a+b)*.5f;if(b-a<.00001f)continue;
                    float top=float.MaxValue;RimSegment line=default;bool found=false;
                    foreach(var candidate in lines)if(candidate.a<=mid&&candidate.b>=mid&&candidate.Height(mid)>floor+.001f&&candidate.Height(mid)<top){top=candidate.Height(mid);line=candidate;found=true;}
                    if(!found||top<=floor+.00001f)continue;
                    Vector3 origin=normal*(Radius*Mathf.Cos(Mathf.PI/6));
                    Vector3 p=origin+tangent*a+Vector3.up*floor,q=origin+tangent*b+Vector3.up*floor;
                    Vector3 r=origin+tangent*b+Vector3.up*Mathf.Max(floor,line.Height(b)),t=origin+tangent*a+Vector3.up*Mathf.Max(floor,line.Height(a));
                    int start=vertices.Count;
                    foreach(var point in new[]{p,r,q,p,t,r})vertices.Add(new Vertex{p=point,n=normal,uv=Vector2.zero,color=Color.white});
                    for(int j=0;j<6;j++)triangles.Add(start+j);
                }
            }
            var seal=new GameObject("Terrain sealed rim");seal.transform.SetParent(root.transform,false);seal.layer=BoardMeshFactory.Layer;
            seal.AddComponent<MeshFilter>().sharedMesh=SaveMesh(folder+"/SealedRim.asset",vertices,new[]{triangles});
            var renderer=seal.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
            Debug.Log("TERRAIN_SEATED_AND_SEALED "+root.name+" correctedHeight="+offset+" wallTriangles="+triangles.Count/3);
        }
        [MenuItem("War & Conquer/Entornos/Ajustar terrenos al hexagono %&h")]
        public static void Rebuild(){EnvironmentAssets.Build();EnvironmentReview.Run();}
    }
}
#endif
