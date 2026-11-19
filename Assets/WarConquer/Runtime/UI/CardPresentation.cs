using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarConquer
{
    public static class CardPresentation
    {
        static readonly Color Gold = new Color32(245, 203, 76, 255);
        static readonly Color Paper = new Color32(251, 237, 207, 255);
        static Font cardFont;
        static readonly Color Ink = new Color32(47, 29, 20, 255);

        public static RectTransform Draw(Transform parent, GameManager game, CardData card, Player player,
            float x, float y, float w, float h, bool resources = false, bool large = false,
            Action click = null, Action enter = null, Action exit = null, Piece piece = null, int? recastEnergy = null)
        {
            var root = GrayboxUI.Box(parent, card.name, x, y, w, h, GrayboxUI.Panel);
            var face = new Face(root, w, h, large);
            face.Bind(game, card, player, resources, piece, recastEnergy);
            if (click != null)
            {
                var button = root.gameObject.AddComponent<Button>();
                button.targetGraphic = root.GetComponent<Image>();
                button.onClick.AddListener(() => click());
            }

            var preview = root.GetComponentInParent<WarConquerController>()?.CardPreview;
            if (preview != null)
                CardHoverPreview.Attach(root, preview, game, card, player, resources, piece, recastEnergy);
            else if (enter != null || exit != null)
            {
                var hover = root.gameObject.AddComponent<CardHover>();
                hover.enter = enter;
                hover.exit = exit;
            }
            return root;
        }

        public static string CategoryLabel(CardData card) => card.combatSpell ? "COMBAT SPELL" :
            card.category == Category.Latent ? "LATENTE" : card.category.ToString().ToUpperInvariant();

        public static Biome[] GetPreferredTerrains(CardData card)
        {
            if (card.IsStructure || (card.category != Category.Unit && card.category != Category.Latent))
                return Array.Empty<Biome>();
            var order = new[] { Biome.Swamp, Biome.Desert, Biome.Wasteland, Biome.Forest, Biome.Volcanic, Biome.Void, Biome.Tundra };
            return order.Where(biome => TerrainManager.Compatible(card.leader, biome)).ToArray();
        }

        static string BiomeName(Biome biome) => biome == Biome.Void ? "Void" : Names.Biomes[(int)biome];
        static string TerrainName(Biome biome) => biome == Biome.Wasteland ? "Yermo" : BiomeName(biome);

        public sealed class Face
        {
            static readonly Color Wood = new Color32(72, 38, 20, 255);
            static readonly Color Cream = new Color32(255, 242, 218, 255);
            readonly CardUnitFrame.Front front;
            readonly RectTransform paper, artworkSlot, artworkFrame;
            readonly Image artwork;
            readonly Text artworkPlaceholder;
            readonly bool large;
            readonly float height, affinityTop;
            readonly Text name, category, subtypes, cost, hp, mov, atk, summary, timing, affinityTitle, details;
            readonly Text[] terrain = new Text[2];

            public Face(RectTransform root, float width, float height, bool large)
            {
                this.height = height;
                this.large = large;
                if (!large)
                {
                    root.GetComponent<Image>().color = Color.clear;
                    float frameHeight = Mathf.Min(height - 19, width * 4 / 3);
                    float frameWidth = frameHeight * 3 / 4;
                    front = new CardUnitFrame.Front(root, (width - frameWidth) / 2, 0, frameWidth, frameHeight, true);
                    timing = Timing(root, 8, frameHeight - 1, width - 16, 20, 10, true);
                    return;
                }

                root.GetComponent<Image>().color = Wood;
                Box(root, "Remate dorado", 0, 0, width, 4, Gold);
                bool taller = height >= 700;
                float artworkWidth = taller ? 218 : 149;
                float artworkHeight = taller ? 216 : 192;
                const float pad = 18;
                artworkSlot = GrayboxUI.Rect(root, "Zona de ilustración", pad, 15, artworkWidth, artworkHeight);
                artworkFrame = Box(artworkSlot, "Marco de ilustración", 0, 0, artworkWidth, artworkHeight, Gold).rectTransform;
                artworkFrame.anchorMin = artworkFrame.anchorMax = artworkFrame.pivot = new Vector2(.5f, .5f);
                artworkFrame.anchoredPosition = Vector2.zero;
                artwork = Box(artworkFrame, "Ilustración ampliada", 0, 0, artworkWidth - 4, artworkHeight - 4, Color.white);
                artwork.rectTransform.anchorMin = Vector2.zero;
                artwork.rectTransform.anchorMax = Vector2.one;
                artwork.rectTransform.offsetMin = new Vector2(2, 2);
                artwork.rectTransform.offsetMax = new Vector2(-2, -2);
                artwork.preserveAspect = true;
                artworkPlaceholder = Label(artworkSlot, "Sin ilustración", 8, 0,
                    artworkWidth - 16, artworkHeight, 15, Cream, FontStyle.Normal, TextAnchor.MiddleCenter);
                artworkPlaceholder.text = "Sin ilustración";
                float infoX = pad + artworkWidth + 12;
                float infoWidth = width - infoX - pad;
                name = Label(root, "Nombre ampliado", infoX, 23, infoWidth, 70, taller ? 29 : 24, Cream, FontStyle.Bold);
                var badge = Box(root, "Categoría", infoX, 98, infoWidth, 27, Gold);
                category = Label(badge.transform, "Tipo", 6, 1, infoWidth - 12, 25, 17, Ink, FontStyle.Bold);
                subtypes = Label(root, "Subtipos", infoX, 130, infoWidth, 36, 15, Cream);
                cost = Label(root, "Coste", infoX, 168, infoWidth * .66f, 23, 17, Gold, FontStyle.Bold);
                hp = Label(root, "HP", infoX + infoWidth * .65f, 168, infoWidth * .35f, 23, 17, Cream, FontStyle.Bold, TextAnchor.UpperRight);
                mov = Label(root, "MOV", infoX, 193, infoWidth / 2, 24, 17, Cream, FontStyle.Bold);
                atk = Label(root, "ATK", infoX + infoWidth / 2, 193, infoWidth / 2, 24, 17, Cream, FontStyle.Bold, TextAnchor.UpperRight);
                affinityTop = Mathf.Max(228, 23 + artworkHeight);
                affinityTitle = Label(root, "Afinidad", pad, affinityTop, width - pad * 2, 19, 14, Gold, FontStyle.Bold);
                float tagWidth = (width - pad * 2 - 6) / 2;
                for (int i = 0; i < terrain.Length; i++)
                {
                    var tag = Box(root, "Terreno predilecto", pad + i * (tagWidth + 6), affinityTop + 21, tagWidth, 24, Color.black);
                    terrain[i] = Label(tag.transform, "Bioma", 2, 0, tagWidth - 4, 23, 15, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
                }
                paper = Box(root, "Pergamino de lectura", pad, affinityTop + 53, width - pad * 2, 260, Paper).rectTransform;
                Label(paper, "Encabezado", 14, 9, width - pad * 2 - 28, 22, 15, Ink, FontStyle.Bold).text = "EFECTO Y CONDICIONES";
                summary = Label(paper, "Texto descriptivo", 14, 35, width - pad * 2 - 28, 215, 25, Ink);
                summary.lineSpacing = 1.12f;
                timing = Timing(root, pad, height - 103, width - pad * 2, 49, 16, false);
                details = Label(root, "Pago y alcance", pad, height - 50, width - pad * 2, 43, 15, Cream);
            }

            static Text Timing(RectTransform root, float x, float y, float w, float h, int fontSize, bool compact)
            {
                var node = GrayboxUI.Rect(root, "Etapa de uso", x, y, w, h);
                var group = node.gameObject.AddComponent<CanvasGroup>();
                group.ignoreParentGroups = compact;
                group.blocksRaycasts = false;
                group.interactable = false;
                var text = Label(node, "Ventana de uso", 0, 0, w, h, fontSize, Gold, FontStyle.Bold);
                if (compact) text.lineSpacing = .85f;
                return text;
            }

            public void Bind(GameManager game, CardData card, Player player, bool resources, Piece piece, int? recastEnergy)
            {
                var quote = EnergyManager.Quote(player, card, resources);
                string energy = recastEnergy?.ToString() ?? quote.EnergyLabel;
                bool spell = card.category == Category.Spell;
                string health = spell ? "—" : (piece?.health ?? card.health).ToString();
                string movement = spell || card.IsStructure ? "—" : (piece?.remainingMovement ?? card.movement).ToString();
                string attack = spell ? "—" : (piece != null ? CombatManager.AttackValue(game, piece) : card.attack).ToString();
                var preferred = GetPreferredTerrains(card);
                bool generic = card.category == Category.Unit && preferred.Length == 0;
                string affinity = generic ? "Sin afinidad" : string.Join(" · ", preferred.Select(TerrainName));
                string effect = CardReading.Effect(card);
                if (large)
                {
                    artwork.sprite = CardUnitFrame.Art(card.id);
                    bool hasArtwork = artwork.sprite != null;
                    artworkFrame.gameObject.SetActive(hasArtwork);
                    artworkPlaceholder.gameObject.SetActive(!hasArtwork);
                    if (hasArtwork)
                    {
                        var border = new Vector2(4, 4);
                        Vector2 available = artworkSlot.rect.size - border;
                        Vector2 original = artwork.sprite.rect.size;
                        float scale = Mathf.Min(available.x / original.x, available.y / original.y);
                        artworkFrame.sizeDelta = original * scale + border;
                    }
                }
                else
                {
                    front.Bind(card, GrayboxUI.PlayerColor(player.id), energy, health, movement, attack, effect, affinity);
                }

                var phases = TimingRules.Order.Where(stage => (card.allowedPhases ?? Array.Empty<TurnStage>()).Contains(stage)).ToArray();
                string stages = phases.Length == 3
                    ? string.Join(" / ", phases.Take(2).Select(TimingRules.StageName)) + "\n" + TimingRules.StageName(phases[2])
                    : string.Join(" / ", phases.Select(TimingRules.StageName));
                timing.text = card.Has("Token") ? "FICHA INVOCADA" : large ? TimingRules.Description(card) : stages;
                Fit(timing, large ? 16 : 10, large ? 12 : 9, false);
                if (!large) return;

                name.text = card.name;
                Fit(name, height >= 700 ? 29 : 24, 18, false);
                category.text = CategoryLabel(card);
                subtypes.text = string.Join(" · ", card.subtypes ?? Array.Empty<string>());
                Fit(subtypes, 15, 12, false);
                cost.text = "ENERGÍA " + energy;
                Fit(cost, 17, 12, false);
                hp.text = "HP " + health;
                mov.text = "MOV " + movement;
                atk.text = "ATK " + attack;

                affinityTitle.text = generic ? "SIN AFINIDAD DE FACCIÓN" : "TERRENOS PREDILECTOS";
                affinityTitle.gameObject.SetActive(generic || preferred.Length > 0);
                for (int i = 0; i < terrain.Length; i++)
                {
                    var tag = terrain[i].transform.parent.gameObject;
                    tag.SetActive(i < preferred.Length);
                    if (i >= preferred.Length) continue;
                    terrain[i].text = "◆ " + TerrainName(preferred[i]);
                    tag.GetComponent<Image>().color = Color.Lerp(GrayboxUI.BiomeColor(preferred[i]), GrayboxUI.Panel, .35f);
                }

                float paperTop = affinityTop + (preferred.Length > 0 ? 53 : generic ? 23 : 4);
                paper.anchoredPosition = new Vector2(18, -paperTop);
                paper.sizeDelta = new Vector2(paper.sizeDelta.x, height - paperTop - 112);
                summary.rectTransform.sizeDelta = new Vector2(summary.rectTransform.sizeDelta.x, paper.rect.height - 40);
                summary.text = effect + (card.requiresAshLand ? "\n\nRequiere Tierra Ceniza propia y libre." : "");
                Fit(summary, 25, 15, false);
                string payment = recastEnergy.HasValue ? "Recast: " + recastEnergy.Value + " Energía (incluye recargos)." : quote.Detail;
                details.text = payment + (card.range > 0 ? " · Alcance " + card.range : "");
                Fit(details, 15, 12, false);
            }
        }

        static Image Box(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var image = GrayboxUI.Box(parent, name, x, y, w, h, color).GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        internal static Text Label(Transform parent, string name, float x, float y, float w, float h, int size, Color color,
            FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            var label = GrayboxUI.Text(parent, "", x, y, w, h, size, color, style);
            label.gameObject.name = name;
            if (cardFont == null)
                cardFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Helvetica", "DejaVu Sans" }, 24);
            label.font = cardFont != null ? cardFont : GrayboxUI.Font;
            label.alignment = anchor;
            label.resizeTextForBestFit = false;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        internal static void Fit(Text text, int max, int min, bool compact)
        {
            for (int size = max; size >= min; size--)
            {
                text.fontSize = size;
                if (text.preferredHeight <= text.rectTransform.rect.height + .1f) return;
            }
            if (!compact) return;
            string full = text.text;
            int end = full.Length;
            while (end > 0 && (end = full.LastIndexOf(' ', end - 1)) > 0)
            {
                text.text = full.Substring(0, end).TrimEnd('.', ',', ';') + "…";
                if (text.preferredHeight <= text.rectTransform.rect.height + .1f) return;
            }
            text.text = "…";
        }
    }

    public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action enter, exit;
        bool hovering;
        public void OnPointerEnter(PointerEventData e) { hovering = true; enter?.Invoke(); }
        public void OnPointerExit(PointerEventData e) => EndHover();
        void OnDisable() => EndHover();

        void EndHover()
        {
            if (!hovering) return;
            hovering = false;
            exit?.Invoke();
        }
    }
}
