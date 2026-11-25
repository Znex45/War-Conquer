using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public partial class WarConquerController
    {
        const float RightWidth = 224;
        RectTransform activityPanel, historyContent, playedPanel, animationLayer;
        Text playedBy, playedInfo;
        ScrollRect playedDetails;
        Text actionToast;
        string lastActionMessage;
        float actionToastUntil;
        bool playedWaiting;
        Button historyButton;
        Text historyButtonLabel;
        GameState presentationState;
        PendingChoice acceptedChoice;
        CardFlow lastPlayed;
        PendingChoice playedChoice;
        RectTransform playedFace;
        float playedUntil;
        bool historyOpen;
        readonly Dictionary<int, Vector2> currentPositions = new Dictionary<int, Vector2>();
        readonly Dictionary<int, Vector2> cardCenters = new Dictionary<int, Vector2>();
        readonly List<CardFlight> flights = new List<CardFlight>();
        Vector2 deckCenter, discardCenter, handCenter;

        sealed class CardFlight
        {
            public RectTransform view;
            public CanvasGroup group;
            public Vector2 from, to;
            public float start, duration;
            public CardFlowKind kind;
            public Sprite reveal;
            public Image image;
        }

        void CreateActivityPanel()
        {
            activityPanel = GrayboxUI.Rect(matchRoot, "Actividad de partida", 1360, 76, RightWidth, 648);
            historyButton = GrayboxUI.Button(activityPanel, "REGISTRO", 0, 0, RightWidth, 34, ToggleHistory, SkinGold);
            historyButtonLabel = historyButton.GetComponentInChildren<Text>();
            SkinMatchButtons(activityPanel);

            historyContent = GrayboxUI.Box(activityPanel, "Registro plegable", 0, 40, RightWidth, 243, SkinPanel);
            liveMatchLog = new MatchLogList(historyContent, 4, 4, RightWidth - 20, 235, true);
            historyContent.gameObject.SetActive(false);

            playedPanel = GrayboxUI.Box(activityPanel, "Última carta jugada", 0, 40, RightWidth, 352, SkinPanel);
            playedPanel.GetComponent<Image>().raycastTarget = false;
            playedBy = GrayboxUI.Text(playedPanel, "", 8, 5, 208, 24, 14, SkinGold, FontStyle.Bold);
            var details = GrayboxUI.Box(playedPanel, "Descripción desplazable", 8, 240, 208, 104, SkinDark);
            details.gameObject.AddComponent<RectMask2D>();
            var content = GrayboxUI.Rect(details, "Descripción", 0, 0, 208, 104);
            playedInfo = GrayboxUI.Text(content, "", 5, 4, 198, 96, 13, SkinCream);
            playedInfo.resizeTextForBestFit = false;
            playedDetails = details.gameObject.AddComponent<ScrollRect>();
            playedDetails.viewport = details;
            playedDetails.content = content;
            playedDetails.horizontal = false;
            playedDetails.movementType = ScrollRect.MovementType.Clamped;
            playedDetails.scrollSensitivity = 25;
            playedPanel.gameObject.SetActive(false);

            animationLayer = GrayboxUI.Rect(root, "Animaciones de cartas", 0, 0, 1600, 1000);
            var group = animationLayer.gameObject.AddComponent<CanvasGroup>();
            group.interactable = group.blocksRaycasts = false;
        }

        void ToggleHistory()
        {
            historyOpen = !historyOpen;
            historyContent.gameObject.SetActive(historyOpen);
            historyButtonLabel.text = historyOpen ? "REGISTRO  −" : "REGISTRO  +";
            playedPanel.anchoredPosition = new Vector2(0, historyOpen ? -290 : -40);
            if (historyOpen) liveMatchLog.Sync(Game.State);
        }

        Vector2 CenterInRoot(RectTransform ui)
        {
            Vector3 center = root.InverseTransformPoint(ui.TransformPoint(ui.rect.center));
            return new Vector2(center.x - root.rect.xMin, root.rect.yMax - center.y);
        }

        void PrepareCardPresentation(GameState state)
        {
            currentPositions.Clear();
            if (presentationState == state) return;
            presentationState = state;
            acceptedChoice = playedChoice = null;
            lastPlayed = null;
            playedPanel.gameObject.SetActive(false);
            playedWaiting = false;
            lastActionMessage = null;
            GrayboxUI.Clear(animationLayer);
            flights.Clear();
            cardCenters.Clear();
        }

        void UpdateCardPresentation(GameState state, Player viewed)
        {
            int draws = 0, discards = 0;
            handCenter = CenterInRoot(hand);
            while (state.TryReadCardFlow(out var flow))
            {
                var source = cardCenters.TryGetValue(flow.Instance, out Vector2 previous) ? previous : handCenter;
                if (flow.Kind == CardFlowKind.Play)
                {
                    DisplayPlayedCard(flow, state);
                    var destination = CenterInRoot(playedFace);
                    LaunchCard(flow, flow.FromDiscard ? discardCenter : source, destination, 0);
                    if (Game.Catalog[flow.CardId].category == Category.Spell &&
                        flow.Player == viewed.id && viewed.discardPile.Any(card => card.instanceId == flow.Instance))
                        LaunchCard(flow, destination, discardCenter, .36f, CardFlowKind.Discard);
                }
                else if (flow.Player == viewed.id)
                {
                    if (flow.Kind == CardFlowKind.Draw)
                    {
                        Vector2 target = currentPositions.TryGetValue(flow.Instance, out Vector2 position) ? position : handCenter;
                        LaunchCard(flow, deckCenter, target, Mathf.Min(draws++, 10) * .065f);
                    }
                    else
                    {
                        if (flow.SourceTile >= 0)
                        {
                            var uv = board.World.ViewportOf(flow.SourceTile);
                            Vector2 middle = CenterInRoot(boardPanel);
                            source = middle + new Vector2((uv.x - .5f) * BoardView.Width, (.5f - uv.y) * BoardView.ViewportHeight);
                        }
                        LaunchCard(flow, source, discardCenter, Mathf.Min(discards++, 10) * .075f);
                    }
                }
            }
            cardCenters.Clear();
            foreach (var entry in currentPositions) cardCenters[entry.Key] = entry.Value;
        }

        void DisplayPlayedCard(CardFlow flow, GameState state)
        {
            lastPlayed = flow;
            playedChoice = state.pendingChoices.FirstOrDefault();
            playedUntil = Time.unscaledTime + 5f;
            if (playedFace != null)
            {
                playedFace.gameObject.SetActive(false);
                Destroy(playedFace.gameObject);
            }
            var player = state.players[flow.Player];
            var card = Game.Catalog[flow.CardId];
            playedBy.text = PlayerLabel(flow.Player) + " · CARTA JUGADA";
            playedFace = CardPresentation.Draw(playedPanel, Game, card, player, 17, 31, 190, 205);
            playedPanel.gameObject.SetActive(true);
            playedWaiting = IsPlayedCardWaiting();
            SetPlayedInfo();
        }

        bool IsPlayedCardWaiting()
        {
            var state = Game.State;
            return (lastPlayed.Battle != null && lastPlayed.Battle == state.battle) ||
                (playedChoice != null && state.pendingChoices.Count > 0);
        }

        void SetPlayedInfo()
        {
            var card = Game.Catalog[lastPlayed.CardId];
            string status = playedWaiting && Game.State.battle == lastPlayed.Battle && lastPlayed.Battle != null
                ? "Esperando respuesta\n"
                : playedWaiting ? "Elección pendiente\n" : "";
            playedInfo.text = status + "Pago: " + lastPlayed.Payment + "\n" + CardReading.Effect(card);
            float textHeight = Mathf.Ceil(playedInfo.preferredHeight) + 4;
            playedInfo.rectTransform.sizeDelta = new Vector2(198, textHeight);
            playedDetails.content.sizeDelta = new Vector2(208, Mathf.Max(104, textHeight + 8));
            playedDetails.StopMovement();
            playedDetails.verticalNormalizedPosition = 1;
        }

        void LaunchCard(CardFlow flow, Vector2 from, Vector2 to, float delay, CardFlowKind? kind = null)
        {
            var mode = kind ?? flow.Kind;
            if (flights.Count >= 24) return;
            var node = GrayboxUI.Box(animationLayer, "Carta animada · " + mode, from.x, from.y, 78, 104, SkinPanel);
            node.pivot = new Vector2(.5f, .5f);
            var image = node.GetComponent<Image>();
            image.raycastTarget = false;
            var sprite = mode == CardFlowKind.Draw ? Resources.Load<Sprite>("UI/Cards/Card_Back") : CardUnitFrame.Art(flow.CardId);
            image.sprite = sprite != null ? sprite : CardUnitFrame.Frame;
            image.color = Color.white;
            image.preserveAspect = true;
            if (mode != CardFlowKind.Draw)
            {
                var name = GrayboxUI.Text(node, Game.Catalog[flow.CardId].name, 3, 69, 72, 32, 11, SkinCream, FontStyle.Bold);
                name.alignment = TextAnchor.MiddleCenter;
            }
            var canvas = node.gameObject.AddComponent<CanvasGroup>();
            canvas.interactable = canvas.blocksRaycasts = false;
            canvas.alpha = 0;
            flights.Add(new CardFlight { view = node, group = canvas, from = from, to = to, kind = mode,
                image = image, reveal = mode == CardFlowKind.Draw ? CardUnitFrame.Art(flow.CardId) : null,
                start = Time.unscaledTime + delay, duration = mode == CardFlowKind.Draw ? .42f : .34f });
            animationLayer.SetAsLastSibling();
        }

        void TickCardPresentation()
        {
            float now = Time.unscaledTime;
            for (int i = flights.Count - 1; i >= 0; i--)
            {
                var flight = flights[i];
                if (now < flight.start) continue;
                float t = Mathf.Clamp01((now - flight.start) / flight.duration);
                float ease = Mathf.SmoothStep(0, 1, t);
                if (flight.reveal != null && t >= .72f)
                {
                    flight.image.sprite = flight.reveal;
                    flight.reveal = null;
                }
                Vector2 p = Vector2.Lerp(flight.from, flight.to, ease);
                p.y -= Mathf.Sin(t * Mathf.PI) * 24;
                flight.view.anchoredPosition = new Vector2(p.x, -p.y);
                flight.view.localScale = Vector3.one * (flight.kind == CardFlowKind.Discard ? Mathf.Lerp(1, .65f, t) : 1 + Mathf.Sin(t * Mathf.PI) * .12f);
                flight.group.alpha = Mathf.Min(t * 10, (1 - t) * 5);
                if (t < 1) continue;
                Destroy(flight.view.gameObject);
                flights.RemoveAt(i);
            }
            if (actionToast != null && now >= actionToastUntil)
                actionToast.transform.parent.gameObject.SetActive(false);
            if (lastPlayed == null) return;
            bool waiting = IsPlayedCardWaiting();
            if (waiting != playedWaiting)
            {
                playedWaiting = waiting;
                if (!waiting)
                {
                    playedChoice = null;
                    playedUntil = Mathf.Max(playedUntil, now + 2f);
                }
                SetPlayedInfo();
            }
            bool visible = waiting || now < playedUntil;
            if (playedPanel.gameObject.activeSelf != visible) playedPanel.gameObject.SetActive(visible);
        }

        string PlayerLabel(int id) => "J" + (id + 1) + " " + Game.State.players[id].leader;

        void DrawActionContext(RectTransform chrome)
        {
            var state = Game.State;
            actionToast = null;
            if (state.battle == null && state.responsePlayer < 0)
            {
                if (lastActionMessage != Game.LastMessage)
                {
                    lastActionMessage = Game.LastMessage;
                    actionToastUntil = Time.unscaledTime + 3.5f;
                }
                if (string.IsNullOrEmpty(lastActionMessage) || Time.unscaledTime >= actionToastUntil) return;
                bool unavailable = lastActionMessage.Contains("Panther") &&
                    state.log.Skip(Mathf.Max(0, state.log.Count - 3)).Any(entry => entry.Contains("Panther sin objetivos válidos"));
                var note = GrayboxUI.Box(chrome, "Acción actual", 12, 44, BoardView.Width - 24, 30, SkinDark);
                note.GetComponent<Image>().raycastTarget = false;
                actionToast = GrayboxUI.Text(note, unavailable ? "Panther · Sin objetivos válidos. Puedes continuar." : lastActionMessage,
                    8, 2, BoardView.Width - 40, 26, 13, SkinCream);
                return;
            }

            var banner = GrayboxUI.Box(chrome, "Ventana de respuesta", 12, 45, BoardView.Width - 24, 90, SkinDark);
            banner.GetComponent<Image>().raycastTarget = false;
            GrayboxUI.Text(banner, "VENTANA DE RESPUESTA", 9, 3, BoardView.Width - 43, 18, 13, SkinGold, FontStyle.Bold);
            string text;
            if (state.battle != null)
            {
                var battle = state.battle;
                var source = BoardManager.Pieces(state).FirstOrDefault(piece => piece.id == battle.attackerId);
                var target = BoardManager.Pieces(state).FirstOrDefault(piece => piece.id == battle.victimId);
                string attack = source == null ? "Ataque" : Game.Data(source).name + " ataca";
                string victim = target == null ? (battle.victimId < 0 ? "base" : "objetivo original") : Game.Data(target).name;
                text = PlayerLabel(battle.attackerOwner) + " · " + attack + " → " + victim + " de " + PlayerLabel(battle.defenderOwner) +
                    " · HEX " + (battle.targetTile + 1);
                if (lastPlayed != null && lastPlayed.Battle == battle)
                    text += "\n" + PlayerLabel(lastPlayed.Player) + " responde con " + Game.Catalog[lastPlayed.CardId].name + " · " + lastPlayed.Targets;
            }
            else
            {
                text = "Intervención durante " + TimingRules.StageName(state.stage) + " de " + PlayerLabel(state.activePlayer);
                text += "\n" + PlayerLabel(state.responsePlayer) + " tiene permiso para intervenir.";
            }
            GrayboxUI.Text(banner, text, 9, 23, BoardView.Width - 43, 42, 13, SkinCream);
            bool thirdParty = state.battle != null && Game.ActingPlayerId != state.battle.attackerOwner &&
                Game.ActingPlayerId != state.battle.defenderOwner;
            string available = state.pendingChoices.Count > 0 ? "resuelve la elección" :
                thirdParty ? "puede intervenir o pasar" : "puede responder o pasar";
            GrayboxUI.Text(banner, "Prioridad: " + PlayerLabel(Game.ActingPlayerId) + " · " + available,
                9, 66, BoardView.Width - 43, 20, 14, SkinGold, FontStyle.Bold);
        }
    }
}
