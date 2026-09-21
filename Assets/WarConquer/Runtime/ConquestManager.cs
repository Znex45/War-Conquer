using System.Linq;
namespace WarConquer
{
    public static class ConquestManager
    {
        public const int Goal=10;
        public static bool IsCenter(HexTile tile)=>tile.conquestSite;
        public static int EligibleOwner(GameState s,HexTile t)
        {
            if(t.unit==null||t.unit.owner!=t.owner||t.biome==Biome.Neutral||s.players[t.owner].eliminated)return -1;
            return t.conquestSite||(t.baseOwner>=0&&t.baseOwner!=t.owner&&s.players[t.baseOwner].eliminated)?t.owner:-1;
        }
        public static void Refresh(GameState s)
        {
            if(s.players.Count!=4)return;
            foreach(var t in s.tiles)
            {
                int owner=EligibleOwner(s,t);if(t.garrisonOwner==owner)continue;
                t.garrisonOwner=owner;t.garrisonSinceTurn=owner<0?-1:s.turn;
            }
        }
        public static int Income(GameState s,int owner)=>s.tiles.Count(t=>EligibleOwner(s,t)==owner);
        public static void ScoreStart(GameState s,int owner)
        {
            if(s.phase==Phase.Finished)return;Refresh(s);int points=0;
            foreach(var t in s.tiles.Where(t=>t.garrisonOwner==owner&&t.garrisonSinceTurn<s.turn&&t.lastConquestTurn!=s.turn))
            {points++;t.lastConquestTurn=s.turn;}
            if(points==0)return;
            var p=s.players[owner];p.conquestPoints+=points;s.Log("J"+(owner+1)+": +"+points+" PC por mantener unidad y bioma hasta su siguiente turno · "+p.conquestPoints+"/10.");
            if(p.conquestPoints>=Goal)Finish(s,owner,VictoryReason.Conquest);
        }
        public static void RoundCompleted(GameState s){Refresh(s);}
        public static void CheckLastLeader(GameState s)
        {var alive=s.players.Where(p=>!p.eliminated).ToList();if(alive.Count==1)Finish(s,alive[0].id,VictoryReason.LastLeader);}
        public static void Finish(GameState s,int player,VictoryReason reason)
        {s.winner=player;s.victoryReason=reason;s.phase=Phase.Finished;s.battle=null;s.responsePlayer=-1;s.Log("J"+(player+1)+" gana: "+Reason(reason)+".");}
        public static string Reason(VictoryReason reason)=>reason==VictoryReason.Conquest?"10 PUNTOS DE CONQUISTA":"ÚLTIMO LÍDER EN PIE";
    }
}
