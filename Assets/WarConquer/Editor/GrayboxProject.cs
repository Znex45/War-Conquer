#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarConquer.Editor
{
    public static class GrayboxProject
    {
        const string ScenePath="Assets/WarConquer/Scenes/WarConquer_Graybox.unity";
        [MenuItem("War & Conquer/Abrir Graybox")]
        public static void Open()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!File.Exists(ScenePath))CreateScene();else EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("War & Conquer/Crear o actualizar escena Graybox")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/WarConquer/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
            camera.GetComponent<Camera>().backgroundColor=GrayboxUI.Background;camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().orthographic=true;
            new GameObject("War & Conquer - Graybox",typeof(WarConquerController));
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("WAR_CONQUER_SCENE_READY "+ScenePath);
        }
        public static void ValidateAndBuild()
        {
            try
            {
                CreateScene();GrayboxTests.RunAll();
                string output=Environment.GetEnvironmentVariable("WAR_CONQUER_BUILD");
                if(!string.IsNullOrEmpty(output))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                    PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=1000;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
                    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
                    if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build falló: "+report.summary.result);
                    Debug.Log("WAR_CONQUER_BUILD_PASSED");
                }
                EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
#endif
