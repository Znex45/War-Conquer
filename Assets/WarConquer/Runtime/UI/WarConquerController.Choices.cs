using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        readonly List<int> discardSelection=new List<int>();
        PendingChoice displayedChoice;
        int discardPage;
        void DrawPendingChoice()
        {
            var choice=Game.State.pendingChoices[0];
            GrayboxUI.Text(left,"ELECCIÓN · J"+(choice.owner+1),12,12,214,28,18,GrayboxUI.Green,FontStyle.Bold);
            GrayboxUI.Text(left,choice.prompt,12,62,214,125,17,GrayboxUI.Ink);
            if(ChoiceManager.Targets(Game,choice).Count==0&&!Game.ActingPlayer.isAI)
            {GrayboxUI.Button(left,"Sin objetivos · continuar",12,220,214,34,()=>ChoiceManager.Resolve(Game,Array.Empty<int>()));return;}
            if(choice.kind=="DeathTerraform"&&!Game.ActingPlayer.isAI)
            {
                int i=0;foreach(int biome in ChoiceManager.Targets(Game,choice))
                {int selected=biome;GrayboxUI.Button(left,Names.Biomes[biome],12+(i%2)*108,180+(i/2)*34,104,30,()=>ChoiceManager.Resolve(Game,new[]{selected}));i++;}
                if(i==0)GrayboxUI.Button(left,"Continuar",12,220,214,34,()=>ChoiceManager.Resolve(Game,Array.Empty<int>()));return;
            }
            GrayboxUI.Text(left,Game.ActingPlayer.isAI?"La IA está resolviendo.":choice.kind=="Discard"?"Selecciona cartas en el panel.":"Selecciona una casilla resaltada en el tablero.",12,195,214,62,14,GrayboxUI.Muted);
            if(choice.optional&&!Game.ActingPlayer.isAI)GrayboxUI.Button(left,"Omitir efecto opcional",12,280,214,34,()=>ChoiceManager.Resolve(Game,Array.Empty<int>(),true));
        }
        void ShowDiscardChoice()
        {
            var choice=Game.State.pendingChoices[0];
            if(displayedChoice!=choice){discardSelection.Clear();discardPage=0;displayedChoice=choice;}
            var player=Game.State.players[choice.owner];int required=Math.Min(choice.count,player.hand.Count);discardSelection.RemoveAll(id=>!player.hand.Any(c=>c.instanceId==id));
            var body=OpenModal(choice.prompt,false);
            int pages=Math.Max(1,(player.hand.Count+7)/8);discardPage=Math.Clamp(discardPage,0,pages-1);
            int index=0;
            foreach(var instance in player.hand.Skip(discardPage*8).Take(8))
            {
                int id=instance.instanceId;var card=Game.Catalog[instance.cardId];float x=28+(index%4)*270,y=84+(index/4)*276;
                var view=CardPresentation.Draw(body,Game,card,player,x,y,254,264,false,false,()=>{
                    if(discardSelection.Contains(id))discardSelection.Remove(id);else if(discardSelection.Count<choice.count)discardSelection.Add(id);ShowDiscardChoice();},()=>ShowTooltip(card,player),HideTooltip);
                if(discardSelection.Contains(id))GrayboxUI.Box(view,"Carta para descartar",0,0,254,7,GrayboxUI.Green);
                index++;
            }
            GrayboxUI.Button(body,"Anterior",28,650,155,34,()=>{discardPage--;ShowDiscardChoice();},null,discardPage>0);
            GrayboxUI.Text(body,(discardPage+1)+" / "+pages+" · Seleccionadas "+discardSelection.Count+" / "+choice.count,225,657,665,30,18,GrayboxUI.Ink);
            GrayboxUI.Button(body,"Siguiente",938,650,155,34,()=>{discardPage++;ShowDiscardChoice();},null,discardPage<pages-1);
            GrayboxUI.Button(body,"CONFIRMAR DESCARTE",28,712,1065,52,()=>{
                var ids=discardSelection.ToArray();CloseModal();ChoiceManager.Resolve(Game,ids);discardSelection.Clear();displayedChoice=null;Render();},GrayboxUI.Green,discardSelection.Count==required);
        }
    }
}
