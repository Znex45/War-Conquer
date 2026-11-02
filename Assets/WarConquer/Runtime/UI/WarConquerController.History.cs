using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public partial class WarConquerController
    {
        Button rewindButton;Image rewindImage;Text rewindText;
        void RefreshRewind()
        {
            if(rewindButton==null)
            {
                // Keep reachable over choice dialogs and while dice are animating.
                // It is deliberately outside the match CanvasGroup input lock.
                rewindButton=GrayboxUI.Button(root,"Rewind / Deshacer",1210,10,182,28,UndoDecision,edge);
                rewindImage=rewindButton.GetComponent<Image>();rewindText=rewindButton.GetComponentInChildren<Text>();
            }
            rewindButton.gameObject.SetActive(!setupVisible&&Game.State.phase!=Phase.Setup);
            rewindButton.interactable=Game.CanUndo;
            rewindImage.color=Game.CanUndo?Color.Lerp(edge,Color.white,.56f):GrayboxUI.Disabled;
            rewindText.color=Game.CanUndo?GrayboxUI.ActionInk:GrayboxUI.Muted;
            if(rewindButton.transform.GetSiblingIndex()!=rewindButton.transform.parent.childCount-1)rewindButton.transform.SetAsLastSibling();
        }
        void UndoDecision()
        {
            if(!Game.CanUndo)return;
            CloseModal();ClearAction();discardSelection.Clear();displayedChoice=null;
            focus=-1;page=0;lastActor=-1;handFilter="Todas";useResources=false;
            aiPlayer=new AiPlayer();nextAiAction=Time.unscaledTime+1;
            Game.Undo();
        }
    }
}
