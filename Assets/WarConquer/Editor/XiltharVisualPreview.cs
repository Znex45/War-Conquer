#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class XiltharVisualPreview
    {
        static WarConquerController ui;static bool batch;static int step;static double due;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(string method,params object[] args)=>typeof(WarConquerController).GetMethod(method,Flags).Invoke(ui,args);
        static void Set(string field,object value)=>typeof(WarConquerController).GetField(field,Flags).SetValue(ui,value);
        public static void Begin(WarConquerController controller,bool exit)
        {
            ui=controller;batch=exit;step=0;Set("humanPlayers",4);Set("leaders",new[]{"XIL'THAR","SAHRIA","ZUKGROK","FAUNAR"});Invoke("ShowSetup");due=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;
        }
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/xilthar-seleccion.png"));step++;due=EditorApplication.timeSinceStartup+1;return;}
                if(step==1)
                {
                    Invoke("StartMatch",true);var g=ui.Game;g.State.Active.currentEnergy=30;
                    foreach(var id in new[]{"the-eternal-eye","void-hole","dark-fly","portal","void-beacon","insolator"})
                    {
                        var t=g.State.tiles.Where(t=>!t.IsOccupied&&!t.blocked&&t.baseOwner<0).OrderBy(t=>BoardManager.AxialDistance(t,g.State.tiles.First(x=>x.q==0&&x.r==0))).First();
                        t.biome=Biome.Void;t.owner=0;PrototypeScenario.Spawn(g,0,id,t.id);
                    }
                    for(int i=0;i<4;i++){var t=g.State.tiles.First(t=>g.TerraformTarget(t.id)&&t.biome!=Biome.Void&&t.baseOwner<0);TerrainManager.Terraform(g,t.id,Biome.Void,0);}
                    ui.GetComponentInChildren<Board3DScene>().Zoom=2;g.Notify("XIL'THAR · Void, portales y retorno");step++;due=EditorApplication.timeSinceStartup+3;return;
                }
                if(step==2){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/xilthar-partida.png"));step++;due=EditorApplication.timeSinceStartup+1;return;}
                EditorApplication.update-=Tick;Debug.Log("XILTHAR_VISUAL_PASSED: selección, líder, cartas y Void 3D renderizados.");Set("humanPlayers",1);Invoke("StartMatch",false);Invoke("ShowSetup");if(batch)EditorApplication.Exit(0);
            }
            catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);if(batch)EditorApplication.Exit(1);}
        }
    }
}
#endif
