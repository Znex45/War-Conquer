#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace WarConquer.Editor
{
    public static class StructureAssemblyTests
    {
        const string Pending="StructureAssembly.Pending";static bool active;static double due;static int step;
        static GameManager game;static Board3DScene world;static Piece building;static Vector3[] final;static Transform[] parts;static string snapshot;
        public static void BeginBatch(){SessionState.SetBool(Pending,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Sync()=>world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false)){SessionState.SetBool(Pending,false);active=true;step=0;due=EditorApplication.timeSinceStartup+2;}
            if(!active||EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    var cards=CardCatalog.Load();game=new GameManager(cards);game.NewGame(CardCatalog.Leaders,42,CardCatalog.LoadRules(),4);
                    var card=cards.All.First(c=>c.IsStructure&&!c.Has("Passable"));
                    var tiles=game.State.tiles.Where(t=>t.unit==null&&t.structure==null&&!t.blocked).Take(3).ToArray();
                    var old=game.Place(card.id,0,tiles[0].id);world=new GameObject("Assembly QA").AddComponent<Board3DScene>();Sync();
                    Check(world.UnitVisual(old.id).GetComponent<BoardStructureAssembly>()==null,"Existing structure replayed on load");
                    building=game.Place(card.id,0,tiles[1].id);Sync();
                    var root=world.UnitVisual(building.id);Check(root.GetComponent<BoardStructureAssembly>().IsBuilding,"New structure not animated");
                    // Sculptures now have articulated/nested groups; inspect every staged mesh,
                    // excluding status labels added after Begin(), rather than just the plinth.
                    parts=root.GetComponentsInChildren<MeshFilter>().Select(f=>f.transform).Where(t=>t.localPosition.y>5).ToArray();final=parts.Select(t=>t.localPosition-Vector3.up*6).ToArray();
                    Check(parts.Length>1&&parts.All(t=>!t.GetComponent<Renderer>().enabled),"Parts not staged in sky");
                    var unit=game.Place(cards.All.First(c=>c.category==Category.Unit).id,0,tiles[2].id);Sync();Check(world.UnitVisual(unit.id).GetComponent<BoardStructureAssembly>()==null,"Unit received building animation");
                    snapshot=GamePersistence.Serialize(game.State);step=1;due=EditorApplication.timeSinceStartup+.18;return;
                }
                if(step==1)
                {
                    int shown=parts.Count(t=>t.GetComponent<Renderer>().enabled);Check(shown>0&&shown<parts.Length,"Parts did not fall sequentially");
                    Check(parts.Any((t)=>t.localPosition.y>2f),"No falling pieces");for(int i=0;i<10;i++)Sync();step=2;due=EditorApplication.timeSinceStartup+2.5;return;
                }
                Check(!world.IsAnimating,"Assembly restarted on refresh");
                for(int i=0;i<parts.Length;i++)Check(Vector3.Distance(parts[i].localPosition,final[i])<.001f&&parts[i].GetComponent<Renderer>().enabled,"Part did not settle exactly");
                Check(snapshot==GamePersistence.Serialize(game.State),"Assembly altered rules/state");
                game.Restore(GamePersistence.Deserialize(snapshot));Sync();Check(!world.IsAnimating&&world.UnitVisual(building.id).GetComponent<BoardStructureAssembly>()==null,"Restore replays construction");
                Debug.Log("STRUCTURE_ASSEMBLY_PASSED: new buildings only, staggered falling parts, exact assembly, stable refresh, unchanged state, restore without replay.");active=false;EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);active=false;EditorApplication.Exit(1);}
        }
    }
}
#endif
