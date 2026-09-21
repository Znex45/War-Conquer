#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    public static class InterfaceRulesTests
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static GameManager New(CardCatalog catalog)
        {var g=new GameManager(catalog);g.NewGame(new[]{"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules());return g;}
        static int Home(GameManager g,int player=0)=>g.State.tiles.First(t=>t.owner==player&&t.baseOwner<0&&!t.IsOccupied).id;
        static CardInstance Hand(GameManager g,int owner,string id)
        {var c=PrototypeScenario.Take(g,owner,id);g.State.players[owner].hand.Add(c);return c;}
        static void End(GameManager g){while(g.State.stage!=TurnStage.Terraforming)g.AdvanceStage();g.EndTurn();}
        static CardCatalog WithPermission(CardCatalog catalog,string id,ActionTiming timing,bool interrupt=true)
        {
            var data=catalog.All.Select(c=>JsonUtility.FromJson<CardData>(JsonUtility.ToJson(c))).ToArray();var card=data.First(c=>c.id==id);
            // Synthetic permission is confined to the test catalog. No published card text is changed.
            string permission=timing==ActionTiming.Defending?"Cuando una unidad aliada es atacada.":timing==ActionTiming.OtherTurn?"Durante el turno de otro jugador.":"Durante una batalla.";
            card.description+=" "+permission;card.timingPermissionText=permission;card.allowedTiming=new[]{ActionTiming.OwnTurn,timing};card.canRespond=true;card.canReact=true;card.canInterrupt=interrupt;
            return new CardCatalog(data);
        }
        static GameManager BattleFixture(CardCatalog catalog)
        {
            var g=New(catalog);PrototypeScenario.Load(g);g.State.stage=TurnStage.Assault;
            foreach(var p in g.State.players){p.deck.AddRange(p.hand);p.hand.Clear();p.currentEnergy=10;}
            return g;
        }
        public static void RunAll(CardCatalog catalog,Action<string,Action> test)
        {
            test("Coste 3: energía 7 a 4 sin descuentos automáticos por recursos",()=>{
                var g=New(catalog);g.State.Active.currentEnergy=7;EnergyManager.AddResource(g.State,g.State.Active,Biome.Forest,9);var c=Hand(g,0,"bestia-micelial");
                var quote=EnergyManager.Quote(g.State.Active,catalog[c.cardId]);Check(quote.energy==3&&quote.EnergyLabel=="3","Coste mostrado incorrecto.");
                Check(g.Play(c.instanceId,new[]{Home(g)})&&g.State.Active.currentEnergy==4,"No descuenta exactamente 3.");Check(g.State.Active.resources[0].amount==9,"Gastó recursos sin elegirlos.");
            });
            test("Coste 5: energía 7 a 2 y rechazo atómico si no alcanza",()=>{
                var g=New(catalog);var c=Hand(g,0,"coloso-de-esporas");g.State.Active.currentEnergy=4;string before=GamePersistence.Serialize(g.State);
                Check(!g.Play(c.instanceId,new[]{Home(g)})&&before==GamePersistence.Serialize(g.State),"Permite gastar energía no disponible.");
                g.State.Active.currentEnergy=7;Check(g.Play(c.instanceId,new[]{Home(g)})&&g.State.Active.currentEnergy==2,"No descuenta exactamente 5.");
            });
            test("Descuento real de Sacerdote: coste visible 3 → 2, solo siguiente estructura",()=>{
                var g=New(catalog);End(g);g.State.Active.currentEnergy=9;var card=Hand(g,1,"arena-profunda");
                Check(EnergyManager.Quote(g.State.Active,catalog[card.cardId]).energy==3,"Descuento sin efecto.");
                var priest=PrototypeScenario.Spawn(g,1,"sacerdote-solar",Home(g,1));Check(AbilityManager.Activate(g,priest,Array.Empty<int>()),"No activa Sacerdote.");
                var quote=EnergyManager.Quote(g.State.Active,catalog[card.cardId]);Check(quote.energy==2&&quote.EnergyLabel=="3 → 2","UI y descuento difieren.");
                Check(g.Play(card.instanceId,new[]{Home(g,1)})&&g.State.Active.currentEnergy==7&&g.State.Active.structureDiscount==0,"Descuento incorrecto o no consumido.");
                Check(EnergyManager.Quote(g.State.Active,catalog[card.cardId]).energy==3,"Descuento repetido.");
            });
            test("Pago mixto explícito: fuente única para energía y recursos compatibles",()=>{
                var g=New(catalog);EnergyManager.AddResource(g.State,g.State.Active,Biome.Forest,1);var c=Hand(g,0,"bestia-micelial");
                var quote=EnergyManager.Quote(g.State.Active,catalog[c.cardId],true);Check(quote.energy==2&&quote.resources==1&&quote.EnergyLabel=="3 → 2","Desglose no coincide.");
                Check(g.Play(c.instanceId,new[]{Home(g)},true)&&g.State.Active.currentEnergy==1&&g.State.Active.resources[0].amount==0,"Pago mixto diferente del mostrado.");
            });
            test("Despliegue Ataque Terraformación conserva energía y mano",()=>{
                var g=New(catalog);g.State.Active.currentEnergy=10;var unit=Hand(g,0,"bestia-micelial");var spell=Hand(g,0,"brote-repentino");
                Check(!g.EndTurn(),"Finaliza antes de completar etapas.");int tile=Home(g);Check(g.Play(unit.instanceId,new[]{tile}),"No despliega.");
                Check(g.AdvanceStage()&&g.State.stage==TurnStage.Assault&&g.State.Active.currentEnergy==7,"No pasa a Ataque.");
                Check(MovementManager.Paths(g,g.State.tiles[tile].unit).Count>0&&!g.Play(spell.instanceId,new[]{Home(g)}),"Ventanas incorrectas.");
                Check(g.AdvanceStage()&&g.State.stage==TurnStage.Terraforming&&g.State.Active.currentEnergy==7,"No pasa a Terraformación.");
                Check(g.Play(spell.instanceId,new[]{Home(g)})&&g.State.Active.currentEnergy==6,"No resuelve magia territorial.");
                Check(g.EndTurn()&&g.State.activePlayer==1&&g.State.stage==TurnStage.Deployment,"No termina turno.");
            });
            test("Los 4 jugadores completan las 3 etapas; mazo -1, mano +1 solo al robar",()=>{
                var g=New(catalog);int deck=g.State.Active.deck.Count,hand=g.State.Active.hand.Count;
                for(int i=0;i<4;i++){Check(g.State.activePlayer==i,"Turno incorrecto.");Check(!g.DrawPending(),"Robo libre.");End(g);}
                Check(g.State.Active.deck.Count==deck-1&&g.State.Active.hand.Count==hand+1&&g.State.Active.pendingDraw==0,"Robo no conserva cartas.");
            });
            test("Mazos reales no conceden intervenciones que sus textos no autorizan",()=>{
                var g=BattleFixture(catalog);foreach(var p in g.State.players)Check(!BattleManager.HasResponse(g,p.id),"Permiso inventado.");
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();int hp=victim.health;
                Check(CombatManager.Attack(g,attacker,victim.tileId)&&g.State.battle==null&&victim.health<hp,"Ataque sin respuestas no se resuelve.");
            });
            test("Atacante, defensor y terceros responden en orden pagando desde sus propios estados",()=>{
                var custom=WithPermission(WithPermission(catalog,"lluvia-de-esporas",ActionTiming.Battle),"erosion",ActionTiming.Battle);
                var g=BattleFixture(custom);var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();victim.health=8;
                int thirdTile=g.State.tiles.First(t=>t.territory==4&&!t.IsOccupied&&t.neighbors.Contains(attacker.tileId)).id;var third=PrototypeScenario.Spawn(g,2,"bestia-micelial",thirdTile);
                int fourthTile=g.State.tiles.First(t=>t.territory==4&&!t.IsOccupied&&t.neighbors.Contains(thirdTile)).id;PrototypeScenario.Spawn(g,3,"nomada-de-arena",fourthTile);
                var a=Hand(g,0,"lluvia-de-esporas");var b=Hand(g,1,"erosion");var c=Hand(g,2,"lluvia-de-esporas");var d=Hand(g,3,"erosion");
                Check(CombatManager.Attack(g,attacker,victim.tileId)&&g.ActingPlayerId==0&&victim.health==8,"No abre ventana antes del daño.");
                Check(!g.AdvanceStage()&&!g.EndTurn()&&MovementManager.Paths(g,attacker).Count==0,"Acciones paralelas a batalla.");
                Check(!BattleManager.Pass(g,2),"Tercero salta la prioridad.");
                Check(g.Play(a.instanceId,new[]{attacker.tileId})&&g.ActingPlayerId==1,"No responde atacante.");
                Check(g.Play(b.instanceId,new[]{attacker.tileId})&&g.ActingPlayerId==2,"No responde defensor.");
                Check(g.Play(c.instanceId,new[]{third.tileId})&&g.ActingPlayerId==3,"No interviene tercero.");
                Check(g.Play(d.instanceId,new[]{third.tileId})&&g.State.battle==null,"No interviene cuarto ni resuelve.");
                Check(victim.health==4&&attacker.health==3&&g.State.players[0].currentEnergy==8&&g.State.players[1].currentEnergy==7&&g.State.players[2].currentEnergy==8&&g.State.players[3].currentEnergy==7,"Daño o pago de respuestas incorrecto.");
                Check(g.State.activePlayer==0&&g.State.stage==TurnStage.Assault,"La respuesta cambió el turno.");Check(StateValidator.Validate(g.State,custom)=="","Integridad tras batalla.");
            });
            test("Permiso de defensa no habilita al atacante ni a terceros",()=>{
                var custom=WithPermission(catalog,"erosion",ActionTiming.Defending);var g=BattleFixture(custom);Hand(g,1,"erosion");Hand(g,3,"erosion");
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();
                CombatManager.Attack(g,attacker,victim.tileId);Check(g.ActingPlayerId==1&&!BattleManager.HasResponse(g,3),"Permiso de defensa demasiado amplio.");
                Check(BattleManager.Pass(g,1)&&g.State.battle==null,"Pasar no continúa el ataque.");
            });
            test("Eliminar al atacante en respuesta cancela el daño pendiente",()=>{
                var custom=WithPermission(catalog,"erosion",ActionTiming.Defending);var g=BattleFixture(custom);var card=Hand(g,1,"erosion");
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();attacker.health=1;int hp=victim.health;
                CombatManager.Attack(g,attacker,victim.tileId);Check(g.Play(card.instanceId,new[]{attacker.tileId}),"Respuesta no resuelta.");
                Check(g.State.battle==null&&victim.health==hp&&!g.Allies(0).Contains(attacker),"Atacante muerto inflige daño.");
            });
            test("Dormir al atacante desde una tercera intervención cancela el asalto pendiente",()=>{
                var custom=WithPermission(catalog,"espora-somnifera",ActionTiming.Battle);var g=BattleFixture(custom);var card=Hand(g,2,"espora-somnifera");
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();int hp=victim.health;
                var near=g.State.tiles.First(t=>t.territory==4&&!t.IsOccupied&&t.neighbors.Contains(attacker.tileId));PrototypeScenario.Spawn(g,2,"bestia-micelial",near.id);
                CombatManager.Attack(g,attacker,victim.tileId);Check(g.ActingPlayerId==2&&g.Play(card.instanceId,new[]{attacker.tileId}),"Tercero no pudo usar permiso expreso.");
                Check(g.State.battle==null&&g.IsSleeping(attacker)&&victim.health==hp,"Dormido resuelve ataque.");
            });
            test("Habilidad activa de estructura interviene solo con permiso y una vez",()=>{
                var custom=WithPermission(catalog,"campamento-nomada",ActionTiming.Defending);var g=BattleFixture(custom);
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();victim.health=9;
                var camp=PrototypeScenario.Spawn(g,1,"campamento-nomada",Home(g,1));int movement=victim.remainingMovement;
                CombatManager.Attack(g,attacker,victim.tileId);Check(g.ActingPlayerId==1,"No da prioridad a habilidad válida.");
                Check(AbilityManager.Activate(g,camp,new[]{victim.tileId})&&camp.abilityUsed&&victim.remainingMovement==movement+1&&g.State.battle==null,"Habilidad de respuesta no resuelve.");
                Check(!AbilityManager.Activate(g,camp,new[]{victim.tileId}),"Habilidad repetida fuera de ventana.");
            });
            test("Turno ajeno exige texto expreso, conserva jugador activo y recursos del dueño",()=>{
                var custom=WithPermission(catalog,"lluvia-de-esporas",ActionTiming.OtherTurn);var g=New(custom);var card=Hand(g,2,"lluvia-de-esporas");
                var ally=PrototypeScenario.Spawn(g,2,"bestia-micelial",Home(g,2));g.State.players[2].currentEnergy=5;
                Check(!BattleManager.RequestOutsideTurn(g,1),"Permiso ajeno inventado.");
                Check(BattleManager.RequestOutsideTurn(g,2)&&g.ActingPlayerId==2&&g.State.activePlayer==0,"No abre turno ajeno autorizado.");
                Check(g.Play(card.instanceId,new[]{ally.tileId})&&g.State.players[2].currentEnergy==3&&g.State.Active.currentEnergy==3,"Se cobra a jugador equivocado.");
                if(g.State.responsePlayer>=0)BattleManager.Pass(g,2);Check(g.ActingPlayerId==0&&g.State.stage==TurnStage.Deployment,"No retorna al turno original.");
            });
            test("Guardar y cargar batalla conserva prioridad y no repite daño",()=>{
                var custom=WithPermission(catalog,"erosion",ActionTiming.Defending);var g=BattleFixture(custom);Hand(g,1,"erosion");
                var attacker=g.Allies(0).First(p=>!g.Data(p).IsStructure);var victim=g.Allies(1).First();victim.health=9;
                CombatManager.Attack(g,attacker,victim.tileId);var saved=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));
                Check(StateValidator.Validate(saved,custom)==""&&saved.battle.priorityPlayer==1,"Batalla no se guarda.");
                var loaded=new GameManager(custom);loaded.Restore(saved);BattleManager.Pass(loaded,1);Check(loaded.State.tiles[victim.tileId].unit.health==6&&loaded.State.battle==null,"Daño incorrecto tras carga.");
                Check(!BattleManager.Pass(loaded,1)&&loaded.State.tiles[victim.tileId].unit.health==6,"Daño repetido al pasar.");
            });
            test("Control sin guarnición o sin bioma no genera puntos",()=>{
                var g=New(catalog);var center=g.State.tiles.First(ConquestManager.IsCenter);center.owner=0;
                for(int i=0;i<4;i++)End(g);Check(g.State.players[0].conquestPoints==0,"Puntúa sin unidad y bioma.");
                center.biome=Biome.Forest;ConquestManager.Refresh(g.State);
                for(int i=0;i<4;i++)End(g);Check(g.State.players[0].conquestPoints==0,"Puntúa sin guarnición.");
            });
            test("Derrotar Líder conquista su base, que puede ser ocupada y puntúa",()=>{
                var g=New(catalog);g.State.stage=TurnStage.Assault;var enemyBase=g.State.tiles.First(t=>t.baseOwner==1);g.State.players[1].leaderHealth=1;
                int origin=enemyBase.neighbors.First(n=>!g.State.tiles[n].IsOccupied&&g.State.tiles[n].baseOwner<0);var attacker=PrototypeScenario.Spawn(g,0,"bestia-micelial",origin);
                Check(CombatManager.Attack(g,attacker,enemyBase.id)&&g.State.players[1].eliminated&&enemyBase.owner==0,"No conquista base derrotada.");
                enemyBase.biome=Biome.Forest;Check(MovementManager.Move(g,attacker,enemyBase.id)&&ConquestManager.Income(g.State,0)==1,"No puede ocupar la base.");
                Check(g.State.phase!=Phase.Finished,"Victoria prematura con tres líderes vivos.");
            });
            test("Victoria inmediata a 10 Conquista bloquea cartas, movimiento y avances",()=>{
                var g=New(catalog);g.State.players[0].conquestPoints=9;var point=g.State.tiles.First(ConquestManager.IsCenter);point.owner=0;point.biome=Biome.Forest;PrototypeScenario.Spawn(g,0,"bestia-micelial",point.id);ConquestManager.Refresh(g.State);
                for(int i=0;i<4;i++)End(g);Check(g.State.phase==Phase.Finished&&g.State.winner==0&&g.State.victoryReason==VictoryReason.Conquest,"No detecta 10 puntos.");
                Check(!g.CanAct&&!g.AdvanceStage()&&!g.EndTurn()&&g.CardTargets(catalog["bestia-micelial"]).Count==0,"Partida terminada permite acciones.");
            });
            test("Victoria por último Líder en pie conserva motivo y ganador",()=>{
                var g=New(catalog);g.State.players[2].eliminated=g.State.players[3].eliminated=true;g.State.stage=TurnStage.Assault;
                var enemyBase=g.State.tiles.First(t=>t.baseOwner==1);g.State.players[1].leaderHealth=1;int origin=enemyBase.neighbors.First(n=>g.State.tiles[n].baseOwner<0);
                var attacker=PrototypeScenario.Spawn(g,0,"bestia-micelial",origin);CombatManager.Attack(g,attacker,enemyBase.id);
                Check(g.State.winner==0&&g.State.phase==Phase.Finished&&g.State.victoryReason==VictoryReason.LastLeader,"Victoria por Líder incorrecta.");
                var save=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(save.victoryReason==VictoryReason.LastLeader&&save.winner==0,"No conserva victoria.");
            });
            test("Vista ampliada no muta estado y utiliza el mismo coste y estadísticas",()=>{
                var g=New(catalog);var holder=new GameObject("Test card",typeof(RectTransform));string before=GamePersistence.Serialize(g.State);
                try
                {
                    var card=catalog["bestia-micelial"];var view=CardPresentation.Draw(holder.transform,g,card,g.State.Active,0,0,388,624,false,true);
                    var text=view.GetComponentsInChildren<Text>().Select(t=>t.text).ToArray();
                    Check(text.Contains("ENERGÍA")&&text.Contains("VIDA")&&text.Contains("FUERZA")&&text.Contains("MOV")&&text.Any(t=>t.Contains("VENTANAS DE USO")),"Carta incompleta.");
                    Check(text.Contains(EnergyManager.Quote(g.State.Active,card).EnergyLabel)&&before==GamePersistence.Serialize(g.State),"Tooltip modifica estado o coste.");
                }
                finally{UnityEngine.Object.DestroyImmediate(holder);}
            });
        }
    }
}
#endif
