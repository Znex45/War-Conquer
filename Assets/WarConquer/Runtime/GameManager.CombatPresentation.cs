using System;

namespace WarConquer
{
    public enum CombatVisualKind { Attack, Damage, Death, LeaderDamage, SpellDraw, Terraform, Buff, Target }

    // Transient presentation messages are intentionally outside the saved game and undo history.
    public readonly struct CombatVisualSignal
    {
        public readonly GameState state;
        public readonly CombatVisualKind kind;
        public readonly bool spellUnitDamage;
        public readonly int piece,source,tile,sourceTile,amount,owner,range;
        public CombatVisualSignal(GameState state,CombatVisualKind kind,int piece,int source,int tile,int sourceTile,int amount,int owner,int range=0,bool spellUnitDamage=false)
        {this.state=state;this.kind=kind;this.piece=piece;this.source=source;this.tile=tile;this.sourceTile=sourceTile;this.amount=amount;this.owner=owner;this.range=range;this.spellUnitDamage=spellUnitDamage;}
    }

    public partial class GameManager
    {
        public event Action<CombatVisualSignal> CombatVisual;
        internal void ReportAttack(Piece attacker,int target,int visualRange=-1)=>CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Attack,attacker.id,attacker.id,target,attacker.tileId,0,attacker.owner,visualRange<0?Data(attacker).range:visualRange));
        internal void ReportDamage(Piece target,int amount,Piece source=null,bool spellDamage=false)
        {if(amount>0)CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Damage,target.id,source?.id??-1,target.tileId,source?.tileId??target.tileId,amount,target.owner,spellUnitDamage:spellDamage&&!Data(target).IsStructure));}
        internal void ReportDeath(Piece target)=>CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.Death,target.id,-1,target.tileId,target.tileId,0,target.owner));
        internal void ReportLeaderDamage(Player target,int tile,int amount,Piece source)
        {if(amount>0)CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.LeaderDamage,-1,source.id,tile,source.tileId,amount,target.id));}

        internal void DrawFromSpell(Player player,int count)
        {
            int previousHandSize=player.hand.Count;
            DeckManager.Draw(State,player,count);
            int drawn=player.hand.Count-previousHandSize;
            if(drawn>0)
                CombatVisual?.Invoke(new CombatVisualSignal(State,CombatVisualKind.SpellDraw,-1,-1,-1,-1,drawn,player.id));
        }

        internal void ReportBoardVfx(CombatVisualKind kind,int tile,int owner)
        {CombatVisual?.Invoke(new CombatVisualSignal(State,kind,-1,-1,tile,tile,0,owner));}
    }
}
