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
        public void NewGame(string[] leaders, int seed, Rules rules, int humanPlayers=4, bool startImmediately=true)
        {
            if(humanPlayers<1||humanPlayers>4)throw new ArgumentOutOfRangeException(nameof(humanPlayers));
            int participants=humanPlayers==1?2:humanPlayers;
            if(leaders==null||leaders.Length<participants||leaders.Take(participants).Any(l=>!CardCatalog.Leaders.Contains(l)))throw new ArgumentException("Selecciona un mazo para cada participante.");
            State=new GameState {seed=seed,randomState=seed==0?12345:seed,rules=rules,phase=Phase.Setup};
            BoardManager.Create(State,participants);
            for(int i=0;i<4;i++)
            {
                bool inactive=i>=participants;string leader=inactive?"ZUKGROK":leaders[i];
                var p=new Player {id=i,leader=leader,factionTag=leader=="ZUKGROK"?"MICELIAL":leader=="FAUNAR"?"FAUNAR":"SOLAR",leaderHealth=inactive?0:rules.leaderHealth,inactive=inactive,eliminated=inactive,isAI=humanPlayers==1&&i==1};
                State.players.Add(p);if(!inactive)DeckManager.Build(State,p,Catalog);
            }
            State.Log("Partida local • "+humanPlayers+" persona(s)"+(humanPlayers==1?" + IA":"")+" • semilla "+seed+" • "+participants+" mazos de 50.");
            if(startImmediately){TurnManager.Start(this);Notify("Turno de J1 · Despliegue. Selecciona una carta permitida.");}
            else Notify("Partida en pausa. Elige participantes y mazos antes de comenzar.");
        }
        public void Restore(GameState state) { State=state; Notify("Partida cargada."); }
        public void Notify(string message) { if(State!=null)ConquestManager.Refresh(State);LastMessage=message; Changed?.Invoke(); }
        public int RollDie(Piece piece,HexTile tile,int threshold,string reason)
        {
            int value=State.Random(6)+1;
            State.diceRolls.Add(new DiceRoll{id=State.nextRollId++,value=value,threshold=threshold,tileId=tile.id,pieceId=piece?.id??-1,reason=reason});
            if(State.diceRolls.Count>30)State.diceRolls.RemoveAt(0);
            State.Log(reason+": d6="+value+" / "+threshold+"+ · "+(value>=threshold?"supera":"falla"));return value;
        }
        public bool Fail(string reason) { Notify(reason); return false; }
        public bool CanAct => State!=null && State.phase==Phase.Actions && !State.Active.eliminated;
        int queryPlayer=-1;
        public int ActingPlayerId=>queryPlayer>=0?queryPlayer:State.pendingChoices.Count>0?State.pendingChoices[0].owner:State.battle!=null?State.battle.priorityPlayer:State.responsePlayer>=0?State.responsePlayer:State.activePlayer;
        public Player ActingPlayer=>State.players[ActingPlayerId];
        internal T ForPlayer<T>(int player,Func<T> query){int previous=queryPlayer;queryPlayer=player;try{return query();}finally{queryPlayer=previous;}}
        public bool CanTakeTurnAction(TurnStage stage)=>CanAct&&State.pendingChoices.Count==0&&State.battle==null&&State.responsePlayer<0&&ActingPlayerId==State.activePlayer&&State.stage==stage;
        public bool AdvanceStage()
        {
            if(!CanAct||State.pendingChoices.Count>0||State.battle!=null||State.responsePlayer>=0)return Fail("Resuelve la intervención antes de continuar.");
            if(State.stage==TurnStage.Assault)return EndTurn();
            State.stage=State.stage==TurnStage.Deployment?TurnStage.Terraforming:TurnStage.Assault;Notify("Etapa de "+TimingRules.StageName(State.stage)+".");return true;
        }
        public bool DrawPending()
        {
            if(!CanTakeTurnAction(TurnStage.Deployment)||ActingPlayer.pendingDraw<=0)return Fail("No corresponde robar cartas ahora.");
            int count=ActingPlayer.pendingDraw;ActingPlayer.pendingDraw=0;DeckManager.Draw(State,ActingPlayer,count);Notify("Robo resuelto.");return true;
        }
        public CardData Data(Piece p) => Catalog[p.cardId];
        public IEnumerable<Piece> Allies(int player) => BoardManager.Pieces(State).Where(p=>p.owner==player);
        public bool HasTrait(int player,string trait) => Allies(player).Any(p=>Data(p).Has(trait));
        public bool InRange(int tile, int range)
        {
            return State.tiles.Any(t=>(t.baseOwner==ActingPlayerId || (t.unit!=null&&t.unit.owner==ActingPlayerId) || (t.structure!=null&&t.structure.owner==ActingPlayerId)) && BoardManager.Distance(State,t.id,tile,range)<=range);
        }
        public bool TerraformTarget(int tile)
        {
            var t=State.tiles[tile];
            if(t.blocked || t.permanentAsh || t.biome==Biome.AshLand || (t.baseOwner>=0&&t.baseOwner!=ActingPlayerId&&!State.players[t.baseOwner].eliminated)) return false;
            if((t.unit!=null&&t.unit.owner!=ActingPlayerId)||(t.structure!=null&&t.structure.owner!=ActingPlayerId)) return false;
            return t.owner==ActingPlayerId || t.neighbors.Any(n=>State.tiles[n].owner==ActingPlayerId);
        }
        public string CardBlockReason(CardInstance instance,bool resources=false)
        {
            if(!CanAct) return "No es la fase de acciones.";
            if(instance==null || !ActingPlayer.hand.Any(c=>c.instanceId==instance.instanceId)) return "La carta no está en la mano del jugador activo.";
            var c=Catalog[instance.cardId];
            if(!TimingRules.CardAllowed(this,c))return "Ventana no permitida. "+TimingRules.Description(c);
            if(!EnergyManager.CanPay(ActingPlayer,c,resources)) return "Energía insuficiente.";
            if(c.requiresAshLand&&!State.tiles.Any(t=>t.owner==ActingPlayerId&&t.biome==Biome.AshLand&&!t.IsOccupied)) return "Requiere Tierra Ceniza libre bajo tu control.";
            if(c.effects.Any(e=>e.operation=="SporeDamage")&&ActingPlayer.spores==0) return "Requiere al menos 1 Espora.";
            if(c.effects.Any(e=>e.operation=="March")&&!State.fastRoutes.Any(f=>f.owner==ActingPlayerId)) return "Requiere una Vía Rápida propia.";
            string extra=GenericCardRules.BlockReason(this,c,instance);if(extra.Length>0)return extra;
            if(!GenericCardRules.NoBoardTarget(c)&&CardTargets(c).Count==0) return c.category==Category.Spell?"No hay objetivos compatibles dentro del alcance.":"No hay casillas propias libres con terreno compatible.";
            return "";
        }
        public List<int> CardTargets(CardData c, IList<int> selected=null)
        {
            if(!CanAct||!TimingRules.CardAllowed(this,c)) return new List<int>();
            var ids=selected??new List<int>();
            var special=GenericCardRules.SpecialTargets(this,c,ids);if(special!=null)return special;
            if(c.category!=Category.Spell)
                return State.tiles.Where(t=>!t.IsOccupied&&!t.blocked&&t.baseOwner<0&&(t.owner==ActingPlayerId||GenericCardRules.BuildExtension(this,c,t))
                    &&(c.requiresAshLand?t.biome==Biome.AshLand:((t.biome==Biome.Neutral&&State.rules.allowNeutralDeployment) || c.biomes.Contains(t.biome))))
                    .Select(t=>t.id).ToList();
            var e=c.effects[0];
            if(e.operation=="March")
            {
                if(ids.Count%2==0) return Allies(ActingPlayerId).Where(p=>!Data(p).IsStructure&&!IsSleeping(p)&&!ids.Contains(p.tileId)
                    &&MovementManager.RouteDestinations(this,p).Count>0).Select(p=>p.tileId).ToList();
                var unit=State.tiles[ids[ids.Count-1]].unit;
                return unit==null?new List<int>():MovementManager.RouteDestinations(this,unit).Where(n=>!ids.Contains(n)).ToList();
            }
            int range=c.range + (e.operation=="Poison"&&HasTrait(ActingPlayerId,"PoisonRange")&&!ActingPlayer.towerUsed?1:0);
            return State.tiles.Where(t=>(e.operation=="SporeDamage"||!ids.Contains(t.id))&&ValidSpellTarget(e.target,t,ids)&&(c.range<=0||e.target.Contains("Terraform")||InRange(t.id,range))).Select(t=>t.id).ToList();
        }
        bool ValidSpellTarget(string kind,HexTile t,IList<int> selected)
        {
            bool enemy=t.unit!=null&&t.unit.owner!=ActingPlayerId, ally=t.unit!=null&&t.unit.owner==ActingPlayerId;
            switch(kind)
            {
                case "Unit": return t.unit!=null;
                case "Structure": return t.structure!=null;
                case "Slab": return !t.blocked;
                case "Wasteland": return t.biome==Biome.Wasteland&&!t.permanentAsh;
                case "Terraform": return TerraformTarget(t.id);
                case "AdjacentTerraform": return TerraformTarget(t.id)&&(selected.Count==0||State.tiles[selected[0]].neighbors.Contains(t.id));
                case "AllyUnit": return ally;
                case "EnemyUnit": return enemy;
                case "EnemyGround": return enemy&&Data(t.unit).movementType=="Ground";
                case "EnemyForest": return enemy&&t.biome==Biome.Forest;
                case "EnemyForestSwamp": return enemy&&(t.biome==Biome.Forest||t.biome==Biome.Swamp);
                case "EnemyPiece": return enemy||(t.structure!=null&&t.structure.owner!=ActingPlayerId);
                case "EnemyAdjacent": return enemy&&BoardManager.Nearby(State,t.id).Any(p=>p.owner==ActingPlayerId);
                case "EnemyBiome": return t.owner>=0&&t.owner!=ActingPlayerId&&TerrainManager.Normal(t);
                case "AllyDesert": return t.owner==ActingPlayerId&&t.biome==Biome.Desert;
                case "Desert": return t.biome==Biome.Desert&&(t.unit==null||t.unit.owner==ActingPlayerId)&&(t.structure==null||t.structure.owner==ActingPlayerId);
                case "AllyDesertUnit": return ally&&t.biome==Biome.Desert&&t.unit.health<MaxHealth(t.unit);
                case "UsedStructure": return t.structure!=null&&t.structure.owner==ActingPlayerId&&t.structure.abilityUsed&&AbilityManager.HasActive(Data(t.structure));
                default: return false;
            }
        }
        public bool Play(int instanceId,IList<int> targets,bool useResources=false,Biome choice=Biome.Forest)
        {
            var instance=ActingPlayer.hand.Find(c=>c.instanceId==instanceId); string error=CardBlockReason(instance,useResources);
            if(error.Length>0) return Fail(error);
            var card=Catalog[instance.cardId];
            int max=GenericCardRules.MaxTargets(card);
            bool march=card.effects.Any(e=>e.operation=="March");
            if(targets==null||targets.Count<GenericCardRules.MinTargets(card)||targets.Count>max||(march&&targets.Count%2!=0)) return Fail("Selecciona objetivos válidos antes de confirmar.");
            if(card.effects.Any(e=>e.operation=="SporeDamage")&&targets.Count>ActingPlayer.spores) return Fail("No hay suficientes Esporas para esos objetivos.");
            var previous=new List<int>();
            foreach(int target in targets) { if(!CardTargets(card,previous).Contains(target)) return Fail("Objetivo inválido, ocupado, incompatible o fuera de alcance."); previous.Add(target); }
            if(card.id=="terraform"?(choice==Biome.Neutral||choice==Biome.AshLand||!Enum.IsDefined(typeof(Biome),choice)):(choice!=Biome.Forest&&choice!=Biome.Swamp)) return Fail("Elige Bosque o Pantano.");
            var payer=ActingPlayer;EnergyManager.Pay(payer,card,useResources); payer.hand.Remove(instance);
            if(card.category==Category.Spell)
            { EffectManager.ResolveSpell(this,card,targets,choice); payer.discardPile.Add(instance); }
            else { State.tiles[targets[0]].owner=payer.id;var piece=Place(instance.cardId,payer.id,targets[0],instance.instanceId,false); EffectManager.OnEnter(this,piece); }
            State.Log("J"+(payer.id+1)+" juega "+card.name+" ["+string.Join(",",targets.Select(t=>t+1))+"]");
            BattleManager.AfterResponse(this);Notify(card.name+" resuelta."); return true;
        }
        public Piece Place(string cardId,int owner,int tile,int id=0,bool token=true)
        {
            var c=Catalog[cardId]; var piece=new Piece {id=id==0?State.nextId++:id,cardId=cardId,owner=owner,tileId=tile,health=c.health,token=token,
                remainingMovement=State.rules.summoningSickness?0:c.movement,attacked=State.rules.summoningSickness};
            if(c.IsStructure) State.tiles[tile].structure=piece; else State.tiles[tile].unit=piece;
            if(c.Has("Evolve")&&State.tiles[tile].biome==Biome.Forest) piece.forestSinceTurn=State.turn;
            GenericCardRules.SyncAuras(this);return piece;
        }
        public int MaxHealth(Piece p) => (p.evolved?State.rules.evolvedHealth:Data(p).health)+GenericCardRules.ForestHealth(this,p);
        public bool IsSleeping(Piece p) => p.sleepUntilTurn>=State.turn;
        public int NextTurnOf(int player) { int delta=(player-State.activePlayer+4)%4; return State.turn+(delta==0?4:delta); }
        public int TerraformCost(Biome biome)
        {
            bool discount=!State.Active.terraformDiscountUsed&&((biome==Biome.Forest&&HasTrait(State.activePlayer,"ForestDiscount"))||(biome==Biome.Desert&&HasTrait(State.activePlayer,"DesertDiscount")));
            return Math.Max(0,State.rules.terraformCost-(discount?1:0));
        }
        public int AshCost()=>Math.Max(0,State.rules.revealAshCost-(HasTrait(State.activePlayer,"AshDiscount")?1:0));
        public bool Terraform(int tile,Biome biome)
        {
            if(!CanTakeTurnAction(TurnStage.Terraforming)||tile<0||tile>=State.tiles.Count||!TerraformTarget(tile)) return Fail("No puedes terraformar esta casilla.");
            if(biome==Biome.Neutral||biome==Biome.AshLand) return Fail("Usa destruir bioma o revelar ceniza para esa transformación.");
            if(State.tiles[tile].biome==biome) return Fail("La casilla ya tiene ese bioma.");
            int cost=TerraformCost(biome);bool discount=cost<State.rules.terraformCost;
            if(State.Active.currentEnergy<cost) return Fail("Energía insuficiente.");
            State.Active.currentEnergy-=cost; if(discount) State.Active.terraformDiscountUsed=true;
            TerrainManager.Terraform(this,tile,biome,State.activePlayer); Notify("Casilla "+(tile+1)+": "+Names.Biomes[(int)biome]+"."); return true;
        }
        public bool RevealAsh(int tile)
        {
            if(!CanTakeTurnAction(TurnStage.Terraforming)||tile<0||tile>=State.tiles.Count||!TerraformTarget(tile)||!TerrainManager.Normal(State.tiles[tile])||State.tiles[tile].owner!=State.activePlayer) return Fail("Requiere un bioma normal propio sin ocupantes enemigos.");
            int cost=AshCost();
            if(State.Active.currentEnergy<cost) return Fail("Energía insuficiente.");
            State.Active.currentEnergy-=cost; TerrainManager.RevealAsh(this,State.tiles[tile]); Notify("Tierra Ceniza revelada."); return true;
        }
        public bool EndTurn()
        {
            if(!CanTakeTurnAction(TurnStage.Assault)) return Fail("Termina Despliegue, Terraformación y Asalto antes de finalizar.");
            TurnManager.End(this); Notify(State.phase==Phase.Finished?"Partida terminada.":"Turno de J"+(State.activePlayer+1)+" · "+State.Active.leader); return true;
        }
    }
}
