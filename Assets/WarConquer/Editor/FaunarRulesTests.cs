#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class FaunarRulesTests
    {
        public static void RunStandalone()
        {
            try{int count=0;RunAll(CardCatalog.Load(),(name,body)=>{body();count++;Debug.Log("PASS "+name);});Debug.Log("FAUNAR_RULES_PASSED "+count);UnityEditor.EditorApplication.Exit(0);}
            catch(Exception e){Debug.LogException(e);UnityEditor.EditorApplication.Exit(1);}
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static GameManager New(CardCatalog c,int humans=4){var g=new GameManager(c);g.NewGame(new[]{"FAUNAR","SAHRIA","ZUKGROK","FAUNAR"},381,CardCatalog.LoadRules(),humans);g.State.Active.currentEnergy=50;return g;}
        static HexTile At(GameManager g,int q,int r){return g.State.tiles.First(t=>t.q==q&&t.r==r);}
        static HexTile Home(GameManager g)=>g.State.tiles.First(t=>t.owner==0&&!t.IsOccupied&&t.baseOwner<0);
        static CardInstance Hand(GameManager g,string id)
        {
            var p=g.ActingPlayer;var card=p.hand.FirstOrDefault(c=>c.cardId==id);
            if(card==null){card=p.deck.FirstOrDefault(c=>c.cardId==id);if(card!=null)p.deck.Remove(card);else card=new CardInstance{instanceId=g.State.nextId++,cardId=id};p.hand.Add(card);}
            return card;
        }
        static Piece Put(GameManager g,string id,int owner,int q,int r,Biome biome=Biome.Neutral)
        {var t=At(g,q,r);t.owner=owner;t.biome=biome;return g.Place(id,owner,t.id);}
        static void End(GameManager g){while(g.State.battle!=null)BattleManager.Pass(g,g.ActingPlayerId);while(g.State.stage!=TurnStage.Assault)g.AdvanceStage();Check(g.EndTurn(),"No termina turno.");}
        static void Round(GameManager g){int owner=g.State.activePlayer;do{End(g);}while(g.State.activePlayer!=owner);}
        static void Play(GameManager g,string id,params int[] targets){var c=Hand(g,id);int energy=g.ActingPlayer.currentEnergy,cost=EnergyManager.Quote(g.ActingPlayer,g.Catalog[id]).energy;Check(g.Play(c.instanceId,targets),id+": "+g.LastMessage);Check(g.State.players[0].currentEnergy==energy-cost,"Coste distinto del mostrado: "+id);}
        public static void RunAll(CardCatalog c,Action<string,Action> test)
        {
            test("FAUNAR 50 exactas, sin Slashing Bear, MOV HP ATK y Token fuera del mazo",()=>{
                var deck=c.All.Where(x=>x.leader=="FAUNAR").ToArray();Check(deck.Sum(x=>x.quantity)==50&&deck.All(x=>x.quantity<=3),"Copias incorrectas.");
                Check(deck.Where(x=>x.category==Category.Unit).Sum(x=>x.quantity)==12&&deck.Where(x=>x.IsStructure).Sum(x=>x.quantity)==18&&deck.Where(x=>x.combatSpell).Sum(x=>x.quantity)==5,"Distribución incorrecta.");
                string[] ids={"charming-mongoose","ferret-eluding-hunter","bober-reinforcer","panther-stealthy-assassin","rabbit-token"};int[,] stats={{2,2,2},{3,5,2},{2,4,3},{3,6,3},{2,2,1}};
                for(int i=0;i<ids.Length;i++)Check(c[ids[i]].movement==stats[i,0]&&c[ids[i]].health==stats[i,1]&&c[ids[i]].attack==stats[i,2],"Orden de estadísticas: "+ids[i]);
                Check(!c.All.Any(x=>x.name.Contains("Slashing Bear"))&&c["rabbit-token"].quantity==0,"Carta no autorizada.");
                foreach(int n in new[]{1,2,3,4}){var g=New(c,n);Check(StateValidator.Validate(g.State,c)=="","Integridad de mazos en modo "+n);}
            });
            test("RAD hexagonal 2 3 4 5 6 8 independiente de MOV y rutas",()=>{
                var g=New(c);var center=At(g,0,0);
                foreach(int rad in new[]{2,3,4,5,6,8}){var tiles=BoardManager.SlabsWithinRadius(g.State,center.id,rad).ToArray();Check(tiles.All(t=>BoardManager.AxialDistance(center,t)<=rad)&&tiles.Any(t=>BoardManager.AxialDistance(center,t)==rad),"Radio incorrecto.");}
                var p=Put(g,"charming-mongoose",0,5,0);p.remainingMovement=0;Check(BoardManager.UnitsWithinRadius(g.State,center.id,5).Contains(p)&&!BoardManager.UnitsWithinRadius(g.State,center.id,4).Contains(p),"RAD depende de MOV.");
            });
            test("Faunar descuento visible una vez por turno, no consume en rechazo ni unidad o magia",()=>{
                var g=New(c);var p=g.State.Active;var bush=Hand(g,"berry-bush");var tile=Home(g);int e=p.currentEnergy;
                Check(EnergyManager.Cost(p,c["berry-bush"])==1&&EnergyManager.Cost(p,c["charming-mongoose"])==2&&EnergyManager.Cost(p,c["terraform"])==1,"Descuento incorrecto.");
                Check(!g.Play(bush.instanceId,new[]{999})&&!p.firstStructureUsed&&p.currentEnergy==e,"Rechazo consume descuento.");
                Check(g.Play(bush.instanceId,new[]{tile.id})&&p.firstStructureUsed&&p.currentEnergy==e-1,"Primera estructura incorrecta.");
                Check(EnergyManager.Cost(p,c["berry-bush"])==2,"Segunda estructura rebajada.");Round(g);Check(!p.firstStructureUsed&&EnergyManager.Cost(p,c["berry-bush"])==1,"No reinicia.");
                Check(StateValidator.Validate(g.State,c)=="","Se alteró el mazo.");
            });
            test("Terraform y Clearing Space modifican Slab, cobran y roban realmente",()=>{
                var g=New(c);Hand(g,"terraform");int deck=g.State.Active.deck.Count;var t=Home(g);Play(g,"terraform",t.id);Check(t.biome==Biome.Forest&&g.State.Active.deck.Count==deck-1,"Terraform no roba.");
                deck=g.State.Active.deck.Count;Play(g,"clearing-space",t.id);Check(t.biome==Biome.Wasteland&&g.State.Active.deck.Count==deck-2,"Yield no es Yermo.");
                t.biome=Biome.AshLand;Check(!g.CardTargets(c["clearing-space"]).Contains(t.id),"Reemplaza Tierra Ceniza.");
            });
            test("Cleansing Conquest exige dos objetivos y radio 8, no MOV",()=>{
                var g=New(c);var t=At(g,0,0);t.owner=0;var p=Put(g,"charming-mongoose",0,8,0);p.remainingMovement=0;
                var card=Hand(g,"cleansing-conquest");int e=g.State.Active.currentEnergy;
                Check(!g.Play(card.instanceId,new[]{t.id})&&g.State.Active.currentEnergy==e,"Resuelve parcialmente.");
                Check(g.CardTargets(c[card.cardId],new[]{t.id}).Contains(p.tileId),"Rechaza 8rad.");
                Check(g.Play(card.instanceId,new[]{t.id,p.tileId})&&t.unit==p&&t.biome==Biome.Wasteland&&p.remainingMovement==0,"No traslada al Yermo.");
            });
            test("It's a Trap muerte y descarte; Last Gift usa vida previa",()=>{
                var g=New(c);var p=PrototypeScenario.Spawn(g,0,"bober-reinforcer",Home(g).id);p.health=3;var last=Hand(g,"last-gift");int deck=g.State.Active.deck.Count;
                Check(g.Play(last.instanceId,new[]{p.tileId})&&g.State.Active.deck.Count==deck-3&&g.State.Active.discardPile.Any(x=>x.instanceId==p.id),"Last Gift calcula HP después de matar.");
                var enemy=Put(g,"charming-mongoose",1,0,0);Play(g,"its-a-trap",enemy.tileId);Check(enemy.health==0&&g.State.tiles[enemy.tileId].unit==null,"Trap no mata.");
            });
            test("One for the Team elige descarte antes del robo y bloquea acciones intermedias",()=>{
                var g=New(c);var spell=Hand(g,"one-for-the-team");int deck=g.State.Active.deck.Count;Check(g.Play(spell.instanceId,Array.Empty<int>()),"No inicia descarte.");
                Check(g.State.pendingChoices.Count==1&&g.State.Active.deck.Count==deck&&!g.AdvanceStage(),"Robó antes o deja actuar.");int id=g.State.Active.hand[0].instanceId;
                Check(ChoiceManager.Resolve(g,new[]{id})&&g.State.Active.deck.Count==deck-2&&g.State.Active.discardPile.Any(x=>x.instanceId==id),"Descarte/robo incorrecto.");
                Check(StateValidator.Validate(g.State,c)=="","Cartas duplicadas.");
            });
            test("A Price to Pay roba antes de descartar, selección obligatoria persiste en guardado",()=>{
                var g=New(c);var spell=Hand(g,"a-price-to-pay");int deck=g.State.Active.deck.Count;Check(g.Play(spell.instanceId,Array.Empty<int>())&&g.State.Active.deck.Count==deck-3,"No roba tres antes.");
                var restored=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(StateValidator.Validate(restored,c)==""&&restored.pendingChoices.Count==1,"No guarda elección.");g.Restore(restored);
                int[] ids=g.State.Active.hand.Take(2).Select(x=>x.instanceId).ToArray();int count=g.State.Active.hand.Count;
                Check(!ChoiceManager.Resolve(g,new[]{ids[0],ids[0]})&&!g.EndTurn(),"Permite duplicados o escapar.");
                Check(ChoiceManager.Resolve(g,ids)&&g.State.Active.hand.Count==count-2&&StateValidator.Validate(g.State,c)=="","No descarta exactamente dos.");
            });
            test("From the Ashes solo Yermo; Resourceful Replenish solo Tokens propios",()=>{
                var g=New(c);var t=Home(g);t.biome=Biome.Wasteland;t.hiddenAsh=false;Play(g,"from-the-ashes",t.id);Check(t.biome==Biome.Neutral,"No destruye Yermo.");
                t.biome=Biome.AshLand;Check(!g.CardTargets(c["from-the-ashes"]).Contains(t.id),"Destruye Ceniza como Yermo.");
                Put(g,"rabbit-token",0,0,0);Put(g,"rabbit-token",0,1,0);Put(g,"rabbit-token",1,2,0);var card=Hand(g,"resourceful-replenish");int deck=g.State.Active.deck.Count;
                Check(g.Play(card.instanceId,Array.Empty<int>())&&g.State.Active.deck.Count==deck-2,"Cuenta Tokens enemigos o recursos.");
            });
            test("Engage alcance6, ATK real, cura Ferret al matar y ocupa destino",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var ally=Put(g,"ferret-eluding-hunter",0,0,0);ally.health=1;ally.bonusAttack=2;ally.remainingMovement=0;var enemy=Put(g,"bober-reinforcer",2,6,0);int dest=enemy.tileId;
                Put(g,"charming-mongoose",3,7,0);Check(!g.CardTargets(c["engage"],new[]{ally.tileId}).Contains(At(g,7,0).id),"Engage fuera de 6rad.");
                Play(g,"engage",ally.tileId,dest);Check(enemy.health==0&&ally.tileId==dest&&ally.health==5&&ally.remainingMovement==0,"Engage ATK/cura/movimiento incorrecto.");
            });
            test("Time to Move radio8 todos los bandos, temporal hasta fin del turno",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var a=Put(g,"charming-mongoose",0,0,0);var b=Put(g,"charming-mongoose",2,8,0);var far=Put(g,"charming-mongoose",3,9,0);
                Play(g,"time-to-move",At(g,0,0).id);Check(a.remainingMovement==6&&b.remainingMovement==6&&far.remainingMovement==2,"Radio o bandos incorrectos.");End(g);
                Check(a.temporaryMovement==0&&b.temporaryMovement==0&&b.remainingMovement==2&&c[a.cardId].movement==2,"Bono no expira o altera base.");
            });
            test("Bomb Structure exacto3, no unidad ni consumo inválido",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var structure=Put(g,"med-camp",1,0,0);var unit=Put(g,"bober-reinforcer",1,1,0);var bomb=Hand(g,"bomb");int e=g.State.Active.currentEnergy;
                Check(!g.Play(bomb.instanceId,new[]{unit.tileId})&&g.State.Active.currentEnergy==e,"Bomb acepta Unit.");Check(g.Play(bomb.instanceId,new[]{structure.tileId})&&structure.health==2&&g.State.Active.currentEnergy==e-2,"Bomb no hace tres.");
            });
            test("Combat Spells atacan defienden e intervienen; Spell normal bloqueada en batalla",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var ally=Put(g,"charming-mongoose",0,0,0);var enemy=Put(g,"bober-reinforcer",3,1,0);var tower=Put(g,"vigilant-tower",2,2,0);Hand(g,"bomb");Hand(g,"terraform");
                Check(CombatManager.Attack(g,ally,enemy.tileId)&&g.State.battle!=null,"No abre respuesta.");Check(TimingRules.ResponseAllowed(g,c["bomb"],0)&&TimingRules.ResponseAllowed(g,c["bomb"],3)&&TimingRules.ResponseAllowed(g,c["bomb"],2),"Falta permiso de combate.");
                var spell=g.State.Active.hand.First(x=>x.cardId=="terraform");Check(g.CardBlockReason(spell)!="","Spell normal permitida en combate.");
                Check(g.Play(g.State.Active.hand.First(x=>x.cardId=="bomb").instanceId,new[]{tower.tileId}),"Bomb no responde.");
            });
            test("Sellos globales dinámicos y no permanentes",()=>{
                var g=New(c);var unit=Put(g,"charming-mongoose",0,0,0);var enemy=Put(g,"charming-mongoose",1,1,0);var val=Put(g,"valiant-seal",0,5,0);var courage=Put(g,"courage-seal",0,6,0);
                Check(CombatManager.AttackValue(g,unit)==3&&unit.remainingMovement==3&&CombatManager.AttackValue(g,enemy)==2&&enemy.remainingMovement==2,"Aura global incorrecta.");
                CombatManager.Remove(g,val);CombatManager.Remove(g,courage);Check(CombatManager.AttackValue(g,unit)==2&&unit.remainingMovement==2&&c[unit.cardId].movement==2,"Aura persiste al destruir.");
            });
            test("Med Camp hasta4 a5rad, Resting Camp2 a4rad y Vigilant Tower1 a6rad",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var med=Put(g,"med-camp",0,0,0);var a=Put(g,"panther-stealthy-assassin",0,5,0);a.health=1;var b=Put(g,"bober-reinforcer",0,6,0);b.health=1;
                Check(!AbilityManager.Targets(g,med).Contains(b.tileId)&&AbilityManager.Activate(g,med,new[]{a.tileId})&&a.health==5&&!AbilityManager.CanActivate(g,med),"Med Camp radio o límite incorrecto.");
                var rest=Put(g,"resting-camp",0,4,0);Check(AbilityManager.Activate(g,rest,Array.Empty<int>())&&a.health==6&&b.health==3,"Resting Camp incorrecto.");
                var tower=Put(g,"vigilant-tower",0,0,1);var target=Put(g,"charming-mongoose",2,6,1);Check(AbilityManager.Activate(g,tower,new[]{target.tileId})&&target.health==1,"Tower no llega a6rad.");
            });
            test("Emergency Seal elige aliado del dueño aun durante turno enemigo",()=>{
                var g=New(c);var seal=Put(g,"emergency-seal",3,0,0);var ally=Put(g,"charming-mongoose",3,7,0);int from=ally.tileId;
                CombatManager.Remove(g,seal);Check(g.ActingPlayerId==3&&g.State.pendingChoices.Count==1&&!g.AdvanceStage(),"No cede elección al dueño.");
                Check(ChoiceManager.Resolve(g,new[]{from})&&ally.tileId==seal.tileId&&g.State.tiles[from].unit==null&&g.ActingPlayerId==0,"No mueve o no devuelve prioridad.");
                CombatManager.Remove(g,seal);Check(g.State.pendingChoices.Count==0,"Disparo duplicado de muerte.");
            });
            test("Berry Bober Mana y descuento al inicio una vez, tokens independientes del mazo",()=>{
                var g=New(c);Put(g,"berry-bush",0,0,0,Biome.Forest);Put(g,"bober-reinforcer",0,2,0);var mana=Put(g,"mana-pool",0,3,0);var seal=Put(g,"valiant-seal",0,4,0);seal.health=1;
                Round(g);int rabbits=g.Allies(0).Count(x=>x.cardId=="rabbit-token");Check(rabbits==1&&seal.health==2&&g.State.Active.currentEnergy==g.State.Active.maxEnergy+1,"Efectos de inicio incorrectos.");
                int energy=g.State.Active.currentEnergy;TurnManager.Start(g);Check(g.Allies(0).Count(x=>x.cardId=="rabbit-token")==1&&energy==g.State.Active.currentEnergy&&seal.health==2,"Inicio duplicado.");
                var rabbit=g.Allies(0).First(x=>x.cardId=="rabbit-token");int discard=g.State.Active.discardPile.Count;CombatManager.Remove(g,rabbit);Check(g.State.Active.discardPile.Count==discard,"Token entra al descarte.");
            });
            test("Rabbit HP Bosque calculado sin acumular y conserva daño",()=>{
                var g=New(c);var rabbit=Put(g,"rabbit-token",0,0,0,Biome.Forest);Check(rabbit.health==4&&g.MaxHealth(rabbit)==4,"Bono inicial.");
                for(int i=0;i<4;i++)GenericCardRules.SyncAuras(g);Check(rabbit.health==4,"Bono acumulativo.");
                CombatManager.DirectDamage(g,rabbit,1);var t=At(g,1,0);t.biome=Biome.Neutral;MovementManager.Relocate(g,rabbit,t.id);Check(rabbit.health==1&&g.MaxHealth(rabbit)==2,"Salida no quita bono.");
                t=At(g,0,0);MovementManager.Relocate(g,rabbit,t.id);Check(rabbit.health==3&&g.MaxHealth(rabbit)==4,"Reentrada borra daño.");
            });
            test("Mongoose roba solo al jugar en Bosque",()=>{
                var g=New(c);var first=Hand(g,"charming-mongoose");var home=Home(g);home.biome=Biome.Forest;int deck=g.State.Active.deck.Count;
                Check(g.Play(first.instanceId,new[]{home.id})&&g.State.Active.deck.Count==deck-1,"No roba en Bosque.");
                var second=Hand(g,"charming-mongoose");home=Home(g);deck=g.State.Active.deck.Count;Check(g.Play(second.instanceId,new[]{home.id})&&g.State.Active.deck.Count==deck,"Roba fuera de Bosque.");
                GenericCardRules.OnMove(g,g.State.tiles[home.id].unit,Biome.Neutral);Check(g.State.Active.deck.Count==deck,"Movimiento se trata como jugar.");
            });
            test("Ferret salto a estructura Bosque a4rad una vez por unidad y turno",()=>{
                var g=New(c);g.State.stage=TurnStage.Assault;var a=Put(g,"ferret-eluding-hunter",0,0,0);var b=Put(g,"ferret-eluding-hunter",0,0,1);Put(g,"berry-bush",0,4,0,Biome.Forest);a.remainingMovement=0;
                int dest=AbilityManager.Targets(g,a).First();Check(AbilityManager.Activate(g,a,new[]{dest})&&!AbilityManager.CanActivate(g,a)&&AbilityManager.CanActivate(g,b),"Uso global o necesita MOV.");
                Round(g);g.State.stage=TurnStage.Assault;Check(!a.abilityUsed&&AbilityManager.CanActivate(g,a),"No reinicia salto.");
            });
            test("Bober coloca a2rad solo en Bosque y respeta casillas y biomas",()=>{
                var g=New(c);var b=Put(g,"bober-reinforcer",0,0,0,Biome.Forest);var t=At(g,2,0);var far=At(g,3,0);
                Check(g.CardTargets(c["valiant-seal"]).Contains(t.id)&&!g.CardTargets(c["valiant-seal"]).Contains(far.id),"Radio2 incorrecto.");
                Put(g,"charming-mongoose",1,1,0);Check(!g.CardTargets(c["valiant-seal"]).Contains(At(g,1,0).id),"Construye ocupado.");
                t.biome=Biome.Desert;Check(!g.CardTargets(c["berry-bush"]).Contains(t.id),"No respeta bioma.");g.State.tiles[b.tileId].biome=Biome.Neutral;Check(!g.CardTargets(c["valiant-seal"]).Contains(t.id),"Bober conserva capacidad sin Bosque.");
            });
            test("Panther transiciones Bosque, elige a2rad y no repite dentro del mismo bioma",()=>{
                var g=New(c);var p=Put(g,"panther-stealthy-assassin",0,0,0,Biome.Forest);p.health=2;var t=At(g,1,0);t.biome=Biome.Forest;
                MovementManager.Relocate(g,p,t.id);Check(p.health==2&&g.State.pendingChoices.Count==0,"Dispara entre Bosques.");
                var enemy=Put(g,"bober-reinforcer",2,4,0);var far=Put(g,"bober-reinforcer",3,5,0);MovementManager.Relocate(g,p,At(g,2,0).id);
                var choice=g.State.pendingChoices.Single();Check(ChoiceManager.Targets(g,choice).Contains(enemy.tileId)&&!ChoiceManager.Targets(g,choice).Contains(far.tileId),"Radio de Panther.");
                Check(ChoiceManager.Resolve(g,new[]{enemy.tileId})&&enemy.health==2,"No aplica daño elegido.");MovementManager.Relocate(g,p,t.id);Check(p.health==6,"No cura al entrar.");
            });
            test("IA Faunar usa elecciones y avanza sin bloquear el turno",()=>{
                var g=New(c,2);g.State.players[0].isAI=true;var ai=new AiPlayer();Hand(g,"one-for-the-team");int steps=0;
                while(g.State.activePlayer==0&&steps++<120)Check(ai.Step(g),"IA no puede completar una decisión.");
                Check(g.State.activePlayer==1&&g.State.pendingChoices.Count==0&&StateValidator.Validate(g.State,c)=="","IA detenida o integridad incorrecta.");
            });
        }
    }
}
#endif
