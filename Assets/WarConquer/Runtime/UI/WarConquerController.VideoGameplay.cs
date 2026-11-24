using UnityEngine;
using UnityEngine.InputSystem;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void CheckVideoGameplayCommand()
        {
            var keyboard=Keyboard.current;
            if(keyboard==null||!keyboard.f9Key.wasPressedThisFrame)return;
            bool control=keyboard.leftCtrlKey.isPressed||keyboard.rightCtrlKey.isPressed;
            bool shift=keyboard.leftShiftKey.isPressed||keyboard.rightShiftKey.isPressed;
            if(control&&shift)StartVideoGameplay();
        }
        void StartVideoGameplay()
        {
            CloseModal();ClearAction();lastActor=-1;viewedPlayer=0;focus=-1;page=0;handFilter="Todas";useResources=false;
            humanPlayers=1;leaders[0]="ZUKGROK";leaders[1]="SAHRIA";seed=25112026;
            SetBackground("UI/Game_Background");mainMenuVisible=false;
            if(mainMenuRoot!=null)mainMenuRoot.gameObject.SetActive(false);
            if(matchRoot!=null)matchRoot.gameObject.SetActive(true);
            aiPlayer=new AiPlayer();nextAiAction=Time.unscaledTime+1f;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules(),1);
            PrototypeScenario.Load(Game);
            videoGameplay.Activate(Game);
            board.Reset();matchMusic?.StartMatch();SetMatchVisible(true);Render();
            Debug.Log("[VIDEO GAMEPLAY] MODO COREOGRAFIADO ACTIVADO · Ctrl+Shift+F9 · semilla 25112026.");
            Debug.Log("[VIDEO GAMEPLAY] GUÍA J1: despliega una unidad, avanza a Asalto, mueve hacia el centro, muestra un combate y termina terraformando varias casillas.");
        }
    }
}
