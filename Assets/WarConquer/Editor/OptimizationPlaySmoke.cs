#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer.Editor
{
 public static class OptimizationPlaySmoke
 {
  const string Key="WarConquer.OptimizationPlaySmoke";static int step;static double due;static WarConquerController ui;static string error;
  public static void Batch(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
  [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
  static void Log(string condition,string stack,LogType type){if(SessionState.GetBool(Key,false)&&(type==LogType.Exception||type==LogType.Error)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))error=condition;}
  static void Invoke(string name)=>typeof(WarConquerController).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
  static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
  static void Tick()
  {
   if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<due)return;
   try
   {
    if(step==0){ui=new GameObject("Optimization play smoke").AddComponent<WarConquerController>();step++;due=EditorApplication.timeSinceStartup+1;return;}
    if(step==1)
    {
     Invoke("StartMainGame");Invoke("CloseModal");ui.Game.NewGame(new[]{"SAHRIA","FAUNAR","ZUKGROK","XIL'THAR"},2026,CardCatalog.LoadRules(),4);
     step++;due=EditorApplication.timeSinceStartup+1;return;
    }
    if(step==2)
    {
     var world=ui.GetComponentInChildren<Board3DScene>(true);Check(world.gameObject.activeInHierarchy&&world.BoardCamera.enabled,"Board rendering did not resume");
     Check(ui.Game.SetSahriaDeployment(true),"Could not create undo decision");var button=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Rewind / Deshacer");Check(button.interactable,"Cached undo UI stale");button.onClick.Invoke();Check(!ui.Game.State.Active.sahriaDeployment,"Undo UI did not restore state");
     step++;due=EditorApplication.timeSinceStartup+1;return;
    }
    Check(error==null,"Runtime log error: "+error);Directory.CreateDirectory("Library/OptimizationQA");File.WriteAllText("Library/OptimizationQA/play-smoke.txt","PASS: real Play Mode startup, main menu/setup, live board camera, cached canvas components, undo button/state restore and multiple Update frames without runtime errors.");
    Debug.Log("OPTIMIZATION_PLAY_SMOKE_PASSED");SessionState.SetBool(Key,false);EditorApplication.Exit(0);
   }
   catch(Exception e){Directory.CreateDirectory("Library/OptimizationQA");File.WriteAllText("Library/OptimizationQA/play-smoke.txt",e.ToString());Debug.LogException(e);SessionState.SetBool(Key,false);EditorApplication.Exit(1);}
  }
 }
}
#endif

