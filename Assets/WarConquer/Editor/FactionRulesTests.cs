#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class FactionRulesTests
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static GameManager New(CardCatalog c,int humans=4){var g=new GameManager(c);g.NewGame(new[]{"SAHRIA","ZUKGROK","FAUNAR","ZUKGROK"},654,CardCatalog.LoadRules(),humans);g.State.Active.currentEnergy=40;return g;}
        static HexTile At(GameManager g,int q,int r)=>g.State.tiles.First(t=>t.q==q&&t.r==r);
        static Piece Put(GameManager g,string id,int owner,int q,int r,Biome biome=Biome.Neutral,bool token=false)
        {var t=At(g,q,r);t.biome=biome;t.owner=owner;return g.Place(id,owner,t.id,0,token);}
        static CardInstance Hand(GameManager g,string id){var c=PrototypeScenario.Take(g,g.ActingPlayerId,id);g.ActingPlayer.hand.Add(c);return c;}
        static void Choices(GameManager g)
        {
            for(int i=0;g.State.pendingChoices.Count>0&&i<100;i++)
            {var c=g.State.pendingChoices[0];Check(ChoiceManager.Resolve(g,ChoiceManager.Targets(g,c).Take(c.count).ToArray()),"Elección bloqueada: "+c.kind);}
            Check(g.State.pendingChoices.Count==0,"Bucle de elecciones.");
        }
        static void End(GameManager g)
        {
            Choices(g);while(g.State.battle!=null){BattleManager.Pass(g,g.ActingPlayerId);Choices(g);}
            while(g.State.stage!=TurnStage.Assault)g.AdvanceStage();Check(g.EndTurn(),"Fin de turno bloqueado.");Choices(g);
        }
        static void Round(GameManager g){int id=g.State.activePlayer;do{End(g);}while(g.State.activePlayer!=id);}
        public static void RunBatch()
        {
            try{GrayboxTests.RunAll();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void RunStandalone()
        {
            try{int count=0;RunAll(CardCatalog.Load(),(name,body)=>{body();count++;Debug.Log("PASS "+name);});FaunarRulesTests.RunAll(CardCatalog.Load(),(name,body)=>{body();count++;Debug.Log("PASS "+name);});Debug.Log("FACTION_RULES_PASSED "+count);EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void RunAll(CardCatalog catalog,Action<string,Action> test)
        {
            test("SAHRIA y ZUKGROK: distribución exacta de 50 y Tokens excluidos",()=>{
                foreach(var leader in CardCatalog.Leaders){var deck=catalog.Deck(leader);Check(deck.Sum(c=>catalog.Copies(leader,c))==50&&deck.All(c=>catalog.Copies(leader,c)<=3&&!c.Has("Token")),leader);}
                var sah=catalog.Deck("SAHRIA").ToArray();var zuk=catalog.Deck("ZUKGROK").ToArray();
                Func<CardData[],string,Func<CardData,bool>,int> sum=(d,l,p)=>d.Where(p).Sum(c=>catalog.Copies(l,c));
                Check(sum(sah,"SAHRIA",c=>c.IsStructure)==13&&sum(sah,"SAHRIA",c=>c.category==Category.Unit)==13&&sum(sah,"SAHRIA",c=>c.combatSpell)==8&&sum(sah,"SAHRIA",c=>c.category==Category.Spell&&!c.combatSpell)==16,"Distribución Sahria.");
                Check(sum(zuk,"ZUKGROK",c=>c.IsStructure)==18&&sum(zuk,"ZUKGROK",c=>c.category==Category.Unit)==5&&sum(zuk,"ZUKGROK",c=>c.combatSpell)==8&&sum(zuk,"ZUKGROK",c=>c.category==Category.Spell&&!c.combatSpell)==19,"Distribución Zukgrok.");
                Check(catalog.Copies("ZUKGROK",catalog["cleansing-conquest"])==0&&catalog.Copies("ZUKGROK",catalog["alligator-revengeful-bite"])==3,"Revisión final de Zukgrok.");
                foreach(int n in new[]{1,2,3,4}){var g=New(catalog,n);Check(StateValidator.Validate(g.State,catalog)=="","Mazo inválido para "+n);}
            });
            test("MOV HP ATK, costes y VIDA de Structures confirmados por el usuario",()=>{
                string[] ids={"sandy-ember","dune-cammel","old-mummy","wandering-beast","baby-phoenix","lushroom","alligator-revengeful-bite","battle-ant-token","frog-token","mushroom-token","firefly-token","alligator-token"};
                int[,] stats={{3,2,2},{4,2,1},{0,5,4},{2,5,5},{2,1,7},{0,5,1},{1,4,1},{1,1,2},{2,2,2},{1,2,2},{4,1,1},{1,1,1}};
                for(int i=0;i<ids.Length;i++)Check(catalog[ids[i]].movement==stats[i,0]&&catalog[ids[i]].health==stats[i,1]&&catalog[ids[i]].attack==stats[i,2],ids[i]);
                Check(catalog["sandy-anthill"].health==3&&catalog["frogpit"].health==4&&catalog["micelium-root"].health==3&&catalog["firefly-nest"].health==3,"HP de Structures.");
            });
            test("SAHRIA: pago opcional real +1, procedencia, entrada y ningún cobro inválido",()=>{
                var g=New(catalog);var t=At(g,0,0);t.setBy=0;t.owner=1;t.biome=Biome.Forest;var c=Hand(g,"sandy-ember");
                Check(!g.CardTargets(catalog[c.cardId]).Contains(t.id),"Despliegue especial gratuito.");g.State.Active.sahriaDeployment=true;
                Check(EnergyManager.Quote(g.State.Active,catalog[c.cardId]).energy==3,"Cotización sin recargo.");
                g.State.Active.currentEnergy=2;Check(!g.Play(c.instanceId,new[]{t.id})&&g.State.Active.currentEnergy==2,"Pagó sin energía.");
                g.State.Active.currentEnergy=3;Check(g.Play(c.instanceId,new[]{t.id})&&g.State.Active.currentEnergy==0&&g.State.pendingChoices.Single().kind=="Discard","Recargo o entrada ausente.");Choices(g);
                Check(!FactionCardRules.CanDeploy(g,catalog["dune-cammel"],At(g,2,0)),"Sin procedencia.");
                Check(EnergyManager.Quote(g.State.Active,catalog["sandy-anthill"]).energy==2,"Recargo a Structure.");
                Check(StateValidator.Validate(g.State,catalog)=="","No conserva 50 cartas.");
            });
            test("Terraformación registra quién establece el Slab; movimiento no falsifica procedencia",()=>{
                var g=New(catalog);var t=At(g,0,0);TerrainManager.Terraform(g,t.id,Biome.Desert,0);Check(t.setBy==0,"No registra procedencia.");
                var p=Put(g,"frog-token",1,1,0,token:true);MovementManager.Relocate(g,p,t.id);Check(t.owner==1&&t.setBy==0,"Movimiento altera autor de bioma.");
                TerrainManager.DestroyBiome(g,t);Check(t.setBy==-1,"Destrucción conserva territorio establecido.");
            });
            test("Wandering Beast: solo Yermo incluso con SAHRIA o neutral permitido",()=>{
                var g=New(catalog);var t=At(g,0,0);t.owner=t.setBy=0;g.State.Active.sahriaDeployment=true;
                Check(!FactionCardRules.CanDeploy(g,catalog["wandering-beast"],t),"Beast neutral.");t.biome=Biome.Desert;Check(!FactionCardRules.CanDeploy(g,catalog["wandering-beast"],t),"Beast Desierto.");
                t.biome=Biome.Wasteland;Check(FactionCardRules.CanDeploy(g,catalog["wandering-beast"],t),"Beast Yermo bloqueada.");
            });
            test("ZUKGROK: muerte Token por todas las causas, coste cero y no por Unit",()=>{
                foreach(int cause in new[]{0,1,2,3})
                {
                    var g=New(catalog);var p=Put(g,"frog-token",1,0,0,token:true);int energy=g.State.players[1].currentEnergy;
                    if(cause==0)CombatManager.Damage(g,p,99,true);else if(cause==1)CombatManager.PoisonDamage(g,p,99);else if(cause==2)CombatManager.DirectDamage(g,p,99);else CombatManager.Remove(g,p);
                    Check(g.State.pendingChoices.Single().owner==1&&g.State.pendingChoices[0].tileId==p.tileId,"No detecta muerte Token.");
                    Check(ChoiceManager.Resolve(g,new[]{(int)Biome.Swamp})&&g.State.tiles[p.tileId].biome==Biome.Swamp&&g.State.players[1].currentEnergy==energy,"Terraformación no gratuita.");
                    CombatManager.Remove(g,p);Check(g.State.pendingChoices.Count==0,"Muerte duplicada.");
                }
                var other=New(catalog);CombatManager.Remove(other,Put(other,"lushroom",1,0,0));Check(other.State.pendingChoices.Count==0,"Activa con Unit.");
                CombatManager.Remove(other,Put(other,"frog-token",0,0,0,token:true));Check(other.State.pendingChoices.Count==0,"Activa con Token enemigo.");
            });
            test("Structures invocan sus Tokens adyacentes y Firefly Nest crea dos",()=>{
                string[] ids={"sandy-anthill","frogpit","micelium-root","firefly-nest"};string[] tokens={"battle-ant-token","frog-token","mushroom-token","firefly-token"};
                for(int i=0;i<ids.Length;i++)
                {var g=New(catalog);var p=Put(g,ids[i],i==0?0:1,0,0);EffectManager.OnEnter(g,p);Choices(g);var summoned=g.Allies(p.owner).Where(x=>x.token).ToArray();Check(summoned.Length==(i==3?2:1)&&summoned.All(x=>x.cardId==tokens[i]&&g.State.tiles[p.tileId].neighbors.Contains(x.tileId)),ids[i]);}
            });
            test("Invocación respeta ocupación y no sustituye piezas ni crea Tokens fantasma",()=>{
                var g=New(catalog);var p=Put(g,"firefly-nest",1,0,0);foreach(int id in g.State.tiles[p.tileId].neighbors)g.State.tiles[id].blocked=true;
                EffectManager.OnEnter(g,p);Check(!g.Allies(1).Any(x=>x.token)&&g.State.pendingChoices.Count==0,"Invocación inválida.");
            });
            test("Battle Ant: otra Ant a 3rad concede solo +1 ATK y se retira",()=>{
                var g=New(catalog);var p=Put(g,"battle-ant-token",0,0,0,token:true);Check(CombatManager.AttackValue(g,p)==2,"Bonus propio.");
                var a=Put(g,"sandy-anthill",1,3,0);Put(g,"battle-ant-token",1,1,0,token:true);Check(CombatManager.AttackValue(g,p)==3,"Bonus acumulado o ignora Ant enemiga.");
                CombatManager.Remove(g,a);CombatManager.Remove(g,At(g,1,0).unit);Choices(g);Check(CombatManager.AttackValue(g,p)==2,"Bonus permanente.");
            });
            test("Sandy Ember permite Structure amiga o enemiga en cualquier Desierto",()=>{
                var g=New(catalog);var friendly=Put(g,"sandy-anthill",0,5,0,Biome.Desert);var enemy=Put(g,"med-camp",1,0,0,Biome.Desert);var p=Put(g,"sandy-ember",0,1,0);
                EffectManager.OnEnter(g,p);var c=g.State.pendingChoices.Single();Check(ChoiceManager.Targets(g,c).Contains(friendly.tileId)&&ChoiceManager.Targets(g,c).Contains(enemy.tileId),"Filtro inventado.");
                Check(ChoiceManager.Resolve(g,new[]{enemy.tileId})&&enemy.health==4,"Daño incorrecto.");
            });
            test("Dune Cammel desplaza en la misma dirección y no atraviesa obstáculos",()=>{
                var g=New(catalog);var p=Put(g,"dune-cammel",0,0,0);var ally=Put(g,"old-mummy",0,0,1);var blocked=Put(g,"frog-token",1,-1,1,token:true);At(g,0,1).unit=ally;
                MovementManager.Relocate(g,p,At(g,1,0).id);var c=g.State.pendingChoices.Single();Check(ChoiceManager.Targets(g,c).Contains(ally.tileId)&&!ChoiceManager.Targets(g,c).Contains(blocked.tileId),"Selección de acompañante.");
                Check(ChoiceManager.Resolve(g,new[]{ally.tileId})&&ally.tileId==At(g,1,1).id,"Dirección incorrecta.");
            });
            test("Dune Cammel no encadena acompañantes y retira HP ATK al próximo turno",()=>{
                var g=New(catalog);var p=Put(g,"dune-cammel",0,0,0,Biome.Desert);var ally=Put(g,"dune-cammel",0,0,1);int hp=ally.health,atk=CombatManager.AttackValue(g,ally);
                MovementManager.Relocate(g,p,At(g,1,0).id);Check(ally.health==hp+1&&CombatManager.AttackValue(g,ally)==atk+1,"No aplica mejora.");
                ChoiceManager.Resolve(g,new[]{ally.tileId});Check(g.State.pendingChoices.Count==0,"Cadena infinita de Cammels.");Round(g);
                Check(ally.health==hp&&CombatManager.AttackValue(g,ally)==atk,"Mejora permanente.");
            });
            test("Old Mummy cura y recibe exactamente +1 MOV cada turno en Desierto",()=>{
                var g=New(catalog);var p=Put(g,"old-mummy",0,0,0,Biome.Desert);p.health=1;Round(g);Check(p.health==5&&p.remainingMovement==1,"Mummy inicio.");Round(g);Check(p.remainingMovement==1,"Acumula MOV.");
                g.State.tiles[p.tileId].biome=Biome.Wasteland;Round(g);Check(p.remainingMovement==0,"MOV fuera de Desierto.");
            });
            test("Wandering Beast: habilidad una vez por turno, 3rad, dos daño y robo cuatro",()=>{
                var g=New(catalog);var p=Put(g,"wandering-beast",0,0,0);var target=Put(g,"med-camp",1,3,0);Put(g,"med-camp",1,4,0);g.State.stage=TurnStage.Assault;
                Check(AbilityManager.Targets(g,p).Contains(target.tileId)&&!AbilityManager.Targets(g,p).Contains(At(g,4,0).id),"Radio de Beast.");Check(AbilityManager.Activate(g,p,new[]{target.tileId})&&target.health==3&&!AbilityManager.Activate(g,p,new[]{target.tileId}),"Daño/uso de Beast.");
                int count=g.State.Active.hand.Count;CombatManager.Remove(g,p);Check(g.State.Active.hand.Count==count+4,"No roba cuatro.");
            });
            test("Phoenix: atacar al jugar solo si Yermo y la excepción no se gana terraformando después",()=>{
                var g=New(catalog);var p=Put(g,"baby-phoenix",0,0,0,Biome.Desert);Put(g,"old-mummy",1,1,0);g.State.stage=TurnStage.Assault;
                Check(CombatManager.Targets(g,p).Count==0,"Phoenix ataca al entrar.");g.State.tiles[p.tileId].biome=Biome.Wasteland;Check(!FactionCardRules.CanAttack(g,p),"Excepción retroactiva.");
                var other=Put(g,"baby-phoenix",0,1,-1,Biome.Wasteland);Check(CombatManager.Targets(g,other).Count>0,"Phoenix Yermo no ataca.");
                Round(g);Check(FactionCardRules.CanAttack(g,p),"Phoenix sigue bloqueado.");
            });
            test("Phoenix: terraforma, retorna la carta original, sin descarte ni duplicación; guardado pendiente",()=>{
                var g=New(catalog);var tile=At(g,0,0);var p=PrototypeScenario.Spawn(g,0,"baby-phoenix",tile.id);int id=p.id;CombatManager.Remove(g,p);
                Check(!g.State.Active.discardPile.Any(c=>c.instanceId==id)&&!g.State.Active.hand.Any(c=>c.instanceId==id),"Retorno antes de terraformar.");
                Check(StateValidator.Validate(g.State,catalog)=="","Carta pendiente no contabilizada.");var restored=GamePersistence.Deserialize(GamePersistence.Serialize(g.State));Check(StateValidator.Validate(restored,catalog)=="","Guardado pendiente inválido.");g.Restore(restored);
                ChoiceManager.Resolve(g,new[]{(int)Biome.Desert});Check(g.State.tiles[tile.id].biome==Biome.Desert&&g.State.Active.hand.Count(c=>c.instanceId==id)==1&&StateValidator.Validate(g.State,catalog)=="","Phoenix duplicado o perdido.");
            });
            test("Frog y Alligator Token: MOV en Pantano se aplica y retira sin acumulación",()=>{
                foreach(string id in new[]{"frog-token","alligator-token"})
                {var g=New(catalog);var p=Put(g,id,0,0,0,Biome.Swamp,true);Check(p.remainingMovement==4,"MOV Pantano.");GenericCardRules.SyncAuras(g);Check(p.remainingMovement==4,"MOV acumulado.");TerrainManager.Terraform(g,p.tileId,Biome.Desert,0);Check(p.remainingMovement==catalog[id].movement,"MOV no se retira.");}
            });
            test("Frog envenena al atacar sin exigir Pantano",()=>{
                var g=New(catalog);foreach(var player in g.State.players){player.deck.AddRange(player.hand);player.hand.Clear();}
                var p=Put(g,"frog-token",0,0,0,token:true);var enemy=Put(g,"old-mummy",1,1,0);g.State.stage=TurnStage.Assault;
                Check(CombatManager.Attack(g,p,enemy.tileId)&&enemy.poison==1&&enemy.health==3,"Frog no aplica Poison.");
            });
            test("Salir y volver a Pantano no regenera MOV de aura ya gastado",()=>{
                var g=New(catalog);var p=Put(g,"frog-token",0,0,0,Biome.Swamp,true);p.remainingMovement=0;
                TerrainManager.Terraform(g,p.tileId,Biome.Forest,0);TerrainManager.Terraform(g,p.tileId,Biome.Swamp,0);
                Check(p.remainingMovement==0,"Regenera el bonus de MOV gastado.");Round(g);Check(p.remainingMovement==4,"No recupera MOV en el siguiente turno.");
            });
            test("Mushroom radio 2 + otros Tokens propios, excluye propio y enemigo",()=>{
                var g=New(catalog);var p=Put(g,"mushroom-token",0,0,0,token:true);Put(g,"mushroom-token",1,-1,0,token:true);var enemy=Put(g,"old-mummy",1,3,0);g.State.stage=TurnStage.Assault;
                Check(FactionCardRules.MushroomRange(g,p)==2&&!AbilityManager.Targets(g,p).Contains(enemy.tileId),"Cuenta enemigo o propio.");
                var other=Put(g,"mushroom-token",0,1,0,token:true);Check(FactionCardRules.MushroomRange(g,p)==3&&AbilityManager.Activate(g,p,new[]{enemy.tileId})&&enemy.poison==1,"No amplifica o aplica Poison.");
                Check(!AbilityManager.Activate(g,p,new[]{enemy.tileId}),"Doble habilidad.");CombatManager.Remove(g,other);Check(FactionCardRules.MushroomRange(g,p)==2,"Radio permanente.");
            });
            test("Firefly: sacrificio, terraformación gratuita, después Poison; objetivo inválido conserva Token",()=>{
                var g=New(catalog);g.State.activePlayer=1;g.State.stage=TurnStage.Assault;var p=Put(g,"firefly-token",1,0,0,token:true);var target=Put(g,"old-mummy",0,2,0);var far=Put(g,"old-mummy",0,3,0);
                Check(!AbilityManager.Activate(g,p,new[]{far.tileId})&&p.health==1,"Sacrificio con objetivo inválido.");int energy=g.State.Active.currentEnergy;
                Check(AbilityManager.Activate(g,p,new[]{target.tileId})&&p.health==0&&target.poison==0&&g.State.pendingChoices.Single().kind=="DeathTerraform","Orden de efectos incorrecto.");
                ChoiceManager.Resolve(g,new[]{(int)Biome.Swamp});Check(target.poison==1&&g.State.Active.currentEnergy==energy,"Poison/coste final incorrecto.");
            });
            test("Poison: un daño por turno propio, reaplicar no duplica ni acumula",()=>{
                var g=New(catalog);var p=Put(g,"wandering-beast",0,0,0);EffectManager.Poison(g,p,9,1);EffectManager.Poison(g,p,1,1);
                Round(g);Check(p.health==4&&p.poison==1,"Tick uno.");Round(g);Check(p.health==3&&p.poison==1,"Tick dos.");Round(g);Check(p.health==2,"Tick tres.");
            });
            test("Lushroom: dos daño Poison, aura booleana, bloqueo curación exige las cuatro condiciones",()=>{
                var g=New(catalog);var lush=Put(g,"lushroom",1,0,0,Biome.Swamp);Put(g,"lushroom",1,0,1,Biome.Swamp);var p=Put(g,"old-mummy",0,1,0,Biome.Swamp);p.health=2;EffectManager.Poison(g,p,1,1);
                Check(FactionCardRules.PoisonDamage(g,p)==2,"Lushroom acumula bonus.");GenericCardRules.Heal(g,p,9);Check(p.health==2,"Cura en Pantano envenenado.");
                g.State.tiles[p.tileId].biome=Biome.Desert;GenericCardRules.Heal(g,p,9);Check(p.health==5,"Bloquea fuera de Pantano.");
                p.health=2;p.poison=0;g.State.tiles[p.tileId].biome=Biome.Swamp;GenericCardRules.Heal(g,p,9);Check(p.health==5,"Bloquea no envenenado.");
                EffectManager.Poison(g,lush,1,0);lush.health=2;GenericCardRules.Heal(g,lush,9);Check(lush.health==5,"Bloquea aliado.");
                p.poison=1;g.State.tiles[lush.tileId].biome=Biome.Forest;g.State.tiles[At(g,0,1).id].biome=Biome.Forest;Check(FactionCardRules.PoisonDamage(g,p)==1&&!FactionCardRules.HealingBlocked(g,p),"Lushroom fuera de Pantano sigue activo.");
            });
            test("Lushroom modifica el tick real y los Tokens muertos por Poison activan Zukgrok",()=>{
                var g=New(catalog);Put(g,"lushroom",0,0,0,Biome.Swamp);var p=Put(g,"frog-token",1,1,0,Biome.Swamp,true);EffectManager.Poison(g,p,1,0);End(g);
                Check(p.health==0&&g.State.tiles[p.tileId].biome!=Biome.Neutral,"Poison no mata Token o no terraforma.");
            });
            test("Alligator: +HP +ATK por Token real, conserva daño y elimina aura al morir Token",()=>{
                var g=New(catalog);var p=Put(g,"alligator-revengeful-bite",0,0,0);var token=Put(g,"battle-ant-token",0,1,0,token:true);Check(p.health==5&&g.MaxHealth(p)==5&&CombatManager.AttackValue(g,p)==2,"Aura de Token.");
                p.health-=2;GenericCardRules.SyncAuras(g);Check(p.health==3,"Aura cura daño.");Put(g,"frog-token",1,2,0,token:true);Check(g.MaxHealth(p)==5,"Cuenta Token enemigo.");
                CombatManager.Remove(g,token);Check(p.health==2&&g.MaxHealth(p)==4&&CombatManager.AttackValue(g,p)==1,"Aura permanece o cura por muerte de Token.");
            });
            test("Alligator cura al morir Unit, no Structure ni Token; kill ofrece Token opcional",()=>{
                var g=New(catalog);var p=Put(g,"alligator-revengeful-bite",0,0,0);p.health=2;var enemy=Put(g,"old-mummy",1,1,0);CombatManager.DirectDamage(g,enemy,99,p);
                Check(p.health==3&&g.State.pendingChoices.Single().kind=="Summon"&&g.State.pendingChoices[0].optional,"Muerte Unit o kill incorrecto.");Choices(g);Check(g.Allies(0).Count(x=>x.token&&x.cardId=="alligator-token")==1,"No invoca Alligator Token.");
                p.health=2;CombatManager.Remove(g,Put(g,"med-camp",1,2,0));Check(p.health==2,"Cura por Structure.");
            });
            test("Alligator Token: efecto incompleto no aumenta HP a nadie",()=>{
                var g=New(catalog);var ally=Put(g,"old-mummy",0,0,0);var token=Put(g,"alligator-token",0,1,0,token:true);CombatManager.Remove(g,token);Check(ally.health==5&&g.MaxHealth(ally)==5,"Inventó destinatario de +1HP.");
            });
            test("Escenarios 2, 3, 4 jugadores conservan los tres mazos reales",()=>{
                foreach(int n in new[]{1,2,3,4}){var g=New(catalog,n);PrototypeScenario.Load(g);Check(StateValidator.Validate(g.State,catalog)=="","Escenario inválido "+n+": "+StateValidator.Validate(g.State,catalog));}
            });
            test("IA usa Zukgrok actualizado, resuelve elecciones y progresa sin cartas fantasma",()=>{
                var g=New(catalog,2);foreach(var p in g.State.players.Where(p=>!p.inactive))p.isAI=true;var ai=new AiPlayer();int initial=g.State.turn;
                for(int i=0;i<500&&g.State.turn<initial+24&&g.CanAct;i++){Check(ai.Step(g),"IA sin acción.");Check(StateValidator.Validate(g.State,catalog)=="","IA altera mazo: "+StateValidator.Validate(g.State,catalog));}
                Check(g.State.turn>=initial+16||g.State.phase==Phase.Finished,"IA no progresa.");
            });
        }
    }
}
#endif
