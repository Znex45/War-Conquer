#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WarConquer.Editor
{
 public static class NeonReview
 {
  static void Check(bool value,string reason){if(!value)throw new Exception(reason);}
  [MenuItem("War & Conquer/Entornos/Comprobar neon y turnos")]
  public static void Run()
  {
   string output="Library/NeonQA";Directory.CreateDirectory(output);var scene=EditorSceneManager.NewPreviewScene();GameObject root=null;var previousLeaders=GrayboxUI.PlayerLeaders;
   try
   {
    root=new GameObject("Neon integration review");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
    var game=new GameManager(CardCatalog.Load());game.NewGame(new[]{"ZUKGROK","FAUNAR","SAHRIA","XIL'THAR"},2026,CardCatalog.LoadRules(),4);GrayboxUI.PlayerLeaders=game.State.players.Select(p=>p.leader).ToArray();
    var biomes=new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland,Biome.Void};int index=0;
    foreach(var tile in game.State.tiles.Where(t=>t.owner>=0&&t.baseOwner<0))tile.biome=biomes[index++%biomes.Length];
    var world=root.AddComponent<Board3DScene>();world.Initialize();world.BoardCamera.scene=scene;world.ResizeTexture(1400,900);
    for(int owner=0;owner<4;owner++)
    {
     game.State.activePlayer=owner;string state=GamePersistence.Serialize(game.State);
     world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);var neon=root.GetComponent<BoardTurnNeon>();
     Check(neon.PriorityPlayer==owner,"Wrong neon turn");Check(Vector4.Distance(neon.PriorityColor,GrayboxUI.PlayerColor(owner))<.001f,"Neon differs from base color");
     Check(neon.ActiveContourCount==game.State.tiles.Count(t=>t.owner==owner||t.baseOwner==owner),"Incorrect territory contours");
     Check(neon.BaseLight.enabled&&neon.BaseLight.shadows==LightShadows.None,"Missing bounded base light");
     neon.Animate(0);float intensity=neon.BaseLight.intensity;neon.Animate(.6f);Check(Math.Abs(neon.BaseLight.intensity-intensity)>.01f,"Neon pulse is static");
     Check(state==GamePersistence.Serialize(game.State),"Neon mutated game state");
     foreach(var ps in root.GetComponentsInChildren<ParticleSystem>())if(ps.name.StartsWith("Ambiente ")||ps.name=="Priority neon particles")
     {Check(ps.main.maxParticles<=32,"Unbounded neon particles");Check(ps.GetComponent<Renderer>().sharedMaterial.shader.name=="WarConquer/NeonGlow","Incorrect neon shader");ps.Simulate(.9f,true,true,true);}
     var home=game.State.tiles.First(t=>t.baseOwner==owner);var focus=Board3DScene.Position(home)+Board3DScene.Position(home).normalized*2+Vector3.up*1.3f;
     world.BoardCamera.orthographicSize=6.5f;world.BoardCamera.transform.position=focus+new Vector3(0,8,-10);world.BoardCamera.transform.LookAt(focus);Capture(world.BoardCamera,output+"/neon-"+owner+".png");
    }
    var effect=root.GetComponent<BoardTurnNeon>();
    game.State.activePlayer=0;game.State.responsePlayer=2;world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Check(effect.PriorityPlayer==2,"Reaction neon incorrect");
    game.State.battle=new PendingBattle{priorityPlayer=1};world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Check(effect.PriorityPlayer==1,"Battle neon incorrect");
    game.State.pendingChoices.Add(new PendingChoice{owner=3});world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Check(effect.PriorityPlayer==3,"Choice neon incorrect");
    game.State.pendingChoices.Clear();game.State.battle=null;game.State.responsePlayer=-1;world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Check(effect.PriorityPlayer==0,"Turn neon not restored");
    game.NewGame(new[]{"ZUKGROK","FAUNAR","SAHRIA","XIL'THAR"},2027,CardCatalog.LoadRules(),4);world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Check(effect.BaseLight.enabled,"Neon failed after board rebuild");
    File.WriteAllText(output+"/verificacion-neon.txt","PASS: neon particle shader and bounded counts; 4 base colors; active territory contours; real light pulse; no gameplay mutations; reaction, battle, choice, return to turn and new-board cleanup. Unity "+Application.unityVersion);Debug.Log("NEON_REVIEW_PASSED");
   }
   catch(Exception e){Debug.LogException(e);File.WriteAllText(output+"/verificacion-neon.txt",e.ToString());throw;}
   finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);EditorSceneManager.ClosePreviewScene(scene);GrayboxUI.PlayerLeaders=previousLeaders;}
  }
  public static void Batch(){try{Run();EditorApplication.Exit(0);}catch{EditorApplication.Exit(1);}}
  static void Capture(Camera camera,string path)
  {
   var target=new RenderTexture(1400,900,24);target.Create();camera.targetTexture=target;camera.aspect=1400f/900;camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
   var image=new Texture2D(1400,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,900),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);
  }
 }
}
#endif
