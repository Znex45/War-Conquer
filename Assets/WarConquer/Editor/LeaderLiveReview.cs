#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class LeaderLiveReview
    {
        const string Pending="LeaderLiveReview.Pending";
        static readonly string[] Names={"XIL'THAR","ZUKGROK","FAUNAR","SAHRIA"};
        static GameObject root;static Camera cam;static RenderTexture target;static double due;static int step;static bool active;
        static Transform[][] bones;static Vector3[][] positions;static Quaternion[][] rotations;
        public static void BeginBatch(){SessionState.SetBool(Pending,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false)){SessionState.SetBool(Pending,false);active=true;step=0;due=EditorApplication.timeSinceStartup+3;}
            if(!active||EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    root=new GameObject("Live leader review");bones=new Transform[4][];
                    for(int i=0;i<4;i++)
                    {
                        var slot=new GameObject(Names[i]);slot.transform.SetParent(root.transform);slot.transform.position=new Vector3((i-1.5f)*2.4f,0,0);
                        if(!LeaderModels.Spawn(slot.transform,Names[i]))throw new Exception("Missing leader");bones[i]=slot.GetComponentsInChildren<Transform>();
                    }
                    foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
                    var light=new GameObject("Light").AddComponent<Light>();light.transform.SetParent(root.transform);light.type=LightType.Directional;light.intensity=1.4f;light.cullingMask=1<<29;light.transform.rotation=Quaternion.Euler(40,-35,0);
                    cam=new GameObject("Camera").AddComponent<Camera>();cam.transform.SetParent(root.transform);cam.orthographic=true;cam.orthographicSize=2.2f;cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.055f,.065f,.09f);
                    cam.transform.position=new Vector3(0,2.4f,-7);cam.transform.LookAt(new Vector3(0,1.1f,0));target=new RenderTexture(1800,700,24);target.Create();cam.targetTexture=target;cam.aspect=1800f/700;
                    step=1;due=EditorApplication.timeSinceStartup+3;return;
                }
                if(step==1)
                {
                    positions=bones.Select(set=>set.Select(t=>t.localPosition).ToArray()).ToArray();rotations=bones.Select(set=>set.Select(t=>t.localRotation).ToArray()).ToArray();
                    Capture("live-front");step=2;due=EditorApplication.timeSinceStartup+2;return;
                }
                for(int i=0;i<4;i++)
                {
                    if(!bones[i].Where((t,j)=>Vector3.Distance(t.localPosition,positions[i][j])>.00001f||Quaternion.Angle(t.localRotation,rotations[i][j])>.01f).Any())throw new Exception("Live idle frozen: "+Names[i]);
                    foreach(var skin in bones[i][0].GetComponentsInChildren<SkinnedMeshRenderer>())if(skin.sharedMesh==null||skin.bones.Any(b=>b==null)||!float.IsFinite(skin.bounds.size.x))throw new Exception("Broken skin: "+Names[i]);
                }
                Capture("live-front-later");
                foreach(float angle in new[]{90f,180f,270f}){for(int i=0;i<4;i++)root.transform.Find(Names[i]).localRotation=Quaternion.Euler(0,angle,0);Capture("live-"+angle);}
                Debug.Log("LEADER_LIVE_PASSED: all four leaders animate in Play Mode; valid skins; front, sides and back rendered.");active=false;EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);active=false;EditorApplication.Exit(1);}
        }
        static void Capture(string name)
        {
            cam.Render();var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(1800,700,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1800,700),0,0);pixels.Apply();RenderTexture.active=previous;
            Directory.CreateDirectory("Library/LeaderModelQA");File.WriteAllBytes("Library/LeaderModelQA/"+name+".png",pixels.EncodeToPNG());UnityEngine.Object.Destroy(pixels);
        }
    }
}
#endif
