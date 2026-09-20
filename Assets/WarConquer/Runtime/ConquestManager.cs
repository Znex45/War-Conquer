using System.Linq;

namespace WarConquer
{
    public static class ConquestManager
    {
        public const int Goal=10;
        public static bool IsCenter(HexTile tile)=>tile.territory==4&&tile.q==0&&tile.r==0;
        public static int Income(GameState s,int owner)=>s.tiles.Count(t=>t.owner==owner&&(IsCenter(t)||(t.baseOwner>=0&&t.baseOwner!=owner)));
        public static void RoundCompleted(GameState s)
        {
            if(s.phase==Phase.Finished||s.lastScoredRound>=s.round)return;
            s.lastScoredRound=s.round;
            foreach(var p in s.players.Where(p=>!p.eliminated))
            {
                int points=Income(s,p.id);p.conquestPoints+=points;
                if(points>0)s.Log("J"+(p.id+1)+": +"+points+" Conquista · "+p.conquestPoints+"/10.");
                if(p.conquestPoints>=Goal){Finish(s,p.id,VictoryReason.Conquest);return;}
            }
        }
        public static void CheckLastLeader(GameState s)
        {
            var alive=s.players.Where(p=>!p.eliminated).ToList();
            if(alive.Count==1)Finish(s,alive[0].id,VictoryReason.LastLeader);
        }
        public static void Finish(GameState s,int player,VictoryReason reason)
        {
            s.winner=player;s.victoryReason=reason;s.phase=Phase.Finished;s.battle=null;s.responsePlayer=-1;
            s.Log("J"+(player+1)+" gana: "+Reason(reason)+".");
        }
        public static string Reason(VictoryReason reason)=>reason==VictoryReason.Conquest?"10 PUNTOS DE CONQUISTA":"ÚLTIMO LÍDER EN PIE";
    }
}
