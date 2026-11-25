namespace WarConquer
{
    public enum CardFlowKind { Draw, Play, Discard }

    // A short-lived presentation event. Game snapshots contain no animation state.
    public sealed class CardFlow
    {
        public readonly CardFlowKind Kind;
        public readonly int Player, Instance, SourceTile;
        public readonly bool FromDiscard;
        public readonly string CardId, Payment, Targets;
        public readonly PendingBattle Battle;

        public CardFlow(CardFlowKind kind, int player, CardInstance card, int sourceTile = -1,
            string payment = null, string targets = null, PendingBattle battle = null, bool fromDiscard = false)
        {
            Kind = kind;
            Player = player;
            Instance = card.instanceId;
            CardId = card.cardId;
            SourceTile = sourceTile;
            Payment = payment;
            Targets = targets;
            Battle = battle;
            FromDiscard = fromDiscard;
        }
    }
}
