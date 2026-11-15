using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    // Carta de Unit con la plantilla Card_Front (solo visual).
    // Capas: arte (o relleno temporal) -> marco -> textos.
    public static class CardUnitFrame
    {
        const float TW = 768f, TH = 1024f;

        public static bool TryDraw(RectTransform root, GameManager game, CardData card, Player player,
            float w, float h, bool resources, bool large, Piece piece, int? recastEnergy)
        {
            bool isSpell = card.category == Category.Spell;
            bool isStructure = card.IsStructure;

            var frame = Resources.Load<Sprite>("UI/Cards/Card_Front");

            if (frame == null)
            {
                Debug.LogWarning("Falta Resources/UI/Cards/Card_Front (Sprite)");
                return false;
            }

            // Cartas pequeñas (mano, listas): se muestran sin descripción.
            bool compact = w < 260f;

            float s = Mathf.Min(w / TW, h / TH);
            float fw = TW * s, fh = TH * s;
            float ox = (w - fw) / 2f, oy = (h - fh) / 2f;

            var rootImage = root.GetComponent<Image>();
            if (rootImage != null) rootImage.color = new Color(0, 0, 0, 0);

            // ---------- 1. ARTE (detrás del marco) ----------
            Color tone = card.biomes != null && card.biomes.Length > 0 && card.biomes[0] != Biome.Neutral
                ? GrayboxUI.BiomeColor(card.biomes[0])
                : GrayboxUI.PlayerColor(player.id);

            var artBox = GrayboxUI.Box(root, "Arte", ox + 80 * s, oy + 168 * s, 608 * s, 390 * s,
                Color.Lerp(tone, GrayboxUI.Panel, .45f));
            var artImage = artBox.GetComponent<Image>();
            artImage.raycastTarget = false;

            var art = Resources.Load<Sprite>("UI/CardArt/" + card.id);

            if (art != null)
            {
                artImage.sprite = art;
                artImage.color = Color.white;
                artImage.preserveAspect = false;
            }
            else
            {
                Label(artBox, card.name.Substring(0, 1), 0, 20 * s, 608 * s, 280 * s, 220 * s,
                    new Color(1, 1, 1, .25f), TextAnchor.MiddleCenter, false, FontStyle.Bold, false);

                if (!compact)
                    Label(artBox, card.id, 0, 330 * s, 608 * s, 50 * s, 26 * s,
                        new Color(1, 1, 1, .55f), TextAnchor.MiddleCenter, false, FontStyle.Bold, false);
            }

            // ---------- 2. MARCO ----------
            var frameBox = GrayboxUI.Box(root, "Marco de carta", ox, oy, fw, fh, Color.white);
            var frameImage = frameBox.GetComponent<Image>();
            frameImage.sprite = frame;
            frameImage.color = Color.white;
            frameImage.preserveAspect = false;
            frameImage.raycastTarget = false;

            // ---------- 3. TEXTOS ----------
            var cream = new Color32(255, 250, 235, 255);
            var ink = new Color32(48, 26, 8, 255);

            var quote = EnergyManager.Quote(player, card, resources);
            string cost = recastEnergy.HasValue ? recastEnergy.Value.ToString() : quote.EnergyLabel;

            // Nombre (placa de madera) y coste (círculo azul)
            Label(root, card.name, ox + 80 * s, oy + 72 * s, 500 * s, 86 * s, compact ? 54 * s : 46 * s, cream, TextAnchor.MiddleCenter, true, FontStyle.Bold, true);
            Label(root, cost, ox + 590 * s, oy + 68 * s, 106 * s, 94 * s, 62 * s, Color.white, TextAnchor.MiddleCenter, true, FontStyle.Bold, true);

            // Tipo y descripción (pergamino)
            string type = (card.combatSpell ? "Combat Spell" : Names.Categories[(int)card.category])
                + (card.subtypes != null && card.subtypes.Length > 0 ? " · " + string.Join(" / ", card.subtypes) : "");

            if (!compact)
            {
                Label(root, type, ox + 92 * s, oy + 586 * s, 584 * s, 32 * s, 24 * s, ink, TextAnchor.MiddleLeft, true, FontStyle.Bold, false);
                Label(root, card.description, ox + 92 * s, oy + 620 * s, 584 * s, 118 * s, 32 * s, ink, TextAnchor.UpperLeft, true, FontStyle.Bold, false);
            }
            else if (isSpell || isStructure)
            {
                // En la mano, las Magias y Estructuras muestran su tipo en el pergamino.
                string shortType = card.combatSpell ? "Magia de combate" : isSpell ? "Magia" : "Estructura";
                Label(root, shortType, ox + 92 * s, oy + 630 * s, 584 * s, 90 * s, 58 * s, ink, TextAnchor.MiddleCenter, true, FontStyle.Bold, false);
            }

            // Estadísticas: izquierda MOV, centro (dorada) VIDA, derecha ATK. Sin dato: ---
            const string none = "---";
            int attackValue = piece != null ? CombatManager.AttackValue(game, piece) : card.attack;

            string mov = isSpell || isStructure ? none : (piece != null ? piece.remainingMovement : card.movement).ToString();
            string hp = isSpell ? none : (piece != null ? piece.health : card.health).ToString();
            string atk = isSpell ? none : (isStructure && attackValue <= 0 ? none : attackValue.ToString());

            Stat(root, "MOV", mov, ox + 82 * s, oy + 828 * s, 170 * s, 107 * s, s, cream, compact);
            Stat(root, "VIDA", hp, ox + 290 * s, oy + 806 * s, 188 * s, 109 * s, s, cream, compact);
            Stat(root, "ATK", atk, ox + 517 * s, oy + 828 * s, 170 * s, 107 * s, s, cream, compact);

            return true;
        }

        static void Stat(RectTransform root, string label, string value, float x, float y, float bw, float bh, float s, Color color, bool compact)
        {
            if (compact)
            {
                // Sin etiqueta: el número ocupa toda la caja
                Label(root, value, x, y + 6 * s, bw, bh - 12 * s, 92 * s, color, TextAnchor.MiddleCenter, true, FontStyle.Bold, true);
                return;
            }

            Label(root, label, x, y + 2 * s, bw, 30 * s, 24 * s, new Color(color.r, color.g, color.b, .95f), TextAnchor.MiddleCenter, false, FontStyle.Bold, true);
            Label(root, value, x, y + 30 * s, bw, bh - 34 * s, 70 * s, color, TextAnchor.MiddleCenter, true, FontStyle.Bold, true);
        }

        // outline = true: contorno marrón oscuro (para texto claro sobre madera).
        static Text Label(Transform parent, string text, float x, float y, float w, float h, float size,
            Color color, TextAnchor align, bool fit, FontStyle style, bool outline)
        {
            int fontSize = Mathf.Max(7, Mathf.RoundToInt(size));
            var t = GrayboxUI.Text(parent, text, x, y, w, h, fontSize, color, style);

            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;

            if (fit)
            {
                t.resizeTextForBestFit = true;
                t.resizeTextMinSize = 6;
                t.resizeTextMaxSize = fontSize;
            }

            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                float d = Mathf.Max(1f, size * .045f);

                o.effectColor = new Color(.14f, .07f, .02f, .95f);
                o.effectDistance = new Vector2(d, -d);
            }

            return t;
        }
    }
}