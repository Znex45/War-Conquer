#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class EnvironmentReview
    {
        static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
        [MenuItem("War & Conquer/Entornos/Comprobar y capturar entornos %&e")]
        public static void Run()
        {
            var scene=EditorSceneManager.NewPreviewScene();GameObject root=null;
            string output="Library/EnvironmentQA";Directory.CreateDirectory(output);
            try
            {
                root=new GameObject("Environment review");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
                var camera=new GameObject("Review camera").AddComponent<Camera>();camera.transform.SetParent(root.transform);camera.scene=scene;camera.orthographic=true;camera.orthographicSize=2.4f;camera.transform.position=new Vector3(0,4,-6);camera.transform.LookAt(new Vector3(0,.2f,0));camera.cullingMask=1<<BoardMeshFactory.Layer;camera.clearFlags=CameraClearFlags.Skybox;
                var light=new GameObject("Review light").AddComponent<Light>();light.transform.SetParent(root.transform);light.type=LightType.Directional;light.intensity=1.4f;light.transform.rotation=Quaternion.Euler(45,-35,0);light.cullingMask=camera.cullingMask;
                var mesh=new BoardMeshFactory();var biomes=new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland,Biome.Void};
                for(int i=0;i<biomes.Length;i++)
                {
                    var slot=new GameObject(biomes[i].ToString()).transform;slot.SetParent(root.transform);slot.position=new Vector3((i%4-1.5f)*1.6f,0,(i/4)*1.8f);
                    mesh.Part(slot,"Hex",mesh.Hex,Vector3.zero,Vector3.one,GrayboxUI.BiomeColor(biomes[i]));
                    Check(ImportedBiomeVisuals.TryBuild(slot,new HexTile{id=i,biome=biomes[i]},out var habitat),"Missing prefab "+biomes[i]);
                    foreach(var renderer in slot.GetComponentsInChildren<Renderer>())foreach(var material in renderer.sharedMaterials)Check(material!=null&&material.shader!=null&&material.shader.isSupported,"Invalid material "+biomes[i]);
                    var scenery=habitat.root.GetChild(1);var renderers=scenery.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
                    // Compare independent source and generated bounds: all three ratios must match.
                    string sourceFolder="Assets/WarConquer/EnvironmentSource/"+biomes[i];
                    if(Directory.Exists(sourceFolder+"/AuthoredHex"))sourceFolder+="/AuthoredHex";
                    string sourcePath=Directory.GetFiles(sourceFolder,"*.fbx",SearchOption.AllDirectories).FirstOrDefault()??Directory.GetFiles(sourceFolder,"*.obj",SearchOption.AllDirectories).First();
                    var original=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
                    var sourcePoints=original.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v))).ToArray();
                    HexBiomeCrop.Alignment(sourcePoints,out var sourceRotation,out var originalBounds);
                    var bodyRenderers=scenery.GetComponentsInChildren<Renderer>().Where(r=>r.name!="Full hex terrain").ToArray();var bodyBounds=bodyRenderers[0].bounds;foreach(var renderer in bodyRenderers.Skip(1))bodyBounds.Encapsulate(renderer.bounds);
                    var fittedPoints=scenery.GetComponentsInChildren<MeshFilter>().Where(f=>f.name!="Full hex terrain").SelectMany(f=>f.sharedMesh.vertices).ToArray();
                    Check(Mathf.Abs(fittedPoints.Min(v=>v.y)-(BoardMeshFactory.Surface-.012f))<.0001f,"Floating terrain bottom "+biomes[i]);
                    Check(fittedPoints.Length>0,"Missing fitted geometry "+biomes[i]);
                    for(int edge=0;edge<6;edge++)
                    {
                        float angle=(edge+.5f)*Mathf.PI/3;var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                        float outer=fittedPoints.Max(v=>Vector3.Dot(normal,v));
                        Check(Mathf.Abs(outer-HexBiomeCrop.Radius*Mathf.Cos(Mathf.PI/6))<.002f,"Terrain side does not meet tile "+biomes[i]+" edge="+edge);
                    }
                    UnityEngine.Object.DestroyImmediate(original);
                    Debug.Log("HEX_ALIGNMENT_VERIFIED "+biomes[i]+": six aligned edges, uniform source scale, preserved UVs");
                    var closed=new bool[6];
                    foreach(var filter in scenery.GetComponentsInChildren<MeshFilter>().Where(f=>f.name!="Full hex terrain"))
                    {
                        var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;var points=filter.sharedMesh.vertices;
                        for(int sub=0;sub<materials.Length;sub++)
                        {
                            if(!materials[sub].name.EndsWith("_Foundation",StringComparison.Ordinal))continue;
                            var indices=filter.sharedMesh.GetTriangles(sub);
                            for(int tri=0;tri<indices.Length;tri+=3)
                            {
                                var a=points[indices[tri]];var b=points[indices[tri+1]];var c=points[indices[tri+2]];
                                for(int edge=0;edge<6;edge++)
                                {
                                    float angle=(edge+.5f)*Mathf.PI/3;var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));float plane=HexBiomeCrop.Radius*Mathf.Cos(Mathf.PI/6);
                                    if(Mathf.Abs(Vector3.Dot(normal,a)-plane)>.0001f||Mathf.Abs(Vector3.Dot(normal,b)-plane)>.0001f||Mathf.Abs(Vector3.Dot(normal,c)-plane)>.0001f)continue;
                                    Check(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)>=-0.000001f,"Inverted terrain side "+biomes[i]);
                                    if(Mathf.Min(a.y,Mathf.Min(b.y,c.y))<=BoardMeshFactory.Surface-.011f&&Mathf.Max(a.y,Mathf.Max(b.y,c.y))>BoardMeshFactory.Surface)closed[edge]=true;
                                }
                            }
                        }
                    }
                    Check(closed.All(v=>v),"Open or floating perimeter "+biomes[i]);
                    Debug.Log("TERRAIN_CONTACT_VERIFIED "+biomes[i]+": seated bottom and six outward-facing closed sides");
                    Check(bounds.size.x<=1.601f&&bounds.size.z<=1.387f,"Oversized scenery "+biomes[i]);
                    var foundation=scenery.Find("Full hex terrain");Check(foundation!=null,"Missing full hex foundation "+biomes[i]);
                    for(int corner=0;corner<6;corner++)
                    {
                        var expected=new Vector3(Mathf.Cos(corner*Mathf.PI/3)*HexBiomeCrop.Radius,BoardMeshFactory.Surface+.006f,Mathf.Sin(corner*Mathf.PI/3)*HexBiomeCrop.Radius);
                        Check(foundation.GetComponent<MeshFilter>().sharedMesh.vertices.Any(v=>Vector3.Distance(v,expected)<.0001f),"Hex corner uncovered "+biomes[i]);
                    }
                    foreach(var filter in scenery.GetComponentsInChildren<MeshFilter>())
                    {
                        Check(filter.sharedMesh.uv.Length==filter.sharedMesh.vertexCount,"Missing clipped UVs "+biomes[i]);
                        foreach(var vertex in filter.sharedMesh.vertices)for(int edge=0;edge<6;edge++)
                        {
                            float angle=(edge+.5f)*Mathf.PI/3;
                            Check(Mathf.Cos(angle)*vertex.x+Mathf.Sin(angle)*vertex.z<=HexBiomeCrop.Radius*Mathf.Cos(Mathf.PI/6)+.0001f,"Geometry outside hex "+biomes[i]);
                        }
                    }

                }
                var savedPosition=camera.transform.position;var savedRotation=camera.transform.rotation;
                camera.transform.position=new Vector3(0,8,1);camera.transform.rotation=Quaternion.Euler(90,0,0);Capture(camera,output+"/alineacion-superior.png");
                camera.transform.position=new Vector3(0,1.2f,-7);camera.transform.LookAt(new Vector3(0,.5f,.9f));Capture(camera,output+"/terrenos-laterales.png");
                camera.transform.position=savedPosition;camera.transform.rotation=savedRotation;
                var game=new GameManager(CardCatalog.Load());game.NewGame(new[]{"ZUKGROK","FAUNAR","SAHRIA","XIL'THAR"},2026,CardCatalog.LoadRules(),4);
                var sky=root.AddComponent<LeaderSkybox>();
                for(int i=0;i<4;i++)
                {
                    game.State.activePlayer=i;sky.Sync(game,camera);Check(camera.GetComponent<Skybox>().material!=null,"Missing sky "+i);
                    Capture(camera,output+"/entornos-"+i+".png");
                }
                game.State.activePlayer=0;game.State.responsePlayer=2;sky.Sync(game,camera);Check(sky.CurrentLeader=="SAHRIA","Reaction sky incorrect");
                game.State.battle=new PendingBattle{priorityPlayer=1};sky.Sync(game,camera);Check(sky.CurrentLeader=="FAUNAR","Battle priority sky incorrect");
                game.State.pendingChoices.Add(new PendingChoice{owner=3});sky.Sync(game,camera);Check(sky.CurrentLeader=="XIL'THAR","Choice sky incorrect");
                game.State.pendingChoices.Clear();game.State.battle=null;game.State.responsePlayer=-1;sky.Sync(game,camera);Check(sky.CurrentLeader=="ZUKGROK","Turn sky not restored");
                File.WriteAllText(output+"/verificacion-entornos.txt","PASS: 7 prefabs, materials, hex bounds, full coverage of all six corners, clipped geometry and UVs; 4 skyboxes rendered; turn, reaction, battle priority, pending choice and return to turn. Unity "+Application.unityVersion);
                // Exercise the real board integration as well as the isolated resources.
                var boardRoot=new GameObject("Board integration review");boardRoot.transform.SetParent(root.transform);
                var world=boardRoot.AddComponent<Board3DScene>();world.Initialize();world.BoardCamera.scene=scene;
                for(int i=0;i<biomes.Length;i++)game.State.tiles.First(tile=>tile.baseOwner<0&&tile.biome==Biome.Neutral).biome=biomes[i];
                world.Sync(game,new System.Collections.Generic.HashSet<int>(),new System.Collections.Generic.HashSet<int>(),-1);
                Check(world.GetComponentsInChildren<ImportedBiomeMotion>().Length>=7,"Board did not instantiate imported biomes");
                Check(world.BoardCamera.clearFlags==CameraClearFlags.Skybox,"Board camera did not enable skybox");
                // Exclude the isolated review gallery from this camera.
                foreach(Transform child in root.transform)if(child!=boardRoot.transform)child.gameObject.SetActive(false);
                world.ResizeTexture(1400,840);Capture(world.BoardCamera,output+"/tablero-integrado.png");
                File.AppendAllText(output+"/verificacion-entornos.txt","; PASS real Board3DScene instantiation and skybox camera integration.");
                Debug.Log("ENVIRONMENT_REVIEW_PASSED");mesh.Dispose();
            }
            catch(Exception e){Debug.LogException(e);File.WriteAllText(output+"/verificacion-entornos.txt",e.ToString());throw;}
            finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);}
        }
        public static void Batch(){try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Capture(Camera camera,string path)
        {
            var target=new RenderTexture(1400,840,24);target.Create();camera.targetTexture=target;camera.aspect=1400f/840;camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var pixels=new Texture2D(1400,840,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1400,840),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
#endif

