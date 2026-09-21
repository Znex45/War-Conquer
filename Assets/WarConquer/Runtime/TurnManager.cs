using System;
using System.Linq;

namespace WarConquer
{
    public static class TurnManager
    {
        public static void Start(GameManager g)
        {
            var s=g.State;var p=s.Active;s.phase=Phase.Start;
            ConquestManager.ScoreStart(s,p.id);if(s.phase==Phase.Finished)return;p.turnsTaken++;
            p.maxEnergy=Math.Min(s.rules.maxEnergy,s.rules.initialEnergy+(p.turnsTaken-1)*s.rules.energyGrowth);
            p.currentEnergy=p.maxEnergy;p.terraformDiscountUsed=false;p.towerUsed=false;p.structureDiscount=0;
            foreach(var tile in s.tiles)
            {
                var e=tile.specialEffect;if(e!=null&&((e.expiresTurn>=0&&s.turn>=e.expiresTurn)||(e.expiresRound>=0&&s.round>=e.expiresRound)))tile.specialEffect=null;
            }
            foreach(var unit in g.Allies(p.id).ToList())
            {
                var c=g.Data(unit);unit.attacked=false;unit.abilityUsed=false;unit.bonusAttack=0;unit.moved=false;unit.fastBonusUsed=false;
                if(unit.poison>0)
                {
                    CombatManager.PoisonDamage(g,unit,unit.poison);unit.poisonTurns++;
                    unit.poison=unit.poison>int.MaxValue/2?int.MaxValue:unit.poison*2;
                    if(unit.health<=0)continue;
                }
                if(c.Has("Evolve")&&!unit.evolved&&unit.forestSinceTurn>=0&&s.turn-unit.forestSinceTurn>=4&&s.tiles[unit.tileId].biome==Biome.Forest)
                {unit.evolved=true;unit.health+=s.rules.evolvedHealth-c.health;s.Log("Larva Micelial → Espora Evolucionada.");}
                int aura=BoardManager.Nearby(s,unit.tileId).Count(a=>a.owner==p.id&&g.Data(a).Has("NomadAura")&&c.subtypes.Contains("Nómada")&&s.tiles[unit.tileId].biome==Biome.Desert);
                unit.remainingMovement=g.IsSleeping(unit)?0:Math.Max(0,(unit.evolved?s.rules.evolvedMovement:c.movement)+aura-(unit.slowUntilTurn>=s.turn?unit.slow:0));
                if(c.Has("Evolve")&&s.tiles[unit.tileId].biome==Biome.Forest&&unit.forestSinceTurn<0)unit.forestSinceTurn=s.turn;
            }
            // Income is tied to a biome; neutral hexes never generate spendable biome resources.
            foreach(var tile in s.tiles.Where(t=>t.owner==p.id&&t.biome!=Biome.Neutral&&t.biome!=Biome.AshLand))
                EnergyManager.AddResource(s,p,tile.biome,Math.Max(1,tile.resource));
            foreach(var piece in g.Allies(p.id).ToList())EffectManager.OnStart(g,piece);
            p.pendingDraw=p.turnsTaken>1||s.rules.drawOnFirstTurn?s.rules.drawPerTurn:0;
            DeckManager.Draw(s,p,p.pendingDraw);p.pendingDraw=0;
            s.stage=TurnStage.Deployment;
            s.phase=Phase.Actions;s.Log("Ronda "+s.round+" · J"+(p.id+1)+" · Energía "+p.currentEnergy+"/"+p.maxEnergy);
        }
        public static void End(GameManager g)
        {
            var s=g.State;s.phase=Phase.End;
            foreach(var piece in g.Allies(s.activePlayer))
            {
                piece.bonusAttack=0;
                if(piece.sleepUntilTurn<=s.turn)piece.sleepUntilTurn=0;
                if(piece.slowUntilTurn<=s.turn){piece.slow=0;piece.slowUntilTurn=0;}
                if(g.Data(piece).Has("Evolve")&&piece.moved)piece.forestSinceTurn=-1;
            }
            s.Active.freeSteps=0;
            do
            {
                int next=(s.activePlayer+1)%4;
                if(next==0){ConquestManager.RoundCompleted(s);if(s.phase==Phase.Finished)return;s.round++;}
                s.activePlayer=next;s.turn++;
            }while(s.Active.eliminated);
            Start(g);
        }
    }
}
