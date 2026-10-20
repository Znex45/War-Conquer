#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class MatchSetupTests
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        static readonly string[] Leaders={"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"};
        static GameManager New(CardCatalog cards,int humans,bool start=true,int seed=2026)
        {var g=new GameManager(cards);g.NewGame(Leaders,seed,CardCatalog.LoadRules(),humans,start);return g;}
        static void End(GameManager g){while(g.State.stage!=TurnStage.Terraforming)g.AdvanceStage();g.EndTurn();}
        public static void RunAll(CardCatalog catalog,Action<string,Action> test)
        {
            test("Preparación pausada: no hay acciones, turnos ni decisiones de IA antes de comenzar",()=>{
                var g=New(catalog,1,false);string before=GamePersistence.Serialize(g.State);
                Check(g.State.phase==Phase.Setup&&!g.CanAct&&g.State.players.All(p=>p.turnsTaken==0),"La preparación inicia un turno.");
                Check(!g.AdvanceStage()&&!g.EndTurn()&&!new AiPlayer().Step(g)&&before==GamePersistence.Serialize(g.State),"La preparación altera la partida.");
            });
            test("1 persona: un humano y una IA, bases opuestas y dos mazos exactos de 50",()=>{
                var g=New(catalog,1);Check(g.State.players.Count(p=>!p.inactive)==2&&g.State.players[1].isAI&&!g.State.players[0].isAI,"Modo solo incorrecto.");
                Check(g.State.tiles.Single(t=>t.baseOwner==0).territory==0&&g.State.tiles.Single(t=>t.baseOwner==1).territory==1,"Bases no opuestas.");
                Check(g.State.tiles.Count(t=>t.baseOwner>=0)==2&&g.State.tiles.All(t=>t.owner<2),"Puestos vacíos conservan bases.");
                Check(StateValidator.Validate(g.State,catalog)=="","Mazos o casillas inválidos.");
            });
            foreach(int humans in new[]{2,3,4})
            {
                int count=humans;test(count+" personas: solo participantes humanos y rondas sin puestos vacíos",()=>{
                    var g=New(catalog,count);Check(g.State.players.Count(p=>!p.inactive)==count&&g.State.players.All(p=>!p.isAI),"Recuento de personas incorrecto.");
                    for(int i=0;i<count;i++){Check(g.State.activePlayer==i,"Turno asignado a puesto vacío.");End(g);}
                    Check(g.State.activePlayer==0&&g.State.round==2&&g.State.players.Where(p=>p.inactive).All(p=>p.turnsTaken==0),"Ronda incorrecta.");
                    Check(StateValidator.Validate(g.State,catalog)=="","Estado multijugador inválido.");
                });
            }
            test("La selección permite ambos mazos para humano y rival; se conserva al guardar",()=>{
                var g=new GameManager(catalog);g.NewGame(new[]{"SAHRIA","ZUKGROK"},42,CardCatalog.LoadRules(),1);
                End(g);var saved=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));
                Check(saved.players[0].leader=="SAHRIA"&&saved.players[1].leader=="ZUKGROK"&&saved.players[1].isAI&&saved.activePlayer==1,"Se pierde selección o turno de IA.");
                g.Restore(saved);Check(new AiPlayer().Step(g)&&StateValidator.Validate(saved,catalog)=="","La IA no continúa después de cargar.");
            });
            test("Los puestos vacíos no impiden victoria por último Líder ni generan puntos",()=>{
                var g=New(catalog,2);g.State.players[1].eliminated=true;ConquestManager.CheckLastLeader(g.State);
                Check(g.State.winner==0&&g.State.victoryReason==VictoryReason.LastLeader,"Puesto vacío cuenta como rival.");
                Check(g.State.players.Where(p=>p.inactive).All(p=>ConquestManager.Income(g.State,p.id)==0),"Ingresos de puesto vacío.");
            });
            test("La IA no juega un turno humano ni una partida terminada",()=>{
                var g=New(catalog,1);var ai=new AiPlayer();string before=GamePersistence.Serialize(g.State);
                Check(!ai.Step(g)&&before==GamePersistence.Serialize(g.State),"IA controla al humano.");
                End(g);ConquestManager.Finish(g.State,0,VictoryReason.Conquest);before=GamePersistence.Serialize(g.State);
                Check(!ai.Step(g)&&before==GamePersistence.Serialize(g.State),"IA sigue tras victoria.");
            });
            test("IA despliega sin energía extra, terraforma, mueve, ataca y devuelve el turno",()=>{
                foreach(string leader in new[]{"SAHRIA","ZUKGROK"})
                {
                    var g=new GameManager(catalog);g.NewGame(new[]{"ZUKGROK",leader},2026,CardCatalog.LoadRules(),1);End(g);
                    var p=g.ActingPlayer;p.deck.AddRange(p.hand);p.hand.Clear();
                    string cardId=leader=="SAHRIA"?"nomada-de-arena":"bestia-micelial";var card=PrototypeScenario.Take(g,1,cardId);p.hand.Add(card);
                    var ai=new AiPlayer();int energy=p.currentEnergy;Check(ai.Step(g)&&g.Allies(1).Any(u=>u.cardId==cardId),"IA no despliega unidad.");
                    Check(p.currentEnergy==energy-catalog[cardId].energyCost,"La IA altera el coste.");
                    int decisions=1;while(g.State.activePlayer==1&&g.CanAct&&decisions++<110)Check(ai.Step(g),"La IA no progresa.");
                    Check(g.State.activePlayer==0&&decisions<110&&g.Allies(1).Any(u=>u.moved),"No mueve o no devuelve turno.");
                    g.State.activePlayer=1;g.State.stage=TurnStage.Assault;p.currentEnergy=0;
                    var attacker=g.Allies(1).First();attacker.attacked=false;
                    int near=g.State.tiles[attacker.tileId].neighbors.First(n=>!g.State.tiles[n].IsOccupied&&g.State.tiles[n].baseOwner<0);
                    var enemy=PrototypeScenario.Spawn(g,0,"hongo-explorador",near);int hp=enemy.health;
                    Check(new AiPlayer().Step(g)&&enemy.health<hp,"No ataca a un enemigo alcanzable.");
                    Check(StateValidator.Validate(g.State,catalog)=="","IA rompe integridad.");
                }
            });
            test("IA completa partidas con ambos mazos, sin atascarse ni alterar las cartas",()=>{
                foreach(string leader in new[]{"SAHRIA","ZUKGROK"})
                {
                    var g=new GameManager(catalog);g.NewGame(new[]{"ZUKGROK",leader},2026,CardCatalog.LoadRules(),1);var ai=new AiPlayer();int steps=0;bool conquered=false;
                    while(g.CanAct&&g.State.round<=30&&steps<1800)
                    {
                        if(g.ActingPlayer.isAI){Check(ai.Step(g),"IA detenida.");steps++;}
                        else End(g);
                        conquered|=ConquestManager.Income(g.State,1)>0;
                        string error=StateValidator.Validate(g.State,catalog);Check(error=="",error);
                    }
                    Check(g.State.phase==Phase.Finished&&g.State.winner==1,"La IA no consigue cerrar una partida contra un rival pasivo.");
                    Check(steps<1800&&conquered,"La IA no conquista.");
                }
            });
            test("La IA responde a una batalla únicamente con permiso explícito y paga su coste",()=>{
                var cards=catalog.All.Select(c=>JsonUtility.FromJson<CardData>(JsonUtility.ToJson(c))).ToArray();var sleep=cards.First(c=>c.id=="espora-somnifera");
                sleep.description+=" Durante una batalla.";sleep.timingPermissionText="Durante una batalla.";
                sleep.allowedTiming=new[]{ActionTiming.OwnTurn,ActionTiming.Battle};sleep.canRespond=true;sleep.canReact=true;sleep.canInterrupt=true;
                var g=new GameManager(new CardCatalog(cards));g.NewGame(new[]{"ZUKGROK","ZUKGROK"},2026,CardCatalog.LoadRules(),1);PrototypeScenario.Load(g);
                foreach(var p in g.State.players.Where(p=>!p.inactive)){p.deck.AddRange(p.hand);p.hand.Clear();}
                var response=PrototypeScenario.Take(g,1,"espora-somnifera");g.State.players[1].hand.Add(response);g.State.players[1].currentEnergy=3;
                g.State.stage=TurnStage.Assault;var defender=g.Allies(1).First(p=>!g.Data(p).IsStructure);
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);int hp=defender.health;
                Check(CombatManager.Attack(g,attacker,defender.tileId)&&g.ActingPlayerId==1,"No ofrece prioridad de defensa a la IA.");
                Check(new AiPlayer().Step(g)&&g.State.players[1].currentEnergy==1&&g.IsSleeping(attacker),"Respuesta o coste de IA incorrectos.");
                Check(g.State.activePlayer==0&&g.State.battle==null&&defender.health==hp,"Intervención altera turno o resuelve golpe dormido.");
            });
            test("Guardar a mitad del turno de IA permite completar las mismas tres etapas",()=>{
                var g=New(catalog,1);End(g);new AiPlayer().Step(g);g.Restore(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)));
                var ai=new AiPlayer();int steps=0;while(g.State.activePlayer==1&&steps++<110)Check(ai.Step(g),"No reanuda IA.");
                Check(g.State.activePlayer==0&&StateValidator.Validate(g.State,catalog)=="","Turno cargado inválido.");
            });
        }
    }
}
#endif
