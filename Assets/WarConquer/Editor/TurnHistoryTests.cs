#if UNITY_EDITOR
using System;
using System.Linq;

namespace WarConquer.Editor
{
    public static class TurnHistoryTests
    {
        public static void RunBatch()
        {
            try{int count=0;RunAll(CardCatalog.Load(),(name,body)=>{body();count++;UnityEngine.Debug.Log("PASS "+name);});UnityEngine.Debug.Log("TURN_HISTORY_RULES_PASSED "+count);UnityEditor.EditorApplication.Exit(0);}
            catch(Exception e){UnityEngine.Debug.LogException(e);UnityEditor.EditorApplication.Exit(1);}
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static GameManager New(CardCatalog c)
        {var g=new GameManager(c);g.NewGame(new[]{"FAUNAR","ZUKGROK","SAHRIA","FAUNAR"},381,CardCatalog.LoadRules());g.State.Active.currentEnergy=40;return g;}
        static HexTile At(GameManager g,int q,int r)=>g.State.tiles.First(t=>t.q==q&&t.r==r);
        static Piece Put(GameManager g,string id,int owner,int q,int r)
        {var t=At(g,q,r);t.owner=owner;t.blocked=false;return g.Place(id,owner,t.id);}
        static CardInstance Hand(GameManager g,string id)
        {var c=PrototypeScenario.Take(g,g.ActingPlayerId,id);g.ActingPlayer.hand.Add(c);return c;}
        static string Snapshot(GameManager g){ConquestManager.Refresh(g.State);return GamePersistence.Serialize(g.State);}
        static void Restored(GameManager g,string before)
        {Check(g.Undo(),"No permite deshacer.");Check(Snapshot(g)==before,"El estado completo no coincide con el anterior.");}
        public static void RunAll(CardCatalog catalog,Action<string,Action> test)
        {
            test("Rewind: varias decisiones, cruza ambas etapas y restaura pago, mano, estructura y territorio",()=>{
                var g=New(catalog);var c=Hand(g,"resting-camp");int home=g.CardTargets(catalog[c.cardId]).First();string start=Snapshot(g);
                Check(!g.CanUndo&&!g.EndTurn()&&g.UndoCount==0,"Historial inicial o fin anticipado.");
                Check(g.Play(c.instanceId,new[]{home}),g.LastMessage);string deployed=Snapshot(g);
                Check(g.AdvanceStage()&&g.State.stage==TurnStage.Assault,"Orden Asalto.");string assault=Snapshot(g);
                Check(g.AdvanceStage()&&g.State.stage==TurnStage.Terraforming,"Orden Terraformación.");string terra=Snapshot(g);
                int tile=g.State.tiles.First(t=>g.TerraformTarget(t.id)&&t.biome!=Biome.Forest).id;
                Check(g.Terraform(tile,Biome.Forest),g.LastMessage);Check(g.UndoCount==4,"Cantidad de decisiones incorrecta.");
                Restored(g,terra);Restored(g,assault);Restored(g,deployed);Restored(g,start);
                Check(!g.CanUndo&&!g.Undo(),"Retrocede más allá del inicio.");Check(StateValidator.Validate(g.State,catalog)=="","Cartas duplicadas.");
            });
            test("Rewind: fase conserva sus validaciones al volver atrás",()=>{
                var g=New(catalog);var p=Put(g,"ferret-eluding-hunter",0,0,0);p.remainingMovement=3;
                Check(MovementManager.Paths(g,p).Count==0,"Movimiento en Despliegue.");g.AdvanceStage();
                Check(MovementManager.Paths(g,p).Count>0,"No mueve en Asalto.");g.AdvanceStage();
                Check(MovementManager.Paths(g,p).Count==0,"Movimiento en Terraformación.");Check(g.Undo(),"No vuelve.");
                p=g.State.tiles[p.tileId].unit;Check(g.State.stage==TurnStage.Assault&&MovementManager.Paths(g,p).Count>0,"No recupera permiso.");
                Check(!g.EndTurn(),"Termina en Asalto.");
            });
            test("Rewind: pago, robo y descarte de una magia se revierten juntos",()=>{
                var g=New(catalog);var c=Hand(g,"a-price-to-pay");string before=Snapshot(g);
                Check(g.Play(c.instanceId,Array.Empty<int>())&&g.State.pendingChoices.Count==1,"No inicia magia.");
                Check(ChoiceManager.Resolve(g,g.State.Active.hand.Take(2).Select(x=>x.instanceId).ToArray()),"No descarta.");
                Check(g.UndoCount==1,"Elección parcial registrada como otra acción.");Restored(g,before);
                Check(g.Play(c.instanceId,Array.Empty<int>()),"No repite.");Restored(g,before);
            });
            test("Rewind: sacrificar Firefly, terraformar y envenenar es una acción atómica",()=>{
                var g=New(catalog);g.State.activePlayer=1;g.State.stage=TurnStage.Assault;
                var firefly=Put(g,"firefly-token",1,0,0);var enemy=Put(g,"old-mummy",0,2,0);string before=Snapshot(g);
                Check(AbilityManager.Activate(g,firefly,new[]{enemy.tileId}),g.LastMessage);
                Check(g.State.pendingChoices.Count==1&&g.CanUndo,"No permite deshacer elección pendiente.");
                Check(ChoiceManager.Resolve(g,new[]{(int)Biome.Swamp})&&enemy.poison==1,"No resuelve veneno.");
                Check(g.UndoCount==1,"Sacrificio fragmentado.");Restored(g,before);
            });
            test("Rewind: dados repiten el resultado y restauran daño, efectos y movimiento",()=>{
                var g=New(catalog);g.State.stage=TurnStage.Assault;var p=Put(g,"charming-mongoose",0,0,0);p.remainingMovement=3;
                int source=p.tileId;int dest=MovementManager.Paths(g,p).Keys.First();
                g.State.tiles[dest].specialEffect=new TerrainEffect{threshold=7,damage=1,compatibleTag="NONE",bonusDamage=1};string before=Snapshot(g);
                Check(MovementManager.Move(g,p,dest)&&g.State.diceRolls.Count>0,"No tira dado.");string result=Snapshot(g);int roll=g.State.diceRolls.Last().value;
                Restored(g,before);p=g.State.tiles[source].unit;
                Check(MovementManager.Move(g,p,dest)&&g.State.diceRolls.Last().value==roll&&Snapshot(g)==result,"Repetición cambia azar o consecuencias.");
                Restored(g,before);
            });
            test("Rewind: movimiento recupera posición, contadores, bonificaciones y conquista",()=>{
                var g=New(catalog);g.State.stage=TurnStage.Assault;var p=Put(g,"dune-cammel",2,0,0);g.State.activePlayer=2;p.remainingMovement=4;
                p.statBonuses.Add(new StatBonus{hp=2,attack=1,expiresTurn=8});p.health+=2;p.poison=1;p.slow=1;p.slowUntilTurn=7;
                int dest=MovementManager.Paths(g,p).Keys.First();g.State.tiles[dest].biome=Biome.Desert;g.State.tiles[dest].conquestSite=true;
                string before=Snapshot(g);Check(MovementManager.Move(g,p,dest),g.LastMessage);Restored(g,before);
            });
            test("Rewind: ataque y respuestas se revierten como una batalla completa",()=>{
                var g=New(catalog);g.State.stage=TurnStage.Assault;
                var attacker=Put(g,"wandering-beast",0,0,0);attacker.attacked=false;var enemy=Put(g,"old-mummy",2,1,0);
                var response=PrototypeScenario.Take(g,2,"engage");g.State.players[2].hand.Add(response);g.State.players[2].currentEnergy=20;
                string before=Snapshot(g);Check(CombatManager.Attack(g,attacker,enemy.tileId),g.LastMessage);
                Check(g.State.battle!=null,"La prueba no abrió una ventana de respuesta.");
                for(int i=0;g.State.battle!=null&&i<12;i++)Check(BattleManager.Pass(g,g.ActingPlayerId),"No pasa prioridad.");
                Check(g.State.battle==null&&g.UndoCount==1,"Batalla sin resolver o fragmentada.");Restored(g,before);
            });
            test("Rewind: intentos inválidos y consultas no consumen historial",()=>{
                var g=New(catalog);g.AdvanceStage();int count=g.UndoCount;string before=Snapshot(g);
                Check(!g.Terraform(-1,Biome.Forest)&&!g.Play(-10,Array.Empty<int>())&&!g.EndTurn(),"Acción inválida aceptada.");
                g.Notify("Consulta de carta");Check(g.UndoCount==count&&Snapshot(g)==before,"Consulta o fallo altera historial.");
            });
            test("Rewind: referencias a piezas anteriores se rechazan tras restaurar",()=>{
                var g=New(catalog);g.State.stage=TurnStage.Assault;var p=Put(g,"charming-mongoose",0,0,0);p.remainingMovement=3;
                int dest=MovementManager.Paths(g,p).Keys.First();string before=Snapshot(g);
                Check(MovementManager.Move(g,p,dest),"No mueve.");Restored(g,before);
                Check(!MovementManager.Move(g,p,dest)&&g.UndoCount==0&&Snapshot(g)==before,"Una pieza obsoleta modifica la partida.");
            });
            test("Rewind: terminar turno cierra historial y conserva ingresos y veneno del siguiente",()=>{
                var g=New(catalog);var p=Put(g,"old-mummy",1,0,0);p.poison=1;
                g.AdvanceStage();g.AdvanceStage();Check(g.EndTurn(),"No finaliza.");
                Check(g.State.activePlayer==1&&!g.CanUndo&&g.UndoCount==0&&p.health==4,"Historial anterior o daño inicial incorrecto.");
                string beginning=Snapshot(g);Check(!g.Undo(),"Deshace turno anterior.");g.AdvanceStage();Restored(g,beginning);
            });
            test("Rewind: nueva partida y carga cierran historial, incluso mismo turno",()=>{
                var g=New(catalog);g.AdvanceStage();string saved=Snapshot(g);g.Restore(GamePersistence.Deserialize(saved));
                Check(!g.CanUndo,"Carga conserva historial ajeno.");g.AdvanceStage();Check(g.CanUndo,"Carga impide nuevas decisiones.");
                g.NewGame(new[]{"FAUNAR","ZUKGROK"},381,CardCatalog.LoadRules(),2);Check(!g.CanUndo,"Nueva partida conserva historial.");
            });
            test("Rewind: el cambio opcional de Sahria se restaura sin consumir energía",()=>{
                var g=New(catalog);g.State.activePlayer=2;string before=Snapshot(g);
                Check(g.SetSahriaDeployment(true)&&g.State.Active.sahriaDeployment,"No activa.");Restored(g,before);
            });
            test("Rewind: la vista recibe el historial actualizado con una notificación por decisión",()=>{
                var g=New(catalog);int notifications=0,count=-1;g.Changed+=()=>{notifications++;count=g.UndoCount;};
                g.AdvanceStage();Check(notifications==1&&count==1&&g.CanUndo,"UI notificada antes de guardar historial.");
                g.Undo();Check(notifications==2&&count==0&&!g.CanUndo,"UI conserva botón activo.");
            });
        }
    }
}
#endif
