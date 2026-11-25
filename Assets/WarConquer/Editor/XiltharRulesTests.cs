#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class XiltharRulesTests
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static GameManager New(){var g=new GameManager(CardCatalog.Load());g.NewGame(new[]{"XIL'THAR","SAHRIA","ZUKGROK","FAUNAR"},421,CardCatalog.LoadRules());foreach(var p in g.State.players)p.currentEnergy=80;return g;}
        static HexTile At(GameManager g,int q,int r)=>g.State.tiles.First(t=>t.q==q&&t.r==r);
        static Piece Put(GameManager g,string id,int owner,int q,int r,Biome biome=Biome.Void)
        {var t=At(g,q,r);t.biome=biome;t.owner=owner;t.blocked=false;return PrototypeScenario.Spawn(g,owner,id,t.id);}
        static CardInstance Hand(GameManager g,string id){var c=PrototypeScenario.Take(g,g.ActingPlayerId,id);g.ActingPlayer.hand.Add(c);return c;}
        static CardInstance Discard(GameManager g,string id){var c=PrototypeScenario.Take(g,0,id);g.State.players[0].discardPile.Add(c);return c;}
        static void End(GameManager g){while(g.State.stage!=TurnStage.Terraforming)Check(g.AdvanceStage(),g.LastMessage);Check(g.EndTurn(),g.LastMessage);}
        static void Choose(GameManager g,int id){Check(ChoiceManager.Resolve(g,new[]{id}),g.LastMessage);}
        static void Valid(GameManager g){Check(StateValidator.Validate(g.State,g.Catalog)=="",StateValidator.Validate(g.State,g.Catalog));}
        public static void RunBatch()
        {
            try{int count=0;RunAll((name,run)=>{run();count++;Debug.Log("PASS XIL "+name);});Debug.Log("XILTHAR_RULES_PASSED "+count);EditorApplication.Exit(0);}
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void RunAll(Action<string,Action> test)
        {
            test("50 cartas, distribución 13/6/18/13 y cuatro mazos preservados",()=>{
                var g=New();foreach(string leader in CardCatalog.Leaders){Check(g.Catalog.Deck(leader).Sum(c=>g.Catalog.Copies(leader,c))==50,leader);Check(g.Catalog.Deck(leader).All(c=>g.Catalog.Copies(leader,c)<=3),"Más de tres.");}
                var deck=g.Catalog.Deck("XIL'THAR").ToArray();Func<Func<CardData,bool>,int> total=f=>deck.Where(f).Sum(c=>g.Catalog.Copies("XIL'THAR",c));
                Check(total(c=>c.category==Category.Spell&&!c.combatSpell)==13&&total(c=>c.combatSpell)==6&&total(c=>c.IsStructure)==18&&total(c=>c.category==Category.Unit)==13,"Distribución.");Valid(g);
            });
            test("costes y orden MOV HP ATK exactos",()=>{
                var g=New();string[] ids={"crystal-minion","dark-fly","the-eternal-eye","void-hole","sprinting-sprite"};int[,] stats={{3,1,3,3},{3,3,2,2},{9,0,10,1},{5,1,3,0},{2,4,1,1}};
                for(int i=0;i<ids.Length;i++){var c=g.Catalog[ids[i]];Check(c.energyCost==stats[i,0]&&c.movement==stats[i,1]&&c.health==stats[i,2]&&c.attack==stats[i,3],ids[i]);}
                foreach(string id in new[]{"void-beacon","portal","insolator"})Check(g.Catalog[id].energyCost==4&&g.Catalog[id].health==2,id);
            });
            test("4, 8 y 12 terraformaciones; fallos, mismo bioma y control no cuentan",()=>{
                var g=New();int energy=g.State.Active.currentEnergy;
                var tiles=g.State.tiles.Where(t=>t.baseOwner<0&&!t.blocked).Take(12).ToArray();
                for(int i=0;i<12;i++){TerrainManager.Terraform(g,tiles[i].id,Biome.Void,0);Check(g.State.Active.currentEnergy==energy+(i+1)/4,"Umbral de energía.");}
                TerrainManager.Terraform(g,tiles[0].id,Biome.Void,0);tiles[1].owner=1;
                var occupied=Put(g,"old-mummy",1,0,0);TerrainManager.Terraform(g,occupied.tileId,Biome.Void,0);
                Check(g.State.Active.xilTerraformCount==12&&g.State.Active.xilEnergyGenerated==3,"Cuenta repetición, ocupación o cambio de control.");
                End(g);TerrainManager.Terraform(g,tiles[0].id,Biome.Forest,0);Check(g.State.players[0].xilTerraformCount==12,"Cuenta fuera de turno.");
                End(g);End(g);End(g);Check(g.State.Active.xilTerraformCount==0&&g.State.Active.xilEnergyGenerated==0,"No reinicia.");
            });
            test("Terraform y Clearing Space usan el contador y conservan cartas",()=>{
                var g=New();foreach(string id in new[]{"terraform","clearing-space"}){var c=Hand(g,id);int dest=g.CardTargets(g.Catalog[id]).First(t=>g.State.tiles[t].biome==Biome.Neutral);Check(g.Play(c.instanceId,new[]{dest},false,Biome.Void),g.LastMessage);}
                Check(g.State.Active.xilTerraformCount==2,"No contó magias.");Valid(g);
            });
            test("Void se guarda, restaura y deshace junto a energía y contador",()=>{
                var g=New();g.State.stage=TurnStage.Terraforming;var before=GamePersistence.Serialize(g.State);int tile=g.State.tiles.First(t=>g.TerraformTarget(t.id)&&t.biome!=Biome.Void).id;
                Check(g.Terraform(tile,Biome.Void),g.LastMessage);var saved=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(saved.tiles[tile].biome==Biome.Void&&saved.Active.xilTerraformCount==1,"Guardado.");
                Check(g.Undo()&&before==GamePersistence.Serialize(g.State),"Deshacer.");Valid(g);
            });
            test("Crystal Minion salta Void a Void a 3rad consumiendo MOV",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var p=Put(g,"crystal-minion",0,0,0);int dest=At(g,3,0).id;At(g,3,0).biome=Biome.Void;At(g,4,0).biome=Biome.Void;
                Check(MovementManager.Paths(g,p).ContainsKey(dest)&&!MovementManager.Paths(g,p).ContainsKey(At(g,4,0).id),"RAD de salto.");
                int movement = p.remainingMovement;
                Check(MovementManager.Move(g,p,dest)&&p.remainingMovement==movement-1,"El salto no consume 1 MOV.");
                while(p.remainingMovement>0)
                {
                    int next = MovementManager.Paths(g,p).Keys.First();
                    int before = p.remainingMovement;
                    Check(MovementManager.Move(g,p,next)&&p.remainingMovement<before,"Salto sin consumo.");
                }
                Check(MovementManager.Paths(g,p).Count==0,"Salto infinito.");Valid(g);
            });
            test("Dark Fly cura al moverse y segundo ataque reemplaza todo movimiento",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var fly=Put(g,"dark-fly",0,0,0);var enemy=Put(g,"old-mummy",1,1,0);fly.health=1;
                foreach(var p in g.State.players){p.deck.AddRange(p.hand);p.hand.Clear();}
                Check(CombatManager.Attack(g,fly,enemy.tileId)&&fly.attacked,"Primer ataque.");Check(CombatManager.Attack(g,fly,enemy.tileId)&&fly.darkFlyExtraAttack&&fly.remainingMovement==0,"Segundo ataque.");
                Check(!CombatManager.Attack(g,fly,enemy.tileId)&&MovementManager.Paths(g,fly).Count==0,"Tercer ataque o movimiento.");
                var other=Put(g,"dark-fly",0,-2,0);other.health=2;At(g,-1,0).biome=Biome.Void;
                Check(MovementManager.Move(g,other,At(g,-1,0).id)&&other.health==g.MaxHealth(other),"No cura movimiento.");Valid(g);
            });
            test("Eye y Hole no se curan; Eye en Void es inmune a objetivos",()=>{
                var g=New();var eye=Put(g,"the-eternal-eye",0,0,0);var hole=Put(g,"void-hole",0,1,0);eye.health=2;hole.health=1;GenericCardRules.Heal(g,eye,99);GenericCardRules.Heal(g,hole,99);Check(eye.health==2&&hole.health==1,"Curación prohibida.");
                g.State.activePlayer=1;g.State.stage=TurnStage.Assault;Hand(g,"engage");Put(g,"old-mummy",1,2,0);
                Check(!g.CardTargets(g.Catalog["engage"],new[]{At(g,2,0).id}).Contains(eye.tileId),"Engage selecciona Eye.");
                var tower=Put(g,"vigilant-tower",1,3,0);Check(!AbilityManager.Targets(g,tower).Contains(eye.tileId),"Tower selecciona Eye.");
                Check(g.TerraformTarget(eye.tileId),"No permite excepción global.");g.State.stage=TurnStage.Terraforming;Check(g.Terraform(eye.tileId,Biome.Desert)&&XiltharRules.CanTarget(g,eye),"No anula Void.");
            });
            test("Engage y Cleansing Conquest solo ofrecen cadenas de objetivos que se pueden completar", () =>
            {
                var g = New();
                var source = Put(g,"crystal-minion",0,0,0);
                var eyeTile = At(g,1,0);
                eyeTile.biome = Biome.Void;
                g.Place("the-eternal-eye",1,eyeTile.id);
                g.State.stage = TurnStage.Assault;
                var engage = g.Catalog["engage"];
                Check(!g.CardTargets(engage).Contains(source.tileId), "Engage ofrece un aliado si solamente hay enemigos inmunes.");
                var enemy = Put(g,"old-mummy",1,2,0);
                Check(g.CardTargets(engage).Contains(source.tileId) &&
                    g.CardTargets(engage,new[] {source.tileId}).SequenceEqual(new[] {enemy.tileId}), "Engage pierde el otro objetivo legal.");
                CombatManager.Remove(g,source);
                var actorEye = Put(g,"the-eternal-eye",0,0,0);
                var slab = At(g,-1,0);
                slab.owner=0;
                g.State.stage = TurnStage.Deployment;
                var conquest = g.Catalog["cleansing-conquest"];
                Check(!g.CardTargets(conquest).Contains(slab.id), "Conquest ofrece un destino que solo puede alcanzar Eye inmune.");
                TerrainManager.Terraform(g,actorEye.tileId,Biome.Forest,0);
                Check(g.CardTargets(conquest).Contains(slab.id), "Conquest pierde el destino tras quitar la inmunidad.");
            });
            test("Erosion alcanza la estructura enemiga aunque en el mismo hex haya una unidad inmune", () =>
            {
                var g = New();
                var tile = At(g,1,0);
                tile.biome = Biome.Void;
                tile.owner = 1;
                var structure = g.Place("arena-profunda",1,tile.id);
                var eye = g.Place("the-eternal-eye",1,tile.id);
                Put(g,"crystal-minion",0,0,0);
                g.State.stage = TurnStage.Assault;
                var card = new CardInstance { instanceId = g.State.nextId++, cardId = "erosion" };
                g.State.Active.hand.Add(card);
                int structureHealth = structure.health, eyeHealth = eye.health;
                Check(g.Play(card.instanceId,new[] {tile.id}) && structure.health == structureHealth-2 && eye.health == eyeHealth,
                    "Erosion no coincide entre selección y daño: " + g.LastMessage);
            });
            test("Med Camp y Resting Camp evitan objetivos que no pueden curarse", () =>
            {
                var g = New();
                var med = g.Place("med-camp",0,At(g,0,0).id);
                var rest = g.Place("resting-camp",0,At(g,0,1).id);
                var hole = Put(g,"void-hole",0,1,0);
                hole.health--;
                g.State.stage = TurnStage.Assault;
                Check(!AbilityManager.CanActivate(g,med) && !AbilityManager.CanActivate(g,rest), "Ofrece curar a Void Hole.");
                var ally = Put(g,"crystal-minion",0,2,0);
                ally.health--;
                Check(AbilityManager.Targets(g,med).SequenceEqual(new[] {ally.tileId}) &&
                    AbilityManager.Activate(g,med,new[] {ally.tileId}) && ally.health == g.MaxHealth(ally), "Med Camp no cura al aliado válido.");
            });
            test("Eye paga 1 en cada salto sin límite por turno y aplica impuesto real",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var eye=Put(g,"the-eternal-eye",0,0,0);At(g,8,0).biome=Biome.Void;int start=eye.tileId,far=At(g,8,0).id,energy=g.State.Active.currentEnergy;
                Check(AbilityManager.Activate(g,eye,new[]{far})&&AbilityManager.Activate(g,eye,new[]{start}),"No repite salto.");Check(g.State.Active.currentEnergy==energy-2,"Salto gratuito.");
                Check(EnergyManager.Quote(g.State.players[0],g.Catalog["terraform"]).energy==1&&EnergyManager.Quote(g.State.players[1],g.Catalog["terraform"]).energy==2,"Impuesto visible.");
                g.State.activePlayer=1;g.State.stage=TurnStage.Deployment;var card=Hand(g,"terraform");int before=g.State.Active.currentEnergy;
                Check(g.Play(card.instanceId,new[]{eye.tileId},false,Biome.Forest)&&g.State.Active.currentEnergy==before-2,"Impuesto real.");Check(g.State.players[1].opponentCardTax==0,"Impuesto persiste fuera de Void.");Valid(g);
            });
            test("Portal: coste 2, Unit en Void a 6rad, destino adyacente y un uso",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var portal=Put(g,"portal",0,0,0);var p=Put(g,"crystal-minion",0,6,0);var far=Put(g,"crystal-minion",0,7,0);int e=g.State.Active.currentEnergy;
                Check(AbilityManager.Targets(g,portal).Contains(p.tileId)&&!AbilityManager.Targets(g,portal).Contains(far.tileId),"RAD Portal.");
                Check(AbilityManager.Activate(g,portal,new[]{p.tileId}),g.LastMessage);var choices=ChoiceManager.Targets(g,g.State.pendingChoices[0]);Check(choices.All(id=>g.State.tiles[portal.tileId].neighbors.Contains(id)),"Destino no adyacente.");Choose(g,choices.First());
                Check(g.State.Active.currentEnergy==e-2&&!AbilityManager.CanActivate(g,portal),"Coste o repetición.");Valid(g);
            });
            test("Insolator: 2 daño a 3rad, una terraformación por muerte, fuera no afecta",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var source=Put(g,"insolator",0,0,0);var near=Put(g,"dark-fly",0,3,0);var far=Put(g,"dark-fly",0,4,0);
                near.health=2;int farHealth=far.health;
                Check(AbilityManager.Activate(g,source,Array.Empty<int>()),g.LastMessage);Check(near.health==0&&far.health==farHealth&&g.State.pendingChoices.Single().kind=="VoidTerraform","Daño, RAD o disparador.");
                int count=g.State.Active.xilTerraformCount;Choose(g,ChoiceManager.Targets(g,g.State.pendingChoices[0]).First());Check(g.State.Active.xilTerraformCount==count+1,"No cuenta terraformación.");Valid(g);
            });
            test("Insolator sin muertes no terraforma",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var source=Put(g,"insolator",0,0,0);var p=Put(g,"crystal-minion",0,1,0);
                int before=p.health;
                Check(AbilityManager.Activate(g,source,Array.Empty<int>())&&p.health==before-2&&g.State.pendingChoices.Count==0,"Disparó sin matar.");
            });
            test("Void Hole prevención opcional a 5rad, coste 1 HP y una vez por turno",()=>{
                var g=New();var hole=Put(g,"void-hole",0,0,0);var ally=Put(g,"crystal-minion",0,5,0);int allyHealth=ally.health,holeHealth=hole.health;CombatManager.DirectDamage(g,ally,3);
                Check(ally.health==allyHealth&&g.State.pendingChoices.Single().kind=="VoidPrevent","Daño antes de elección.");Choose(g,hole.tileId);
                Check(hole.health==holeHealth-1&&ally.health==allyHealth,"Prevención.");CombatManager.DirectDamage(g,ally,1);Check(ally.health==allyHealth-1&&g.State.pendingChoices.Count==0,"Reutiliza prevención.");
                End(g);CombatManager.DirectDamage(g,ally,1);Check(g.State.pendingChoices.Count==1,"No reinicia por turno.");Check(ChoiceManager.Resolve(g,Array.Empty<int>(),true)&&ally.health==allyHealth-2,"Omitir no aplica daño.");Valid(g);
            });
            test("Void Hole fuera de 5rad no previene; guarda y deshace hit pendiente",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var hole=Put(g,"void-hole",0,0,0);var far=Put(g,"crystal-minion",0,6,0);int farHealth=far.health;CombatManager.DirectDamage(g,far,1);Check(far.health==farHealth-1&&g.State.pendingChoices.Count==0,"RAD confundido.");
                var ally=Put(g,"crystal-minion",0,1,0);var tower=Put(g,"vigilant-tower",0,2,0);ConquestManager.Refresh(g.State);string before=GamePersistence.Serialize(g.State);
                Check(AbilityManager.Activate(g,tower,new[]{ally.tileId}),g.LastMessage);var saved=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(saved.pendingChoices[0].victimId==ally.id,"No conserva hit.");
                Choose(g,hole.tileId);Check(g.Undo()&&GamePersistence.Serialize(g.State)==before,"No restaura prevención atómica.");Valid(g);
            });
            test("Void Beacon solo Units reales muertas en Void",()=>{
                var g=New();Put(g,"void-beacon",0,0,0);var normal=Put(g,"crystal-minion",0,1,0,Biome.Forest);CombatManager.Remove(g,normal);Check(g.State.pendingChoices.Count==0,"Muerte fuera de Void.");
                var token=g.Place("rabbit-token",0,At(g,2,0).id);At(g,2,0).biome=Biome.Void;CombatManager.Remove(g,token);Check(g.State.pendingChoices.Count==0,"Token dispara Beacon.");
                var unit=Put(g,"crystal-minion",0,3,0);CombatManager.Remove(g,unit);Check(g.State.pendingChoices.Single().kind=="BeaconPick","No ofrece Beacon.");Valid(g);
            });
            test("Void Beacon recast coste original +2 y carta única",()=>{
                var g=New();var beacon=Put(g,"void-beacon",0,0,0);var unit=Put(g,"crystal-minion",0,1,0);int e=g.State.Active.currentEnergy;CombatManager.Remove(g,unit);Choose(g,beacon.tileId);Choose(g,1);
                var revived=g.State.tiles[beacon.tileId].unit;Check(revived.id==unit.id&&!revived.beaconDebt&&g.State.Active.currentEnergy==e-5&&g.State.tiles[beacon.tileId].structure==null,"Recast/pago.");Valid(g);
            });
            test("Void Beacon muerte diferida espera al fin de turno; pago evita muerte",()=>{
                foreach(bool pay in new[]{false,true}){var g=New();var beacon=Put(g,"void-beacon",0,0,0);var unit=Put(g,"crystal-minion",0,1,0);CombatManager.Remove(g,unit);Choose(g,beacon.tileId);Choose(g,0);
                    Check(g.State.tiles[beacon.tileId].unit?.id==unit.id,"Muerte inmediata.");End(g);Check(g.State.endingTurn&&g.State.pendingChoices[0].kind=="BeaconDebt"&&!g.CanUndo,"No espera pago final.");Choose(g,pay?1:0);
                    Check(g.State.activePlayer==1&&!g.State.endingTurn&&(g.State.tiles[beacon.tileId].unit!=null)==pay,"Liquidación del fin de turno.");Valid(g);}
            });
            test("Under the Darkness usa descarte real; persiste en Void y mata al salir",()=>{
                var g=New();var unit=Discard(g,"crystal-minion");var spell=Hand(g,"under-the-darkness");int tile=g.State.tiles.First(t=>t.owner==0&&!t.IsOccupied&&t.baseOwner<0).id;g.State.tiles[tile].biome=Biome.Void;
                Check(g.Play(spell.instanceId,Array.Empty<int>()),g.LastMessage);Choose(g,unit.instanceId);Choose(g,tile);Check(g.State.tiles[tile].unit.id==unit.instanceId&&g.State.tiles[tile].unit.underDarkness,"No usa carta real.");
                End(g);Check(g.State.tiles[tile].unit!=null,"Mata en Void.");g.State.tiles[tile].biome=Biome.Forest;End(g);Check(g.State.tiles[tile].unit==null&&g.State.players[0].discardPile.Count(c=>c.instanceId==unit.instanceId)==1,"No mata fuera de Void o duplica.");Valid(g);
            });
            test("Under the Darkness pendiente se guarda y todo se deshace como una decisión",()=>{
                var g=New();var unit=Discard(g,"dark-fly");var spell=Hand(g,"under-the-darkness");string before=GamePersistence.Serialize(g.State);
                Check(g.Play(spell.instanceId,Array.Empty<int>()),g.LastMessage);Choose(g,unit.instanceId);Check(StateValidator.Validate(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)),g.Catalog)=="","Guardado de elección.");
                Choose(g,ChoiceManager.Targets(g,g.State.pendingChoices[0]).First());Check(g.Undo()&&GamePersistence.Serialize(g.State)==before,"Deshacer parcial.");Valid(g);
            });
            test("Sprinting Sprite: coste 1, muerte real y otra Unit en su Slab",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var sprite=Put(g,"sprinting-sprite",0,0,0);var other=Put(g,"crystal-minion",0,5,0);int e=g.State.Active.currentEnergy,tile=sprite.tileId;
                Check(AbilityManager.Activate(g,sprite,new[]{other.tileId}),g.LastMessage);Check(sprite.health==0&&other.tileId==tile&&g.State.Active.currentEnergy==e-1&&g.State.Active.discardPile.Any(c=>c.instanceId==sprite.id),"Coste, movimiento o muerte.");
                Check(g.State.pendingChoices.All(c=>c.kind!="DeathTerraform"),"Sprite es tratado como Token.");Valid(g);
            });
            test("Void Hole solo protege a otra unidad aliada, incluso si es otro Void Hole", () =>
            {
                var g = New();
                var victim = Put(g,"void-hole",0,0,0);
                int health = victim.health;
                CombatManager.DirectDamage(g,victim,1);
                Check(victim.health == health-1 && g.State.pendingChoices.Count == 0, "Void Hole se protege a sí mismo.");
                var protector = Put(g,"void-hole",0,1,0);
                int protectorHealth = protector.health;
                CombatManager.DirectDamage(g,victim,2);
                var choice = g.State.pendingChoices.Single();
                Check(ChoiceManager.Targets(g,choice).SequenceEqual(new[] {protector.tileId}), "Se puede seleccionar a la víctima.");
                Choose(g,protector.tileId);
                Check(victim.health == health-1 && protector.health == protectorHealth-1 &&
                    g.State.pendingChoices.Count == 0, "El pago no es exactamente 1 HP o provocó recursión.");
                Valid(g);
            });
            test("Insolator espera prevención antes de conceder terraformación",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var source=Put(g,"insolator",0,0,0);var victim=Put(g,"dark-fly",0,1,0);victim.health=2;var hole=Put(g,"void-hole",0,5,0);
                Check(AbilityManager.Activate(g,source,Array.Empty<int>()),g.LastMessage);Check(g.State.pendingChoices[0].kind=="VoidPrevent"&&victim.health==2,"Daño antes de prevención.");
                Check(ChoiceManager.Resolve(g,Array.Empty<int>(),true),g.LastMessage);Check(victim.health==0&&g.State.pendingChoices.Single().kind=="VoidTerraform","No atribuye muerte diferida al Insolator.");Valid(g);
            });
            test("Sprite no cobra ni muere si su Slab sería un destino ilegal",()=>{
                var g=New();g.State.stage=TurnStage.Assault;var sprite=Put(g,"sprinting-sprite",0,0,0);var other=Put(g,"crystal-minion",0,5,0);g.State.tiles[sprite.tileId].baseOwner=1;int e=g.State.Active.currentEnergy;
                Check(!AbilityManager.Targets(g,sprite).Contains(other.tileId)&&!AbilityManager.Activate(g,sprite,new[]{other.tileId})&&sprite.health==g.MaxHealth(sprite)&&g.State.Active.currentEnergy==e,"Consume sacrificio inválido.");
            });
            test("IA Xil'thar resuelve descarte, Void y las tres etapas sin atascarse",()=>{
                var g=New();g.State.Active.isAI=true;g.State.Active.deck.AddRange(g.State.Active.hand);g.State.Active.hand.Clear();
                Discard(g,"crystal-minion");Hand(g,"under-the-darkness");Hand(g,"terraform");Put(g,"portal",0,0,0);Put(g,"void-beacon",0,2,0);
                var ai=new AiPlayer();int steps=0;while(g.State.activePlayer==0&&steps++<160){Check(ai.Step(g),"IA detenida: "+g.LastMessage);Valid(g);}
                Check(g.State.activePlayer==1&&steps<160,"La IA no devuelve el turno.");
            });
            test("Mana Pool, Valiant Seal y costes visibles reutilizan los sistemas existentes",()=>{
                var g=New();Put(g,"mana-pool",0,0,0);Put(g,"valiant-seal",0,1,0);var unit=Put(g,"crystal-minion",0,2,0);Check(CombatManager.AttackValue(g,unit)==g.Data(unit).attack+TerrainManager.GetTerrainStatModifier(g,unit)+1,"Valiant Seal.");
                End(g);End(g);End(g);End(g);Check(g.State.Active.currentEnergy==g.State.Active.maxEnergy+1,"Mana Pool.");
                foreach(var c in g.Catalog.Deck("XIL'THAR"))Check(EnergyManager.Quote(g.State.Active,c).energy==c.energyCost,"Coste mostrado: "+c.id);Valid(g);
            });
        }
    }
}
#endif
