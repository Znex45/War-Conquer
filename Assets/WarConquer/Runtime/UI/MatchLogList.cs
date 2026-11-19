using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    // Uses the existing match log for both the live panel and the expanded history.
    public sealed class MatchLogList
    {
        static readonly Color Background = new Color32(12, 22, 28, 235);
        static readonly Color RowColor = new Color32(19, 31, 39, 255);
        static readonly Color Gold = new Color32(245, 203, 76, 255);

        sealed class Row
        {
            public RectTransform root;
            public Text stamp, message;
        }

        readonly ScrollRect scroll;
        readonly RectTransform content;
        readonly Text empty;
        readonly List<Row> rows = new List<Row>();
        readonly List<string> previousEntries = new List<string>();
        readonly float width, height;
        readonly bool compact;
        GameState state;

        public MatchLogList(Transform parent, float x, float y, float width, float height, bool compact)
        {
            this.width = width;
            this.height = height;
            this.compact = compact;

            var viewport = GrayboxUI.Box(parent, "Eventos de partida", x, y, width, height, Background);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = GrayboxUI.Rect(viewport, "Entradas del historial", 0, 0, width, height);
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = compact ? 34 : 48;

            var track = GrayboxUI.Box(parent, "Barra del historial", x + width + 4, y, 8, height, Background);
            var handle = GrayboxUI.Box(track, "Control", 0, 0, 8, 30, Gold);
            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = handle.GetComponent<Image>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            empty = GrayboxUI.Text(content, "Todavía no hay acciones registradas.", 12, 6, width - 24, 26,
                compact ? 14 : 20, GrayboxUI.Muted);
        }

        public void Sync(GameState current)
        {
            var entries = current.log;
            if (ReferenceEquals(state, current) && SameEntries(entries)) return;

            // Follow new events at the top. Keep readers who scrolled down on the same entry.
            bool follow = !ReferenceEquals(state, current) || content.anchoredPosition.y <= 1f;
            float previousY = content.anchoredPosition.y;
            string previousTop = previousEntries.Count > 0 ? previousEntries[previousEntries.Count - 1] : null;
            float newEntriesHeight = 0;
            bool foundPreviousTop = false;
            float rowY = 4;
            for (int index = 0; index < entries.Count; index++)
            {
                string entry = entries[entries.Count - 1 - index];
                Row row = index < rows.Count ? rows[index] : AddRow();
                row.root.gameObject.SetActive(true);
                SetEntry(row, entry);
                row.root.anchoredPosition = new Vector2(4, -rowY);
                float occupied = row.root.rect.height + 4;
                rowY += occupied;
                if (!foundPreviousTop && entry == previousTop) foundPreviousTop = true;
                if (!foundPreviousTop) newEntriesHeight += occupied;
            }
            for (int index = entries.Count; index < rows.Count; index++)
                rows[index].root.gameObject.SetActive(false);

            empty.gameObject.SetActive(entries.Count == 0);
            content.sizeDelta = new Vector2(width, Mathf.Max(height, rowY));
            scroll.StopMovement();
            float offset = follow ? 0 : previousY + (foundPreviousTop ? newEntriesHeight : 0);
            content.anchoredPosition = new Vector2(0, Mathf.Clamp(offset, 0, content.rect.height - height));
            state = current;
            previousEntries.Clear();
            previousEntries.AddRange(entries);
        }

        bool SameEntries(List<string> entries)
        {
            if (entries.Count != previousEntries.Count) return false;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != previousEntries[i]) return false;
            return true;
        }

        Row AddRow()
        {
            var root = GrayboxUI.Box(content, "Entrada", 4, 0, width - 8, 30, RowColor);
            root.GetComponent<Image>().raycastTarget = false;
            float textWidth = width - 30;
            var stamp = GrayboxUI.Text(root, "", 10, 4, textWidth, compact ? 17 : 23, compact ? 12 : 14, Gold, FontStyle.Bold);
            var message = GrayboxUI.Text(root, "", 10, 4, textWidth, 30, compact ? 15 : 19, GrayboxUI.Ink);
            stamp.resizeTextForBestFit = message.resizeTextForBestFit = false;
            stamp.supportRichText = message.supportRichText = false;
            var row = new Row { root = root, stamp = stamp, message = message };
            rows.Add(row);
            return row;
        }

        void SetEntry(Row row, string entry)
        {
            int separator = entry.IndexOf('\n');
            float top = 4;
            row.stamp.gameObject.SetActive(separator >= 0);
            if (separator >= 0)
            {
                row.stamp.text = entry.Substring(0, separator);
                top += row.stamp.rectTransform.rect.height;
            }
            string body = separator < 0 ? entry : entry.Substring(separator + 1);
            row.message.text = compact ? body.Replace("\n", " · ") : body;
            row.message.rectTransform.anchoredPosition = new Vector2(10, -top);
            float textHeight = Mathf.Ceil(row.message.preferredHeight) + 2;
            row.message.rectTransform.sizeDelta = new Vector2(width - 30, textHeight);
            row.root.sizeDelta = new Vector2(width - 8, top + textHeight + 5);
        }
    }
}

