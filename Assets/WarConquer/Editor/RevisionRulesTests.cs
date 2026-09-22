#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class RevisionRulesTests
    {
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
        static GameManager New(CardCatalog c,int humans=4){var g=new GameManager(c);g.NewGame(new[]{"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules(),humans);return g;}
        static void End(GameManager g){while(g.State.stage!=TurnStage.Assault)g.AdvanceStage();g.EndTurn();}
        static CardInstance Hand(GameManager g,string id){var c=PrototypeScenario.Take(g,g.State.activePlayer,id);g.State.Active.hand.Add(c);g.State.Active.currentEnergy=20;g.State.stage=g.Catalog[id].allowedPhases[0];return c;}
        public static void RunAll(CardCatalog cards,Action<string,Action> test)
        {
            foreach(int humans in new[]{1,2,3,4})
            {
                int count=humans;test("Mapa "+count+": simetría, expansión equivalente, tres objetivos y bases de siete",()=>{
                    var s=New(cards,count).State;int players=Math.Max(2,count);var homes=s.tiles.Where(t=>t.baseOwner>=0).ToArray();var sites=s.tiles.Where(ConquestManager.IsCenter).ToArray();
                    Check(homes.Length==players&&sites.Length==3,"Número de bases/objetivos.");
                    var distances=sites.Select(t=>BoardManager.Distance(s,homes[0].id,t.id)).OrderBy(x=>x).ToArray();
                    var expansion=s.tiles.Select(t=>BoardManager.Distance(s,homes[0].id,t.id)).OrderBy(x=>x).ToArray();
                    foreach(var home in homes){Check(s.tiles.Count(t=>t.owner==home.baseOwner)==7&&home.neighbors.All(n=>s.tiles[n].owner==home.baseOwner),"Base incompleta.");
                        Check(distances.SequenceEqual(sites.Select(t=>BoardManager.Distance(s,home.id,t.id)).OrderBy(x=>x)),"Distancia desigual a objetivos.");
                        Check(expansion.SequenceEqual(s.tiles.Select(t=>BoardManager.Distance(s,home.id,t.id)).OrderBy(x=>x)),"Expansión desigual.");}
                    foreach(var a in sites)foreach(var b in sites.Where(t=>t!=a))Check(BoardManager.AxialDistance(a,b)>=2,"Objetivos pegados.");
                    foreach(var t in s.tiles){Check(BoardManager.Distance(s,0,t.id)<999,"Isla desconectada.");
                        int q=players==3?-t.q-t.r:-t.q,r=players==3?t.q: t.q+t.r;
                        Check(s.tiles.Any(x=>x.q==q&&x.r==r&&x.hiddenResource==t.hiddenResource&&x.resource==t.resource),"Geometría o recursos asimétricos.");}
                    int previousCount=players==2?63:players==3?91:125;
                    Check(s.tiles.Count>=previousCount*2,"El mapa no duplica la superficie jugable anterior.");
                    Check(s.tiles.Max(t=>Math.Abs(t.q))>=(players==2?10:players==3?10:14)&&s.tiles.Max(t=>Math.Abs(2*t.r+t.q))>=(players==2?12:players==3?20:18),"El mapa no duplica sus dimensiones.");
                    if(players==3){Check(s.tiles.Count==331,"Hexágono de radio 10 incompleto.");foreach(var home in homes)foreach(var site in sites)Check(BoardManager.Distance(s,home.id,site.id)>=7,"Objetivo demasiado próximo a una base.");}
                });
            }
            test("Solo y duelo comparten exactamente el mismo mapa",()=>{var a=New(cards,1).State;var b=New(cards,2).State;Check(a.tiles.Select(t=>(t.q,t.r,t.baseOwner)).SequenceEqual(b.tiles.Select(t=>(t.q,t.r,t.baseOwner))),"Geometrías distintas.");});
            test("Conquista exige unidad y bioma y solo puntúa al siguiente turno propio",()=>{
                var g=New(cards);var t=g.State.tiles.First(ConquestManager.IsCenter);t.owner=0;t.biome=Biome.Forest;ConquestManager.Refresh(g.State);
                Check(ConquestManager.Income(g.State,0)==0,"Bioma solo puntúa.");var p=PrototypeScenario.Spawn(g,0,"bestia-micelial",t.id);t.biome=Biome.Neutral;ConquestManager.Refresh(g.State);
                Check(ConquestManager.Income(g.State,0)==0,"Unidad sola puntúa.");TerrainManager.Terraform(g,t.id,Biome.Forest,0);ConquestManager.ScoreStart(g.State,0);Check(g.State.players[0].conquestPoints==0,"Punto inmediato.");
                for(int i=0;i<3;i++)End(g);Check(g.State.players[0].conquestPoints==0,"Puntúa en turno ajeno.");End(g);Check(g.State.players[0].conquestPoints==1,"No puntúa al volver.");
                g.Restore(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)));ConquestManager.ScoreStart(g.State,0);Check(g.State.players[0].conquestPoints==1,"Guardar duplica puntos.");
            });
            test("Abandonar o perder el bioma reinicia la espera de conquista",()=>{
                var g=New(cards);var t=g.State.tiles.First(ConquestManager.IsCenter);var p=PrototypeScenario.Spawn(g,0,"bestia-micelial",t.id);TerrainManager.Terraform(g,t.id,Biome.Forest,0);
                int adjacent=t.neighbors.First(n=>g.State.tiles[n].baseOwner<0&&!g.State.tiles[n].IsOccupied);MovementManager.Relocate(g,p,adjacent);
                Check(t.garrisonOwner==-1,"No libera guarnición.");MovementManager.Relocate(g,p,t.id);ConquestManager.ScoreStart(g.State,0);Check(g.State.players[0].conquestPoints==0,"Recaptura da puntos inmediatos.");
                t.hiddenAsh=false;TerrainManager.DestroyBiome(g,t);Check(t.garrisonOwner==-1&&ConquestManager.Income(g.State,0)==0,"Sigue puntuando sin bioma.");
            });
            test("Tres guarniciones dan tres PC y la base enemiga derrotada requiere ambas condiciones",()=>{
                var g=New(cards);foreach(var t in g.State.tiles.Where(ConquestManager.IsCenter)){g.Place("espora",0,t.id);t.owner=0;t.biome=Biome.Forest;}
                var home=g.State.tiles.Single(t=>t.baseOwner==1);g.Place("espora",0,home.id);home.owner=0;home.biome=Biome.Forest;
                Check(ConquestManager.Income(g.State,0)==3,"Base viva genera puntos.");g.State.players[1].eliminated=true;ConquestManager.Refresh(g.State);Check(ConquestManager.Income(g.State,0)==4,"Base derrotada no cuenta.");
                g.State.turn+=4;ConquestManager.ScoreStart(g.State,0);Check(g.State.players[0].conquestPoints==4,"Puntuación múltiple incorrecta.");
            });
            test("Terraformación manual, por magia y por IA rechaza casillas con enemigo",()=>{
                var g=New(cards,1);var tile=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0);var enemy=PrototypeScenario.Spawn(g,1,"nomada-de-arena",tile.id);tile.owner=0;
                string before=GamePersistence.Serialize(g.State);TerrainManager.Terraform(g,tile.id,Biome.Desert,0);Check(GamePersistence.Serialize(g.State)==before,"Atajo de IA altera terreno ocupado.");
                var c=Hand(g,"brote-repentino");before=GamePersistence.Serialize(g.State);Check(!g.Play(c.instanceId,new[]{tile.id})&&before==GamePersistence.Serialize(g.State),"Magia terraformó bajo enemigo.");
                g.State.stage=TurnStage.Terraforming;Check(!g.Terraform(tile.id,Biome.Forest)&&!g.TerraformTarget(tile.id),"Terraformación manual permitida.");
            });
            test("Veneno acumulativo 1 2 4 atraviesa defensa y reaplicarlo no reinicia el daño",()=>{
                var g=New(cards);int tile=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0).id;var p=PrototypeScenario.Spawn(g,1,"guardian-del-obelisco",tile);p.health=30;
                EffectManager.Poison(g,p,1,0);End(g);Check(p.health==29&&p.poison==2,"Primer tick.");EffectManager.Poison(g,p,1,0);Check(p.poison==2&&p.poisonApplications==2,"Reaplicar reinicia daño.");
                for(int i=0;i<4;i++)End(g);Check(p.health==27&&p.poison==4,"Segundo tick.");for(int i=0;i<4;i++)End(g);Check(p.health==23&&p.poison==8,"Tercer tick.");
                p.health=1;for(int i=0;i<4;i++)End(g);Check(g.State.tiles[tile].unit==null,"El veneno no mata.");
            });
            test("Magias sin límite de alcance admiten un objetivo lejano y respetan su tipo",()=>{
                var g=New(cards);var far=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0);var p=PrototypeScenario.Spawn(g,1,"nomada-de-arena",far.id);far.biome=Biome.Forest;var c=Hand(g,"espora-somnifera");
                Check(!g.InRange(far.id,3)&&g.CardTargets(cards[c.cardId]).Contains(far.id),"Alcance artificial sigue bloqueando magia.");
                Check(g.Play(c.instanceId,new[]{far.id})&&g.IsSleeping(p)&&p.remainingMovement==0,"Sueño no aplicado.");
                var own=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0);var ally=PrototypeScenario.Spawn(g,0,"bestia-micelial",own.id);c=Hand(g,"nube-de-esporas");
                Check(!g.CardTargets(cards[c.cardId]).Contains(own.id),"Permite objetivo aliado.");far.biome=Biome.Forest;Check(g.Play(c.instanceId,new[]{far.id})&&p.poison==1,"Veneno dirigido no funciona.");
            });
            test("Magias de mejora, sueño y ralentización respetan objetivos y efectos",()=>{
                var g=New(cards);PrototypeScenario.Load(g);var ally=g.Allies(0).First(p=>!g.Data(p).IsStructure);var enemy=g.Allies(1).First(p=>!g.Data(p).IsStructure);
                int spores=g.State.Active.spores;var c=Hand(g,"lluvia-de-esporas");Check(g.Play(c.instanceId,new[]{ally.tileId})&&ally.bonusAttack==1&&g.State.Active.spores==spores+1,"Lluvia de Esporas.");
                EffectManager.Poison(g,enemy,1,0);int hp=enemy.health;c=Hand(g,"sueno-micelial");Check(g.Play(c.instanceId,new[]{enemy.tileId})&&g.IsSleeping(enemy)&&enemy.health==hp-1,"Sueño Micelial.");
                End(g);c=Hand(g,"tormenta-de-arena");Check(g.Play(c.instanceId,new[]{ally.tileId})&&ally.slow==1,"Tormenta de Arena.");
            });
            test("Erosión elige la estructura enemiga aunque comparta casilla con aliado",()=>{
                var g=New(cards);int id=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0).id;var enemy=PrototypeScenario.Spawn(g,1,"oasis-de-cristal",id);var ally=PrototypeScenario.Spawn(g,0,"bestia-micelial",id);int hp=ally.health,structureHp=enemy.health;
                // A second Sahria player attacks the enemy structure, sharing the tile with its own unit.
                End(g);End(g);End(g);ally.owner=3;var c=Hand(g,"erosion");Check(g.Play(c.instanceId,new[]{id})&&ally.health==hp&&enemy.health==structureHp-2,"Erosión dañó una unidad aliada.");
            });
            test("Mar de Arena permite desiertos libres sin inventar propiedad y excluye ocupantes enemigos",()=>{
                var g=New(cards);End(g);var t=g.State.tiles.First(t=>t.territory==4);t.owner=0;t.biome=Biome.Desert;var c=Hand(g,"mar-de-arena");
                Check(g.CardTargets(cards[c.cardId]).Contains(t.id),"Restricción de propiedad ausente en la carta.");var p=PrototypeScenario.Spawn(g,0,"bestia-micelial",t.id);
                Check(!g.CardTargets(cards[c.cardId]).Contains(t.id),"Modifica terreno bajo enemigo.");CombatManager.Remove(g,p);Check(g.Play(c.instanceId,new[]{t.id})&&t.specialEffect.threshold==5&&t.specialEffect.onExit,"Mar no aplica su dado.");
            });
            test("Habilidad en terreno afectado tira dado y fallo consume la activación",()=>{
                var g=New(cards);PrototypeScenario.Load(g);var source=g.Allies(0).First(p=>g.Data(p).Has("FastNetwork"));g.State.stage=TurnStage.Terraforming;g.State.tiles[source.tileId].specialEffect=new TerrainEffect{threshold=7,damage=1};
                var targets=AbilityManager.Targets(g,source).Take(2).ToArray();int hp=source.health;Check(AbilityManager.Activate(g,source,targets)&&source.abilityUsed&&source.health==hp-1&&g.State.diceRolls.Count==1,"Habilidad sin tirada.");
            });
            test("Dados de movimiento y ataque registran el resultado real y consumen el intento fallido",()=>{
                var g=New(cards);PrototypeScenario.Load(g);g.State.stage=TurnStage.Assault;var p=g.Allies(0).First(x=>!g.Data(x).IsStructure);var enemy=g.Allies(1).First(x=>!g.Data(x).IsStructure);int hp=enemy.health;
                g.State.tiles[p.tileId].specialEffect=new TerrainEffect{threshold=7,damage=1};int health=p.health;
                Check(CombatManager.Attack(g,p,enemy.tileId)&&p.attacked&&enemy.health==hp&&p.health==health-1,"Ataque no interrumpido.");
                Check(g.State.diceRolls.Count==1&&g.State.diceRolls[0].value>=1&&g.State.diceRolls[0].value<=6&&g.State.diceRolls[0].threshold==7,"Tirada incorrecta.");
                var saved=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(saved.diceRolls[0].value==g.State.diceRolls[0].value&&saved.nextRollId==g.State.nextRollId,"Tirada perdida al guardar.");
                string before=GamePersistence.Serialize(g.State);Check(!CombatManager.Attack(g,p,enemy.tileId)&&before==GamePersistence.Serialize(g.State),"Segundo intento tira de nuevo.");
            });
        }
    }
}
#endif

