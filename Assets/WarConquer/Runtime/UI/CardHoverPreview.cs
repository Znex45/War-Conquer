using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public sealed class CardHoverPreview : MonoBehaviour
    {
        const float Width = 552, Height = 790, Gap = 12;
        const float Duration = .14f;
        readonly Vector3[] corners = new Vector3[4];
        RectTransform panel, host, source;
        CanvasGroup visibility;
        CardPresentation.Face face;
        float shownAt;

        public static CardHoverPreview Create(RectTransform host)
        {
            var node = GrayboxUI.Box(host, "Vista ampliada (solo lectura)", 0, 0, Width, Height, GrayboxUI.Panel);
            var preview = node.gameObject.AddComponent<CardHoverPreview>();
            preview.host = host;
            preview.panel = node;
            preview.visibility = node.gameObject.AddComponent<CanvasGroup>();
            preview.visibility.blocksRaycasts = false;
            preview.visibility.interactable = false;
            node.GetComponent<Image>().raycastTarget = false;
            preview.face = new CardPresentation.Face(node, Width, Height, true);
            node.gameObject.SetActive(false);
            return preview;
        }

        public static void Attach(RectTransform cardView, CardHoverPreview preview, GameManager game, CardData card,
            Player player, bool resources = false, Piece piece = null, int? recastEnergy = null)
        {
            var hover = cardView.gameObject.AddComponent<CardHover>();
            // An independent overlay preserves the clickable footprint and existing selection outline.
            var highlight = GrayboxUI.Box(cardView, "Realce de carta", 0, 0, cardView.rect.width, 3, GrayboxUI.Yellow);
            highlight.GetComponent<Image>().raycastTarget = false;
            var highlightGroup = highlight.gameObject.AddComponent<CanvasGroup>();
            highlightGroup.ignoreParentGroups = true;
            highlightGroup.blocksRaycasts = false;
            highlight.gameObject.SetActive(false);
            hover.enter = () =>
            {
                highlight.gameObject.SetActive(true);
                preview.Show(cardView, game, card, player, resources, piece, recastEnergy);
            };
            hover.exit = () =>
            {
                if (highlight != null) highlight.gameObject.SetActive(false);
                if (preview != null && preview.source == cardView) preview.Hide();
            };
        }

        void Show(RectTransform cardView, GameManager game, CardData card, Player player, bool resources,
            Piece piece, int? recastEnergy)
        {
            source = cardView;
            face.Bind(game, card, player, resources, piece, recastEnergy);
            transform.SetAsLastSibling();
            shownAt = Time.unscaledTime;
            visibility.alpha = 0;
            panel.localScale = Vector3.one * .98f;
            Position();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            source = null;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (source == null || !source.gameObject.activeInHierarchy)
            {
                Hide();
                return;
            }
            float progress = Mathf.Clamp01((Time.unscaledTime - shownAt) / Duration);
            visibility.alpha = progress;
            panel.localScale = Vector3.one * Mathf.Lerp(.98f, 1, progress);
            Position();
        }

        void Position()
        {
            source.GetWorldCorners(corners);
            var bottom = host.InverseTransformPoint(corners[0]);
            var top = host.InverseTransformPoint(corners[1]);
            var right = host.InverseTransformPoint(corners[3]);
            var area = host.rect;
            panel.anchoredPosition = Place(area, Rect.MinMaxRect(bottom.x, bottom.y, right.x, top.y));
        }

        public static Vector2 Place(Rect area, Rect card)
        {
            float x = card.xMin - Width - Gap;
            float top = card.yMax;
            if (x < area.xMin + Gap)
            {
                x = card.xMax + Gap;
                if (x + Width > area.xMax - Gap)
                {
                    x = card.center.x - Width / 2;
                    top = card.yMax + Gap + Height;
                }
            }
            x = Mathf.Clamp(x, area.xMin + Gap, area.xMax - Width - Gap);
            top = Mathf.Clamp(top, area.yMin + Height + Gap, area.yMax - Gap);
            return new Vector2(x - area.xMin, top - area.yMax);
        }
    }
}
