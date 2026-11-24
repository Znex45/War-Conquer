using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WarConquer
{
    public partial class WarConquerController
    {
        string announcedStage;
        Coroutine stageRoutine;
        RectTransform stageBanner;
        void AnnounceStage()
        {
            var s=Game.State;
            if(setupVisible||s.phase==Phase.Setup||s.phase==Phase.Finished){announcedStage=null;return;}
            string key=s.round+"/"+s.activePlayer+"/"+s.stage;
            if(key==announcedStage)return;
            announcedStage=key;
            if(stageRoutine!=null)StopCoroutine(stageRoutine);
            if(stageBanner!=null)Destroy(stageBanner.gameObject);
            stageRoutine=StartCoroutine(StageNotice(TimingRules.StageName(s.stage),s.activePlayer));
        }
        IEnumerator StageNotice(string label,int owner)
        {
            stageBanner=GrayboxUI.Box(root,"Cambio de fase",350,270,900,130,new Color(.025f,.04f,.07f,.95f));
            var group=stageBanner.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            var title=GrayboxUI.Text(stageBanner,label.ToUpperInvariant(),20,20,860,52,36,GrayboxUI.PlayerColor(owner),FontStyle.Bold);title.alignment=TextAnchor.MiddleCenter;
            var sub=GrayboxUI.Text(stageBanner,"TURNO J"+(owner+1),20,80,860,28,20);sub.alignment=TextAnchor.MiddleCenter;
            float t=0;
            while(t<1.6f){group.alpha=Mathf.Min(Mathf.Clamp01(t/.15f),Mathf.Clamp01((1.6f-t)/.35f));t+=Time.unscaledDeltaTime;yield return null;}
            Destroy(stageBanner.gameObject);stageBanner=null;stageRoutine=null;
        }
        bool CanChoosePiece(Piece p,string action)=>action=="ability"?AbilityManager.CanActivate(Game,p):action=="attack"?CombatManager.Targets(Game,p).Count>0:MovementManager.Paths(Game,p).Count>0;
        void ChooseActionPiece(string action,int choicePage=0)
        {
            var options=Game.Allies(Game.ActingPlayerId).Where(p=>CanChoosePiece(p,action)).ToList();
            if(options.Count==0)return;
            if(options.Count==1){UseActionPiece(options[0],action);return;}
            var body=OpenModal(action=="ability"?"Elige la pieza que usará su efecto":action=="move"?"Elige la unidad que quieres mover":"Elige la unidad atacante");
            int pages=(options.Count+7)/8;choicePage=Mathf.Clamp(choicePage,0,pages-1);
            int index=0;
            foreach(var p in options.Skip(choicePage*8).Take(8))
            {
                var piece=p;var data=Game.Data(piece);
                GrayboxUI.Button(body,data.name+" · Casilla "+(piece.tileId+1)+" · Vida "+piece.health,35,90+index++*70,1060,58,()=>{CloseModal();UseActionPiece(piece,action);});
            }
            int current=choicePage;
            if(current>0)GrayboxUI.Button(body,"Anterior",35,690,230,45,()=>ChooseActionPiece(action,current-1));
            if(current+1<pages)GrayboxUI.Button(body,"Siguiente",865,690,230,45,()=>ChooseActionPiece(action,current+1));
        }
        void UseActionPiece(Piece piece,string action)
        {
            if(!Game.Allies(Game.ActingPlayerId).Contains(piece)||!CanChoosePiece(piece,action))return;
            ClearAction();selectedPiece=piece;focus=piece.tileId;
            if(action=="ability"&&AbilityManager.TargetCount(Game.Data(piece))==0){AbilityManager.Activate(Game,piece,new int[0]);return;}
            Begin(action);Render();
        }
    }
}
