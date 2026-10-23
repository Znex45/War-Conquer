#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WarConquer.Editor
{
 public static class OptimizationAudit
 {
  [Serializable] public class Metric {public string name;public int iterations;public double milliseconds;public long bytes;}
  [Serializable] public class Report {public string phase;public List<Metric> metrics=new List<Metric>();public List<string> tests=new List<string>();public List<string> aiDigests=new List<string>();public int renderers,vertices,triangles;public string paths;}
  static Report report;
  static void Measure(string name,int count,Action action)
  {
   for(int i=0;i<3;i++)action();GC.Collect();long before=GC.GetAllocatedBytesForCurrentThread();var timer=Stopwatch.StartNew();for(int i=0;i<count;i++)action();timer.Stop();
   report.metrics.Add(new Metric{name=name,iterations=count,milliseconds=timer.Elapsed.TotalMilliseconds,bytes=GC.GetAllocatedBytesForCurrentThread()-before});
  }
  static string Digest(string text){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","");}
  static void Test(string name,Action body){try{body();report.tests.Add("PASS "+name);}catch(Exception e){report.tests.Add("FAIL "+name+": "+e.Message);}}
  public static void Batch()
  {
   string phase=Environment.GetEnvironmentVariable("WAR_OPT_PHASE")??"before";report=new Report{phase=phase};Directory.CreateDirectory("Library/OptimizationQA");
   var scene=EditorSceneManager.NewPreviewScene();GameObject root=null;var leaders=GrayboxUI.PlayerLeaders;
   try
   {
    if(phase=="after")TerrainMeshOptimization.Run();
    var catalog=CardCatalog.Load();var game=new GameManager(catalog);game.NewGame(new[]{"XIL'THAR","FAUNAR","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules(),4);GrayboxUI.PlayerLeaders=game.State.players.Select(p=>p.leader).ToArray();
    int k=0;foreach(var tile in game.State.tiles.Where(t=>t.owner>=0&&t.baseOwner<0))tile.biome=(Biome)(1+(k++%8));
    var tile0=game.State.tiles.First(t=>t.owner==0&&t.baseOwner<0);var piece=PrototypeScenario.Spawn(game,0,"crystal-minion",tile0.id);piece.playedTurn=game.State.turn-1;piece.remainingMovement=3;game.State.stage=TurnStage.Assault;
    report.paths=string.Join(";",MovementManager.Paths(game,piece).OrderBy(p=>p.Key).Select(p=>p.Key+":"+string.Join(",",p.Value)));
    int walk=0;Measure("Distance BFS",1500,()=>{int a=(walk*17)%game.State.tiles.Count,b=(walk++*31+11)%game.State.tiles.Count;BoardManager.Distance(game.State,a,b,12);});
    Measure("Enumerate pieces",1500,()=>{foreach(var p in BoardManager.Pieces(game.State))walk+=p.owner;});
    Measure("Movement paths",150,()=>MovementManager.Paths(game,piece));
    Measure("UI palette lookup",5000,()=>{var c=GrayboxUI.BiomeColor(Biome.Forest);c=GrayboxUI.TerritoryColor(0);});
    root=new GameObject("Optimization benchmark");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);var world=root.AddComponent<Board3DScene>();world.Initialize();world.BoardCamera.scene=scene;
    var empty=new HashSet<int>();world.Sync(game,empty,empty,-1);
    Measure("Camera update",300,()=>{world.Yaw+=.1f;world.UpdateCamera();});
    Measure("Unchanged board sync",15,()=>world.Sync(game,empty,empty,-1));
    report.renderers=root.GetComponentsInChildren<MeshRenderer>().Length;
    foreach(var biome in new[]{"Forest","Swamp","Desert","Tundra","Volcanic","Wasteland","Void"})foreach(var f in Resources.Load<GameObject>("Environment/Biomes/"+biome).GetComponentsInChildren<MeshFilter>()){report.vertices+=f.sharedMesh.vertexCount;report.triangles+=f.sharedMesh.triangles.Length/3;}
    for(int seed=2026;seed<=2027;seed++)
    {
     var aiGame=new GameManager(catalog);aiGame.NewGame(new[]{"XIL'THAR","FAUNAR","ZUKGROK","SAHRIA"},seed,CardCatalog.LoadRules(),1);foreach(var player in aiGame.State.players)player.isAI=true;var ai=new AiPlayer();
     for(int step=0;step<64&&aiGame.CanAct;step++){ai.Step(aiGame);if(step%16==15)report.aiDigests.Add(seed+":"+step+":"+Digest(GamePersistence.Serialize(aiGame.State)));}
    }
    Test("Graybox complete suite",GrayboxTests.RunAll);
    InterfaceRulesTests.RunAll(LegacyTestCatalog.Load(),Test);MatchSetupTests.RunAll(LegacyTestCatalog.Load(),Test);Board3DTests.RunAll(LegacyTestCatalog.Load(),Test);RevisionRulesTests.RunAll(LegacyTestCatalog.Load(),Test);
    FaunarRulesTests.RunAll(catalog,Test);FactionRulesTests.RunAll(catalog,Test);TurnHistoryTests.RunAll(catalog,Test);XiltharRulesTests.RunAll(Test);
    if(phase=="after")OptimizationPreservationTests.Run();
    File.WriteAllText("Library/OptimizationQA/"+phase+".json",JsonUtility.ToJson(report,true));UnityEngine.Debug.Log("OPTIMIZATION_AUDIT_COMPLETE "+phase+" tests="+report.tests.Count+" failures="+report.tests.Count(t=>t.StartsWith("FAIL")));
    EditorApplication.Exit(0);
   }
   catch(Exception e){UnityEngine.Debug.LogException(e);File.WriteAllText("Library/OptimizationQA/"+phase+"-error.txt",e.ToString());EditorApplication.Exit(1);}
   finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);GrayboxUI.PlayerLeaders=leaders;}
  }
 }
}
#endif

