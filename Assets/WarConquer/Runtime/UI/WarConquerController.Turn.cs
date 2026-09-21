using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void DrawTurnActions()
        {
            var s=Game.State;bool responding=s.battle!=null||s.responsePlayer>=0;
            GrayboxUI.Text(left,responding?"INTERVENCIÓN · J"+(Game.ActingPlayerId+1):"ETAPAS · J"+(s.activePlayer+1),12,12,216,26,16,GrayboxUI.Ink,FontStyle.Bold);
            for(int i=0;i<3;i++)
            {
                var stage=TimingRules.Order[i];bool active=s.stage==stage;
                GrayboxUI.Text(left,(active?"● ":"○ ")+TimingRules.StageName(stage),14,46+i*24,214,24,14,active?GrayboxUI.PlayerColor(s.activePlayer):GrayboxUI.Muted,active?FontStyle.Bold:FontStyle.Normal);
            }
            float y=130;
            if(viewedPlayer!=Game.ActingPlayerId)
            {
                GrayboxUI.Button(left,"Volver a J"+(Game.ActingPlayerId+1),12,y,214,35,()=>{viewedPlayer=Game.ActingPlayerId;page=0;ClearAction();Render();});
                if(s.battle==null&&s.responsePlayer<0&&BattleManager.HasResponse(Game,viewedPlayer))GrayboxUI.Button(left,"Intervenir con permiso",12,y+42,214,36,()=>BattleManager.RequestOutsideTurn(Game,viewedPlayer));
                return;
            }
            if(!Game.CanAct){GrayboxUI.Text(left,s.phase==Phase.Setup?"PARTIDA EN PAUSA":"PARTIDA FINALIZADA",14,y,214,32,15);return;}
            if(Game.ActingPlayer.isAI)
            {
                GrayboxUI.Text(left,"TURNO DE LA IA",14,y,214,30,19,GrayboxUI.PlayerColor(Game.ActingPlayerId),FontStyle.Bold);
                GrayboxUI.Text(left,"El rival está jugando.\n\nUsa sus cartas, energía y unidades con las mismas reglas.",14,y+42,208,105,15,GrayboxUI.Muted);
                return;
            }
            if(responding)
            {
                GrayboxUI.Text(left,"Solo respuestas autorizadas por el texto de la carta o habilidad.",14,y,212,50,14,GrayboxUI.Muted);y+=57;
                if(Game.ActingPlayer.hand.Any(c=>Game.CardBlockReason(c,useResources)==""))ActionButton("Elegir respuesta",ref y,()=>FilterHand("Permitidas"));
                DrawAbilityAction(ref y);
                GrayboxUI.Button(left,"PASAR / RESOLVER",12,280,214,34,()=>{ClearAction();BattleManager.Pass(Game,Game.ActingPlayerId);},edge);return;
            }
            if(s.stage==TurnStage.Deployment)
            {
                if(Game.ActingPlayer.hand.Any(c=>!Game.Catalog[c.cardId].IsStructure&&Game.Catalog[c.cardId].category!=Category.Spell&&Game.CardBlockReason(c,useResources)==""))ActionButton("Jugar unidad / Latente",ref y,()=>FilterHand("Unidades"));
                if(Game.ActingPlayer.hand.Any(c=>Game.Catalog[c.cardId].IsStructure&&Game.CardBlockReason(c,useResources)==""))ActionButton("Construir estructura",ref y,()=>FilterHand("Estructuras"));
            }
            else if(s.stage==TurnStage.Terraforming)
            {
                if(Game.State.tiles.Any(t=>Game.TerraformTarget(t.id))&&new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland}.Any(b=>Game.TerraformCost(b)<=Game.ActingPlayer.currentEnergy))ActionButton("Terraformar casilla",ref y,()=>Begin("terraform"));
                if(Game.ActingPlayer.currentEnergy>=Game.AshCost()&&Game.State.tiles.Any(t=>t.owner==Game.ActingPlayerId&&TerrainManager.Normal(t)))ActionButton("Revelar Ceniza · "+Game.AshCost()+" E",ref y,()=>Begin("ash"));
            }
            else
            {
                if(Game.Allies(Game.ActingPlayerId).Any(p=>MovementManager.Paths(Game,p).Count>0))ActionButton("Mover / conquistar",ref y,()=>SelectActionPiece(false));
                if(Game.Allies(Game.ActingPlayerId).Any(p=>CombatManager.Targets(Game,p).Count>0))ActionButton("Atacar unidad o base",ref y,()=>SelectActionPiece(true));
            }
            if(Game.ActingPlayer.hand.Any(c=>Game.Catalog[c.cardId].category==Category.Spell&&Game.CardBlockReason(c,useResources)==""))ActionButton("Magias permitidas",ref y,()=>FilterHand("Magias"));
            DrawAbilityAction(ref y);
            if(y==130)GrayboxUI.Text(left,"Sin acciones disponibles.\nPuedes avanzar de etapa.",14,y,211,65,15,GrayboxUI.Muted);
            string next=s.stage==TurnStage.Deployment?"IR A ATAQUE":s.stage==TurnStage.Assault?"IR A TERRAFORMACIÓN":"FINALIZAR TURNO";
            GrayboxUI.Button(left,next,12,280,214,34,()=>{ClearAction();handFilter="Todas";page=0;Game.AdvanceStage();},Color.Lerp(GrayboxUI.PlayerColor(s.activePlayer),GrayboxUI.Panel,.6f));
        }
        void ActionButton(string name,ref float y,System.Action action)
        {GrayboxUI.Button(left,name,12,y,214,30,action);y+=34;}
        void DrawAbilityAction(ref float y)
        {
            var available=selectedPiece!=null&&AbilityManager.CanActivate(Game,selectedPiece)?selectedPiece:Game.Allies(Game.ActingPlayerId).FirstOrDefault(p=>AbilityManager.CanActivate(Game,p));
            if(available!=null)ActionButton("Habilidad de pieza",ref y,()=>{selectedPiece=available;focus=available.tileId;selectedCard=-1;if(AbilityManager.TargetCount(Game.Data(available))==0)AbilityManager.Activate(Game,available,new int[0]);else Begin("ability");});
        }
        void FilterHand(string filter){ClearAction();handFilter=filter;page=0;Render();}
        void SelectActionPiece(bool attack)
        {
            ClearAction();selectedPiece=Game.Allies(Game.ActingPlayerId).FirstOrDefault(p=>attack?CombatManager.Targets(Game,p).Count>0:MovementManager.Paths(Game,p).Count>0);
            if(selectedPiece==null)return;focus=selectedPiece.tileId;mode=attack?"attack":"move";Render();
        }
        void DrawLeader(Player player)
        {
            bool fungus=player.leader=="ZUKGROK";var color=GrayboxUI.PlayerColor(player.id);
            var portrait=GrayboxUI.Rect(leaderPanel,"Retrato "+player.leader,14,14,62,62);var hex=portrait.gameObject.AddComponent<HexGraphic>();hex.color=Color.Lerp(color,GrayboxUI.Panel,.5f);hex.border=color;hex.raycastTarget=false;
            var glyph=GrayboxUI.Text(portrait,fungus?"Z":"S",0,10,62,45,32,color,FontStyle.Bold);glyph.alignment=TextAnchor.MiddleCenter;
            GrayboxUI.Text(leaderPanel,player.leader,88,16,142,27,20,color,FontStyle.Bold);
            GrayboxUI.Text(leaderPanel,player.factionTag+" · J"+(player.id+1),88,45,140,24,12,GrayboxUI.Muted);
            GrayboxUI.Text(leaderPanel,"VIDA "+player.leaderHealth+" / "+Game.State.rules.leaderHealth+(player.eliminated?" · FUERA":""),14,83,214,26,18,color,FontStyle.Bold);
            GrayboxUI.Text(leaderPanel,fungus?"INFLUENCIA MICELIAL":"DOMINIO DE LAS ARENAS",14,119,214,27,13,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(leaderPanel,TimingRules.StageName(TimingRules.LeaderStage(player))+" · COSTE "+Game.State.rules.leaderAbilityCost+" E",14,158,214,22,13,color,FontStyle.Bold);
            bool own=player.id==Game.ActingPlayerId;bool ready=own&&!player.isAI&&AbilityManager.LeaderTargets(Game).Count>0;
            string condition=player.eliminated?"Líder eliminado":player.isAI?"Controlado por IA":!own?"Espera tu turno":!TimingRules.LeaderAllowed(Game)?"Disponible en "+TimingRules.StageName(TimingRules.LeaderStage(player)):player.currentEnergy<Game.State.rules.leaderAbilityCost?"Energía insuficiente":ready?"Habilidad disponible":fungus?"Requiere red y enemigo en Bosque":"Requiere Desierto propio";
            GrayboxUI.Text(leaderPanel,condition,14,188,212,38,12,GrayboxUI.Muted);
            GrayboxUI.Button(leaderPanel,"Ver habilidad",12,230,214,30,()=>ShowLeaderDetails(player));
            GrayboxUI.Button(leaderPanel,"Activar habilidad",12,275,214,27,()=>Begin("leader"),Color.Lerp(color,GrayboxUI.Panel,.55f),ready);
        }
    }
}
