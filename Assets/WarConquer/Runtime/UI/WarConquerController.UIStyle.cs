using System;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    // Estilo visual (SOLO interfaz) basado en el menú principal.
    // Se reutiliza en las pantallas rediseñadas.
    //
    // Si las imágenes existen en Assets/Resources/UI/Setup/ se usan;
    // si falta alguna, se usa el estilo de colores (nada se rompe).
    public partial class WarConquerController
    {
        // =========================================================
        // PALETA (la misma del menú principal)
        // =========================================================

        static readonly Color32 SkinGold = new Color32(245, 203, 76, 255);
        static readonly Color32 SkinCream = new Color32(238, 236, 228, 255);
        static readonly Color32 SkinDark = new Color32(12, 22, 28, 235);
        static readonly Color32 SkinPanel = new Color32(9, 20, 26, 248);
        static readonly Color32 SkinBorder = new Color32(150, 125, 75, 180);

        // =========================================================
        // IMÁGENES Y AJUSTES (aquí se afina el diseño)
        // =========================================================

        // Carpeta de las imágenes dentro de Assets/Resources
        const string SetupArt = "UI/Setup/";

        // Cuánto sobresale el marco por fuera del panel (en píxeles)
        const float SetupFrameMargin = 50f;

        // Cuánto se extiende el relleno azul por debajo del marco dorado.
        // Sube el número si aún se ve fondo entre el panel y el marco;
        // bájalo si el azul se asoma por fuera del marco.
        const float SetupFillExtend = 22f;

        // Grosor con el que se dibuja el borde del marco y de los botones.
        // Si ves los bordes muy gruesos o muy finos, cambia estos números.
        const float SetupFrameBorder = 45f;
        const float SetupButtonBorder = 16f;

        // Los botones nunca usan un borde mayor a esta fracción de su alto.
        // (Evita que los botones bajos se vean como una línea fina.)
        const float SetupButtonBorderMaxRatio = .30f;

        // Banner del título: ancho, distancia al borde superior
        // y posición del texto dentro del banner (0 = arriba, 1 = abajo)
        const float SetupBannerWidth = 720f;
        const float SetupBannerTop = 6f;
        const float SetupBannerTextTop = .43f;

        Sprite SetupSprite(string name)
        {
            return Resources.Load<Sprite>(SetupArt + name);
        }

        // Aplica una imagen a un Image.
        // Si la imagen tiene bordes 9-slice, se estira sin deformar las esquinas.
        void SkinApplySprite(Image image, Sprite sprite, float targetBorder)
        {
            image.sprite = sprite;
            image.color = Color.white;

            if (sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;

                // Ajuste automático para que el borde se vea del grosor deseado
                float m =
                    sprite.border.x * 100f /
                    (sprite.pixelsPerUnit * targetBorder);

                image.pixelsPerUnitMultiplier = Mathf.Max(.01f, m);
            }
            else
            {
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }
        }

        // Agrega un borde a un elemento.
        void SkinOutline(RectTransform target, Color color, float distance = 1.2f)
        {
            var outline = target.GetComponent<Outline>();

            if (outline == null)
                outline = target.gameObject.AddComponent<Outline>();

            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        // Sombra suave para que un texto se lea sobre cualquier fondo.
        void SkinTextShadow(Text text, float alpha = .7f)
        {
            if (text == null) return;

            var shadow = text.gameObject.AddComponent<Shadow>();

            shadow.effectColor = new Color(0f, 0f, 0f, alpha);
            shadow.effectDistance = new Vector2(1.2f, -1.2f);
        }
        // Logo del juego con la misma animación del menú principal.
        // x, y = posición; width = ancho (la altura se calcula sola).
        // Si no existe la imagen, no se muestra y nada se rompe.
        void SkinLogo(Transform parent, float x, float y, float width)
        {
            var sprite = Resources.Load<Sprite>("UI/Logo");

            if (sprite == null)
                return;

            float height = width * sprite.rect.height / sprite.rect.width;

            var logoRoot = GrayboxUI.Rect(parent, "Logo", x, y, width, height);

            var image = logoRoot.gameObject.AddComponent<Image>();

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;

            // La misma animación que usa el logo del menú principal
            logoRoot.gameObject.AddComponent<LogoPulse>();
        }
        // Recuadro para el retrato de un personaje (líder).
        // Si existe la imagen Portrait_<LIDER> en Resources/UI/Setup/
        // se muestra; si no, queda un recuadro con la inicial del líder como guía.
        // selected = false -> el retrato se ve apagado.
        void SkinPortrait(
            Transform parent,
            string leader,
            float x,
            float y,
            float size,
            bool selected = true)
        {
            // Fondo oscuro y opaco: el borde dorado queda limpio alrededor.
            var frame = GrayboxUI.Box(
                parent,
                "Retrato " + leader,
                x,
                y,
                size,
                size,
                new Color32(8, 14, 18, 255)
            );

            SkinOutline(
                frame,
                selected ? (Color)SkinGold : (Color)SkinBorder,
                selected ? 2.5f : 1.5f
            );

            var sprite = SetupSprite("Portrait_" + leader);

            if (sprite != null)
            {
                // La imagen va en un hijo para que el borde dorado
                // siga el recuadro y no la silueta del dibujo.
                var art = GrayboxUI.Box(frame, "Imagen", 0, 0, size, size, Color.white);
                var image = art.GetComponent<Image>();

                if (image != null)
                {
                    image.sprite = sprite;
                    image.color = selected
                        ? Color.white
                        : new Color(.62f, .62f, .62f, 1f);
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                }
            }
            else if (!string.IsNullOrEmpty(leader))
            {
                // Guía visual mientras no hay imagen: inicial del líder.
                Color accent =
                    leader == "ZUKGROK" ? GrayboxUI.Purple :
                    leader == "FAUNAR" ? GrayboxUI.Green :
                    GrayboxUI.Yellow;

                var letter = GrayboxUI.Text(
                    frame,
                    leader.Substring(0, 1),
                    0,
                    0,
                    size,
                    size,
                    Mathf.RoundToInt(size * .55f),
                    new Color(accent.r, accent.g, accent.b, selected ? .85f : .35f),
                    FontStyle.Bold
                );

                letter.alignment = TextAnchor.MiddleCenter;
                letter.raycastTarget = false;
            }
        }

        // Estados del botón (igual que el menú principal)
        void SkinWireButton(RectTransform buttonRoot, Image image, Action action)
        {
            var button = buttonRoot.gameObject.AddComponent<Button>();

            button.targetGraphic = image;
            button.onClick.AddListener(() => action());

            var colors = button.colors;

            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(235, 220, 180, 255);
            colors.pressedColor = new Color32(190, 160, 90, 255);
            colors.selectedColor = colors.highlightedColor;

            button.colors = colors;
        }

        // Botón con el estilo del menú principal.
        // selected = true  -> resaltado (Btn_Selected, o color si no hay imagen).
        // selected = false -> apagado (Btn_Normal, o color si no hay imagen).
        // La acción que recibe es la misma que tenía el botón original.
        void SkinChoiceButton(
            Transform parent,
            string label,
            float x,
            float y,
            float w,
            float h,
            Action action,
            int fontSize,
            bool selected,
            Color accent)
        {
            var buttonRoot = GrayboxUI.Rect(
                parent,
                "Boton_" + label,
                x,
                y,
                w,
                h
            );

            Color dark = SkinDark;
            Color border = SkinBorder;

            var image = buttonRoot.gameObject.AddComponent<Image>();

            var sprite = SetupSprite(
                selected ? "Btn_Selected" : "Btn_Normal"
            );

            bool useArt = sprite != null;

            if (useArt)
            {
                // El borde se adapta al alto del botón para que
                // nunca se vea aplastado como una línea.
                float border9 = Mathf.Min(
                    SetupButtonBorder,
                    h * SetupButtonBorderMaxRatio
                );

                SkinApplySprite(image, sprite, border9);
            }
            else
            {
                image.color = selected
                    ? Color.Lerp(dark, accent, .22f)
                    : dark;
            }

            SkinWireButton(buttonRoot, image, action);

            if (!useArt)
            {
                var outline = buttonRoot.gameObject.AddComponent<Outline>();

                outline.effectColor = selected
                    ? new Color(accent.r, accent.g, accent.b, .92f)
                    : border;

                outline.effectDistance = new Vector2(1.2f, 1.2f);
            }

            var text = GrayboxUI.Text(
                buttonRoot,
                label,
                10,
                0,
                w - 20,
                h,
                fontSize,
                new Color32(238, 236, 228, 255),
                FontStyle.Bold
            );

            text.alignment = TextAnchor.MiddleCenter;

            SkinTextShadow(text, .6f);
        }

        // Tarjeta de mazo: retrato + nombre + descripción.
        // Al pulsarla se ejecuta la misma acción que tenía el botón original.
        // selected = true -> tarjeta iluminada, retrato resaltado y cinta "ELEGIDO".
        void SkinDeckCard(
            Transform parent,
            string leader,
            string title,
            string detail,
            float x,
            float y,
            float w,
            float h,
            Action action,
            bool selected,
            Color accent,
            int titleSize,
            int detailSize)
        {
            var root = GrayboxUI.Rect(parent, "Mazo_" + leader, x, y, w, h);

            var image = root.gameObject.AddComponent<Image>();

            var sprite = SetupSprite(selected ? "Btn_Selected" : "Btn_Normal");

            if (sprite != null)
            {
                float border9 = Mathf.Min(
                    SetupButtonBorder,
                    h * SetupButtonBorderMaxRatio
                );

                SkinApplySprite(image, sprite, border9);
            }
            else
            {
                image.color = selected
                    ? Color.Lerp(SkinDark, accent, .22f)
                    : (Color)SkinDark;

                var outline = root.gameObject.AddComponent<Outline>();

                outline.effectColor = selected
                    ? new Color(accent.r, accent.g, accent.b, .92f)
                    : (Color)SkinBorder;

                outline.effectDistance = new Vector2(1.2f, 1.2f);
            }

            SkinWireButton(root, image, action);

            // Retrato a la izquierda, con margen para no pisar el borde de la tarjeta
            float padX = 16f;
            float padY = 12f;
            float size = Mathf.Min(h - padY * 2f, 104f);
            float portraitY = (h - size) / 2f;

            SkinPortrait(root, leader, padX, portraitY, size, selected);

            // Cinta "ELEGIDO" sobre la base del retrato (sin símbolos raros)
            if (selected)
            {
                float ribbonH = Mathf.Clamp(size * .24f, 16f, 24f);

                var ribbon = GrayboxUI.Box(
                    root,
                    "Elegido",
                    padX,
                    portraitY + size - ribbonH,
                    size,
                    ribbonH,
                    SkinGold
                );

                var ribbonImage = ribbon.GetComponent<Image>();

                if (ribbonImage != null)
                    ribbonImage.raycastTarget = false;

                var ribbonText = GrayboxUI.Text(
                    ribbon,
                    "ELEGIDO",
                    0,
                    0,
                    size,
                    ribbonH,
                    Mathf.RoundToInt(ribbonH * .6f),
                    new Color32(35, 22, 0, 255),
                    FontStyle.Bold
                );

                ribbonText.alignment = TextAnchor.MiddleCenter;
                ribbonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                ribbonText.raycastTarget = false;
            }

            // Textos a la derecha del retrato, centrados en alto
            float tx = padX + size + 14f;
            float tw = w - tx - 14f;

            // Si la descripción cabe en una línea, usa una; si no, dos.
            int lines = detail.Length * detailSize * .52f <= tw ? 1 : 2;

            float nameH = titleSize + 8f;
            float descH = lines * (detailSize + 4f) + 2f;
            float top = Mathf.Max(2f, (h - nameH - descH) / 2f);

            var name = GrayboxUI.Text(
                root,
                title,
                tx,
                top,
                tw,
                nameH,
                titleSize,
                selected
                    ? new Color32(255, 250, 235, 255)
                    : (Color)new Color32(205, 203, 195, 255),
                FontStyle.Bold
            );

            name.alignment = TextAnchor.MiddleLeft;

            // El nombre nunca baja de línea
            name.horizontalOverflow = HorizontalWrapMode.Overflow;

            SkinTextShadow(name, .75f);

            var desc = GrayboxUI.Text(
                root,
                detail,
                tx,
                top + nameH,
                tw,
                descH,
                detailSize,
                accent
            );

            desc.alignment = TextAnchor.UpperLeft;
            SkinTextShadow(desc, .8f);
        }

        // Botón con una imagen propia (Comenzar, Volver).
        // Si la imagen no existe, usa el estilo de colores.
        //
        // x, y, w, h describen la CAJA donde debe caber el botón.
        // La imagen se dibuja entera (sin deformarse) y centrada dentro de esa caja.
        // textLeft: espacio a la izquierda del texto (para dejar sitio a una flecha).
        // textDown: baja el texto esta cantidad de píxeles.
        void SkinImageButton(
            Transform parent,
            string label,
            float x,
            float y,
            float w,
            float h,
            Action action,
            int fontSize,
            string spriteName,
            bool fallbackSelected,
            float textLeft = 10f,
            float textDown = 0f)
        {
            var sprite = SetupSprite(spriteName);

            if (sprite == null)
            {
                SkinChoiceButton(
                    parent,
                    label,
                    x,
                    y,
                    w,
                    h,
                    action,
                    fontSize,
                    fallbackSelected,
                    SkinGold
                );

                return;
            }

            // Tamaño real de la imagen dentro de la caja, sin deformarla
            float aspect = sprite.rect.width / sprite.rect.height;

            float drawW = Mathf.Min(w, h * aspect);
            float drawH = drawW / aspect;

            float drawX = x + (w - drawW) / 2f;
            float drawY = y + (h - drawH) / 2f;

            var buttonRoot = GrayboxUI.Rect(
                parent,
                "Boton_" + label,
                drawX,
                drawY,
                drawW,
                drawH
            );

            var image = buttonRoot.gameObject.AddComponent<Image>();

            // Estos botones llevan adornos propios: se dibujan enteros.
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            SkinWireButton(buttonRoot, image, action);

            var text = GrayboxUI.Text(
                buttonRoot,
                label,
                textLeft,
                textDown,
                drawW - textLeft - 10f,
                drawH,
                fontSize,
                new Color32(255, 250, 235, 255),
                FontStyle.Bold
            );

            text.alignment = TextAnchor.MiddleCenter;

            // Contorno oscuro + sombra: el texto se lee aunque
            // el botón sea naranja o claro.
            var outline = text.gameObject.AddComponent<Outline>();

            outline.effectColor = new Color(.25f, .10f, 0f, .85f);
            outline.effectDistance = new Vector2(1.6f, -1.6f);

            var shadow = text.gameObject.AddComponent<Shadow>();

            shadow.effectColor = new Color(0f, 0f, 0f, .65f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        // Estilo del marco de la pantalla "Preparar partida".
        // Se llama justo después de OpenModal(...).
        void SkinSetupFrame(RectTransform body)
        {
            // Capa transparente: deja ver el fondo de la partida.
            if (modal != null)
            {
                var overlay = modal.GetComponent<Image>();

                if (overlay != null)
                    overlay.color = new Color(0f, 0f, 0f, 0f);
            }

            // El color del panel lo pone el "Relleno" (más abajo),
            // así panel y marco se ven como una sola pieza.
            var panelImage = body.GetComponent<Image>();

            if (panelImage != null)
                panelImage.color = new Color(0f, 0f, 0f, 0f);

            // Marco con imagen (o borde dorado si no existe la imagen)
            var frameSprite = SetupSprite("Setup_PanelFrame");

            if (frameSprite != null)
            {
                var frameObject = new GameObject(
                    "Marco",
                    typeof(RectTransform),
                    typeof(Image)
                );

                frameObject.transform.SetParent(body, false);
                frameObject.transform.SetAsFirstSibling();

                var frameRect = frameObject.GetComponent<RectTransform>();

                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;

                frameRect.offsetMin = new Vector2(
                    -SetupFrameMargin,
                    -SetupFrameMargin
                );

                frameRect.offsetMax = new Vector2(
                    SetupFrameMargin,
                    SetupFrameMargin
                );

                var frameImage = frameObject.GetComponent<Image>();

                SkinApplySprite(frameImage, frameSprite, SetupFrameBorder);

                // Solo se dibuja el borde, nunca el centro
                frameImage.fillCenter = false;
                frameImage.raycastTarget = false;
            }
            else
            {
                SkinOutline(body, SkinBorder, 2f);
            }

            // Relleno azul: llega hasta debajo del marco dorado
            // para que no quede fondo visible entre ambos.
            float extend = frameSprite != null ? SetupFillExtend : 0f;

            var fillObject = new GameObject(
                "Relleno",
                typeof(RectTransform),
                typeof(Image)
            );

            fillObject.transform.SetParent(body, false);

            var fillRect = fillObject.GetComponent<RectTransform>();

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(-extend, -extend);
            fillRect.offsetMax = new Vector2(extend, extend);

            var fillImage = fillObject.GetComponent<Image>();

            fillImage.color = SkinPanel;
            fillImage.raycastTarget = false;

            // El relleno va al fondo de todo (detrás del marco)
            fillObject.transform.SetAsFirstSibling();

            // Título: el primer texto del panel (lo crea OpenModal).
            var title = body.GetComponentInChildren<Text>();

            var bannerSprite = SetupSprite("Setup_TitleBanner");

            if (title != null && bannerSprite != null)
            {
                // El título pasa a escribirse sobre el banner.
                string titleText = title.text;

                title.enabled = false;

                float bannerWidth = SetupBannerWidth;

                float bannerHeight =
                    bannerWidth *
                    bannerSprite.rect.height /
                    bannerSprite.rect.width;

                var banner = GrayboxUI.Rect(
                    modal,
                    "Banner titulo",
                    (1600f - bannerWidth) / 2f,
                    SetupBannerTop,
                    bannerWidth,
                    bannerHeight
                );

                var bannerImage = banner.gameObject.AddComponent<Image>();

                bannerImage.sprite = bannerSprite;
                bannerImage.color = Color.white;
                bannerImage.preserveAspect = true;
                bannerImage.raycastTarget = false;

                var bannerText = GrayboxUI.Text(
                    banner,
                    titleText,
                    0,
                    bannerHeight * SetupBannerTextTop,
                    bannerWidth,
                    bannerHeight * .36f,
                    34,
                    new Color32(255, 250, 235, 255),
                    FontStyle.Bold
                );

                bannerText.alignment = TextAnchor.MiddleCenter;

                var shadow = bannerText.gameObject.AddComponent<Shadow>();

                shadow.effectColor = new Color(0f, 0f, 0f, .7f);
                shadow.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                if (title != null)
                {
                    title.color = SkinGold;
                    title.fontSize = 28;
                }

                // Línea dorada debajo del título.
                GrayboxUI.Box(
                    body,
                    "Separador",
                    24,
                    64,
                    1082,
                    2,
                    new Color32(245, 203, 76, 150)
                );
            }
        }
    }
}