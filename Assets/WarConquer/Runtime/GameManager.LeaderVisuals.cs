using System;
namespace WarConquer
{
    public sealed class LeaderAbilitySignal
    {
        public GameState state; public string leader; public int owner,tile,piece;
    }
    public partial class GameManager
    {
        // Presentation notification only, emitted after the existing rule resolves.
        public event Action<LeaderAbilitySignal> LeaderAbilityResolved;
        internal void SignalLeaderAbility(string leader,int owner,int tile=-1,int piece=-1)
        {LeaderAbilityResolved?.Invoke(new LeaderAbilitySignal{state=State,leader=leader,owner=owner,tile=tile,piece=piece});}
    }
}
