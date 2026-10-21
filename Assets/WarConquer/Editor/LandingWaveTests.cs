#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class LandingWaveTests
    {
        const string Pending="LandingWave.Pending";static bool active;static int step;static double due,started;
        static BoardLandingWave wave;static Transform[] surfaces;
        public static void BeginBatch(){SessionState.SetBool(Pending,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false)){SessionState.SetBool(Pending,false);active=true;step=0;due=EditorApplication.timeSinceStartup+2;}
            if(!active||EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    var root=new GameObject("Landing wave QA");wave=root.AddComponent<BoardLandingWave>();surfaces=new Transform[8];
                    for(int i=0;i<8;i++){surfaces[i]=new GameObject("Radius "+i).transform;surfaces[i].SetParent(root.transform);wave.Register(new HexTile{id=i,q=i,r=0},surfaces[i]);}
                    wave.Impact(0);started=EditorApplication.timeSinceStartup;step=1;due=started+.1;return;
                }
                if(step==1)
                {
                    Check(Mathf.Abs(surfaces[0].localPosition.y)>.005f,"No central impact");Check(surfaces[6].localPosition==Vector3.zero,"Wave reached outer ring too early");
                    step=2;due=started+.61;return;
                }
                if(step==2)
                {
                    Check(Mathf.Abs(surfaces[6].localPosition.y)>.005f,"Wave did not reach radius six");Check(surfaces[7].localPosition==Vector3.zero,"Wave exceeds radius six");
                    step=3;due=started+1.1;return;
                }
                Check(!wave.IsActive,"Wave never ends");foreach(var s in surfaces)Check(s.localPosition==Vector3.zero,"Residual tile displacement");
                wave.Impact(0);wave.Clear();Check(!wave.IsActive,"Clear did not cancel wave");
                Debug.Log("LANDING_WAVE_PASSED: outward propagation, radius six included, radius seven excluded, exact reset and cancellation.");active=false;EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);active=false;EditorApplication.Exit(1);}
        }
    }
}
#endif
