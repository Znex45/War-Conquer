#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class FactionVisualPreview
    {
        static WarConquerController ui;static bool batch;static int step;static double due;static EditorWindow view;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Set(string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        public static void Begin(WarConquerController controller,bool exit)
        {
            ui=controller;batch=exit;step=0;Set("humanPlayers",4);Set("leaders",new[]{"SAHRIA","ZUKGROK","FAUNAR","ZUKGROK"});Invoke("ShowSetup");
            view=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));view.maximized=true;view.Focus();
            due=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;
        }
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/"+name+".png"));
        static void Tick()
        {
            view?.Repaint();EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0){Capture("mazos-actualizados");step++;due=EditorApplication.timeSinceStartup+1;return;}
                if(step==1){Invoke("StartMatch",true);ui.Game.State.Active.sahriaDeployment=true;ui.Game.Notify("SAHRIA · cartas actualizadas");step++;due=EditorApplication.timeSinceStartup+2;return;}
                if(step==2){Capture("sahria-actualizada");step++;due=EditorApplication.timeSinceStartup+1;return;}
                if(step==3)
                {
                    var g=ui.Game;var tile=g.State.tiles.First(t=>t.q==2&&t.r==0);var token=g.Place("firefly-token",1,tile.id);CombatManager.Remove(g,token);g.Notify("ZUKGROK · elegir bioma por muerte de Token");step++;due=EditorApplication.timeSinceStartup+2;return;
                }
                if(step==4){Capture("zukgrok-terraformacion-token");step++;due=EditorApplication.timeSinceStartup+1;return;}
                ChoiceManager.Resolve(ui.Game,new[]{(int)Biome.Swamp});Invoke("CloseModal");Set("humanPlayers",1);Invoke("StartMatch",false);Invoke("ShowSetup");
                EditorApplication.update-=Tick;Debug.Log("FACTION_VISUAL_PASSED: selección, SAHRIA, ZUKGROK y bioma de muerte renderizados.");if(batch)EditorApplication.Exit(0);
            }
            catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);if(batch)EditorApplication.Exit(1);}
        }
    }
}
#endif
