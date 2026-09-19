using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace WarConquer
{
    // Explicit command-line verification hook; inactive in ordinary play.
    public class GrayboxCapture : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"--wc-capture")>=0)new GameObject("Graybox Capture",typeof(GrayboxCapture));
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--wc-capture");
            if(index<0||index+1>=args.Length)yield break;
            yield return new WaitForSeconds(2);
            var controller=FindAnyObjectByType<WarConquerController>();
            if(Array.IndexOf(args,"--wc-demo")>=0){PrototypeScenario.Load(controller.Game);controller.Game.Notify("Escenario de pruebas: combate, ceniza, red micelial y terreno inestable preparados.");}
            Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            string path=args[index+1];ScreenCapture.CaptureScreenshot(path);
            File.WriteAllText(Path.ChangeExtension(path,"json"),JsonUtility.ToJson(controller.Game.State,true));
            Debug.Log("WAR_CONQUER_RENDER_OK "+path);yield return new WaitForSeconds(2);
            if(Array.IndexOf(args,"--wc-exit")>=0)Application.Quit();
        }
    }
}
