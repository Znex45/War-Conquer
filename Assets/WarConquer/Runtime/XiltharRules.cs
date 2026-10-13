using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    // Xil'thar uses the shared board, choices, damage, death, energy and turn hooks.
    public static class XiltharRules
    {
        static Piece Piece(GameManager g,int id)=>BoardManager.Pieces(g.State).FirstOrDefault(p=>p.id==id);
        public static bool InVoid(GameManager g,Piece p)=>p!=null&&g.State.tiles[p.tileId].biome==Biome.Void;
        public static bool CanTarget(GameManager g,Piece p)=>p==null||!g.Data(p).Has("EternalEye")||!InVoid(g,p);
        public static bool TargetsPiece(string kind)=>new[]{"Unit","AllyUnit","EnemyUnit","EnemyGround","EnemyForest","EnemyForestSwamp","EnemyPiece","EnemyAdjacent","AllyDesertUnit"}.Contains(kind);
        public static List<int> FilterSpellTargets(GameManager g,CardData card,List<int> ids,IList<int> selected)
        {
            string op=card.effects.FirstOrDefault()?.operation;
            return op=="Engage"||op=="CleansingConquest"&&selected.Count>0?ids.Where(id=>CanTarget(g,g.State.tiles[id].unit)).ToList():ids;
        }
        public static bool EyeTerraformException(GameManager g,HexTile t,int owner)=>t.unit!=null&&t.unit.owner!=owner&&g.Data(t.unit).Has("EternalEye")&&(t.structure==null||t.structure.owner==owner);
        public static void OnTerraform(GameManager g,int owner)
        {
            var p=g.State.players[owner];
            if(p.leader!="XIL'THAR"||g.State.activePlayer!=owner||(g.State.phase!=Phase.Actions&&g.State.phase!=Phase.Start&&g.State.phase!=Phase.End))return;
            p.xilTerraformCount++;
            if(p.xilTerraformCount%4==0){p.currentEnergy++;p.xilEnergyGenerated++;g.State.Log("XIL'THAR · 4 terraformaciones reales: +1 Energía.");}
        }
        public static void SyncTaxes(GameManager g)
        {
            var eyes=BoardManager.Pieces(g.State).Where(p=>p.health>0&&g.Data(p).Has("EternalEye")&&InVoid(g,p)).ToArray();
            foreach(var player in g.State.players)player.opponentCardTax=eyes.Count(p=>p.owner!=player.id);
        }
        public static bool CanSecondAttack(GameManager g,Piece p)=>g.Data(p).Has("DarkFly")&&p.attacked&&!p.moved&&!p.darkFlyExtraAttack&&p.remainingMovement>0;
        public static bool Active(CardData c)=>new[]{"Portal","Insolator","EternalEye","SprintingSprite"}.Any(c.Has);
        public static int AbilityCost(GameManager g,Piece p)=>g.Data(p).Has("Portal")?2:g.Data(p).Has("EternalEye")||g.Data(p).Has("SprintingSprite")?1:0;
        public static int RecastCost(GameManager g,int owner,CardData card,bool free)=> (free?0:card.energyCost+2)+g.State.players[owner].opponentCardTax;
        static bool Land(GameManager g,Piece unit,int id)=>unit!=null&&CanTarget(g,unit)&&GenericCardRules.CanLand(g,unit,g.State.tiles[id])&&!g.IsSleeping(unit);
        static IEnumerable<int> PortalLandings(GameManager g,Piece portal,Piece unit)=>g.State.tiles[portal.tileId].neighbors.Where(id=>Land(g,unit,id));
        public static List<int> AbilityTargets(GameManager g,Piece p)
        {
            var c=g.Data(p);var s=g.State;
            if(s.players[p.owner].currentEnergy<AbilityCost(g,p))return new List<int>();
            if(c.Has("EternalEye"))return InVoid(g,p)?s.tiles.Where(t=>t.id!=p.tileId&&t.biome==Biome.Void&&GenericCardRules.CanLand(g,p,t)).Select(t=>t.id).ToList():new List<int>();
            if(c.Has("Portal"))return BoardManager.UnitsWithinRadius(s,p.tileId,6).Where(u=>InVoid(g,u)&&CanTarget(g,u)&&PortalLandings(g,p,u).Any()).Select(u=>u.tileId).ToList();
            if(c.Has("SprintingSprite"))return !InVoid(g,p)?new List<int>():BoardManager.Pieces(s).Where(u=>u.id!=p.id&&CanTarget(g,u)&&!g.IsSleeping(u)&&GenericCardRules.CanLand(g,u,s.tiles[p.tileId],p)).Select(u=>u.tileId).ToList();
            if(c.Has("Insolator"))return BoardManager.UnitsWithinRadius(s,p.tileId,3).Select(u=>u.tileId).ToList();
            return new List<int>();
        }
        public static void Activate(GameManager g,Piece p,IList<int> targets)
        {
            var c=g.Data(p);g.State.players[p.owner].currentEnergy-=AbilityCost(g,p);
            if(c.Has("EternalEye")){GenericCardRules.SpecialMove(g,p,targets[0]);return;}
            if(c.Has("Portal"))
            {
                var unit=g.State.tiles[targets[0]].unit;
                ChoiceManager.Enqueue(g,new PendingChoice{kind="PortalLand",owner=p.owner,sourceId=p.id,victimId=unit.id,tileId=p.tileId,prompt="Portal · elige un Slab adyacente libre"});
            }
            if(c.Has("SprintingSprite"))
            {
                var unit=g.State.tiles[targets[0]].unit;int destination=p.tileId;CombatManager.Remove(g,p);GenericCardRules.SpecialMove(g,unit,destination);
            }
            if(c.Has("Insolator"))
            {
                var finish=new PendingChoice{kind="InsolatorFinish",owner=p.owner,sourceId=p.id,tileId=p.tileId};
                g.State.pendingChoices.Add(finish);
                foreach(var unit in BoardManager.UnitsWithinRadius(g.State,p.tileId,3).ToArray())CombatManager.DirectDamage(g,unit,2,p);
                DrainAuto(g);
            }
        }
        public static bool PendingHit(GameManager g,int victim)=>g.State.pendingChoices.Any(c=>c.kind=="VoidPrevent"&&c.victimId==victim);
        public static bool PreventHit(GameManager g,Piece victim,int damage,Piece source,bool direct,bool attack)
        {
            if(victim==null||victim.health<=0||damage<=0||g.Data(victim).IsStructure)return false;
            var choice=new PendingChoice{kind="VoidPrevent",owner=victim.owner,sourceId=source?.id??-1,victimId=victim.id,tileId=victim.tileId,amount=damage,direct=direct,attack=attack,optional=true,prompt="Void Hole · recibe 1 daño para prevenir el impacto, u omite para recibirlo"};
            if(ChoiceTargets(g,choice).Count==0)return false;
            int auto=g.State.pendingChoices.FindIndex(IsAuto);
            if(auto<0)g.State.pendingChoices.Add(choice);else g.State.pendingChoices.Insert(auto,choice);
            return true;
        }
        public static void RegisterKill(GameManager g,Piece source)
        {
            if(source==null)return;
            RegisterKill(g,source.id);
        }
        static void RegisterKill(GameManager g,int sourceId)
        {
            var finish=g.State.pendingChoices.FirstOrDefault(c=>c.kind=="InsolatorFinish"&&c.sourceId==sourceId);
            if(finish!=null)finish.kills++;
        }
        public static void DeferEngage(GameManager g,Piece ally,Piece enemy,int destination)
        {g.State.pendingChoices.Add(new PendingChoice{kind="EngageFinish",owner=ally.owner,sourceId=ally.id,victimId=enemy.id,tileId=destination});}
        static bool IsAuto(PendingChoice c)=>c.kind=="EngageFinish"||c.kind=="InsolatorFinish";
        public static void DrainAuto(GameManager g)
        {
            while(g.State.pendingChoices.Count>0&&IsAuto(g.State.pendingChoices[0]))
            {
                var c=g.State.pendingChoices[0];g.State.pendingChoices.RemoveAt(0);
                if(c.kind=="EngageFinish"&&Piece(g,c.victimId)==null)
                {var ally=Piece(g,c.sourceId);if(ally!=null)GenericCardRules.SpecialMove(g,ally,c.tileId);}
                if(c.kind=="InsolatorFinish"&&c.kills>0)
                    ChoiceManager.Enqueue(g,new PendingChoice{kind="VoidTerraform",owner=c.owner,tileId=c.tileId,prompt="Insolator · una Unit murió: elige un Slab para terraformar en Void"});
            }
        }
        public static IEnumerable<CardInstance> DarknessCards(GameManager g,int owner)=>g.State.players[owner].discardPile.Where(c=>g.Catalog[c.cardId].category==Category.Unit&&RecastTiles(g,c,owner).Any()&&g.State.players[owner].currentEnergy>=RecastCost(g,owner,g.Catalog[c.cardId],true));
        static IEnumerable<int> RecastTiles(GameManager g,CardInstance card,int owner)=>g.ForPlayer(owner,()=>g.State.tiles.Where(t=>FactionCardRules.CanDeploy(g,g.Catalog[card.cardId],t)).Select(t=>t.id).ToArray());
        public static void BeginDarkness(GameManager g,int owner)
        {ChoiceManager.Enqueue(g,new PendingChoice{kind="DarknessPick",owner=owner,tileId=0,prompt="Under the Darkness · elige una Unit de tu descarte"});}
        public static void OnDeath(GameManager g,Piece p)
        {
            if(p.token||g.Data(p).category!=Category.Unit||!InVoid(g,p)||g.State.players[p.owner].eliminated)return;
            if(!g.State.players[p.owner].discardPile.Any(c=>c.instanceId==p.id))return;
            ChoiceManager.Enqueue(g,new PendingChoice{kind="BeaconPick",owner=p.owner,cardRefId=p.id,tileId=p.tileId,optional=true,prompt="Void Beacon · elige uno para destruirlo y recuperar la Unit, u omite"});
        }
        static CardInstance DiscardCard(GameManager g,PendingChoice c)=>g.State.players[c.owner].discardPile.FirstOrDefault(x=>x.instanceId==c.cardRefId);
        public static bool KnownChoice(string kind)=>new[]{"VoidPrevent","PortalLand","VoidTerraform","DarknessPick","DarknessLand","BeaconPick","BeaconMode","BeaconDebt","EngageFinish","InsolatorFinish"}.Contains(kind);
        public static bool BoardChoice(string kind)=>kind!="DarknessPick"&&kind!="BeaconMode"&&kind!="BeaconDebt";
        public static List<int> ChoiceTargets(GameManager g,PendingChoice c)
        {
            if(c.kind=="VoidPrevent")return g.Allies(c.owner).Where(p=>p.health>0&&g.Data(p).Has("VoidHole")&&p.voidHoleTurn!=g.State.turn&&BoardManager.AxialDistance(g.State.tiles[p.tileId],g.State.tiles[c.tileId])<=5).Select(p=>p.tileId).ToList();
            if(c.kind=="PortalLand"){var source=Piece(g,c.sourceId);var unit=Piece(g,c.victimId);return source==null||unit==null?new List<int>():PortalLandings(g,source,unit).ToList();}
            if(c.kind=="VoidTerraform")return g.ForPlayer(c.owner,()=>g.State.tiles.Where(t=>t.biome!=Biome.Void&&g.TerraformTarget(t.id)).Select(t=>t.id).ToList());
            if(c.kind=="DarknessPick")return DarknessCards(g,c.owner).Select(x=>x.instanceId).ToList();
            var card=DiscardCard(g,c);
            if(c.kind=="DarknessLand")return card==null?new List<int>():RecastTiles(g,card,c.owner).ToList();
            if(c.kind=="BeaconPick")return card==null?new List<int>():g.Allies(c.owner).Where(p=>g.Data(p).Has("VoidBeacon")&&g.State.tiles[p.tileId].unit==null).Select(p=>p.tileId).ToList();
            if(c.kind=="BeaconMode")
            {
                var beacon=Piece(g,c.sourceId);if(card==null||beacon==null||g.State.tiles[beacon.tileId].unit!=null)return new List<int>();
                var ids=new List<int>();if(g.State.players[c.owner].currentEnergy>=RecastCost(g,c.owner,g.Catalog[card.cardId],true))ids.Add(0);
                if(g.State.players[c.owner].currentEnergy>=RecastCost(g,c.owner,g.Catalog[card.cardId],false))ids.Add(1);return ids;
            }
            if(c.kind=="BeaconDebt")
            {
                var unit=Piece(g,c.sourceId);if(unit==null)return new List<int>();var ids=new List<int>{0};
                if(g.State.players[c.owner].currentEnergy>=g.Data(unit).energyCost+2)ids.Add(1);return ids;
            }
            return new List<int>();
        }
        static void Recast(GameManager g,PendingChoice c,int tile,bool darkness,bool debt)
        {
            var card=DiscardCard(g,c);if(card==null)return;
            g.State.players[c.owner].discardPile.Remove(card);g.State.tiles[tile].owner=c.owner;
            var unit=g.Place(card.cardId,c.owner,tile,card.instanceId,false);unit.underDarkness=darkness;unit.beaconDebt=debt;g.ForPlayer(c.owner,()=>{EffectManager.OnEnter(g,unit);return true;});
            if(debt&&g.State.endingTurn&&c.owner==g.State.activePlayer)
                ChoiceManager.Enqueue(g,new PendingChoice{kind="BeaconDebt",owner=c.owner,sourceId=unit.id,tileId=tile,prompt="Void Beacon · pagar coste +2 o dejar morir"});
        }
        public static bool ResolveChoice(GameManager g,PendingChoice c,IList<int> targets,bool skip)
        {
            if(!KnownChoice(c.kind))return false;
            int target=targets.Count>0?targets[0]:-1;var player=g.State.players[c.owner];
            if(c.kind=="VoidPrevent")
            {
                var victim=Piece(g,c.victimId);if(victim==null)return true;
                if(!skip&&target>=0){var hole=g.State.tiles[target].unit;hole.voidHoleTurn=g.State.turn;CombatManager.DirectDamage(g,hole,1,null,true);g.State.Log("Void Hole previene "+c.amount+" de daño.");}
                else {var source=Piece(g,c.sourceId);if(c.direct){CombatManager.DirectDamage(g,victim,c.amount,source,true);if(source==null&&victim.health<=0)RegisterKill(g,c.sourceId);}else{CombatManager.Damage(g,victim,c.amount,c.attack,source,true);if(c.attack)GenericCardRules.OnKill(g,source,victim);}}
                return true;
            }
            if(skip||target<0)return true;
            if(c.kind=="PortalLand")GenericCardRules.SpecialMove(g,Piece(g,c.victimId),target);
            if(c.kind=="VoidTerraform")TerrainManager.Terraform(g,target,Biome.Void,c.owner);
            if(c.kind=="DarknessPick")ChoiceManager.Enqueue(g,new PendingChoice{kind="DarknessLand",owner=c.owner,cardRefId=target,tileId=0,prompt="Under the Darkness · elige dónde jugar la Unit"});
            if(c.kind=="DarknessLand")
            {
                var card=DiscardCard(g,c);int tax=RecastCost(g,c.owner,g.Catalog[card.cardId],true);
                if(player.currentEnergy>=tax){player.currentEnergy-=tax;Recast(g,c,target,true,false);}
            }
            if(c.kind=="BeaconPick")ChoiceManager.Enqueue(g,new PendingChoice{kind="BeaconMode",owner=c.owner,cardRefId=c.cardRefId,sourceId=g.State.tiles[target].structure.id,tileId=target,optional=true,prompt="Void Beacon · elige el pago o la muerte diferida"});
            if(c.kind=="BeaconMode")
            {
                var card=DiscardCard(g,c);player.currentEnergy-=RecastCost(g,c.owner,g.Catalog[card.cardId],target==0);
                var beacon=Piece(g,c.sourceId);int tile=beacon.tileId;CombatManager.Remove(g,beacon);Recast(g,c,tile,false,target==0);
            }
            if(c.kind=="BeaconDebt")
            {
                var unit=Piece(g,c.sourceId);if(target==1){player.currentEnergy-=g.Data(unit).energyCost+2;unit.beaconDebt=false;}
                else CombatManager.Remove(g,unit);
            }
            return true;
        }
        public static void OnEnd(GameManager g)
        {
            foreach(var p in BoardManager.Pieces(g.State).ToArray())
            {
                if(p.underDarkness&&!InVoid(g,p))CombatManager.Remove(g,p);
                else if(p.beaconDebt&&p.owner==g.State.activePlayer)
                    ChoiceManager.Enqueue(g,new PendingChoice{kind="BeaconDebt",owner=p.owner,sourceId=p.id,tileId=p.tileId,prompt="Void Beacon · paga "+(g.Data(p).energyCost+2)+" Energía o deja morir la Unit"});
            }
        }
        public static bool AiChoice(GameManager g,PendingChoice c)
        {
            var targets=ChoiceTargets(g,c);
            if(targets.Count==0)return ChoiceManager.Resolve(g,Array.Empty<int>(),c.optional);
            if(c.kind=="BeaconMode"||c.kind=="BeaconDebt")return ChoiceManager.Resolve(g,new[]{targets.Contains(1)?1:0});
            if(c.kind=="DarknessPick")targets=targets.OrderByDescending(id=>g.Catalog[g.State.players[c.owner].discardPile.First(x=>x.instanceId==id).cardId].energyCost).ToList();
            if(c.kind=="DarknessLand"||c.kind=="PortalLand")targets=targets.OrderByDescending(id=>g.State.tiles[id].biome==Biome.Void).ToList();
            return ChoiceManager.Resolve(g,targets.Take(c.count).ToArray(),c.optional&&targets.Count==0);
        }
    }
}
