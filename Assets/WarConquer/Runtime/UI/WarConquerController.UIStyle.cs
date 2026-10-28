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

        // Fondo y borde de los paneles de la partida (solo visual).
        // Se puede llamar muchas veces sin duplicar nada.
        // Estilo de la interfaz de partida (solo visual).
        // Usa Setup_PanelFrame, Btn_Normal y Btn_Selected.
        // Si falta alguna imagen, cae al estilo de colores y nada se rompe.
        void SkinMatchPanels()
        {
            var frameSprite = SetupSprite("Setup_PanelFrame");

            foreach (var panel in new[] { left, leaderPanel, inspector, hand, piles })
            {
                if (panel == null)
                    continue;

                var image = panel.GetComponent<Image>();

                if (image != null)
                    image.color = SkinPanel;

                // Quita el marco anterior (Render redibuja los paneles).
                for (int i = panel.childCount - 1; i >= 0; i--)
                {
                    var child = panel.GetChild(i);

                    if (child.name == "Marco")
                    {
                        child.name = "Marco_old";
                        Destroy(child.gameObject);
                    }
                }

                if (frameSprite != null)
                {
                    var frameObject = new GameObject(
                        "Marco",
                        typeof(RectTransform),
                        typeof(Image)
                    );

                    frameObject.transform.SetParent(panel, false);
                    frameObject.transform.SetAsFirstSibling();

                    var frameRect = frameObject.GetComponent<RectTransform>();

                    frameRect.anchorMin = Vector2.zero;
                    frameRect.anchorMax = Vector2.one;
                    frameRect.offsetMin = new Vector2(-MatchFrameMargin, -MatchFrameMargin);
                    frameRect.offsetMax = new Vector2(MatchFrameMargin, MatchFrameMargin);

                    var frameImage = frameObject.GetComponent<Image>();

                    SkinApplySprite(frameImage, frameSprite, MatchFrameBorder);

                    frameImage.fillCenter = false;
                    frameImage.raycastTarget = false;
                }
                else
                {
                    SkinOutline(panel, SkinBorder, 1.5f);
                }
            }

            // Botones de los paneles y del encabezado
            foreach (var panel in new[] { header, left, leaderPanel, inspector, hand, piles, board != null ? board.Overlay : null, rewindButton != null ? rewindButton.transform as RectTransform : null })
            {
                if (panel != null)
                    SkinMatchButtons(panel);
            }

            SkinRewindButton();


        }

        // Ajustes del marco de los paneles de la partida
        const float MatchFrameMargin = 3f;
        const float MatchFrameBorder = 16f;

        bool SkinIsGreen(Color c)
        {
            return c.g > c.r + .08f && c.g > c.b + .02f;
        }

        void SkinMatchButtons(RectTransform panel)
        {
            var normalSprite = SetupSprite("Btn_Normal");
            var selectedSprite = SetupSprite("Btn_Selected");

            if (normalSprite == null)
                return;

            foreach (var button in panel.GetComponentsInChildren<Button>(true))
            {
                var image = button.GetComponent<Image>();
                var rect = button.GetComponent<RectTransform>();

                if (image == null || rect == null)
                    continue;

                // Solo botones normales: no tarjetas, pilas ni filas de puntuación.
                if (rect.rect.height > 70f || rect.rect.height < 20f)
                    continue;

                if (button.name.StartsWith("Puntuación") || button.name.StartsWith("Boton_"))
                    continue;

                Color original = image.color;
                bool green = selectedSprite != null && SkinIsGreen(original);

                float border = Mathf.Min(
                     SetupButtonBorder,
                     rect.rect.height * (rect.rect.height < 34f ? .2f : SetupButtonBorderMaxRatio)
                );
             

                SkinApplySprite(image, green ? selectedSprite : normalSprite, border);

                // Conserva una pista del color original (estado), sin teñir de más.
                Color tint = Color.Lerp(Color.white, original, .45f);

                if (!button.interactable)
                    tint = new Color(.55f, .55f, .55f, .8f);

                tint.a = Mathf.Min(1f, Mathf.Max(.8f, original.a));
                image.color = tint;

                foreach (var text in button.GetComponentsInChildren<Text>(true))
                {
                    text.color = button.interactable
                        ? (Color)new Color32(238, 236, 228, 255)
                        : new Color(.7f, .7f, .7f, .8f);

                    if (text.fontSize > 12 && text.GetComponent<Shadow>() == null)
                        SkinTextShadow(text, .6f);
                }
            }
        }

        // Marco + banner + CERRAR dentro del cuadro, para cualquier ventana (solo visual).
        void SkinModalFrame(RectTransform body)
        {
            SkinSetupFrame(body);

            if (modal != null)
            {
                var overlay = modal.GetComponent<Image>();

                if (overlay != null)
                    overlay.color = new Color(0, 0, 0, .6f);
            }

            // Quita el botón "Cerrar" viejo que crea OpenModal
            for (int i = body.childCount - 1; i >= 0; i--)
            {
                var child = body.GetChild(i);

                if (child.GetComponent<Button>() == null)
                    continue;

                var label = child.GetComponentInChildren<Text>();

                if (label != null && label.text == "Cerrar")
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }

            SkinImageButton(body, "CERRAR", 925, 76, 180, 57, CloseModal, 20, "Btn_Back", false, 70f);
        }

        // Deshacer: se le pone el estilo y se mantiene aunque el juego le cambie el color.
        void SkinRewindButton()
        {
            if (rewindButton == null)
                return;

            var button = rewindButton.GetComponent<Button>();
            var sprite = SetupSprite("Btn_Normal");

            if (button == null || sprite == null)
                return;

            if (button.GetComponent<SkinButtonKeeper>() != null)
                return;

            var image = button.GetComponent<Image>();
            var rect = button.GetComponent<RectTransform>();

            if (image == null || rect == null)
                return;

            float border = Mathf.Min(SetupButtonBorder, rect.rect.height * .2f);
            float mult = Mathf.Max(
                .01f,
                sprite.border.x * 100f / (sprite.pixelsPerUnit * border)
            );

            var keeper = button.gameObject.AddComponent<SkinButtonKeeper>();

            keeper.button = button;
            keeper.image = image;
            keeper.sprite = sprite;
            keeper.multiplier = mult;
        }

        // Baja (o sube) todo el contenido de un panel unos píxeles (solo visual).
        void SkinShiftPanel(RectTransform panel, float down)
        {
            if (panel == null)
                return;

            for (int i = 0; i < panel.childCount; i++)
            {
                var rect = panel.GetChild(i) as RectTransform;

                if (rect == null)
                    continue;

                // Si está anclado arriba, bajar es restar; si está anclado abajo, sumar.
                float sign = rect.anchorMin.y > .5f ? -1f : 1f;

                rect.anchoredPosition += new Vector2(0f, sign * down);
            }
        }

        // Flecha que señala al jugador en turno, en el marcador de la derecha (solo visual).
        // Usa Resources/UI/Setup/Turn_Arrow si existe; si no, dibuja una flecha dorada.
        void SkinTurnArrow()
        {
            if (inspector == null || Game?.State == null)
                return;

            int active = Game.State.activePlayer;

            // Toma la fila más reciente y activa (las viejas aún no se han destruido)
            RectTransform row = null;

            for (int i = inspector.childCount - 1; i >= 0; i--)
            {
                var child = inspector.GetChild(i);

                if (child.name == "Puntuación J" + (active + 1) && child.gameObject.activeInHierarchy)
                {
                    row = child as RectTransform;
                    break;
                }
            }

            if (row == null)
                return;

            // Borde dorado en la fila del jugador en turno
            SkinOutline(row, SkinGold, 2f);

            float rowH = row.sizeDelta.y;
            float w = 34f, h = 34f;

            var arrow = GrayboxUI.Rect(row, "Flecha de turno", -w - 2f, (rowH - h) / 2f, w, h);

            var sprite = SetupSprite("Turn_Arrow");

            if (sprite != null)
            {
                var image = arrow.gameObject.AddComponent<Image>();

                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
            else
            {
                var symbol = GrayboxUI.Text(arrow, "→", 0, 0, w, h, 34, SkinGold, FontStyle.Bold);

                symbol.alignment = TextAnchor.MiddleCenter;
                symbol.horizontalOverflow = HorizontalWrapMode.Overflow;
                symbol.raycastTarget = false;

                SkinTextShadow(symbol, .9f);
            }

            arrow.gameObject.AddComponent<SkinArrowBob>();
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

            var sprite = SetupSprite("Portrait_" + leader.Replace("'", ""));

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
            float size = Mathf.Min(h - padY * 2f, Mathf.Min(124f, w * .34f));
            titleSize = Mathf.Min(titleSize, Mathf.RoundToInt(w * .105f));
            detailSize = Mathf.Min(detailSize, 14);
            if (h < 90f) { size = Mathf.Min(size, 64f); titleSize = 18; detailSize = 12; }
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
            int lines = Mathf.Clamp(Mathf.CeilToInt(detail.Length * detailSize * .52f / Mathf.Max(1f, tw)), 1, 4);

            float nameH = titleSize + 4f;
            float descH = Mathf.Min(lines * (detailSize + 2f) + 2f, h - nameH - 4f);
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
                    title.fontSize = 28; title.resizeTextMaxSize = 28;
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

    // Mantiene el estilo de un botón aunque otro código le cambie el color.
    public class SkinButtonKeeper : MonoBehaviour
    {
        public Button button;
        public Image image;
        public Sprite sprite;
        public float multiplier = 1f;

        void LateUpdate()
        {
            if (button == null || image == null)
                return;

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = multiplier;

            bool on = button.interactable;

            image.color = on ? Color.white : new Color(.65f, .65f, .65f, .9f);

            foreach (var text in button.GetComponentsInChildren<Text>(true))
                text.color = on
                    ? (Color)new Color32(238, 236, 228, 255)
                    : new Color(.75f, .75f, .75f, .9f);
        }

 
    }

    // Movimiento suave de la flecha de turno (izquierda y derecha).
    public class SkinArrowBob : MonoBehaviour
    {
        RectTransform rect;
        Vector2 start;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            start = rect.anchoredPosition;
        }

        void Update()
        {
            rect.anchoredPosition = start + new Vector2(Mathf.Sin(Time.unscaledTime * 4f) * 4f, 0f);
        }
    }

    // Flotación suave de la flecha 3D sobre el líder en turno.
    public class SkinBob3D : MonoBehaviour
    {
        Vector3 start;

        void Awake()
        {
            start = transform.localPosition;
        }

        void Update()
        {
            transform.localPosition = start + new Vector3(0f, Mathf.Sin(Time.unscaledTime * 3f) * .18f, 0f);
        }
    }
}