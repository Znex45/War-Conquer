#if UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WarConquer.Editor
{
    public static class SculptureQualityReview
    {
        public static void Batch()
        {
            try{Review();Debug.Log("SCULPTURE_QUALITY_PASSED");EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static void Review()
        {
            var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Sculpture quality review");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
            var factory=new BoardMeshFactory();var report=new List<string>();
            const string output="Library/SculptureQualityQA";Directory.CreateDirectory(output);
            try
            {
                CheckBox(factory.SculptureBlock);
                var cam=new GameObject("Camera").AddComponent<Camera>();cam.transform.SetParent(root.transform);cam.scene=scene;
                cam.orthographic=true;cam.orthographicSize=1.04f;cam.cullingMask=1<<BoardMeshFactory.Layer;
                cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.035f,.046f,.06f);
                var key=new GameObject("Key").AddComponent<Light>();key.transform.SetParent(root.transform);key.type=LightType.Directional;key.intensity=1.4f;key.transform.rotation=Quaternion.Euler(42,-28,0);
                var fill=new GameObject("Fill").AddComponent<Light>();fill.transform.SetParent(root.transform);fill.type=LightType.Directional;fill.intensity=.65f;fill.transform.rotation=Quaternion.Euler(20,150,0);
                foreach(var card in CardCatalog.Load().All.Where(c=>c.category!=Category.Spell))
                {
                    var slot=factory.Group(root.transform,card.id,Vector3.zero);factory.PieceModel(slot,card,Color.yellow);
                    var renderers=slot.GetComponentsInChildren<MeshRenderer>();
                    var reached=new HashSet<int>{0};bool changed=true;
                    while(changed)
                    {
                        changed=false;
                        for(int i=0;i<renderers.Length;i++)if(!reached.Contains(i))foreach(int j in reached.ToArray())
                        {
                            var bound=renderers[j].bounds;bound.Expand(.025f);
                            if(bound.Intersects(renderers[i].bounds)){reached.Add(i);changed=true;break;}
                        }
                    }
                    var floating=Enumerable.Range(0,renderers.Length).Where(i=>!reached.Contains(i)).Select(i=>renderers[i].name).ToArray();
                    if(floating.Length>0)report.Add(card.id+": "+string.Join(" | ",floating));
                    // These five units deliberately hover or fly. Grounded models must attach to their plinth.
                    var airborne=new[]{"avatar-del-sol","baby-phoenix","firefly-token","dark-fly","the-eternal-eye"};
                    Check(floating.Length==0||airborne.Contains(card.id),card.id+": detached geometry: "+string.Join(" | ",floating));
                    foreach(var f in slot.GetComponentsInChildren<MeshFilter>())
                    {
                        Check(f.sharedMesh!=null,card.id+": missing mesh");
                        foreach(var v in f.sharedMesh.vertices)
                        {
                            var p=slot.InverseTransformPoint(f.transform.TransformPoint(v));
                            Check(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z),card.id+": non-finite vertex");
                            Check(Mathf.Abs(p.x)<.67f&&Mathf.Abs(p.z)<.75f&&p.y>=-.025f&&p.y<1.9f,card.id+": footprint "+p);
                        }
                    }
                    foreach(var angle in new[]{new Vector3(0,1.45f,-4),new Vector3(-2.2f,1.8f,3.8f)})
                    {
                        cam.transform.position=angle;cam.transform.LookAt(new Vector3(0,.68f,0));
                        Capture(cam,output+"/"+card.id+(angle.z<0?"-front.png":"-back.png"));
                    }
                    BoardMeshFactory.Release(slot.gameObject);
                }
                File.WriteAllLines(output+"/attachment-review.txt",report);
            }
            finally{BoardMeshFactory.Release(root);factory.Dispose();EditorSceneManager.ClosePreviewScene(scene);}
        }
        static void CheckBox(Mesh mesh)
        {
            var verts=mesh.vertices;var tris=mesh.triangles;var edges=new Dictionary<string,int>();
            string Key(Vector3 p)=>p.ToString("F4");
            for(int i=0;i<tris.Length;i+=3)
            {
                var a=verts[tris[i]];var b=verts[tris[i+1]];var c=verts[tris[i+2]];var normal=Vector3.Cross(b-a,c-a);
                Check(normal.sqrMagnitude>1e-10f,"Degenerate masonry face");
                Check(Vector3.Dot(normal,(a+b+c)/3)>0,"Inverted masonry face");
                var keys=new[]{Key(a),Key(b),Key(c)};
                for(int j=0;j<3;j++){var pair=new[]{keys[j],keys[(j+1)%3]};Array.Sort(pair,StringComparer.Ordinal);string k=string.Join("/",pair);edges[k]=edges.ContainsKey(k)?edges[k]+1:1;}
            }
            Check(edges.Values.All(count=>count==2),"Open masonry mesh seam");
        }
        static void Capture(Camera cam,string path)
        {
            var rt=new RenderTexture(300,360,24);var tex=new Texture2D(300,360,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,300,360),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
            finally{cam.targetTexture=null;RenderTexture.active=previous;BoardMeshFactory.Release(tex);BoardMeshFactory.Release(rt);}
        }
    }
}
#endif
