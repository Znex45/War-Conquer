using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public class GameManager
    {
        public GameState State { get; private set; }
        public CardCatalog Catalog { get; }
        public string LastMessage { get; private set; } = "Selecciona una carta o una casilla.";
        public event Action Changed;
        public GameManager(CardCatalog catalog) { Catalog=catalog; }
        public void NewGame(string[] leaders, int seed, Rules rules)
        {
            State=new GameState {seed=seed,randomState=seed==0?12345:seed,rules=rules,phase=Phase.Setup};
            BoardManager.Create(State);
            for(int i=0;i<4;i++)
            {
                var p=new Player {id=i,leader=leaders[i],factionTag=leaders[i]=="ZUKGROK"?"MICELIAL":"SOLAR",leaderHealth=rules.leaderHealth};
                State.players.Add(p); DeckManager.Build(State,p,Catalog);
            }
            State.Log("Partida local • semilla "+seed+" • 87 casillas sin bioma • 4 mazos de 50.");
            TurnManager.Start(this); Notify("Turno de J1. Selecciona una carta o terraforma una casilla.");
        }
        public void Restore(GameState state) { State=state; Notify("Partida cargada."); }
        public void Notify(string message) { LastMessage=message; Changed?.Invoke(); }
        public bool Fail(string reason) { Notify(reason); return false; }
        public bool CanAct => State!=null && State.phase==Phase.Actions && !State.Active.eliminated;
        public CardData Data(Piece p) => Catalog[p.cardId];
        public IEnumerable<Piece> Allies(int player) => BoardManager.Pieces(State).Where(p=>p.owner==player);
        public bool HasTrait(int player,string trait) => Allies(player).Any(p=>Data(p).Has(trait));
        public bool InRange(int tile, int range)
        {
            return State.tiles.Any(t=>(t.baseOwner==State.activePlayer || (t.unit!=null&&t.unit.owner==State.activePlayer) || (t.structure!=null&&t.structure.owner==State.activePlayer)) && BoardManager.Distance(State,t.id,tile,range)<=range);
        }
        public bool TerraformTarget(int tile)
        {
            var t=State.tiles[tile];
            if(t.blocked || t.permanentAsh || t.biome==Biome.AshLand || (t.baseOwner>=0&&t.baseOwner!=State.activePlayer)) return false;
            if((t.unit!=null&&t.unit.owner!=State.activePlayer)||(t.structure!=null&&t.structure.owner!=State.activePlayer)) return false;
            return t.owner==State.activePlayer || t.neighbors.Any(n=>State.tiles[n].owner==State.activePlayer);
        }
        public string CardBlockReason(CardInstance instance,bool resources=false)
        {
            if(!CanAct) return "No es la fase de acciones.";
            if(instance==null || !State.Active.hand.Any(c=>c.instanceId==instance.instanceId)) return "La carta no está en la mano del jugador activo.";
            var c=Catalog[instance.cardId];
            if(!EnergyManager.CanPay(State.Active,c,resources)) return "Energía insuficiente.";
            if(c.requiresAshLand&&!State.tiles.Any(t=>t.owner==State.activePlayer&&t.biome==Biome.AshLand&&!t.IsOccupied)) return "Requiere Tierra Ceniza libre bajo tu control.";
            if(c.effects.Any(e=>e.operation=="SporeDamage")&&State.Active.spores==0) return "Requiere al menos 1 Espora.";
            if(c.effects.Any(e=>e.operation=="March")&&!State.fastRoutes.Any(f=>f.owner==State.activePlayer)) return "Requiere una Vía Rápida propia.";
            if(CardTargets(c).Count==0) return c.category==Category.Spell?"No hay objetivos compatibles dentro del alcance.":"No hay casillas propias libres con terreno compatible.";
            return "";
        }
        public List<int> CardTargets(CardData c, IList<int> selected=null)
        {
            if(!CanAct) return new List<int>();
            var ids=selected??new List<int>();
            if(c.category!=Category.Spell)
                return State.tiles.Where(t=>!t.IsOccupied&&!t.blocked&&t.baseOwner<0&&t.owner==State.activePlayer
                    &&(c.requiresAshLand?t.biome==Biome.AshLand:((t.biome==Biome.Neutral&&State.rules.allowNeutralDeployment) || c.biomes.Contains(t.biome))))
                    .Select(t=>t.id).ToList();
            var e=c.effects[0];
            if(e.operation=="March")
            {
                if(ids.Count%2==0) return Allies(State.activePlayer).Where(p=>!Data(p).IsStructure&&!IsSleeping(p)&&!ids.Contains(p.tileId)
                    &&MovementManager.RouteDestinations(this,p).Count>0).Select(p=>p.tileId).ToList();
                var unit=State.tiles[ids[ids.Count-1]].unit;
                return unit==null?new List<int>():MovementManager.RouteDestinations(this,unit).Where(n=>!ids.Contains(n)).ToList();
            }
            int range=State.rules.spellRange + (e.operation=="Poison"&&HasTrait(State.activePlayer,"PoisonRange")&&!State.Active.towerUsed?1:0);
            return State.tiles.Where(t=>(e.operation=="SporeDamage"||!ids.Contains(t.id))&&ValidSpellTarget(e.target,t,ids)&&((e.target.Contains("Terraform"))||InRange(t.id,range))).Select(t=>t.id).ToList();
        }
        bool ValidSpellTarget(string kind,HexTile t,IList<int> selected)
        {
            bool enemy=t.unit!=null&&t.unit.owner!=State.activePlayer, ally=t.unit!=null&&t.unit.owner==State.activePlayer;
            switch(kind)
            {
                case "Terraform": return TerraformTarget(t.id);
                case "AdjacentTerraform": return TerraformTarget(t.id)&&(selected.Count==0||State.tiles[selected[0]].neighbors.Contains(t.id));
                case "AllyUnit": return ally;
                case "EnemyUnit": return enemy;
                case "EnemyGround": return enemy&&Data(t.unit).movementType=="Ground";
                case "EnemyForest": return enemy&&t.biome==Biome.Forest;
                case "EnemyForestSwamp": return enemy&&(t.biome==Biome.Forest||t.biome==Biome.Swamp);
                case "EnemyPiece": return enemy||(t.structure!=null&&t.structure.owner!=State.activePlayer);
                case "EnemyAdjacent": return enemy&&BoardManager.Nearby(State,t.id).Any(p=>p.owner==State.activePlayer);
                case "EnemyBiome": return t.owner>=0&&t.owner!=State.activePlayer&&TerrainManager.Normal(t);
                case "AllyDesert": return t.owner==State.activePlayer&&t.biome==Biome.Desert;
                case "AllyDesertUnit": return ally&&t.biome==Biome.Desert&&t.unit.health<MaxHealth(t.unit);
                case "UsedStructure": return t.structure!=null&&t.structure.owner==State.activePlayer&&t.structure.abilityUsed&&AbilityManager.HasActive(Data(t.structure));
                default: return false;
            }
        }
        public bool Play(int instanceId,IList<int> targets,bool useResources=false,Biome choice=Biome.Forest)
        {
            var instance=State.Active.hand.Find(c=>c.instanceId==instanceId); string error=CardBlockReason(instance,useResources);
            if(error.Length>0) return Fail(error);
            var card=Catalog[instance.cardId];
            int max=card.category==Category.Spell?card.effects[0].count:1;
            bool march=card.effects.Any(e=>e.operation=="March");
            if(targets==null||targets.Count<1||targets.Count>max*(march?2:1)||(march&&targets.Count%2!=0)) return Fail("Selecciona objetivos válidos antes de confirmar.");
            if(card.effects.Any(e=>e.operation=="SporeDamage")&&targets.Count>State.Active.spores) return Fail("No hay suficientes Esporas para esos objetivos.");
            var previous=new List<int>();
            foreach(int target in targets) { if(!CardTargets(card,previous).Contains(target)) return Fail("Objetivo inválido, ocupado, incompatible o fuera de alcance."); previous.Add(target); }
            if(choice!=Biome.Forest&&choice!=Biome.Swamp) return Fail("Elige Bosque o Pantano.");
            EnergyManager.Pay(State.Active,card,useResources); State.Active.hand.Remove(instance);
            if(card.category==Category.Spell)
            { EffectManager.ResolveSpell(this,card,targets,choice); State.Active.discardPile.Add(instance); }
            else { var piece=Place(instance.cardId,State.activePlayer,targets[0],instance.instanceId,false); EffectManager.OnEnter(this,piece); }
            State.Log("J"+(State.activePlayer+1)+" juega "+card.name+" ["+string.Join(",",targets.Select(t=>t+1))+"]");
            Notify(card.name+" resuelta."); return true;
        }
        public Piece Place(string cardId,int owner,int tile,int id=0,bool token=true)
        {
            var c=Catalog[cardId]; var piece=new Piece {id=id==0?State.nextId++:id,cardId=cardId,owner=owner,tileId=tile,health=c.health,token=token,
                remainingMovement=State.rules.summoningSickness?0:c.movement,attacked=State.rules.summoningSickness};
            if(c.IsStructure) State.tiles[tile].structure=piece; else State.tiles[tile].unit=piece;
            if(c.Has("Evolve")&&State.tiles[tile].biome==Biome.Forest) piece.forestSinceTurn=State.turn;
            return piece;
        }
        public int MaxHealth(Piece p) => p.evolved?State.rules.evolvedHealth:Data(p).health;
        public bool IsSleeping(Piece p) => p.sleepUntilTurn>=State.turn;
        public int NextTurnOf(int player) { int delta=(player-State.activePlayer+4)%4; return State.turn+(delta==0?4:delta); }
        public bool Terraform(int tile,Biome biome)
        {
            if(!CanAct||tile<0||tile>=State.tiles.Count||!TerraformTarget(tile)) return Fail("No puedes terraformar esta casilla.");
            if(biome==Biome.Neutral||biome==Biome.AshLand) return Fail("Usa destruir bioma o revelar ceniza para esa transformación.");
            if(State.tiles[tile].biome==biome) return Fail("La casilla ya tiene ese bioma.");
            int cost=State.rules.terraformCost;
            bool discount=!State.Active.terraformDiscountUsed&&((biome==Biome.Forest&&HasTrait(State.activePlayer,"ForestDiscount"))||(biome==Biome.Desert&&HasTrait(State.activePlayer,"DesertDiscount")));
            if(discount) cost=Math.Max(0,cost-1);
            if(State.Active.currentEnergy<cost) return Fail("Energía insuficiente.");
            State.Active.currentEnergy-=cost; if(discount) State.Active.terraformDiscountUsed=true;
            TerrainManager.Terraform(this,tile,biome,State.activePlayer); Notify("Casilla "+(tile+1)+": "+Names.Biomes[(int)biome]+"."); return true;
        }
        public bool RevealAsh(int tile)
        {
            if(!CanAct||tile<0||tile>=State.tiles.Count||!TerrainManager.Normal(State.tiles[tile])||State.tiles[tile].owner!=State.activePlayer) return Fail("Requiere un bioma normal bajo tu control.");
            int cost=Math.Max(0,State.rules.revealAshCost-(HasTrait(State.activePlayer,"AshDiscount")?1:0));
            if(State.Active.currentEnergy<cost) return Fail("Energía insuficiente.");
            State.Active.currentEnergy-=cost; TerrainManager.RevealAsh(this,State.tiles[tile]); Notify("Tierra Ceniza revelada."); return true;
        }
        public bool EndTurn()
        {
            if(!CanAct) return Fail("No puedes finalizar ahora.");
            TurnManager.End(this); Notify(State.phase==Phase.Finished?"Partida terminada.":"Turno de J"+(State.activePlayer+1)+" · "+State.Active.leader); return true;
        }
    }
}
