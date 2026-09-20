using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void DrawHand(Player player)
        {
            GrayboxUI.Text(hand,"MANO J"+(player.id+1)+" · "+player.hand.Count+" CARTAS · "+handFilter.ToUpperInvariant(),12,9,470,23,14,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            GrayboxUI.Button(hand,"Todas",498,5,65,27,()=>FilterHand("Todas"));
            GrayboxUI.Button(hand,useResources?"Pago mixto":"Solo energía",570,5,126,27,()=>{useResources=!useResources;Render();},null,!Game.ActingPlayer.isAI);
            GrayboxUI.Button(hand,"Registro",707,5,89,27,ShowLog);GrayboxUI.Button(hand,"Guardar",802,5,89,27,Save);GrayboxUI.Button(hand,"Cargar",897,5,89,27,Load);
            var cards=player.hand.Where(c=>HandMatches(Game.Catalog[c.cardId],c)).ToList();int pages=Math.Max(1,(cards.Count+5)/6);page=Math.Clamp(page,0,pages-1);
            GrayboxUI.Button(hand,"<",1030,5,35,27,()=>{page=Math.Max(0,page-1);Render();});GrayboxUI.Text(hand,(page+1)+" / "+pages,1073,9,80,22,13);GrayboxUI.Button(hand,">",1157,5,35,27,()=>{page=Math.Min(pages-1,page+1);Render();});
            int index=0;
            foreach(var instance in cards.Skip(page*6).Take(6))
            {
                var card=Game.Catalog[instance.cardId];int id=instance.instanceId;
                var view=CardPresentation.Draw(hand,Game,card,player,12+index*208,38,198,201,useResources,false,
                    ()=>SelectCard(instance,player),()=>ShowTooltip(card,player),HideTooltip);
                if(player.id!=Game.ActingPlayerId||Game.CardBlockReason(instance,useResources)!="")view.gameObject.AddComponent<CanvasGroup>().alpha=.55f;
                if(id==selectedCard)GrayboxUI.Box(view,"Selección",0,0,198,5,Color.white);
                index++;
            }
            if(cards.Count==0)GrayboxUI.Text(hand,player.hand.Count==0?"Mano vacía. El robo se resuelve al comenzar tu turno.":"No hay cartas de este tipo disponibles en esta etapa.",16,97,1215,51,22,GrayboxUI.Muted);
        }
        bool HandMatches(CardData c,CardInstance instance)
        {
            if(handFilter=="Todas")return true;
            if(viewedPlayer!=Game.ActingPlayerId||Game.CardBlockReason(instance,useResources)!="")return false;
            return handFilter=="Permitidas"||handFilter=="Unidades"&&c.category!=Category.Spell&&!c.IsStructure||handFilter=="Estructuras"&&c.IsStructure||handFilter=="Magias"&&c.category==Category.Spell;
        }
        void SelectCard(CardInstance instance,Player player)
        {
            if(Game.ActingPlayer.isAI){ShowCardModal(Game.Catalog[instance.cardId],player);return;}
            ClearAction();selectedCard=instance.instanceId;mode="card";chosenBiome=Biome.Forest;
            Game.Notify(viewedPlayer==Game.ActingPlayerId?"Selecciona un objetivo válido.":"Consulta de la mano de J"+(player.id+1)+".");
            if(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed)ShowCardModal(Game.Catalog[instance.cardId],player);
        }
        void ShowTooltip(CardData card,Player player)
        {
            if(Game.State.phase==Phase.Finished)return;HideTooltip();
            tooltip=CardPresentation.Draw(root,Game,card,player,847,81,388,624,useResources,true);
            tooltip.name="Vista ampliada (solo lectura)";tooltip.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false;tooltip.SetAsLastSibling();
        }
        void HideTooltip(){if(tooltip!=null){tooltip.gameObject.SetActive(false);Destroy(tooltip.gameObject);tooltip=null;}}
        void ShowCardModal(CardData card,Player player)
        {
            HideTooltip();var body=OpenModal("Carta · "+card.name);
            CardPresentation.Draw(body,Game,card,player,28,83,392,627,useResources,true,piece:selectedPiece!=null&&selectedPiece.cardId==card.id?selectedPiece:null);
            GrayboxUI.Text(body,TimingRules.Description(card),451,126,639,133,24,GrayboxUI.Muted);
            GrayboxUI.Text(body,EnergyManager.Quote(player,card,useResources).Detail,451,285,639,78,25,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            var instance=player.hand.FirstOrDefault(c=>c.instanceId==selectedCard&&c.cardId==card.id)??player.hand.FirstOrDefault(c=>c.cardId==card.id);
            if(instance!=null&&player.id==Game.ActingPlayerId&&!player.isAI)
            {
                string why=Game.CardBlockReason(instance,useResources);GrayboxUI.Text(body,why,451,394,623,87,18,GrayboxUI.Yellow);
                GrayboxUI.Button(body,"Seleccionar objetivos",451,530,623,61,()=>{CloseModal();ClearAction();selectedCard=instance.instanceId;mode="card";Render();},null,why=="");
            }
        }
        void DrawPiles(Player player)
        {
            GrayboxUI.Text(piles,"MAZO / DESCARTE · J"+(player.id+1),12,10,270,25,14,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            DrawPile(player,false,14);DrawPile(player,true,155);
            GrayboxUI.Text(piles,player.pendingDraw>0?"Robo pendiente: "+player.pendingDraw:"Robo automático al comenzar tu turno",12,216,266,26,11,GrayboxUI.Muted);
        }
        void DrawPile(Player player,bool discard,float x)
        {
            var list=discard?player.discardPile:player.deck;var color=GrayboxUI.PlayerColor(player.id);
            for(int layer=2;layer>0;layer--)GrayboxUI.Box(piles,"Pila",x+layer*3,44+layer*3,116,160,Color.Lerp(color,GrayboxUI.Panel,.7f));
            var top=GrayboxUI.Box(piles,discard?"Descarte":"Mazo",x,44,116,160,Color.Lerp(color,GrayboxUI.Panel,.6f));
            var button=top.gameObject.AddComponent<UnityEngine.UI.Button>();button.onClick.AddListener(()=>{if(!discard&&player.id==Game.ActingPlayerId&&!player.isAI&&player.pendingDraw>0)Game.DrawPending();else ShowPile(player,discard,0);});
            var title=GrayboxUI.Text(top,discard?"DESCARTE":"MAZO",5,11,106,22,13,GrayboxUI.Ink,FontStyle.Bold);title.alignment=TextAnchor.MiddleCenter;
            var name=GrayboxUI.Text(top,discard?(list.Count>0?Game.Catalog[list.Last().cardId].name:"Vacío"):"WAR\n&\nCONQUER",8,44,100,70,discard?14:17,GrayboxUI.Ink,FontStyle.Bold);name.alignment=TextAnchor.MiddleCenter;
            var count=GrayboxUI.Text(top,"× "+list.Count,5,121,106,32,24,color,FontStyle.Bold);count.alignment=TextAnchor.MiddleCenter;
            if(discard&&list.Count>0){var card=Game.Catalog[list.Last().cardId];var hover=top.gameObject.AddComponent<CardHover>();hover.enter=()=>ShowTooltip(card,player);hover.exit=HideTooltip;}
        }
        void ShowPile(Player player,bool discard,int pilePage)
        {
            HideTooltip();var body=OpenModal("J"+(player.id+1)+" · "+(discard?"Descarte":"Mazo restante"));
            // The draw order stays hidden. Discard cards retain their actual most-recent-first order.
            var cards=(discard?player.discardPile.AsEnumerable().Reverse():player.deck.OrderBy(c=>c.cardId)).ToList();
            int pages=Math.Max(1,(cards.Count+9)/10);pilePage=Math.Clamp(pilePage,0,pages-1);int index=0;
            foreach(var instance in cards.Skip(pilePage*10).Take(10))
            {
                var card=Game.Catalog[instance.cardId];CardPresentation.Draw(body,Game,card,player,24+(index%5)*217,82+(index/5)*311,205,297,false,false,()=>ShowCardModal(card,player),()=>ShowTooltip(card,player),HideTooltip);index++;
            }
            if(cards.Count==0)GrayboxUI.Text(body,"No hay cartas en esta pila.",30,140,1040,60,27,GrayboxUI.Muted);
            GrayboxUI.Button(body,"Anterior",24,727,210,43,()=>ShowPile(player,discard,pilePage-1),null,pilePage>0);
            GrayboxUI.Text(body,(pilePage+1)+" / "+pages+" · "+cards.Count+" cartas",280,736,460,32,20,GrayboxUI.Muted);
            GrayboxUI.Button(body,"Siguiente",868,727,210,43,()=>ShowPile(player,discard,pilePage+1),null,pilePage<pages-1);
        }
    }
}
