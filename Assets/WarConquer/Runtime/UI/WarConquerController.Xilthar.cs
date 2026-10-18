using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void ShowRecastTooltip(CardData card,Player player)
        {
            HideTooltip();tooltip=CardPresentation.Draw(root,Game,card,player,847,81,388,624,false,true,recastEnergy:XiltharRules.RecastCost(Game,player.id,card,true));
            tooltip.name="Vista ampliada (solo lectura)";tooltip.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false;tooltip.SetAsLastSibling();
        }
        void DrawXiltharLeader(Player p)
        {
            var color=GrayboxUI.PlayerColor(p.id);
            GrayboxUI.Text(leaderPanel,"4 TERRAFORMACIONES → +1 E",14,119,214,36,13,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(leaderPanel,"SLABS: "+p.xilTerraformCount+" · "+p.xilTerraformCount%4+" / 4",14,166,214,30,18,color,FontStyle.Bold);
            GrayboxUI.Text(leaderPanel,"ENERGÍA GENERADA: +"+p.xilEnergyGenerated,14,204,214,30,14,color,FontStyle.Bold);
            GrayboxUI.Button(leaderPanel,"Ver habilidad",12,261,214,32,()=>ShowLeaderDetails(p));
        }
        void DrawXiltharDetails(RectTransform body,Player p)
        {
            GrayboxUI.Text(body,"XIL'THAR · TERRAFORMACIÓN",40,115,1030,60,29,GrayboxUI.PlayerColor(p.id),FontStyle.Bold);
            GrayboxUI.Text(body,"Por cada 4 Slabs realmente terraformados durante tu turno: +1 Energía. El contador vuelve a cero al comenzar tu siguiente turno. Los intentos inválidos, el movimiento, colocar piezas y cambiar el control no cuentan.",40,223,1010,190,27,GrayboxUI.Ink);
            GrayboxUI.Text(body,"Slabs: "+p.xilTerraformCount+"   ·   Progreso: "+p.xilTerraformCount%4+" / 4   ·   Energía: +"+p.xilEnergyGenerated,40,460,1030,50,25,GrayboxUI.PlayerColor(p.id));
        }
        bool DrawXiltharChoice(PendingChoice choice)
        {
            if(choice.kind!="BeaconMode"&&choice.kind!="BeaconDebt")return false;
            if(Game.ActingPlayer.isAI){GrayboxUI.Text(left,"La IA está eligiendo.",12,210,214,50,15,GrayboxUI.Muted);return true;}
            var unit=BoardManager.Pieces(Game.State).FirstOrDefault(p=>p.id==choice.sourceId);
            var card=choice.kind=="BeaconDebt"?unit==null?null:Game.Data(unit):Game.Catalog[Game.State.players[choice.owner].discardPile.First(c=>c.instanceId==choice.cardRefId).cardId];
            int cost=card==null?0:card.energyCost+2+(choice.kind=="BeaconMode"?Game.State.players[choice.owner].opponentCardTax:0);
            var options=ChoiceManager.Targets(Game,choice);
            GrayboxUI.Button(left,choice.kind=="BeaconDebt"?"Dejar morir":"Recast "+Game.State.players[choice.owner].opponentCardTax+" E · diferido",12,189,214,34,()=>ChoiceManager.Resolve(Game,new[]{0}),null,options.Contains(0));
            GrayboxUI.Button(left,"Pagar "+cost+" Energía",12,231,214,34,()=>ChoiceManager.Resolve(Game,new[]{1}),null,options.Contains(1));
            if(choice.optional)GrayboxUI.Button(left,"Omitir",12,279,214,32,()=>ChoiceManager.Resolve(Game,new int[0],true));
            return true;
        }
    }
}
