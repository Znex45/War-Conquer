using System;

namespace WarConquer
{
    public enum CombatVisualKind { Attack, Damage, Death, LeaderDamage }

    // Transient presentation messages are intentionally outside the saved game and undo history.
    public readonly struct CombatVisualSignal
    {
        public readonly GameState state;
        public readonly CombatVisualKind kind;
        public readonly int piece,source,tile,sourceTile,amount,owner,range;
        public CombatVisualSignal(GameState state,CombatVisualKind kind,int piece,int source,int tile,int sourceTile,int amount,int owner,int range=0)
        {this.state=state;this.kind=kind;this.piece=piece;this.source=source;this.tile=tile;this.sourceTile=sourceTile;this.amount=amount;this.owner=owner;this.range=range;}
    }

    public partial class GameManager
    {
        public event Action<CombatVisualSignal> CombatVisual;
        internal void ReportAttack(Piece attacker,int target,int visualRange=-1)=>CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Attack,attacker.id,attacker.id,target,attacker.tileId,0,attacker.owner,visualRange<0?Data(attacker).range:visualRange));
        internal void ReportDamage(Piece target,int amount,Piece source=null)
        {if(amount>0)CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Damage,target.id,source?.id??-1,target.tileId,source?.tileId??target.tileId,amount,target.owner));}
        internal void ReportDeath(Piece target)=>CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Death,target.id,-1,target.tileId,target.tileId,0,target.owner));
        internal void ReportLeaderDamage(Player target,int tile,int amount,Piece source)
        {if(amount>0)CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.LeaderDamage,-1,source.id,tile,source.tileId,amount,target.id));}
    }
}
