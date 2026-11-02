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
 public static class TerrainMeshOptimization
 {
  struct Vertex:IEquatable<Vertex>
  {
   public Vector3 p,n;public Vector2 uv;public Color color;public Vector4 tangent;
   public bool Equals(Vertex v)=>p.Equals(v.p)&&n.Equals(v.n)&&uv.Equals(v.uv)&&color.Equals(v.color)&&tangent.Equals(v.tangent);
   public override bool Equals(object value)=>value is Vertex v&&Equals(v);
   public override int GetHashCode(){unchecked{return ((((p.GetHashCode()*397)^n.GetHashCode())*397^uv.GetHashCode())*397^color.GetHashCode())*397^tangent.GetHashCode();}}
  }
  static Mesh Weld(Mesh source)
  {
   var points=source.vertices;var normals=source.normals;var uv=source.uv;var colors=source.colors;var tangents=source.tangents;
   var lookup=new Dictionary<Vertex,int>();var unique=new List<Vertex>();var remap=new int[points.Length];
   for(int i=0;i<points.Length;i++)
   {
    var v=new Vertex{p=points[i],n=normals.Length>i?normals[i]:Vector3.up,uv=uv.Length>i?uv[i]:Vector2.zero,color=colors.Length>i?colors[i]:Color.white,tangent=tangents.Length>i?tangents[i]:new Vector4(1,0,0,1)};
    if(!lookup.TryGetValue(v,out int id)){id=unique.Count;lookup.Add(v,id);unique.Add(v);}remap[i]=id;
   }
   var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.SetVertices(unique.Select(v=>v.p).ToList());mesh.SetNormals(unique.Select(v=>v.n).ToList());mesh.SetUVs(0,unique.Select(v=>v.uv).ToList());mesh.SetColors(unique.Select(v=>v.color).ToList());mesh.SetTangents(unique.Select(v=>v.tangent).ToList());
   mesh.subMeshCount=source.subMeshCount;for(int sub=0;sub<source.subMeshCount;sub++){var indices=source.GetTriangles(sub);for(int i=0;i<indices.Length;i++)indices[i]=remap[indices[i]];mesh.SetTriangles(indices,sub);}
   mesh.RecalculateBounds();return mesh;
  }
  static Dictionary<(Vertex,Vertex,Vertex,Material),int> Geometry(GameObject root)
  {
   var result=new Dictionary<(Vertex,Vertex,Vertex,Material),int>();
   foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
   {
    var mesh=filter.sharedMesh;var p=mesh.vertices;var n=mesh.normals;var u=mesh.uv;var c=mesh.colors;var tangent=mesh.tangents;var data=new Vertex[p.Length];
    for(int i=0;i<p.Length;i++)data[i]=new Vertex{p=p[i],n=n.Length>i?n[i]:Vector3.up,uv=u.Length>i?u[i]:Vector2.zero,color=c.Length>i?c[i]:Color.white,tangent=tangent.Length>i?tangent[i]:new Vector4(1,0,0,1)};
    var mats=filter.GetComponent<MeshRenderer>().sharedMaterials;
    for(int sub=0;sub<mesh.subMeshCount;sub++)
    {
     var indices=mesh.GetTriangles(sub);for(int i=0;i<indices.Length;i+=3)
     {var key=(data[indices[i]],data[indices[i+1]],data[indices[i+2]],mats[sub]);result.TryGetValue(key,out int count);result[key]=count+1;}
    }
   }
   return result;
  }
  [MenuItem("War & Conquer/Rendimiento/Optimizar mallas sin reducir geometria")]
  public static void Run()
  {
   const string folder="Assets/WarConquer/Resources/Environment";Directory.CreateDirectory(folder+"/Meshes/Optimized");AssetDatabase.Refresh();
   var report=new List<string>();
   foreach(string biome in new[]{"Forest","Swamp","Desert","Tundra","Volcanic","Wasteland","Void"})
   {
    string path=folder+"/Biomes/"+biome+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
    try
    {
     var originalGeometry=Geometry(root);
     var filters=root.GetComponentsInChildren<MeshFilter>();int beforeVertices=filters.Sum(f=>f.sharedMesh.vertexCount),beforeTriangles=filters.Sum(f=>f.sharedMesh.triangles.Length/3),beforeRenderers=filters.Length;
     var groups=new Dictionary<Material,List<CombineInstance>>();var removed=new List<GameObject>();
     foreach(var filter in filters)
     {
      if(filter.name=="Full hex terrain")continue;
      var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)throw new Exception("Non-static terrain renderer");var materials=renderer.sharedMaterials;
      for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
      {
       if(filter.sharedMesh.GetIndexCount(sub)==0)continue;var mat=materials[sub];if(!groups.TryGetValue(mat,out var parts))groups[mat]=parts=new List<CombineInstance>();
       parts.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=sub,transform=root.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix});
      }
      removed.Add(filter.gameObject);
     }
     var combinedParts=new List<CombineInstance>();var combinedMaterials=new List<Material>();var temporary=new List<Mesh>();
     foreach(var entry in groups)
     {
      var part=new Mesh{indexFormat=IndexFormat.UInt32};part.CombineMeshes(entry.Value.ToArray(),true,true,false);temporary.Add(part);
      combinedParts.Add(new CombineInstance{mesh=part,subMeshIndex=0,transform=Matrix4x4.identity});combinedMaterials.Add(entry.Key);
     }
     var merged=new Mesh{indexFormat=IndexFormat.UInt32};merged.CombineMeshes(combinedParts.ToArray(),false,false,false);var welded=Weld(merged);
     UnityEngine.Object.DestroyImmediate(merged);foreach(var part in temporary)UnityEngine.Object.DestroyImmediate(part);
     string meshPath=folder+"/Meshes/Optimized/"+biome+"_Combined.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
     if(existing==null){AssetDatabase.CreateAsset(welded,meshPath);existing=welded;}else{EditorUtility.CopySerialized(welded,existing);UnityEngine.Object.DestroyImmediate(welded);EditorUtility.SetDirty(existing);}
     var combinedNode=new GameObject("Mesh group 0");combinedNode.layer=BoardMeshFactory.Layer;combinedNode.transform.SetParent(root.transform,false);combinedNode.AddComponent<MeshFilter>().sharedMesh=existing;
     var combinedRenderer=combinedNode.AddComponent<MeshRenderer>();combinedRenderer.sharedMaterials=combinedMaterials.ToArray();combinedRenderer.shadowCastingMode=ShadowCastingMode.Off;combinedRenderer.receiveShadows=true;
     foreach(var node in removed)UnityEngine.Object.DestroyImmediate(node);
     int afterTriangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
     if(afterTriangles!=beforeTriangles)throw new Exception("Triangle count changed: "+biome);
     var finalGeometry=Geometry(root);
     if(originalGeometry.Count!=finalGeometry.Count||originalGeometry.Any(pair=>!finalGeometry.TryGetValue(pair.Key,out int count)||count!=pair.Value))throw new Exception("Geometry attributes changed: "+biome);
     PrefabUtility.SaveAsPrefabAsset(root,path);
     report.Add(biome+": renderers "+beforeRenderers+" -> "+root.GetComponentsInChildren<MeshFilter>().Length+"; vertices "+beforeVertices+" -> "+root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount)+"; triangles, materials, positions, normals, UVs, vertex colors and tangents verified unchanged: "+afterTriangles);
    }
    finally{PrefabUtility.UnloadPrefabContents(root);}
   }
   AssetDatabase.SaveAssets();Directory.CreateDirectory("Library/OptimizationQA");File.WriteAllLines("Library/OptimizationQA/meshes.txt",report);Debug.Log("LOSSLESS_TERRAIN_OPTIMIZATION_COMPLETE");
  }
  public static void Batch(){try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
