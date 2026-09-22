#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    // Explicit QA preview only; never runs from normal scene initialization.
    public static class FaunarVisualPreview
    {
        static WarConquerController ui;static bool batch;static int step;static double due,deadline;static EditorWindow view;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        static void Set(string name,object value)=>typeof(WarConquerController).GetField(name,Flags).SetValue(ui,value);
        public static void Begin(WarConquerController controller,bool exit)
        {
            ui=controller;batch=exit;step=0;Set("humanPlayers",4);Set("leaders",new[]{"FAUNAR","SAHRIA","ZUKGROK","FAUNAR"});Invoke("ShowSetup");
            view=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));view.maximized=true;view.Focus();view.Repaint();
            Directory.CreateDirectory("Library/WarConquer3D");due=EditorApplication.timeSinceStartup+2;deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;
        }
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/"+name+".png"));
        static void Tick()
        {
            view?.Repaint();EditorApplication.QueuePlayerLoopUpdate();
            if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(Screen.width<1000||Screen.height<600){if(EditorApplication.timeSinceStartup>deadline)throw new Exception("GameView no alcanzó una resolución legible para la captura.");return;}
                if(step==0){Capture("faunar-seleccion");step++;due=EditorApplication.timeSinceStartup+1;return;}
                if(step==1){Invoke("StartMatch",true);ui.Game.State.stage=TurnStage.Assault;ui.Game.Notify("Faunar · escenario de comprobación");step++;due=EditorApplication.timeSinceStartup+2;return;}
                if(step==2){Capture("faunar-partida");step++;due=EditorApplication.timeSinceStartup+1;return;}
                if(step==3)
                {
                    var g=ui.Game;g.State.Active.currentEnergy=20;var card=PrototypeScenario.Take(g,0,"a-price-to-pay");g.State.Active.hand.Add(card);Invoke("SelectCard",card,g.State.Active);step++;due=EditorApplication.timeSinceStartup+2;return;
                }
                if(step==4){Capture("faunar-descarte");step++;due=EditorApplication.timeSinceStartup+1;return;}
                Invoke("CloseModal");Set("humanPlayers",1);Invoke("StartMatch",false);Invoke("ShowSetup");
                EditorApplication.update-=Tick;Debug.Log("FAUNAR_VISUAL_PASSED: selección de tres mazos, partida 3D y elección de descarte renderizadas.");FactionVisualPreview.Begin(ui,batch);
            }
            catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);if(batch)EditorApplication.Exit(1);}
        }
    }
}
#endif
