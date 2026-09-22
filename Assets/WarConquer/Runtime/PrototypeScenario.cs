using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public static class PrototypeScenario
    {
        public static CardInstance Take(GameManager g,int player,string cardId)
        {
            var p=g.State.players[player];var card=p.deck.Concat(p.hand).FirstOrDefault(c=>c.cardId==cardId);
            if(card==null)throw new InvalidOperationException("No hay copia disponible: "+cardId);
            p.deck.Remove(card);p.hand.Remove(card);return card;
        }
        public static Piece Spawn(GameManager g,int player,string cardId,int tile)
        {var card=Take(g,player,cardId);g.State.tiles[tile].owner=player;return g.Place(cardId,player,tile,card.instanceId,false);}
        public static void Load(GameManager g)
        {
            var s=g.State;s.rules.initialEnergy=10;
            foreach(var player in s.players.Where(p=>!p.inactive))
            {
                player.currentEnergy=player.maxEnergy=10;player.spores=3;
                player.deck.AddRange(player.hand);player.hand.Clear();
                bool fungus=player.leader=="ZUKGROK";bool faunar=player.leader=="FAUNAR";
                string[] hand=faunar?new[]{"terraform","charming-mongoose","berry-bush","bober-reinforcer","ferret-eluding-hunter","panther-stealthy-assassin","bomb","time-to-move"}:fungus?new[]{"brote-repentino","bestia-micelial","semillero-micelial","espora-somnifera","red-micelial","rey-micelial","nube-de-esporas","marcha-micelial"}:
                    new[]{"nomada-de-arena","terraformacion-solar","erosion","arena-profunda","ruptura-del-terreno","titan-de-ceniza","mar-de-arena","obelisco-solar"};
                foreach(var id in hand){var c=Take(g,player.id,id);player.hand.Add(c);}
                var home=s.tiles.Where(t=>t.owner==player.id&&t.baseOwner<0).Take(5).ToList();
                foreach(var t in home)t.biome=fungus||faunar?Biome.Forest:Biome.Desert;
                home[4].biome=Biome.AshLand;
            }
            Func<int,int,HexTile> central=(q,r)=>s.tiles.First(t=>t.territory==4&&t.q==q&&t.r==r);
            int a=central(-1,0).id,b=central(0,0).id;
            central(-1,0).biome=Biome.Forest;central(0,0).biome=Biome.Forest;
            Spawn(g,0,s.players[0].leader=="FAUNAR"?"charming-mongoose":s.players[0].leader=="ZUKGROK"?"bestia-micelial":"nomada-de-arena",a);
            Spawn(g,1,s.players[1].leader=="FAUNAR"?"charming-mongoose":s.players[1].leader=="SAHRIA"?"nomada-de-arena":"bestia-micelial",b);
            if(s.players[0].leader=="ZUKGROK")
            {
                var r=central(-2,1);r.biome=Biome.Forest;var network=Spawn(g,0,"red-micelial",r.id);
                var end=central(1,1);end.biome=Biome.Forest;end.owner=0;TerrainManager.CreateFastRoute(g,a,end.id,network);
            }
            var ash=central(0,1);ash.biome=Biome.AshLand;ash.owner=0;
            var unstable=central(0,-1);unstable.biome=Biome.Desert;unstable.owner=1;
            unstable.specialEffect=new TerrainEffect {threshold=4,damage=2,onExit=true};
            s.Log("ESCENARIO DE PRUEBAS: preparación explícita; se conservan las 50 cartas de cada jugador.");
        }
    }
    public static class StateValidator
    {
        public static string Validate(GameState s,CardCatalog catalog)
        {
            if(s==null||s.version!=6||s.rules==null||s.players==null||s.players.Count!=4||s.tiles==null||s.mapPlayers<2||s.mapPlayers>4||s.tiles.Count<50)return "Partida incompatible con las etapas y la puntuación. Inicia una nueva partida.";
            if(s.activePlayer<0||s.activePlayer>3||s.turn<1)return "Turno inválido.";
            var expected=new GameState();BoardManager.Create(expected,s.mapPlayers);
            if(s.tiles.Count!=expected.tiles.Count||s.tiles.Where((t,i)=>t.q!=expected.tiles[i].q||t.r!=expected.tiles[i].r||t.conquestSite!=expected.tiles[i].conquestSite||!t.neighbors.SequenceEqual(expected.tiles[i].neighbors)).Any())return "Geometría de mapa inválida.";
            if(s.players.Count(p=>!p.inactive)!=s.mapPlayers||s.players[s.activePlayer].inactive)return "Participantes inválidos.";
            if(s.diceRolls==null||s.nextRollId<1||s.diceRolls.Any(d=>d.id<1||d.id>=s.nextRollId||d.value<1||d.value>6||d.tileId<0||d.tileId>=s.tiles.Count)||s.diceRolls.Select(d=>d.id).Distinct().Count()!=s.diceRolls.Count)return "Historial de dados inválido.";
            if(!Enum.IsDefined(typeof(TurnStage),s.stage)||s.lastScoredRound>s.round||s.responsePlayer < -1||s.responsePlayer>3)return "Ventana o ronda inválida.";
            if(s.battle!=null&&(s.stage!=TurnStage.Assault||s.battle.priorityPlayer<0||s.battle.priorityPlayer>3||s.battle.targetTile<0||s.battle.targetTile>=s.tiles.Count||s.battle.order.Count!=4||s.battle.order.Distinct().Count()!=4||s.battle.priorityIndex<0||s.battle.priorityIndex>3))return "Batalla inválida.";
            if(s.pendingChoices==null||s.pendingChoices.Any(c=>c.owner<0||c.owner>3||c.count<1||c.kind!="Discard"&&c.kind!="Panther"&&c.kind!="Emergency"||c.kind!="Discard"&&(c.tileId<0||c.tileId>=s.tiles.Count)))return "Elección pendiente inválida.";
            var validCards=new HashSet<string>(catalog.All.Select(c=>c.id));var ids=new HashSet<int>();
            for(int i=0;i<s.tiles.Count;i++)
            {
                var t=s.tiles[i];if(t.id!=i||t.owner<-1||t.owner>3||t.baseOwner<-1||t.baseOwner>3||t.neighbors.Any(n=>n<0||n>=s.tiles.Count))return "Casilla inválida.";
                if((t.owner>=0&&s.players[t.owner].inactive)||(t.baseOwner>=0&&s.players[t.baseOwner].inactive))return "Casilla asignada a un puesto vacío.";
                if(t.garrisonOwner < -1||t.garrisonOwner>3||t.garrisonSinceTurn>s.turn||t.lastConquestTurn>s.turn)return "Guarnición inválida.";
                if(t.unit!=null&&t.structure!=null&&!catalog[t.structure.cardId].Has("Passable"))return "Casilla ocupada dos veces.";
                foreach(var piece in new[]{t.unit,t.structure}.Where(p=>p!=null))
                {if(!validCards.Contains(piece.cardId)||piece.tileId!=i||piece.owner<0||piece.owner>3||piece.health<=0||!ids.Add(piece.id))return "Pieza inválida o duplicada.";}
            }
            foreach(var p in s.players)
            {
                if(p.currentEnergy<0||p.maxEnergy<0||p.id<0||p.id>3||p.spores<0||p.conquestPoints<0||p.pendingDraw<0||p.resources.Any(r=>r.amount<0))return "Recurso negativo.";
                var all=p.deck.Concat(p.hand).Concat(p.discardPile).ToList();
                foreach(var c in all)if(!validCards.Contains(c.cardId)||!ids.Add(c.instanceId))return "Carta desconocida o duplicada.";
                all.AddRange(BoardManager.Pieces(s).Where(b=>b.owner==p.id&&!b.token).Select(b=>new CardInstance {instanceId=b.id,cardId=b.cardId}));
                if(p.inactive){if(!p.eliminated||p.isAI||p.leaderHealth!=0||all.Count!=0)return "Puesto vacío inválido.";continue;}
                if(all.Count!=50)return "J"+(p.id+1)+": no se conservan sus 50 cartas.";
                foreach(var group in all.GroupBy(c=>c.cardId))if(group.Count()!=catalog[group.Key].quantity||catalog[group.Key].leader!=p.leader)return "Distribución de cartas alterada.";
            }
            if(s.fastRoutes.Any(f=>f.a<0||f.a>=s.tiles.Count||f.b<0||f.b>=s.tiles.Count||f.a==f.b))return "Vía rápida inválida.";
            return "";
        }
    }
}
