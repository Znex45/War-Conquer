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
            if(DrawXiltharChoice(choice))return;
            if(choice.kind=="DeathTerraform"&&!Game.ActingPlayer.isAI)
            {
                int i=0;foreach(int biome in ChoiceManager.Targets(Game,choice))
                {int selected=biome;GrayboxUI.Button(left,Names.Biomes[biome],12+(i%2)*108,168+(i/2)*34,104,30,()=>ChoiceManager.Resolve(Game,new[]{selected}));i++;}
                if(i==0)GrayboxUI.Button(left,"Continuar",12,220,214,34,()=>ChoiceManager.Resolve(Game,Array.Empty<int>()));return;
            }
            GrayboxUI.Text(left,Game.ActingPlayer.isAI?"La IA está resolviendo.":choice.kind=="Discard"?"Selecciona cartas en el panel.":"Selecciona una casilla resaltada en el tablero.",12,195,214,62,14,GrayboxUI.Muted);
            if(choice.optional&&!Game.ActingPlayer.isAI)GrayboxUI.Button(left,"Omitir efecto opcional",12,280,214,34,()=>ChoiceManager.Resolve(Game,Array.Empty<int>(),true));
        }
        void ShowDiscardChoice()
        {
            var choice = Game.State.pendingChoices[0];
            if (displayedChoice != choice) { discardSelection.Clear(); discardPage = 0; displayedChoice = choice; }
            var player = Game.State.players[choice.owner]; var cards = choice.kind == "DarknessPick" ? XiltharRules.DarknessCards(Game, choice.owner).ToList() : player.hand; int required = Math.Min(choice.count, cards.Count); discardSelection.RemoveAll(id => !cards.Any(c => c.instanceId == id));
            var body = OpenModal(choice.kind == "DarknessPick" ? "ELEGIR DEL DESCARTE" : "DESCARTAR CARTAS", false);
            // Elección obligatoria: marco y banner, sin botón CERRAR.
            SkinSetupFrame(body);
            var overlay = modal.GetComponent<UnityEngine.UI.Image>(); if (overlay != null) overlay.color = new Color(0, 0, 0, .75f);
            var prompt = GrayboxUI.Text(body, choice.prompt, 26, 82, 1070, 36, 24, SkinGold, FontStyle.Bold); prompt.alignment = TextAnchor.MiddleCenter; SkinTextShadow(prompt);
            int pages = Math.Max(1, (cards.Count + 7) / 8); discardPage = Math.Clamp(discardPage, 0, pages - 1);
            int index = 0;
            foreach (var instance in cards.Skip(discardPage * 8).Take(8))
            {
                int id = instance.instanceId; var card = Game.Catalog[instance.cardId]; float x = 78 + (index % 4) * 265, y = 128 + (index / 4) * 260;
                var view = CardPresentation.Draw(body, Game, card, player, x, y, 187, 250, false, false, () => {
                    if (discardSelection.Contains(id)) discardSelection.Remove(id); else if (discardSelection.Count < choice.count) discardSelection.Add(id); ShowDiscardChoice();
                }, () => { if (choice.kind == "DarknessPick") ShowRecastTooltip(card, player); else ShowTooltip(card, player); }, HideTooltip, recastEnergy: choice.kind == "DarknessPick" ? (int?)XiltharRules.RecastCost(Game, player.id, card, true) : null);
                if (discardSelection.Contains(id))
                {
                    SkinOutline(view, SkinGold, 4f);
                    var mark = GrayboxUI.Box(view, "Carta para descartar", 0, 0, 187, 8, GrayboxUI.Green); mark.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                }
                index++;
            }
            SkinChoiceButton(body, "Anterior", 26, 656, 200, 46, () => { if (discardPage > 0) { discardPage--; ShowDiscardChoice(); } }, 20, false, SkinGold);
            var counter = GrayboxUI.Text(body, (discardPage + 1) + " / " + pages + " · Seleccionadas " + discardSelection.Count + " / " + choice.count, 260, 660, 600, 38, 22, SkinCream, FontStyle.Bold); counter.alignment = TextAnchor.MiddleCenter; SkinTextShadow(counter);
            SkinChoiceButton(body, "Siguiente", 896, 656, 200, 46, () => { if (discardPage < pages - 1) { discardPage++; ShowDiscardChoice(); } }, 20, false, SkinGold);
            SkinChoiceButton(body, choice.kind == "DarknessPick" ? "ELEGIR UNIT DEL DESCARTE" : "CONFIRMAR DESCARTE", 26, 712, 1070, 58, () => {
                if (discardSelection.Count != required) return;
                var ids = discardSelection.ToArray(); CloseModal(); ChoiceManager.Resolve(Game, ids); discardSelection.Clear(); displayedChoice = null; Render();
            }, 26, discardSelection.Count == required, SkinGold);
        }
    }
}
