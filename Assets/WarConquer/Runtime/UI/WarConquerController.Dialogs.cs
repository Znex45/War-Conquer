using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void SetMatchVisible(bool visible)
        {
            if (rewindButton != null) rewindButton.gameObject.SetActive(visible);
            if (matchRoot != null) matchRoot.gameObject.SetActive(visible);
            if (board?.World != null) board.World.gameObject.SetActive(visible);
        }
        void CloseModal()
        {
            if (modal != null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); modal = null; }
            setupVisible = false; SetMatchVisible(Game?.State != null && Game.State.phase != Phase.Setup);
        }
        RectTransform OpenModal(string title, bool closable = true)
        {
            CloseModal(); modal = GrayboxUI.Box(root, "Modal", 0, 0, 1600, 1000, new Color(0, 0, 0, .88f));
            var body = GrayboxUI.Box(modal, title, 235, 100, 1130, 800, GrayboxUI.Panel); GrayboxUI.Text(body, title, 24, 18, 990, 45, 25, GrayboxUI.Ink, FontStyle.Bold); if (closable) GrayboxUI.Button(body, "Cerrar", 992, 18, 110, 37, CloseModal); return body;
        }
        void ShowSetup()
        {
            HideTooltip(); var body = OpenModal("PREPARAR PARTIDA", false);
            setupVisible = true; SetMatchVisible(false);

            // Rediseño visual (solo interfaz): marco, banner y botones con imagen.
            // Los estilos están en WarConquerController.UIStyle.cs
            SkinSetupFrame(body);

            var question = GrayboxUI.Text(body, "¿Cuántas personas van a jugar?", 26, 74, 1050, 36, 28, SkinGold, FontStyle.Bold);
            SkinTextShadow(question);
            for (int n = 1; n <= 4; n++)
            {
                int count = n; string label = n == 1 ? "1 JUGADOR · VS IA" : n + " JUGADORES";
                SkinChoiceButton(body, label, 26 + (n - 1) * 274, 114, 255, 56, () => { humanPlayers = count; ShowSetup(); }, 21, humanPlayers == n, SkinGold);
            }
            int participants = humanPlayers == 1 ? 2 : humanPlayers;
            var modeText = GrayboxUI.Text(body, humanPlayers == 1 ? "Tú contra una IA. Elige tu mazo y el de tu rival." : humanPlayers + " jugadores · turnos locales en este equipo · sin IA.", 26, 178, 1072, 34, 23, SkinCream);
            SkinTextShadow(modeText);
            var pauseText = GrayboxUI.Text(body, "EN PAUSA · Puedes elegir con calma. La partida empieza al pulsar COMENZAR.", 26, 212, 1070, 30, 17, SkinGold);
            SkinTextShadow(pauseText);

            // Las filas de jugador cambian de alto según cuántos jugadores haya,
            // así siempre caben (2 jugadores = filas grandes, 4 = filas compactas).
            float pitch = participants <= 2 ? 172f : participants == 3 ? 138f : 104f;
            float rowH = pitch - 8f;
            float cardH = rowH - 16f;
            float cardX0 = 136f;
            float cardGap = 8f;
            float cardW = Mathf.Floor((1070f - cardX0 - 8f - cardGap * (CardCatalog.Leaders.Length - 1)) / CardCatalog.Leaders.Length);
            int titleSize = participants <= 2 ? 28 : participants == 3 ? 24 : 22;
            int detailSize = participants <= 2 ? 16 : participants == 3 ? 15 : 14;

            for (int i = 0; i < participants; i++)
            {
                int id = i; float y = 246 + i * pitch;
                var row = GrayboxUI.Box(body, "Mazo de J" + (i + 1), 26, y, 1070, rowH, new Color32(8, 14, 18, 170));

                // Solo en "1 jugador vs IA" se indica quién es persona y quién es IA.
                // En los demás modos solo se muestra "JUGADOR N", centrado.
                bool showKind = humanPlayers == 1;

                var playerLabel = GrayboxUI.Text(row, "JUGADOR " + (i + 1), 14, showKind ? rowH / 2 - 30 : rowH / 2 - 15, 120, 30, 18, SkinCream, FontStyle.Bold);
                playerLabel.horizontalOverflow = HorizontalWrapMode.Overflow;

                if (showKind)
                {

var playerKind = GrayboxUI.Text(row, i == 1 ? "IA" : "Persona", 14, rowH / 2, 120, 26, 15, GrayboxUI.Muted);
playerKind.horizontalOverflow = HorizontalWrapMode.Overflow;
}

foreach (string deck in CardCatalog.Leaders)
{
    bool fungus = deck == "ZUKGROK";
    bool chosen = leaders[id] == deck;

    Color color = deck == "XIL'THAR"
        ? (Color)new Color32(101, 201, 241, 255)
        : fungus ? GrayboxUI.Purple
        : deck == "FAUNAR" ? GrayboxUI.Green
        : GrayboxUI.Yellow;

    float x = cardX0 + System.Array.IndexOf(CardCatalog.Leaders, deck) * (cardW + cardGap);

    string info = deck == "XIL'THAR"
        ? "Void · Portales y retorno"
        : fungus ? "Pantano · Tokens y veneno"
        : deck == "FAUNAR" ? "Bosque · Animales y estructuras"
        : "Desierto · Control del terreno";

    SkinDeckCard(row, deck, deck, info, x, 8, cardW, cardH,
        () => { leaders[id] = deck; ShowSetup(); },
        chosen, color, titleSize, detailSize);
}

                
            }

            // Semilla y Escenario de pruebas: más abajo, para dejar espacio a las filas.
            var seedText = GrayboxUI.Text(body, "Semilla " + seed + " · Mapa " + (participants == 2 ? "duelo" : participants == 3 ? "hexagonal" : "hueso") + " · " + participants + " mazos de 50", 30, 664, 1070, 26, 17, GrayboxUI.Muted);
            seedText.raycastTarget = false;
            SkinTextShadow(seedText);
            SkinChoiceButton(body, "Escenario de pruebas", 836, 716, 260, 56, () => StartMatch(true), 18, false, SkinGold);

            // COMENZAR: centrado en el panel (igual que el banner de arriba).
            // El texto baja un poco para quedar en el centro de la parte naranja.
            SkinImageButton(body, "COMENZAR PARTIDA", 255, 696, 560, 92, () => StartMatch(false), 27, "Btn_Start", true, 10f, 7f);

            // VOLVER: pegado a la esquina superior derecha, sin tocar el panel.
            SkinImageButton(modal, "VOLVER", 1412, 14, 180, 57, () => { CloseModal(); ShowMainMenu(); }, 20, "Btn_Back", false, 70f);

            SkinLogo(modal, 1420, 85, 160);
        }

        // Logo en la esquina superior izquierda (con la animación del menú)
        
        void StartMatch(bool scenario)
        {
            CloseModal(); ClearAction(); lastActor = -1; viewedPlayer = 0; focus = -1; page = 0; handFilter = "Todas"; useResources = false; board.Reset();
            aiPlayer = new AiPlayer(); nextAiAction = Time.unscaledTime + 1;
            Game.NewGame(leaders, seed, CardCatalog.LoadRules(), humanPlayers);
            if (scenario) { PrototypeScenario.Load(Game); Game.Notify("Escenario de pruebas: biomas y unidades preparadas. No es el inicio normal de partida."); }
        }
        void ShowMatchMenu()
        {
            HideTooltip(); var body = OpenModal("PARTIDA EN PAUSA", false);
            SkinSetupFrame(body);
            var overlay = modal.GetComponent<UnityEngine.UI.Image>();
            if (overlay != null) overlay.color = new Color(0, 0, 0, .6f);

            var subtitle = GrayboxUI.Text(body, "Opciones de partida", 70, 90, 600, 40, 28, SkinGold, FontStyle.Bold);
            SkinTextShadow(subtitle);

            SkinChoiceButton(body, "Continuar partida", 70, 150, 990, 84, CloseModal, 30, true, SkinGold);
            SkinChoiceButton(body, "Guardar", 70, 260, 474, 72, () => { CloseModal(); Save(); }, 26, false, SkinGold);
            SkinChoiceButton(body, "Cargar", 586, 260, 474, 72, () => { CloseModal(); Load(); }, 26, false, SkinGold);
            SkinChoiceButton(body, "Registro", 70, 352, 474, 72, ShowLog, 26, false, SkinGold);
            SkinChoiceButton(body, "Ayuda", 586, 352, 474, 72, ShowHelp, 26, false, SkinGold);
            SkinChoiceButton(body, "Nueva partida", 70, 520, 990, 72, ShowSetup, 26, false, SkinGold);
            SkinChoiceButton(body, "Salir al menú principal", 70, 620, 990, 72, ShowExitConfirm, 26, false, SkinGold);

            SkinImageButton(body, "CERRAR", 925, 76, 180, 57, CloseModal, 20, "Btn_Back", false, 70f);
        }
        void ShowExitConfirm()
        {
            var body = OpenModal("¿SALIR DE LA PARTIDA?", false);
            SkinSetupFrame(body);
            var overlay = modal.GetComponent<UnityEngine.UI.Image>();
            if (overlay != null) overlay.color = new Color(0, 0, 0, .7f);

            var warning = GrayboxUI.Text(body, "Se perderá el progreso de esta partida", 70, 150, 990, 60, 32, SkinGold, FontStyle.Bold);
            warning.alignment = TextAnchor.MiddleCenter; SkinTextShadow(warning);

            var detail = GrayboxUI.Text(body, "Si vuelves al menú principal, la partida en curso se cerrará y no podrá recuperarse. Si quieres conservar tu avance, guárdala antes de salir.", 120, 230, 890, 130, 24, SkinCream);
            detail.alignment = TextAnchor.MiddleCenter; SkinTextShadow(detail);

            SkinChoiceButton(body, "Seguir jugando", 70, 400, 990, 84, ShowMatchMenu, 30, true, SkinGold);
            SkinChoiceButton(body, "Guardar y salir", 70, 510, 474, 72, () => ExitToMainMenu(true), 26, false, SkinGold);
            SkinChoiceButton(body, "Salir sin guardar", 586, 510, 474, 72, () => ExitToMainMenu(false), 26, false, SkinGold);
        }
        void ExitToMainMenu(bool saveFirst)
        {
            if (saveFirst) Save();
            CloseModal(); ClearAction(); lastActor = -1; viewedPlayer = 0; focus = -1; page = 0;
            ShowMainMenu();
            // Vuelve al estado inicial, igual que al abrir el juego.
            Game.NewGame(leaders, seed, CardCatalog.LoadRules(), 4, false);
            SetMatchVisible(false);
        }
        void ShowHelp()
        {
            var body = OpenModal("AYUDA DE PARTIDA", false);
            SkinSetupFrame(body);
            var overlay = modal.GetComponent<UnityEngine.UI.Image>();
            if (overlay != null) overlay.color = new Color(0, 0, 0, .6f);

            var h1 = GrayboxUI.Text(body, "TURNO Y CARTAS", 40, 150, 500, 36, 24, SkinGold, FontStyle.Bold);
            SkinTextShadow(h1);
            var t1 = GrayboxUI.Text(body, "Despliegue → Asalto → Terraformación. El botón de etapa permite avanzar. Rewind / Deshacer revierte la última decisión del turno, incluidos los cambios de etapa.\n\nLas cartas oscuras no se pueden usar ahora. Su banda inferior indica la etapa de uso. Pasa el cursor para ampliarlas o selecciónalas y pulsa Ver carta completa.\n\nSelecciona una carta y un hexágono verde. El pago mixto permite elegir recursos compatibles. Resolver selección confirma los objetivos múltiples; Cancelar no consume recursos.", 40, 198, 505, 480, 19, SkinCream);
            SkinTextShadow(t1);

            var h2 = GrayboxUI.Text(body, "TABLERO Y VICTORIA", 590, 150, 500, 36, 24, SkinGold, FontStyle.Bold);
            SkinTextShadow(h2);
            var t2 = GrayboxUI.Text(body, "Arrastra para desplazar, usa el botón derecho para girar y la rueda para acercar. 1:1 restablece la cámara. Ver datos muestra números de casilla y puntos de las bases.\n\nSelecciona una pieza para mover, atacar o usar su habilidad. Pulsa un marcador de jugador para consultar su mano y su Líder. Detalles muestra el control por territorio.\n\nHay 3 objetivos centrales separados. Cada uno y cada base enemiga vencida dan +1 PC al volver tu turno si mantuviste una unidad propia y el terreno terraformado. Salir, perder el bioma o recapturar reinicia la espera. Gana con 10 PC o con el último Líder en pie. Las pilas abren el mazo y descarte.", 590, 198, 505, 480, 19, SkinCream);
            SkinTextShadow(t2);

            SkinChoiceButton(body, "Volver al menú", 40, 700, 1050, 60, ShowMatchMenu, 24, false, SkinGold);
            SkinImageButton(body, "CERRAR", 925, 76, 180, 57, CloseModal, 20, "Btn_Back", false, 70f);
        }
        void ShowLeaderDetails(Player player)
        {

bool fungus = player.leader == "ZUKGROK";
var body = OpenModal(player.leader + " · HABILIDAD DEL LÍDER");
            SkinModalFrame(body);

            if (player.leader == "XIL'THAR")
{
    DrawXiltharDetails(body, player);
    return;
}

if (player.leader == "FAUNAR")

            {
                GrayboxUI.Text(body, "FAUNAR · PRIMERA ESTRUCTURA", 40, 115, 1030, 60, 29, GrayboxUI.Green, FontStyle.Bold);
                GrayboxUI.Text(body, "Pasiva sin coste. La primera Structure de tu turno cuesta 1 Energía menos (mínimo 0). No se acumula, no afecta Units ni Spells. Se reinicia al comenzar tu siguiente turno.", 40, 223, 1010, 190, 27, GrayboxUI.Ink);
                GrayboxUI.Text(body, player.firstStructureUsed ? "USED" : "AVAILABLE", 40, 460, 1030, 50, 25, GrayboxUI.Green, FontStyle.Bold); return;
            }
            GrayboxUI.Text(body, fungus ? "TOKEN DESTRUIDO · TERRAFORMACIÓN" : "DESPLIEGUE DE SAHRIA", 40, 115, 1030, 60, 29, GrayboxUI.PlayerColor(player.id), FontStyle.Bold);
            GrayboxUI.Text(body, fungus ? "Cuando muere uno de tus Tokens por cualquier causa, elige el bioma de la casilla donde murió. Terraformar cuesta 0 Energía. No se activa con Units normales." : "Puedes pagar 1 Energía adicional al jugar una Unit en una casilla que hayas establecido, incluso de otro bioma. Activa la opción SAHRIA +1 E antes de elegir la carta. Respeta la ocupación y las restricciones explícitas, como Yermo de Wandering Beast.", 40, 223, 1010, 190, 27, GrayboxUI.Ink);
            GrayboxUI.Text(body, fungus ? "AUTOMÁTICA · 0 ENERGÍA" : "DESPLIEGUE · COSTE DE UNIT +1 ENERGÍA", 40, 460, 1030, 50, 25, GrayboxUI.PlayerColor(player.id), FontStyle.Bold);
            GrayboxUI.Text(body, "La carta del Líder permanece fuera de la mano y no cuenta entre las 50 cartas.", 40, 566, 1030, 85, 23, GrayboxUI.Muted);
        }
        void ShowLog()
        {
            var body = OpenModal("REGISTRO DE LA PARTIDA", false);
            SkinSetupFrame(body);
            var overlay = modal.GetComponent<UnityEngine.UI.Image>();
            if (overlay != null) overlay.color = new Color(0, 0, 0, .6f);

            var log = GrayboxUI.Text(body, string.Join("\n", Game.State.log.Skip(Math.Max(0, Game.State.log.Count - 29))), 40, 150, 1050, 520, 19, SkinCream);
            SkinTextShadow(log);

            SkinChoiceButton(body, "Volver al menú", 40, 700, 1050, 60, ShowMatchMenu, 24, false, SkinGold);
            SkinImageButton(body, "CERRAR", 925, 76, 180, 57, CloseModal, 20, "Btn_Back", false, 70f);
        }
        void Save() { try { File.WriteAllText(SavePath, GamePersistence.Serialize(Game.State)); Game.Notify("Partida guardada localmente."); } catch (Exception e) { Game.Notify("No se pudo guardar: " + e.Message); } }
        void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) { Game.Notify("Todavía no hay una partida guardada."); return; }
                var state = GamePersistence.Deserialize(File.ReadAllText(SavePath));
                string error = StateValidator.Validate(state, Game.Catalog); if (error.Length > 0) { Game.Notify("Guardado no válido: " + error); return; }
                CloseModal(); ClearAction(); viewedPlayer = state.activePlayer; lastActor = -1; focus = -1; page = 0; aiPlayer = new AiPlayer(); nextAiAction = Time.unscaledTime + 1;
                humanPlayers = state.players.Count(p => !p.inactive && !p.isAI); foreach (var p in state.players.Where(p => !p.inactive)) leaders[p.id] = p.leader;
                Game.Restore(state); if (state.phase == Phase.Setup) ShowSetup();
            }
            catch (Exception e) { Game.Notify("No se pudo cargar: " + e.Message); }
        }
    }
}