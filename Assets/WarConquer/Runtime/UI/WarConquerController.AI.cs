using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        AiPlayer aiPlayer=new AiPlayer();
        float nextAiAction;
        void UpdateAI()
        {
            // Opening any dialog pauses the AI, including the new-match/deck chooser.
            if(modal!=null||Game==null||!Game.CanAct||!Game.ActingPlayer.isAI||board.World.IsAnimating)
            {nextAiAction=Time.unscaledTime+.75f;return;}
            if(Time.unscaledTime<nextAiAction)return;
            nextAiAction=Time.unscaledTime+.65f;
            aiPlayer.Step(Game);
        }
    }
}
