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
            if(matchRoot!=null)matchRoot.gameObject.SetActive(visible);
            if(board?.World!=null)board.World.gameObject.SetActive(visible);
        }
        void CloseModal()
        {
            if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}
            setupVisible=false;SetMatchVisible(Game?.State!=null&&Game.State.phase!=Phase.Setup);
        }
        RectTransform OpenModal(string title,bool closable=true)
        {
            CloseModal();modal=GrayboxUI.Box(root,"Modal",0,0,1600,1000,new Color(0,0,0,.88f));
            var body=GrayboxUI.Box(modal,title,235,100,1130,800,GrayboxUI.Panel);GrayboxUI.Text(body,title,24,18,990,45,25,GrayboxUI.Ink,FontStyle.Bold);if(closable)GrayboxUI.Button(body,"Cerrar",992,18,110,37,CloseModal);return body;
        }
        void ShowSetup()
        {
            HideTooltip();var body=OpenModal("PREPARAR PARTIDA",Game.State.phase!=Phase.Setup);
            setupVisible=true;SetMatchVisible(false);modal.GetComponent<UnityEngine.UI.Image>().color=GrayboxUI.Background;
            GrayboxUI.Text(body,"¿Cuántas personas van a jugar?",26,72,1050,30,21,GrayboxUI.Ink,FontStyle.Bold);
            for(int n=1;n<=4;n++)
            {
                int count=n;string label=n==1?"1 PERSONA · VS IA":n+" PERSONAS";
                GrayboxUI.Button(body,label,26+(n-1)*274,113,255,48,()=>{humanPlayers=count;ShowSetup();},humanPlayers==n?Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.5f):edge);
            }
            int participants=humanPlayers==1?2:humanPlayers;
            GrayboxUI.Text(body,humanPlayers==1?"Tú contra una IA. Elige tu mazo y el de tu rival.":humanPlayers+" personas · turnos locales en este equipo · sin IA.",26,178,1072,31,19,GrayboxUI.Ink);
            GrayboxUI.Text(body,"EN PAUSA · Puedes elegir con calma. La partida empieza al pulsar COMENZAR.",26,212,1070,27,15,GrayboxUI.Green);
            for(int i=0;i<participants;i++)
            {
                int id=i;float y=254+i*90;
                var row=GrayboxUI.Box(body,"Mazo de J"+(i+1),26,y,1070,80,new Color32(32,42,56,255));
                GrayboxUI.Text(row,"JUGADOR "+(i+1)+(humanPlayers==1&&i==1?" · IA":""),12,12,204,26,18,GrayboxUI.Ink,FontStyle.Bold);
                GrayboxUI.Text(row,humanPlayers==1&&i==1?"Rival automático":"Persona",12,44,195,22,14,GrayboxUI.Muted);
                foreach(string deck in new[]{"ZUKGROK","SAHRIA"})
                {
                    bool fungus=deck=="ZUKGROK",chosen=leaders[id]==deck;var color=fungus?GrayboxUI.Purple:GrayboxUI.Yellow;
                    float x=fungus?225:641;
                    GrayboxUI.Button(row,(chosen?"✓ ":"")+deck+" · 50 cartas",x,8,404,34,()=>{leaders[id]=deck;ShowSetup();},Color.Lerp(color,GrayboxUI.Panel,chosen?.35f:.82f));
                    GrayboxUI.Text(row,fungus?"MICELIAL · Bosque, Esporas y veneno":"SOLAR · Desierto y control del terreno",x+7,49,390,24,14,color);
                }
            }
            GrayboxUI.Button(body,"COMENZAR PARTIDA",26,642,1070,60,()=>StartMatch(false),Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.45f));
            GrayboxUI.Text(body,"Semilla "+seed+" · 87 hexágonos · "+participants+" mazos de 50",26,720,510,25,15,GrayboxUI.Muted);
            GrayboxUI.Button(body,"Cambiar semilla",552,715,213,35,()=>{seed=(seed*31+17)&0x7fffffff;ShowSetup();});
            GrayboxUI.Button(body,"Escenario de pruebas",780,715,316,35,()=>StartMatch(true));
        }
        void StartMatch(bool scenario)
        {
            CloseModal();ClearAction();lastActor=-1;viewedPlayer=0;focus=-1;page=0;handFilter="Todas";useResources=false;board.Reset();
            aiPlayer=new AiPlayer();nextAiAction=Time.unscaledTime+1;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules(),humanPlayers);
            if(scenario){PrototypeScenario.Load(Game);Game.Notify("Escenario de pruebas: biomas y unidades preparadas. No es el inicio normal de partida.");}
        }
        void ShowMatchMenu()
        {
            HideTooltip();var body=OpenModal("PARTIDA EN PAUSA");
            GrayboxUI.Text(body,"Opciones de partida",70,100,990,40,24,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Button(body,"Continuar partida",70,173,990,66,CloseModal,Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.55f));
            GrayboxUI.Button(body,"Guardar",70,274,474,66,()=>{CloseModal();Save();});
            GrayboxUI.Button(body,"Cargar",586,274,474,66,()=>{CloseModal();Load();});
            GrayboxUI.Button(body,"Registro",70,370,474,66,ShowLog);
            GrayboxUI.Button(body,"Ayuda",586,370,474,66,ShowHelp);
            GrayboxUI.Button(body,"Nueva partida",70,534,990,66,ShowSetup);
        }
        void ShowHelp()
        {
            var body=OpenModal("AYUDA DE PARTIDA");
            GrayboxUI.Text(body,"TURNO Y CARTAS",35,95,500,36,23,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(body,"Despliegue → Terraformación → Asalto. El botón de etapa permite avanzar.\n\nLas cartas oscuras no se pueden usar ahora. Su banda inferior indica la etapa de uso. Pasa el cursor para ampliarlas o selecciónalas y pulsa Ver carta completa.\n\nSelecciona una carta y un hexágono verde. El pago mixto permite elegir recursos compatibles. Resolver selección confirma los objetivos múltiples; Cancelar no consume recursos.",35,153,505,460,20,GrayboxUI.Muted);
            GrayboxUI.Text(body,"TABLERO Y VICTORIA",590,95,500,36,23,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(body,"Arrastra para desplazar, usa el botón derecho para girar y la rueda para acercar. 1:1 restablece la cámara. Ver datos muestra números de casilla y puntos de las bases.\n\nSelecciona una pieza para mover, atacar o usar su habilidad. Pulsa un marcador de jugador para consultar su mano y su Líder. Detalles muestra el control por territorio.\n\nEl centro y cada base enemiga conquistada dan +1 PC por ronda. Gana con 10 PC o con el último Líder en pie. Las pilas abren el mazo y descarte.",590,153,505,510,20,GrayboxUI.Muted);
            GrayboxUI.Button(body,"Volver al menú",35,714,1060,48,ShowMatchMenu);
        }
        void ShowLeaderDetails(Player player)
        {
            bool fungus=player.leader=="ZUKGROK";var body=OpenModal(player.leader+" · HABILIDAD DEL LÍDER");
            GrayboxUI.Text(body,fungus?"INFLUENCIA MICELIAL":"DOMINIO DE LAS ARENAS",40,115,1030,60,29,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            GrayboxUI.Text(body,fungus?"Envenena 1 a un enemigo en Bosque conectado a tu red y crea una Espora adyacente.":"Hasta 2 Desiertos propios se vuelven inestables (4+). Un fallo causa 1 daño adicional, una sola vez entre ambos.",40,223,1010,190,27,GrayboxUI.Ink);
            GrayboxUI.Text(body,TimingRules.StageName(TimingRules.LeaderStage(player))+" · COSTE "+Game.State.rules.leaderAbilityCost+" E",40,460,1030,50,25,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            GrayboxUI.Text(body,"Actívala desde el panel de tu Líder cuando haya objetivos válidos y energía suficiente.",40,566,1030,85,23,GrayboxUI.Muted);
        }
        void ShowLog(){var body=OpenModal("Registro de la partida");GrayboxUI.Text(body,string.Join("\n",Game.State.log.Skip(Math.Max(0,Game.State.log.Count-29))),26,80,1070,684,19,GrayboxUI.Muted);}
        void Save(){try{File.WriteAllText(SavePath,GamePersistence.Serialize(Game.State));Game.Notify("Partida guardada localmente.");}catch(Exception e){Game.Notify("No se pudo guardar: "+e.Message);}}
        void Load()
        {
            try
            {
                if(!File.Exists(SavePath)){Game.Notify("Todavía no hay una partida guardada.");return;}
                var state=GamePersistence.Deserialize(File.ReadAllText(SavePath));
                string error=StateValidator.Validate(state,Game.Catalog);if(error.Length>0){Game.Notify("Guardado no válido: "+error);return;}
                CloseModal();ClearAction();viewedPlayer=state.activePlayer;lastActor=-1;focus=-1;page=0;aiPlayer=new AiPlayer();nextAiAction=Time.unscaledTime+1;
                humanPlayers=state.players.Count(p=>!p.inactive&&!p.isAI);foreach(var p in state.players.Where(p=>!p.inactive))leaders[p.id]=p.leader;
                Game.Restore(state);if(state.phase==Phase.Setup)ShowSetup();
            }catch(Exception e){Game.Notify("No se pudo cargar: "+e.Message);}
        }
    }
}
