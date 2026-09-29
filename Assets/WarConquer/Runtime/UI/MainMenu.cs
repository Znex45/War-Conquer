using System;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public class MainMenu
    {
        private readonly Transform root;
        private readonly Action onPlay;

        public MainMenu(Transform canvasRoot, Action playAction)
        {
            root = canvasRoot;
            onPlay = playAction;
        }

        public void Show()
        {
            GrayboxUI.Clear(root);

            // =====================================================
            // PANEL CENTRAL
            // =====================================================

            var panel = GrayboxUI.Box(
                root,
                "MainMenuPanel",
                560, 110,
                480, 780,
                new Color32(38, 27, 20, 235)
            );

            // =====================================================
            // LOGO
            // =====================================================

            var logoObject = new GameObject("GameLogo");
            logoObject.transform.SetParent(panel, false);

            var logoImage = logoObject.AddComponent<Image>();

            Sprite logoSprite = Resources.Load<Sprite>("UI/Logo");

            if (logoSprite != null)
            {
                logoImage.sprite = logoSprite;
                logoImage.color = Color.white;
                logoImage.preserveAspect = true;
            }
            else
            {
                Debug.LogError(
                    "No se encontró el logo. " +
                    "Debe estar en Assets/WarConquer/Resources/UI/Logo"
                );
            }

            var logoRect = logoObject.GetComponent<RectTransform>();

            // LOGO MÁS GRANDE Y CENTRADO
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.pivot = new Vector2(0.5f, 1f);

            logoRect.anchoredPosition = new Vector2(0, -25);

            logoRect.sizeDelta = new Vector2(500, 190);

            // =====================================================
            // LÍNEA DECORATIVA
            // =====================================================

            GrayboxUI.Box(
                panel,
                "Separator",
                80, 225,
                320, 3,
                new Color32(245, 203, 76, 200)
            );

            // =====================================================
            // BOTONES
            // =====================================================

            float x = 70;

            // BLOQUE DE BOTONES
            float y = 260;

            float w = 340;
            float h = 60;
            float gap = 12;

            // =====================================================
            // JUGAR
            // =====================================================

            CreateButton(
                panel,
                "JUGAR",
                x, y,
                w, h,
                onPlay
            );

            // =====================================================
            // MULTIJUGADOR
            // =====================================================

            y += h + gap;

            CreateButton(
                panel,
                "MULTIJUGADOR LOCAL",
                x, y,
                w, h,
                () => ShowComingSoon()
            );

            // =====================================================
            // TUTORIAL
            // =====================================================

            y += h + gap;

            CreateButton(
                panel,
                "TUTORIAL",
                x, y,
                w, h,
                () => ShowComingSoon()
            );

            // =====================================================
            // COLECCIÓN
            // =====================================================

            y += h + gap;

            CreateButton(
                panel,
                "COLECCIÓN",
                x, y,
                w, h,
                () => ShowComingSoon()
            );

            // =====================================================
            // OPCIONES
            // =====================================================

            y += h + gap;

            CreateButton(
                panel,
                "OPCIONES",
                x, y,
                w, h,
                () => ShowComingSoon()
            );

            // =====================================================
            // SALIR
            // =====================================================

            y += h + gap;

            CreateButton(
                panel,
                "SALIR",
                x, y,
                w, h,
                () => Application.Quit()
            );
        }

        // =====================================================
        // PRÓXIMAMENTE
        // =====================================================

        private void ShowComingSoon()
        {
            var overlay = GrayboxUI.Box(
                root,
                "ComingSoonOverlay",
                0, 0,
                1600, 1000,
                new Color32(0, 0, 0, 150)
            );

            var popup = GrayboxUI.Box(
                overlay,
                "ComingSoonPopup",
                500, 360,
                600, 280,
                new Color32(19, 27, 40, 250)
            );

            GrayboxUI.Text(
                popup,
                "PRÓXIMAMENTE",
                30, 45,
                540, 55,
                30,
                GrayboxUI.Yellow,
                FontStyle.Bold
            ).alignment = TextAnchor.MiddleCenter;

            GrayboxUI.Text(
                popup,
                "Esta función estará disponible\npróximamente.",
                40, 115,
                520, 65,
                19,
                GrayboxUI.Muted
            ).alignment = TextAnchor.MiddleCenter;

            GrayboxUI.Button(
                popup,
                "ENTENDIDO",
                180, 205,
                240, 45,
                () =>
                {
                    UnityEngine.Object.Destroy(overlay.gameObject);
                },
                GrayboxUI.Green
            );
        }

        // =====================================================
        // CREAR BOTÓN
        // =====================================================

        private void CreateButton(
    Transform parent,
    string label,
    float x,
    float y,
    float w,
    float h,
    Action click)
        {
            // =====================================================
            // BOTÓN PREMIUM
            // =====================================================

            var buttonRoot = GrayboxUI.Rect(
                parent,
                "MenuButton_" + label,
                x, y,
                w, h
            );

            // Fondo oscuro
            var background = buttonRoot.gameObject.AddComponent<Image>();
            background.color = new Color32(24, 27, 32, 235);

            // Botón
            var button = buttonRoot.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => click());

            // Estados del botón
            var colors = button.colors;

            colors.normalColor = new Color32(24, 27, 32, 235);
            colors.highlightedColor = new Color32(48, 50, 53, 245);
            colors.pressedColor = new Color32(70, 62, 45, 255);
            colors.selectedColor = new Color32(48, 50, 53, 245);

            button.colors = colors;

            // =====================================================
            // BORDE DORADO
            // =====================================================

            var border = buttonRoot.gameObject.AddComponent<Outline>();
            border.effectColor = new Color32(190, 157, 91, 210);
            border.effectDistance = new Vector2(1.5f, 1.5f);

            // =====================================================
            // TEXTO
            // =====================================================

            var text = GrayboxUI.Text(
                buttonRoot,
                label,
                10, 2,
                w - 20,
                h - 4,
                16,
                new Color32(235, 230, 215, 255),
                FontStyle.Bold
            );

            text.alignment = TextAnchor.MiddleCenter;
        }
    }
}