#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
 public static class OptimizationPreservationTests
 {
  static int ReferenceDistance(GameState s,int a,int b,int limit)
  {
   if(a==b)return 0;var seen=new HashSet<int>{a};var queue=new Queue<(int,int)>();queue.Enqueue((a,0));
   while(queue.Count>0){var current=queue.Dequeue();if(current.Item2>=limit)continue;foreach(var n in s.tiles[current.Item1].neighbors){if(n==b)return current.Item2+1;if(seen.Add(n))queue.Enqueue((n,current.Item2+1));}}return 999;
  }
  static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
  static string Paths(Dictionary<int,List<int>> paths)=>string.Join(";",paths.OrderBy(p=>p.Key).Select(p=>p.Key+":"+string.Join(",",p.Value)));
  public static void Run()
  {
   int distances=0,paths=0;var catalog=CardCatalog.Load();
   foreach(int count in new[]{2,3,4})
   {
    var state=new GameState();BoardManager.Create(state,count);
    foreach(int limit in new[]{-1,0,1,3,12,100})for(int i=0;i<100;i++)
    {int a=(i*17)%state.tiles.Count,b=(i*31+7)%state.tiles.Count;Check(BoardManager.Distance(state,a,b,limit)==ReferenceDistance(state,a,b,limit),"Graph distance changed");distances++;}
    foreach(var tile in state.tiles)tile.neighbors.Remove(0);state.tiles[0].neighbors.Clear();
    Check(BoardManager.Distance(state,0,1)==ReferenceDistance(state,0,1,100),"Disconnected topology changed");
   }
   foreach(string leader in CardCatalog.Leaders)
   foreach(var card in catalog.Deck(leader).Where(c=>c.category==Category.Unit&&c.movement>0).Take(3))
   {
    var game=new GameManager(catalog);game.NewGame(new[]{leader,"FAUNAR","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules(),4);game.State.stage=TurnStage.Assault;
    int id=game.State.tiles.First(t=>t.owner==0&&t.baseOwner<0).id;var piece=PrototypeScenario.Spawn(game,0,card.id,id);piece.playedTurn=game.State.turn-1;
    foreach(var tile in game.State.tiles)if(tile.baseOwner<0)tile.biome=(Biome)(1+tile.id%8);
    foreach(int budget in new[]{0,1,2,4})
    {piece.remainingMovement=budget;Check(Paths(MovementManager.Paths(game,piece))==Paths(ReferenceMovementPaths.Paths(game,piece)),"Path or tie order changed: "+card.id);paths++;}
    string normal=GamePersistence.Serialize(game.State),compact=GamePersistence.Serialize(game.State,false);
    Check(compact.Length<normal.Length,"History not compact");Check(GamePersistence.Serialize(GamePersistence.Deserialize(compact))==GamePersistence.Serialize(GamePersistence.Deserialize(normal)),"Save compatibility changed");
    Check(BoardManager.Pieces(game.State).SequenceEqual(game.State.tiles.SelectMany(t=>new[]{t.unit,t.structure}).Where(p=>p!=null)),"Piece ordering changed");
   }
   Directory.CreateDirectory("Library/OptimizationQA");File.WriteAllText("Library/OptimizationQA/preservation.txt","PASS: "+distances+" differential BFS queries across 2/3/4-player maps; disconnected topology; "+paths+" exact movement/path-order comparisons against the original algorithm; compact/pretty snapshot compatibility; piece enumeration order.");
   NeonReview.Run();EnvironmentReview.Run();Check(File.ReadAllText("Library/EnvironmentQA/verificacion-entornos.txt").StartsWith("PASS"),"Environment review failed");Debug.Log("OPTIMIZATION_PRESERVATION_PASSED");
  }
  public static void Batch(){try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
