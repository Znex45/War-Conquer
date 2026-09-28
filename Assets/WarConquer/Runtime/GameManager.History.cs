using System;
using System.Collections.Generic;

namespace WarConquer
{
    public partial class GameManager
    {
        // History lives outside GameState so a snapshot never contains other snapshots.
        readonly Stack<string> turnHistory=new Stack<string>();
        int historyTurn=-1,historyPlayer=-1,decisionDepth;
        bool resolvingDecision;
        public int UndoCount=>SameHistoryTurn?turnHistory.Count:0;
        bool SameHistoryTurn=>State!=null&&State.turn==historyTurn&&State.activePlayer==historyPlayer;
        public bool CanUndo=>UndoCount>0&&!State.Active.isAI&&decisionDepth==0
            &&(State.phase==Phase.Actions||State.phase==Phase.Finished);
        internal bool IsCurrentPiece(Piece piece)=>piece!=null&&piece.tileId>=0&&piece.tileId<State.tiles.Count
            &&(State.tiles[piece.tileId].unit==piece||State.tiles[piece.tileId].structure==piece);

        internal void ResetHistory()
        {
            turnHistory.Clear();historyTurn=-1;historyPlayer=-1;resolvingDecision=false;
        }

        // All effects, response windows and mandatory choices caused by one action are
        // part of that action. Never expose a paid-for spell or a half-resolved death as
        // an undo checkpoint. Invalid attempts and UI-only selections add no entries.
        internal bool Decide(Func<bool> action)
        {
            if(decisionDepth>0||State==null)return action();
            if(!SameHistoryTurn){ResetHistory();historyTurn=State.turn;historyPlayer=State.activePlayer;}
            var originalState=State;
            string before=GamePersistence.Serialize(State);
            bool continuing=resolvingDecision;
            int turn=State.turn,player=State.activePlayer;
            decisionDepth++;
            try
            {
                bool success=action();
                if(State.turn!=turn||State.activePlayer!=player||State!=originalState)ResetHistory();
                else if(success&&before!=GamePersistence.Serialize(State))
                {
                    if(!continuing)turnHistory.Push(before);
                    resolvingDecision=State.pendingChoices.Count>0||State.battle!=null||State.responsePlayer>=0;
                }
                return success;
            }
            catch
            {
                State=GamePersistence.Deserialize(before);
                throw;
            }
            finally
            {
                decisionDepth--;
                Changed?.Invoke();
            }
        }

        public bool Undo()
        {
            if(!CanUndo)return Fail("No hay decisiones de este turno que deshacer.");
            // The saved randomState, next IDs and dice records are restored together.
            // Repeating the action therefore consumes exactly the same random stream.
            var restored=GamePersistence.Deserialize(turnHistory.Peek());
            turnHistory.Pop();State=restored;queryPlayer=-1;resolvingDecision=false;
            Notify("Última decisión deshecha · "+TimingRules.StageName(State.stage)+".");
            return true;
        }

        public bool SetSahriaDeployment(bool enabled)=>Decide(()=>
        {
            if(!CanTakeTurnAction(TurnStage.Deployment)||State.Active.leader!="SAHRIA")return Fail("Despliegue de Sahria no disponible.");
            State.Active.sahriaDeployment=enabled;Notify("Despliegue de Sahria actualizado.");return true;
        });
    }
}
