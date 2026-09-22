#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    // Runs only from the explicit editor preview command. Saves QA images outside Assets.
    public static class RevisionVisualPreview
    {
        static WarConquerController ui;static int step;static double due;static bool batch;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(string name,params object[] args)=>typeof(WarConquerController).GetMethod(name,Flags).Invoke(ui,args);
        public static void Begin(WarConquerController controller,bool exit)
        {
            ui=controller;batch=exit;step=0;Map(2);EditorApplication.update+=Tick;
        }
        static void Map(int humans)
        {
            typeof(WarConquerController).GetField("humanPlayers",Flags).SetValue(ui,humans);Invoke("StartMatch",false);due=EditorApplication.timeSinceStartup+1.5;
        }
        static void Snapshot(string name)
        {
            var world=ui.GetComponentInChildren<Board3DScene>();var previous=RenderTexture.active;
            var texture=new Texture2D(world.Texture.width,world.Texture.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=world.Texture;texture.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);texture.Apply();
                var folder=Path.GetFullPath("Library/WarConquer3D");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());}
            finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(texture);}
        }
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step<3)
                {
                    Snapshot("mapa-"+(step+2));step++;
                    if(step<3){Map(step+2);return;}
                    Invoke("StartMatch",true);var g=ui.Game;var units=BoardManager.Pieces(g.State).Where(p=>!g.Data(p).IsStructure).ToArray();
                    EffectManager.Poison(g,units[0],1,1);EffectManager.Sleep(g,units[1],0);g.State.stage=TurnStage.Assault;
                    var target=g.State.tiles.First(t=>t.conquestSite&&!t.IsOccupied);g.RollDie(units[0],target,4,"Comprobación visual del dado");g.Notify("Escenario de pruebas · veneno, sueño y dado 3D.");
                    due=EditorApplication.timeSinceStartup+2.1;return;
                }
                var world=ui.GetComponentInChildren<Board3DScene>();
                if(!world.IsAnimating||!world.GetComponentsInChildren<TextMesh>().Any(t=>t.name=="Resultado del dado"&&t.text.Contains("D6  "+ui.Game.State.diceRolls.Last().value)))throw new Exception("El dado no muestra el resultado real.");
                Snapshot("estados-y-dado");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/WarConquer3D/interfaz-estados.png"));
                Debug.Log("WAR_CONQUER_MAPS_DICE_PASSED: tres mapas renderizados, veneno, sueño y resultado real del dado 3D.");
                EditorApplication.update-=Tick;FaunarVisualPreview.Begin(ui,batch);
            }
            catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);if(batch)EditorApplication.Exit(1);}
        }
    }
}
#endif
