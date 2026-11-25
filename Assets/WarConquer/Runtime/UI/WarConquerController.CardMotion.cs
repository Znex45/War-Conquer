using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public partial class WarConquerController
    {
        readonly List<CardFlight> flights = new List<CardFlight>();

        sealed class CardFlight
        {
            public RectTransform view, face, back;
            public CanvasGroup group;
            public Vector2 from, to;
            public float start, duration;
            public CardFlowKind kind;
        }

        void LaunchCard(CardFlow flow, Vector2 from, Vector2 to, float delay, CardFlowKind? kind = null)
        {
            if (flights.Count >= 24) return;

            var motion = kind ?? flow.Kind;
            const float width = 198, height = 201;
            var view = GrayboxUI.Rect(animationLayer, "Carta animada · " + motion, from.x, from.y, width, height);
            view.pivot = new Vector2(.5f, .5f);
            var group = view.gameObject.AddComponent<CanvasGroup>();
            group.interactable = group.blocksRaycasts = false;
            group.alpha = 0;

            var face = GrayboxUI.Box(view, "Frente", 0, 0, width, height, SkinPanel);
            var presentation = new CardPresentation.Face(face, width, height, false);
            presentation.Bind(Game, Game.Catalog[flow.CardId], Game.State.players[flow.Player], false, null, null);

            RectTransform back = null;
            if (motion == CardFlowKind.Draw)
            {
                face.gameObject.SetActive(false);
                back = GrayboxUI.Box(view, "Dorso", 0, 0, width, height, SkinPanel);
                var image = back.GetComponent<Image>();
                image.sprite = Resources.Load<Sprite>("UI/Cards/Card_Back") ?? CardUnitFrame.Frame;
                image.color = image.sprite != null ? Color.white : SkinPanel;
                image.preserveAspect = true;
            }

            flights.Add(new CardFlight
            {
                view = view, face = face, back = back, group = group,
                from = from, to = to, kind = motion,
                start = Time.unscaledTime + delay, duration = motion == CardFlowKind.Play ? .34f : .55f
            });
            animationLayer.SetAsLastSibling();
        }

        void TickCardFlights(float now)
        {
            for (int i = flights.Count - 1; i >= 0; i--)
            {
                var flight = flights[i];
                if (now < flight.start) continue;

                float t = Mathf.Clamp01((now - flight.start) / flight.duration);
                if (flight.back != null && flight.back.gameObject.activeSelf && t >= .72f)
                {
                    flight.back.gameObject.SetActive(false);
                    flight.face.gameObject.SetActive(true);
                }
                float arc = Mathf.Sin(t * Mathf.PI);
                Vector2 p = Vector2.Lerp(flight.from, flight.to, Mathf.SmoothStep(0, 1, t));
                flight.view.anchoredPosition = new Vector2(p.x, -p.y + arc * 65f);
                flight.view.localRotation = Quaternion.Euler(0, 0, arc * -8f);
                flight.view.localScale = Vector3.one * Mathf.Lerp(.68f, .88f, arc);
                flight.group.alpha = Mathf.Min(t * 10, (1 - t) * 5);
                if (t < 1) continue;

                Destroy(flight.view.gameObject);
                flights.RemoveAt(i);
            }
        }
    }
}
