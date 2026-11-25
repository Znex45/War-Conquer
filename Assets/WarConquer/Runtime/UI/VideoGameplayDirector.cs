using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // Opt-in recording aid. It uses the normal public rules and never runs in an ordinary match.
    public sealed class VideoGameplayDirector : MonoBehaviour
    {
        readonly AiPlayer fallback=new AiPlayer();
        int ensuredTurn=-1,aiTurn=-1;TurnStage aiStage;bool aiActed;
        public bool Active { get; private set; }
        public void Activate(GameManager game)
        {
            Active=true;ensuredTurn=-1;aiTurn=-1;aiActed=false;
            Debug.Log("[VIDEO GAMEPLAY] IA: despliegue breve, presión sobre el centro y terraformación Desierto.");
        }
        public bool EnsurePlayableUnit(GameManager game)
        {
            if(!Active||game?.State==null||!game.CanAct||game.ActingPlayer.isAI||game.State.stage!=TurnStage.Deployment||ensuredTurn==game.State.turn)return false;
            ensuredTurn=game.State.turn;var player=game.ActingPlayer;
            bool playable=player.hand.Any(c=>game.Catalog[c.cardId].category==Category.Unit&&game.CardBlockReason(c,true)=="");
            if(playable)return false;
            var replacement=player.deck.Where(c=>game.Catalog[c.cardId].category==Category.Unit)
                .Where(c=>game.Catalog[c.cardId].energyCost<=player.currentEnergy)
                .Where(c=>game.CardTargets(game.Catalog[c.cardId]).Count>0)
                .OrderBy(c=>game.Catalog[c.cardId].energyCost).FirstOrDefault();
            if(replacement==null)return false;
            var returned=player.hand.FirstOrDefault(c=>game.Catalog[c.cardId].category!=Category.Unit)??player.hand.FirstOrDefault();
            player.deck.Remove(replacement);player.hand.Add(replacement);
            if(returned!=null){player.hand.Remove(returned);player.deck.Add(returned);}
            Debug.Log("[VIDEO GAMEPLAY] J1 recibe unidad utilizable: "+game.Catalog[replacement.cardId].name+".");
            return true;
        }
        public bool StepAI(GameManager game)
        {
            if(!Active||game==null||!game.CanAct||!game.ActingPlayer.isAI)return false;
            if(game.State.pendingChoices.Count>0||game.State.battle!=null||game.State.responsePlayer>=0)return fallback.Step(game);
            if(aiTurn!=game.State.turn||aiStage!=game.State.stage){aiTurn=game.State.turn;aiStage=game.State.stage;aiActed=false;}
            if(!aiActed)
            {
                aiActed=true;
                if(aiStage==TurnStage.Deployment&&Deploy(game))return true;
                if(aiStage==TurnStage.Assault&&(Attack(game)||Move(game)))return true;
                if(aiStage==TurnStage.Terraforming&&Terraform(game))return true;
            }
            Debug.Log("[VIDEO GAMEPLAY] IA avanza desde "+TimingRules.StageName(game.State.stage)+".");
            return game.AdvanceStage();
        }
        static int Distance(GameManager game,int a,int b)=>BoardManager.AxialDistance(game.State.tiles[a],game.State.tiles[b]);
        static int Center(GameManager game)=>game.State.tiles.Where(ConquestManager.IsCenter).OrderBy(t=>t.id).First().id;
        static bool Deploy(GameManager game)
        {
            var option=game.ActingPlayer.hand.Where(c=>game.Catalog[c.cardId].category==Category.Unit)
                .Where(c=>game.CardBlockReason(c,true)=="").OrderBy(c=>game.Catalog[c.cardId].energyCost).FirstOrDefault();
            if(option==null)return false;var targets=game.CardTargets(game.Catalog[option.cardId]);if(targets.Count==0)return false;
            int target=targets.OrderBy(id=>Distance(game,id,Center(game))).First();
            bool done=game.Play(option.instanceId,new[]{target},true);
            if(done)Debug.Log("[VIDEO GAMEPLAY] IA despliega "+game.Catalog[option.cardId].name+" en hex "+(target+1)+".");return done;
        }
        static bool Attack(GameManager game)
        {
            foreach(var unit in game.Allies(game.ActingPlayerId).Where(p=>!game.Data(p).IsStructure).OrderBy(p=>p.id))
            {
                var targets=CombatManager.Targets(game,unit).OrderBy(id=>(game.State.tiles[id].unit??game.State.tiles[id].structure)?.health??99).ToList();
                int target=targets.Count>0?targets[0]:-1;
                if(target>=0&&CombatManager.Attack(game,unit,target)){Debug.Log("[VIDEO GAMEPLAY] IA ataca desde hex "+(unit.tileId+1)+" hacia "+(target+1)+".");return true;}
            }
            return false;
        }
        static bool Move(GameManager game)
        {
            int center=Center(game);
            foreach(var unit in game.Allies(game.ActingPlayerId).Where(p=>!game.Data(p).IsStructure).OrderBy(p=>Distance(game,p.tileId,center)))
            {
                var paths=MovementManager.Paths(game,unit);if(paths.Count==0)continue;
                int destination=paths.Keys.OrderBy(id=>Distance(game,id,center)).ThenBy(id=>id).First();
                if(MovementManager.Move(game,unit,destination)){Debug.Log("[VIDEO GAMEPLAY] IA mueve "+game.Data(unit).name+" al hex "+(destination+1)+".");return true;}
            }
            return false;
        }
        static bool Terraform(GameManager game)
        {
            var target=game.State.tiles.Where(t=>game.TerraformTarget(t.id)&&t.biome!=Biome.Desert)
                .OrderBy(t=>Distance(game,t.id,Center(game))).ThenBy(t=>t.id).FirstOrDefault();
            if(target==null||game.ActingPlayer.currentEnergy<game.TerraformCost(Biome.Desert))return false;
            bool done=game.Terraform(target.id,Biome.Desert);
            if(done)Debug.Log("[VIDEO GAMEPLAY] IA terraforma Desierto en hex "+(target.id+1)+".");return done;
        }
    }
}
