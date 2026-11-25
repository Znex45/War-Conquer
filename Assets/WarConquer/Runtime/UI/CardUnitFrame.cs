using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public static class CardUnitFrame
    {
        static readonly Dictionary<string, Sprite> artwork = new Dictionary<string, Sprite>();
        static Sprite frame;
        public static Sprite Frame => frame != null ? frame : (frame = Resources.Load<Sprite>("UI/Cards/Card_Front"));

        public static Sprite Art(string cardId)
        {
            if (artwork.TryGetValue(cardId, out var sprite) && sprite != null) return sprite;
            sprite = Resources.Load<Sprite>("UI/CardArt/" + cardId);
            artwork[cardId] = sprite;
            return sprite;
        }

        // Both card sizes use the same original artwork window, wooden plates and cost medallion.
        public sealed class Front
        {
            static readonly Color Cream = new Color32(255, 250, 235, 255);
            static readonly Color Ink = new Color32(48, 26, 8, 255);
            readonly Image illustration, textBand;
            readonly Text title, energy, health, movement, attack, category, summary, subtype, terrains, glyph;

            public Front(RectTransform parent, float x, float y, float width, float height, bool showText)
            {
                var root = GrayboxUI.Rect(parent, "Frente original de carta", x, y, width, height);
                float sx = width / 768, sy = height / 1024;
                RectTransform Area(string key, float left, float top, float w, float h)
                    => GrayboxUI.Rect(root, key, left * sx, top * sy, w * sx, h * sy);

                Text Label(string key, float left, float top, float w, float h, int size, Color color, FontStyle style = FontStyle.Bold)
                {
                    var node = Area(key, left, top, w, h);
                    return CardPresentation.Label(node, key, 0, 0, w * sx, h * sy, size, color, style, TextAnchor.MiddleCenter);
                }

                illustration = Area("Ilustración original", 85, 163, 600, 394).gameObject.AddComponent<Image>();
                illustration.raycastTarget = false;
                glyph = Label("Emblema", 90, 195, 585, 235, 40, Cream);
                var wood = Area("Marco de carta", 0, 0, 768, 1024).gameObject.AddComponent<Image>();
                wood.sprite = Frame;
                wood.color = Frame != null ? Color.white : new Color(1, 1, 1, 0);
                wood.raycastTarget = false;

                title = Label("Nombre", 77, 67, 490, 100, 15, Cream);
                energy = Label("Coste", 589, 72, 113, 85, 17, Color.white);
                movement = Label("MOV", 82, 832, 173, 92, 12, Cream);
                health = Label("HP", 290, 811, 188, 103, 13, Cream);
                attack = Label("ATK", 515, 832, 173, 92, 12, Cream);
                if (!showText) return;

                textBand = Area("Subtipos y afinidad", 90, 395, 588, 152).gameObject.AddComponent<Image>();
                textBand.color = new Color(.12f, .07f, .04f, .88f);
                textBand.raycastTarget = false;
                subtype = Label("Subtipos", 94, 397, 580, 76, 11, Cream);
                terrains = Label("Terrenos predilectos", 94, 474, 580, 71, 11, Cream);
                category = Label("Categoría", 90, 584, 588, 63, 13, Ink);
                summary = Label("Descripción breve", 95, 648, 578, 83, 11, Ink, FontStyle.Normal);
            }

            public void Bind(CardData card, Color color, string cost, string hp, string mov, string atk, string effect, string affinity)
            {
                illustration.sprite = Art(card.id);
                illustration.color = illustration.sprite != null ? Color.white : Color.Lerp(color, GrayboxUI.Panel, .55f);
                glyph.text = card.id;
                CardPresentation.Fit(glyph, 40, 9, true);
                glyph.gameObject.SetActive(illustration.sprite == null);
                title.text = card.name;
                energy.text = cost.Replace(" → ", "→");
                health.text = "HP " + hp;
                movement.text = "MOV " + mov;
                attack.text = "ATK " + atk;
                CardPresentation.Fit(title, 15, 10, true);
                CardPresentation.Fit(energy, 17, 9, true);
                CardPresentation.Fit(health, 13, 9, false);
                CardPresentation.Fit(movement, 12, 9, false);
                CardPresentation.Fit(attack, 12, 9, false);
                if (category == null) return;

                category.text = CardPresentation.CategoryLabel(card);
                summary.text = CardReading.Summary(effect);
                subtype.text = string.Join(" · ", card.subtypes ?? System.Array.Empty<string>());
                terrains.text = affinity;
                textBand.gameObject.SetActive(subtype.text.Length > 0 || affinity.Length > 0);
                CardPresentation.Fit(category, 13, 10, false);
                CardPresentation.Fit(summary, 11, 10, true);
                CardPresentation.Fit(subtype, 11, 9, true);
                CardPresentation.Fit(terrains, 11, 9, false);
            }
        }
    }
}
