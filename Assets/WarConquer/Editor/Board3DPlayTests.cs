#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class Board3DPlayTests
    {
        const string Pending="WarConquer.3DPreviewPending",Batch="WarConquer.3DPreviewBatch";
        static double captureAt;
        static bool waiting;
        static double runAt;
        static bool runPending;
        static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Invoke(WarConquerController ui,string method,params object[] args)=>typeof(WarConquerController).GetMethod(method,Flags).Invoke(ui,args);
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        public static void BeginBatch()
        {
            SessionState.SetBool(Batch,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");Begin();
        }
        [MenuItem("War & Conquer/Comprobar y mostrar Graybox 3D")]
        public static void Begin()
        {
            Board3DAssets.Ensure();SessionState.SetBool(Pending,true);
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            EditorApplication.isPlaying=true;
        }
        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorApplication.update+=Poll;
            EditorApplication.playModeStateChanged+=state=>
            {
                if(!SessionState.GetBool(Pending,false))return;
                if(state==PlayModeStateChange.EnteredEditMode){EditorApplication.isPlaying=true;return;}
                if(state!=PlayModeStateChange.EnteredPlayMode)return;
                SessionState.SetBool(Pending,false);runPending=true;runAt=EditorApplication.timeSinceStartup+3;
            };
        }
        static void Poll()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            Requested();
            if(!runPending||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<runAt)return;
            var ui=UnityEngine.Object.FindAnyObjectByType<WarConquerController>();
            if(ui?.Game==null)return;
            runPending=false;Run();
        }
        static void Requested()
        {
            const string path="Temp/WarConquer.3DCheck.request";
            if(!File.Exists(path))return;
            if(File.Exists("Temp/WarConquer.InterfaceCheck.request"))File.Delete("Temp/WarConquer.InterfaceCheck.request");
            SessionState.SetBool("WarConquer.PendingInterfaceCheck",false);
            File.Delete(path);SessionState.SetBool(Batch,false);Begin();
        }
        static void Click(Board3DScene world,BoardViewportInput input,int tile,float height=BoardMeshFactory.Surface)
        {
            var uv=world.ViewportOf(tile,height);Check(world.Pick(uv)==tile,"La casilla de prueba está oculta.");
            var rect=(RectTransform)input.transform;
            var local=new Vector3(rect.rect.xMin+uv.x*rect.rect.width,rect.rect.yMin+uv.y*rect.rect.height,0);
            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(local)),button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(input.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(input.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
        static void Run()
        {
            try
            {
                Application.runInBackground=true;
                var ui=UnityEngine.Object.FindAnyObjectByType<WarConquerController>();Check(ui?.Game!=null,"No hay partida 3D activa.");
                InterfacePlayModeTests.Run();RevisionPlayTests.Run(ui);FaunarPlayTests.Run(ui);FactionPlayTests.Run(ui);
                typeof(WarConquerController).GetField("humanPlayers",Flags).SetValue(ui,4);Invoke(ui,"StartMatch",false);
                var world=ui.GetComponentInChildren<Board3DScene>();var input=ui.GetComponentInChildren<BoardViewportInput>();Canvas.ForceUpdateCanvases();
                Check(world!=null&&world.Texture!=null&&world.Texture.IsCreated()&&input!=null,"No existe la vista 3D interactiva.");
                var card=PrototypeScenario.Take(ui.Game,0,"alligator-revengeful-bite");ui.Game.State.Active.hand.Add(card);ui.Game.State.Active.currentEnergy=5;Invoke(ui,"SelectCard",card,ui.Game.State.Active);
                int target=ui.Game.CardTargets(ui.Game.Catalog[card.cardId]).First(id=>world.Pick(world.ViewportOf(id))==id);
                Click(world,input,target);Check(ui.Game.State.tiles[target].unit!=null&&ui.Game.State.Active.currentEnergy==0,"El clic 3D no coloca ni paga la carta.");
                var piece=ui.Game.State.tiles[target].unit;ui.Game.AdvanceStage();ui.Game.AdvanceStage();Click(world,input,target,.9f);
                int destination=MovementManager.Paths(ui.Game,piece).Keys.First(id=>world.Pick(world.ViewportOf(id))==id);
                Click(world,input,destination);Check(piece.tileId==destination&&world.PieceCount==1,"El clic 3D no mueve la unidad.");
                Invoke(ui,"StartMatch",true);var g=ui.Game;
                Spawn(g,0,"lushroom",Biome.Swamp);Spawn(g,1,"sandy-anthill",Biome.Desert);
                Spawn(g,2,"frogpit",Biome.Swamp);Spawn(g,3,"sandy-ember",Biome.Desert);
                g.Notify("Escenario de pruebas 3D · Nueva partida permite elegir personas y mazos.");
                Check(world.PieceCount==BoardManager.Pieces(g.State).Count()&&StateValidator.Validate(g.State,g.Catalog)=="","Modelos o cartas del escenario incorrectos.");
                var view=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));view.maximized=true;view.Focus();
                captureAt=EditorApplication.timeSinceStartup+4;waiting=true;EditorApplication.update+=Capture;
            }
            catch(Exception e){Debug.LogException(e);if(SessionState.GetBool(Batch,false))EditorApplication.Exit(1);}
        }
        static void Spawn(GameManager g,int owner,string id,Biome biome)
        {
            var center=g.State.tiles.First(ConquestManager.IsCenter);
            var tile=g.State.tiles.Where(t=>t.owner==owner&&!t.IsOccupied&&t.baseOwner<0).OrderBy(t=>BoardManager.AxialDistance(t,center)).First();
            tile.biome=biome;PrototypeScenario.Spawn(g,owner,id,tile.id);
        }
        static void Capture()
        {
            if(!waiting||EditorApplication.timeSinceStartup<captureAt)return;
            waiting=false;EditorApplication.update-=Capture;
            try
            {
                var world=UnityEngine.Object.FindAnyObjectByType<Board3DScene>();
                string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/WarConquer3D"));Directory.CreateDirectory(folder);
                var previous=RenderTexture.active;var texture=new Texture2D(world.Texture.width,world.Texture.height,TextureFormat.RGB24,false);
                try
                {
                    RenderTexture.active=world.Texture;texture.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);texture.Apply();
                    var pixels=texture.GetPixels32();Check(pixels.Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count()>50,"La cámara 3D está vacía.");
                    Check(pixels.Count(c=>c.r>220&&c.b>220&&c.g<40)<pixels.Length/20,"Shader rosa en la vista 3D.");
                    File.WriteAllBytes(Path.Combine(folder,"tablero-3d.png"),texture.EncodeToPNG());
                }
                finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(texture);}
                // Unity queues this capture at the end of the frame, after GameView resizes.
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"interfaz-3d.png"));
                Debug.Log("WAR_CONQUER_3D_PLAY_PASSED: cámara renderizada, modelos, interfaz, colocación y movimiento con clic 3D. "+folder);
                bool exit=SessionState.GetBool(Batch,false);SessionState.SetBool(Batch,false);RevisionVisualPreview.Begin(UnityEngine.Object.FindAnyObjectByType<WarConquerController>(),exit);
            }
            catch(Exception e){Debug.LogException(e);if(SessionState.GetBool(Batch,false))EditorApplication.Exit(1);}
        }
    }
}
#endif
