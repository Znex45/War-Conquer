using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void CloseModal(){if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}}
        RectTransform OpenModal(string title,bool closable=true)
        {
            CloseModal();modal=GrayboxUI.Box(root,"Modal",0,0,1600,1000,new Color(0,0,0,.88f));
            var body=GrayboxUI.Box(modal,title,235,100,1130,800,GrayboxUI.Panel);GrayboxUI.Text(body,title,24,18,990,45,25,GrayboxUI.Ink,FontStyle.Bold);if(closable)GrayboxUI.Button(body,"Cerrar",992,18,110,37,CloseModal);return body;
        }
        void ShowSetup()
        {
            HideTooltip();var body=OpenModal("PREPARAR PARTIDA",Game.State.phase!=Phase.Setup);
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
            CloseModal();ClearAction();lastActor=-1;viewedPlayer=0;focus=-1;page=0;handFilter="Todas";useResources=false;board.Zoom=1;board.Pan=Vector2.zero;
            aiPlayer=new AiPlayer();nextAiAction=Time.unscaledTime+1;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules(),humanPlayers);
            if(scenario){PrototypeScenario.Load(Game);Game.Notify("Escenario de pruebas: biomas y unidades preparadas. No es el inicio normal de partida.");}
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
