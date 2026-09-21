#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class GrayboxTests
    {
        static int passed;
        static CardCatalog catalog;
        static readonly List<string> results=new List<string>();
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        static void Test(string name,Action body){body();passed++;results.Add("PASS "+name);Debug.Log("PASS "+name);}
        static GameManager New(){var g=new GameManager(catalog);g.NewGame(new[]{"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules());return g;}
        static CardInstance Hand(GameManager g,string id)
        {
            g.State.stage=catalog[id].allowedPhases[0];var p=g.State.Active;var existing=p.hand.Find(c=>c.cardId==id);if(existing!=null)return existing;
            var c=PrototypeScenario.Take(g,p.id,id);p.hand.Add(c);return c;
        }
        static int Home(GameManager g,int player=0,int skip=0)=>g.State.tiles.Where(t=>t.territory==player&&t.baseOwner<0&&!t.IsOccupied).Skip(skip).First().id;
        static void Energy(GameManager g,int value=20){g.State.Active.currentEnergy=value;g.State.Active.maxEnergy=value;}
        static void Valid(GameManager g){string error=StateValidator.Validate(g.State,catalog);Check(error=="",error);}
        [MenuItem("War & Conquer/Ejecutar pruebas de reglas")]
        public static void RunAll()
        {
            Board3DAssets.Ensure();passed=0;results.Clear();catalog=CardCatalog.Load();
            Test("Mazos finales: 50 + 50, copias y todos los efectos definidos",()=>{
                Check(catalog.All.Count(c=>c.quantity>0)==58,"Deben existir 58 diseños del documento final.");
                foreach(string leader in new[]{"ZUKGROK","SAHRIA"})Check(catalog.All.Where(c=>c.leader==leader).Sum(c=>c.quantity)==50,"Conteo de mazo incorrecto.");
                Check(catalog.All.All(c=>c.category==Category.Spell?c.effects.Length>0:c.traits.Length>0),"Carta sin efecto o rasgo implementado.");
            });
            Test("87 casillas neutras, 4 territorios de 17 y centro de 19",()=>{
                var g=New();Check(g.State.tiles.Count==87&&g.State.tiles.All(t=>t.biome==Biome.Neutral),"Tablero inicial incorrecto.");
                for(int i=0;i<5;i++)Check(g.State.tiles.Count(t=>t.territory==i)==(i==4?19:17),"Distribución incorrecta.");
                Check(g.State.tiles.Count(t=>t.baseOwner>=0)==4,"Bases incorrectas.");Valid(g);
            });
            Test("Mapa continuo: aristas compartidas, sin puentes, sin huecos",()=>{
                var s=New().State;Check(s.connections.All(c=>!c.bridge),"El mapa no debe contener puentes.");
                Check(s.tiles.Select(t=>(t.q,t.r)).Distinct().Count()==87,"Hay casillas superpuestas.");
                foreach(var a in s.tiles)foreach(var b in s.tiles.Where(t=>t.id>a.id))
                {
                    bool adjacent=BoardManager.AxialDistance(a,b)==1;
                    Check(a.neighbors.Contains(b.id)==adjacent&&b.neighbors.Contains(a.id)==adjacent,"Adyacencia visual y lógica diferentes.");
                    Check(s.connections.Count(c=>c.a==a.id&&c.b==b.id)==(adjacent?1:0),"Arista incorrecta o duplicada.");
                    if(adjacent)Check(Math.Abs(Vector2.Distance(new Vector2(a.x,a.y),new Vector2(b.x,b.y))-1.385640646f)<.0001f,"Los hexágonos no comparten borde.");
                }
                foreach(var column in s.tiles.GroupBy(t=>t.q))Check(column.Count()==column.Max(t=>t.r)-column.Min(t=>t.r)+1,"Hueco en la retícula.");
            });
            Test("Entradas múltiples por territorio y sin un único punto de corte",()=>{
                var s=New().State;
                for(int territory=0;territory<4;territory++)
                {
                    foreach(int destination in new[]{4,(territory+1)%4,(territory+3)%4})
                    {
                        var entrances=s.tiles.Where(t=>t.territory==territory&&t.neighbors.Any(n=>s.tiles[n].territory==destination)).ToList();
                        Check(entrances.Count>=2,"Faltan accesos compartidos desde "+territory+" a "+destination);
                    }
                    var region=s.tiles.Where(t=>t.territory==territory).ToList();var reached=new HashSet<int>();var pending=new Queue<int>();pending.Enqueue(region[0].id);
                    while(pending.Count>0){int id=pending.Dequeue();if(!reached.Add(id))continue;foreach(int n in s.tiles[id].neighbors.Where(n=>s.tiles[n].territory==territory))pending.Enqueue(n);}
                    Check(reached.Count==17,"Territorio inicial dividido.");
                }
                foreach(var removed in s.tiles)
                {
                    var queue=new Queue<int>();var seen=new HashSet<int>();queue.Enqueue(removed.id==0?1:0);
                    while(queue.Count>0){int id=queue.Dequeue();if(id==removed.id||!seen.Add(id))continue;foreach(int n in s.tiles[id].neighbors)queue.Enqueue(n);}
                    Check(seen.Count==86,"Punto de corte en "+removed.id);
                }
            });
            Test("Renderizadores de casillas y fichas disponibles en Unity",()=>{
                var hex=new GameObject("Test hex",typeof(RectTransform));var piece=new GameObject("Test piece",typeof(RectTransform));
                try
                {
                    hex.AddComponent<HexGraphic>();piece.AddComponent<PieceGraphic>();
                    Check(hex.GetComponent<CanvasRenderer>()!=null&&piece.GetComponent<CanvasRenderer>()!=null,"Falta CanvasRenderer en el graybox.");
                }
                finally{UnityEngine.Object.DestroyImmediate(hex);UnityEngine.Object.DestroyImmediate(piece);}
            });
            Test("Guardados del mapa anterior se rechazan sin recrear puentes",()=>{
                var s=New().State;s.version=1;Check(StateValidator.Validate(s,catalog)!="","Se admitió el mapa anterior.");
            });
            Test("Barajado reproducible y manos independientes",()=>{
                var a=New();var b=New();Check(a.State.players.All(p=>p.hand.Count==5&&p.deck.Count==45),"Mano inicial incorrecta.");
                Check(a.State.Active.deck.Select(c=>c.cardId).SequenceEqual(b.State.Active.deck.Select(c=>c.cardId)),"Semilla no reproducible.");
                Check(!ReferenceEquals(a.State.players[0].deck,a.State.players[2].deck),"Mazos compartidos por referencia.");
            });
            Test("Colocar unidad paga una vez y conserva la carta",()=>{
                var g=New();Energy(g);var c=Hand(g,"bestia-micelial");int tile=Home(g),energy=g.State.Active.currentEnergy;
                Check(g.Play(c.instanceId,new[]{tile}),"No se colocó unidad.");Check(g.State.tiles[tile].unit.cardId==c.cardId&&g.State.Active.currentEnergy==energy-3,"Estado de colocación incorrecto.");
                Check(!g.Play(c.instanceId,new[]{Home(g)}),"Se pudo repetir la misma carta.");Valid(g);
            });
            Test("Energía y objetivos inválidos no consumen ni duplican",()=>{
                var g=New();var c=Hand(g,"ancestro-micelial");g.State.Active.currentEnergy=0;string before=JsonUtility.ToJson(g.State);
                Check(!g.Play(c.instanceId,new[]{Home(g)}),"Carta gratis.");Check(before==JsonUtility.ToJson(g.State),"Mutación en acción rechazada.");
                Energy(g);before=JsonUtility.ToJson(g.State);Check(!g.Play(c.instanceId,new[]{999}),"Hex inválido permitido.");Check(before==JsonUtility.ToJson(g.State),"Objetivo inválido cobrado.");
            });
            Test("Estructura persiste, bloquea y muerte va al descarte",()=>{
                var g=New();Energy(g);var c=Hand(g,"semillero-micelial");int id=Home(g);Check(g.Play(c.instanceId,new[]{id}),"Estructura no jugada.");
                Check(g.State.tiles[id].BlocksMovement,"Estructura no bloquea.");CombatManager.Damage(g,g.State.tiles[id].structure,99,false);
                Check(g.State.tiles[id].structure==null&&g.State.Active.discardPile.Any(x=>x.instanceId==c.instanceId),"Destrucción no descarta.");Valid(g);
            });
            Test("Magia terraforma y termina en descarte",()=>{
                var g=New();Energy(g);var c=Hand(g,"brote-repentino");int id=Home(g);Check(g.Play(c.instanceId,new[]{id}),"Magia rechazada.");
                Check(g.State.tiles[id].biome==Biome.Forest&&g.State.Active.discardPile.Contains(c),"Efecto o descarte incorrecto.");Valid(g);
            });
            Test("Terraformación múltiple exige adyacencia",()=>{
                var g=New();Energy(g);var c=Hand(g,"crecimiento-descontrolado");int a=Home(g),b=Home(g,0,15);int e=g.State.Active.currentEnergy;
                Check(!g.Play(c.instanceId,new[]{a,b}),"Permitió dos casillas no adyacentes.");Check(e==g.State.Active.currentEnergy,"Cobró acción rechazada.");
                b=g.State.tiles[a].neighbors.First(n=>g.TerraformTarget(n));Check(g.Play(c.instanceId,new[]{a,b},false,Biome.Swamp),"Terraformación doble no funciona.");Check(g.State.tiles[a].biome==Biome.Swamp&&g.State.tiles[b].biome==Biome.Swamp,"Bioma incorrecto.");Valid(g);
            });
            Test("Compatibilidad de biomas y excepción neutral",()=>{
                var g=New();Energy(g);int id=Home(g);g.State.tiles[id].biome=Biome.Desert;var c=Hand(g,"bestia-micelial");
                Check(!g.Play(c.instanceId,new[]{id}),"Permitió unidad incompatible en Desierto.");g.State.tiles[id].biome=Biome.Neutral;Check(g.Play(c.instanceId,new[]{id}),"Despliegue inicial neutral bloqueado.");
            });
            Test("Latentes bloqueadas hasta revelar Tierra Ceniza",()=>{
                var g=New();Energy(g);int id=Home(g);var c=Hand(g,"rey-micelial");Check(!g.Play(c.instanceId,new[]{id}),"Latente sin ceniza.");
                g.State.tiles[id].biome=Biome.Forest;g.State.stage=TurnStage.Terraforming;Check(g.RevealAsh(id),"No revela ceniza.");g.State.stage=TurnStage.Deployment;Check(g.Play(c.instanceId,new[]{id}),"Latente no se juega desde ceniza.");Valid(g);
            });
            Test("Destruir bioma conserva hex y revela ceniza marcada",()=>{
                var g=New();var a=g.State.tiles[Home(g)];a.biome=Biome.Forest;a.hiddenAsh=false;TerrainManager.DestroyBiome(g,a);Check(a.biome==Biome.Neutral,"No neutraliza.");
                a.biome=Biome.Forest;a.hiddenAsh=true;TerrainManager.DestroyBiome(g,a);Check(a.biome==Biome.AshLand&&g.State.tiles.Count==87,"Ceniza o casillas incorrectas.");
            });
            Test("Recursos solo pagan cartas con etiqueta compatible",()=>{
                var g=New();g.State.Active.currentEnergy=0;EnergyManager.AddResource(g.State,g.State.Active,Biome.Desert,5);var c=Hand(g,"bestia-micelial");
                Check(!g.Play(c.instanceId,new[]{Home(g)},true),"Recursos incompatibles usados.");EnergyManager.AddResource(g.State,g.State.Active,Biome.Forest,3);Check(g.Play(c.instanceId,new[]{Home(g)},true),"Pago compatible rechazado.");Check(g.State.Active.currentEnergy==0,"Energía negativa.");Valid(g);
            });
            Test("Turnos 1→2→3→4, energía automática y robo real",()=>{
                var g=New();int count=g.State.Active.deck.Count;for(int i=1;i<=4;i++){Check(FinishTurn(g),"No termina turno.");Check(g.State.activePlayer==i%4,"Orden incorrecto.");}
                Check(g.State.Active.deck.Count==count-1&&g.State.Active.hand.Count==6&&g.State.Active.currentEnergy==4,"Robo/energía incorrecto.");Valid(g);
            });
            Test("Dormido bloquea movimiento y ataque solo durante su turno",()=>{
                var g=New();var p=PrototypeScenario.Spawn(g,1,"nomada-de-arena",Home(g,1));EffectManager.Sleep(g,p,0);FinishTurn(g);
                Check(g.IsSleeping(p)&&Paths(g,p).Count==0&&CombatManager.Targets(g,p).Count==0,"Dormido permite acciones.");FinishTurn(g);Check(p.sleepUntilTurn==0,"Dormido no expira.");Valid(g);
            });
            Test("Veneno se resuelve en turnos propios y expira",()=>{
                var g=New();var p=PrototypeScenario.Spawn(g,1,"guardian-del-obelisco",Home(g,1));int hp=p.health;EffectManager.Poison(g,p,1,0);FinishTurn(g);Check(p.health==hp-1,"Primer daño de veneno incorrecto.");
                for(int i=0;i<4;i++)FinishTurn(g);Check(p.health==hp-2&&p.poison==0,"Duración del veneno incorrecta.");Valid(g);
            });
            Test("Movimiento respeta ocupación, alcance y coste",()=>{
                var g=New();var p=PrototypeScenario.Spawn(g,0,"recolector-de-esporas",Home(g));int from=p.tileId;
                var paths=Paths(g,p);Check(paths.Count>0,"Sin movimientos.");int dest=paths.Keys.First();Check(Move(g,p,dest)&&g.State.tiles[from].unit==null&&g.State.tiles[dest].unit==p,"No mueve estado real.");
                Check(!Move(g,p,Home(g,2)),"Teletransporte sin ruta.");Valid(g);
            });
            Test("Combate una vez, alcance, veneno y defensa",()=>{
                var g=New();PrototypeScenario.Load(g);var p=g.Allies(0).First(a=>!g.Data(a).IsStructure);var enemy=g.Allies(1).First(a=>!g.Data(a).IsStructure);
                enemy.health=9;Check(Attack(g,p,enemy.tileId),"Ataque válido rechazado.");Check(enemy.health==6&&enemy.poison==1,"Daño o veneno incorrecto.");Check(!Attack(g,p,enemy.tileId),"Doble ataque permitido.");Valid(g);
            });
            Test("Terreno inestable tira dado; fallo detiene y daña",()=>{
                var g=New();var p=PrototypeScenario.Spawn(g,0,"bestia-micelial",Home(g));int from=p.tileId;int dest=Paths(g,p).Keys.First(n=>g.State.tiles[from].neighbors.Contains(n));
                g.State.tiles[dest].specialEffect=new TerrainEffect{threshold=7,damage=2};int hp=p.health;
                Check(Move(g,p,dest),"Intento no resuelto.");Check(p.tileId==from&&p.health==hp-2&&p.remainingMovement==0,"Fallo de entrada incorrecto.");
                Check(g.State.log.Any(l=>l.Contains("d6=")),"No registra tirada.");Valid(g);
            });
            Test("Unidades compatibles y voladoras ignoran terreno",()=>{
                var g=New();FinishTurn(g);var p=PrototypeScenario.Spawn(g,1,"nomada-de-arena",Home(g,1));var t=g.State.tiles[Home(g,1)];t.specialEffect=new TerrainEffect {threshold=7,damage=99,onExit=true};
                Check(TerrainManager.CheckUnstable(g,p,t,false)&&TerrainManager.CheckUnstable(g,p,t,true),"Compatibilidad ignorada.");
                var f=PrototypeScenario.Spawn(g,1,"avatar-del-sol",Home(g,1));Check(TerrainManager.CheckUnstable(g,f,t,false),"Voladora afectada.");Valid(g);
            });
            Test("Vía rápida conecta físicamente; destrucción elimina ruta normal",()=>{
                var g=New();PrototypeScenario.Load(g);var p=g.Allies(0).First(a=>!g.Data(a).IsStructure);var route=g.State.fastRoutes.First();int dest=route.a==p.tileId?route.b:route.a;
                Check(Paths(g,p).ContainsKey(dest),"Ruta no modifica movimiento.");Check(Move(g,p,dest)&&p.tileId==dest,"Ruta no mueve.");
                g.State.tiles[dest].hiddenAsh=false;TerrainManager.DestroyBiome(g,g.State.tiles[dest]);Check(g.State.fastRoutes.Count==0,"Ruta normal persiste sobre terreno incompatible.");Valid(g);
            });
            Test("Marcha usa ruta sin consumir movimiento normal",()=>{
                var g=New();PrototypeScenario.Load(g);Energy(g);var p=g.Allies(0).First(a=>!g.Data(a).IsStructure);var c=Hand(g,"marcha-micelial");int mov=p.remainingMovement;var route=g.State.fastRoutes.First();int dest=route.a==p.tileId?route.b:route.a;
                Check(g.Play(c.instanceId,new[]{p.tileId,dest}),"Marcha rechazada.");Check(p.tileId==dest&&p.remainingMovement==mov,"Marcha consume movimiento.");Valid(g);
            });
            Test("Semillero genera ficha fuera del mazo",()=>{
                var g=New();int id=Home(g);PrototypeScenario.Spawn(g,0,"semillero-micelial",id);for(int i=0;i<4;i++)FinishTurn(g);
                Check(g.Allies(0).Any(p=>p.token&&p.cardId=="espora"),"No genera ficha.");Valid(g);
            });
            Test("Habilidad de estructura se usa una vez por turno",()=>{
                var g=New();FinishTurn(g);var p=PrototypeScenario.Spawn(g,1,"sacerdote-solar",Home(g,1));Check(Activate(g,p,new int[0]),"Habilidad rechazada.");
                Check(!Activate(g,p,new int[0])&&g.State.Active.structureDiscount==1,"Habilidad duplicada.");Valid(g);
            });
            Test("Sahria: cura limitada y erosión a estructuras",()=>{
                var g=New();PrototypeScenario.Load(g);FinishTurn(g);Energy(g);
                var unit=g.Allies(1).First(p=>!g.Data(p).IsStructure);var t=g.State.tiles[unit.tileId];t.biome=Biome.Desert;unit.health=2;
                var heal=Hand(g,"llamado-del-oasis");Check(g.Play(heal.instanceId,new[]{t.id}),"No cura.");Check(unit.health==3,"Curación supera salud máxima.");
                var enemy=g.Allies(0).First(p=>g.Data(p).IsStructure);int hp=enemy.health;var erosion=Hand(g,"erosion");Check(g.Play(erosion.instanceId,new[]{enemy.tileId}),"No daña estructura.");Check(enemy.health==hp-2,"Daño de Erosión incorrecto.");Valid(g);
            });
            Test("Sahria: Despertar permite reactivar estructura usada",()=>{
                var g=New();FinishTurn(g);Energy(g);var camp=PrototypeScenario.Spawn(g,1,"campamento-nomada",Home(g,1));var unit=PrototypeScenario.Spawn(g,1,"nomada-de-arena",Home(g,1));
                Check(Activate(g,camp,new[]{unit.tileId}),"Campamento falla.");var spell=Hand(g,"despertar-de-las-ruinas");
                Check(g.Play(spell.instanceId,new[]{camp.tileId})&&!camp.abilityUsed,"No reactiva.");Check(Activate(g,camp,new[]{unit.tileId}),"No permite segunda activación tras Despertar.");Valid(g);
            });
            Test("Sahria: bonus de Líder se consume una vez entre dos casillas",()=>{
                var g=New();FinishTurn(g);Energy(g);int a=Home(g,1),b=Home(g,1,1);g.State.tiles[a].biome=g.State.tiles[b].biome=Biome.Desert;
                Check(Leader(g,new[]{a,b}),"Habilidad de Líder falló.");var enemy=PrototypeScenario.Spawn(g,0,"bestia-micelial",Home(g));
                g.State.tiles[a].specialEffect.threshold=7;int hp=enemy.health;TerrainManager.CheckUnstable(g,enemy,g.State.tiles[a],false);
                Check(enemy.health==hp-2&&g.State.tiles[b].specialEffect.bonusDamage==0,"Bonus duplicado entre las casillas.");Valid(g);
            });
            Test("Salida inestable fallida conserva posición y vida",()=>{
                var g=New();var unit=PrototypeScenario.Spawn(g,0,"bestia-micelial",Home(g));var t=g.State.tiles[unit.tileId];t.specialEffect=new TerrainEffect{threshold=7,damage=1,onExit=true};int hp=unit.health;
                int dest=Paths(g,unit).Keys.First();Move(g,unit,dest);Check(unit.tileId==t.id&&unit.health==hp&&unit.remainingMovement==0,"Fallo al salir incorrecto.");Valid(g);
            });
            Test("Guardia reduce el primer ataque, no todos los ataques de ronda",()=>{
                var g=New();int guardTile=Home(g);var guard=PrototypeScenario.Spawn(g,0,"guardian-del-bosque",guardTile);int id=g.State.tiles[guardTile].neighbors.First(n=>g.State.tiles[n].baseOwner<0);
                var ally=PrototypeScenario.Spawn(g,0,"bestia-micelial",id);int hp=ally.health;CombatManager.Damage(g,ally,2,true);Check(ally.health==hp-1,"Protección no aplica.");CombatManager.Damage(g,ally,2,true);Check(ally.health==hp-3,"Protección se repitió.");Valid(g);
            });
            Test("Oasis permite entrar y protege a su Guardián",()=>{
                var g=New();FinishTurn(g);int id=Home(g,1);var oasis=PrototypeScenario.Spawn(g,1,"oasis-de-cristal",id);int near=g.State.tiles[id].neighbors.First(n=>g.State.tiles[n].baseOwner<0);
                var guard=PrototypeScenario.Spawn(g,1,"guardian-del-oasis",near);Check(Move(g,guard,id),"Oasis bloquea entrada.");int hp=guard.health;CombatManager.Damage(g,guard,2,true);Check(guard.health==hp-1,"Defensa sobre Oasis incorrecta.");Valid(g);
            });
            Test("Explosión puede gastar varias Esporas sobre el mismo enemigo",()=>{
                var g=New();PrototypeScenario.Load(g);Energy(g);var target=g.Allies(1).First(p=>!g.Data(p).IsStructure);int id=target.tileId;var c=Hand(g,"explosion-de-esporas");
                Check(g.Play(c.instanceId,new[]{id,id,id}),"No permite asignar 3 Esporas a un enemigo.");Check(g.State.Active.spores==0&&g.State.tiles[id].unit==null,"Daño o consumo incorrecto.");Valid(g);
            });
            Test("Larva solo evoluciona tras permanecer sobre Bosque",()=>{
                var g=New();int id=Home(g);g.State.tiles[id].biome=Biome.Forest;var larva=PrototypeScenario.Spawn(g,0,"larva-micelial",id);for(int i=0;i<4;i++)FinishTurn(g);
                Check(larva.evolved&&larva.health==g.State.rules.evolvedHealth,"No evoluciona.");Valid(g);
            });
            Test("Bosque Eterno protege las vías existentes",()=>{
                var g=New();PrototypeScenario.Load(g);Energy(g);var ash=g.State.tiles.First(t=>t.territory==4&&t.biome==Biome.AshLand);var c=Hand(g,"bosque-eterno");
                Check(g.Play(c.instanceId,new[]{ash.id}),"Latente estructura rechazada.");var route=g.State.fastRoutes.First();Check(route.protectedRoute,"Ruta no protegida.");
                var end=g.State.tiles[route.b];end.hiddenAsh=false;TerrainManager.DestroyBiome(g,end);Check(g.State.fastRoutes.Count==1,"Se destruyó ruta protegida.");Valid(g);
            });
            Test("Ciudad Sepultada crea fichas y ceniza permanente",()=>{
                var g=New();FinishTurn(g);Energy(g);int id=g.State.tiles.First(t=>t.baseOwner==1).neighbors.First();g.State.tiles[id].biome=Biome.AshLand;var c=Hand(g,"ciudad-sepultada");
                Check(g.Play(c.instanceId,new[]{id}),"Ciudad no jugada.");Check(g.Allies(1).Count(p=>p.token&&p.cardId=="obelisco-de-arena")==2&&g.State.tiles[id].permanentAsh,"Fichas/ceniza incorrectas.");Valid(g);
            });
            Test("Conservación al preparar escenario y guardar/cargar",()=>{
                var g=New();PrototypeScenario.Load(g);Valid(g);var restored=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(StateValidator.Validate(restored,catalog)=="","Guardado no válido.");
                Check(restored.fastRoutes.Count==g.State.fastRoutes.Count&&restored.randomState==g.State.randomState,"Estado perdido en guardado.");
            });
            Test("Partida de 120 turnos mantiene todas las invariantes",()=>{
                var g=New();
                for(int turn=0;turn<120;turn++)
                {
                    foreach(var card in g.State.Active.hand.ToList())if(GameCardTry(g,card))Valid(g);
                    g.AdvanceStage();
                    foreach(var card in g.State.Active.hand.ToList())if(GameCardTry(g,card))Valid(g);
                    var terraform=g.State.tiles.FirstOrDefault(t=>g.TerraformTarget(t.id)&&t.biome==Biome.Neutral);
                    if(terraform!=null)g.Terraform(terraform.id,g.State.Active.leader=="ZUKGROK"?Biome.Forest:Biome.Desert);
                    g.AdvanceStage();
                    foreach(var card in g.State.Active.hand.ToList())if(GameCardTry(g,card))Valid(g);
                    foreach(var p in g.Allies(g.State.activePlayer).Where(p=>!g.Data(p).IsStructure).ToList())
                    {
                        var attacks=CombatManager.Targets(g,p);if(attacks.Count>0)Attack(g,p,attacks[0]);
                        else{var paths=Paths(g,p);if(paths.Count>0)Move(g,p,paths.Keys.Last());}
                        Valid(g);
                    }
                    if(g.State.phase==Phase.Finished)break;
                    FinishTurn(g);Valid(g);
                }
            });
            InterfaceRulesTests.RunAll(catalog,Test);
            MatchSetupTests.RunAll(catalog,Test);
            Board3DTests.RunAll(catalog,Test);
            Debug.Log("WAR_CONQUER_TESTS_PASSED "+passed);
            string report=Environment.GetEnvironmentVariable("WAR_CONQUER_TEST_REPORT");if(!string.IsNullOrEmpty(report))System.IO.File.WriteAllText(report,string.Join("\n",results)+"\nTOTAL "+passed+" passed\n");
        }
        static bool FinishTurn(GameManager g)
        {
            while(g.State.battle!=null)BattleManager.Pass(g,g.ActingPlayerId);
            while(g.State.stage!=TurnStage.Assault&&g.CanAct)g.AdvanceStage();
            return g.EndTurn();
        }
        static Dictionary<int,List<int>> Paths(GameManager g,Piece p){g.State.stage=TurnStage.Assault;return MovementManager.Paths(g,p);}
        static bool Move(GameManager g,Piece p,int target){g.State.stage=TurnStage.Assault;return MovementManager.Move(g,p,target);}
        static bool Attack(GameManager g,Piece p,int target){g.State.stage=TurnStage.Assault;return CombatManager.Attack(g,p,target);}
        static bool Activate(GameManager g,Piece p,IList<int> targets){g.State.stage=TimingRules.AbilityStage(g.Data(p));return AbilityManager.Activate(g,p,targets);}
        static bool Leader(GameManager g,IList<int> targets){g.State.stage=TimingRules.LeaderStage(g.State.Active);return AbilityManager.Leader(g,targets);}
        static bool GameCardTry(GameManager g,CardInstance instance)
        {
            if(g.CardBlockReason(instance,true)!="")return false;var c=catalog[instance.cardId];var targets=g.CardTargets(c);if(targets.Count==0)return false;
            if(c.effects.Any(e=>e.operation=="March")){var next=g.CardTargets(c,new[]{targets[0]});return next.Count>0&&g.Play(instance.instanceId,new[]{targets[0],next[0]},true);}
            return g.Play(instance.instanceId,new[]{targets[0]},true);
        }
    }
}
#endif
