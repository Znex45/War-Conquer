using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
        void CloseModal(){if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}}
        RectTransform OpenModal(string title)
        {
            CloseModal();modal=GrayboxUI.Box(root,"Modal",0,0,1600,1000,new Color(0,0,0,.88f));
            var body=GrayboxUI.Box(modal,title,235,100,1130,800,GrayboxUI.Panel);GrayboxUI.Text(body,title,24,18,990,45,25,GrayboxUI.Ink,FontStyle.Bold);GrayboxUI.Button(body,"Cerrar",992,18,110,37,CloseModal);return body;
        }
        void ShowSetup()
        {
            var body=OpenModal("Nueva partida local");GrayboxUI.Text(body,"Selecciona un Líder para cada jugador. Morado: Zukgrok. Amarillo: Sahria.\nLas partidas nuevas comienzan con las 87 casillas neutras.",26,80,1050,65,21,GrayboxUI.Muted);
            for(int i=0;i<4;i++){int id=i;GrayboxUI.Text(body,"JUGADOR "+(i+1),26,171+i*77,210,38,21);GrayboxUI.Button(body,leaders[i],250,161+i*77,355,50,()=>{leaders[id]=leaders[id]=="ZUKGROK"?"SAHRIA":"ZUKGROK";ShowSetup();},Color.Lerp(leaders[i]=="ZUKGROK"?GrayboxUI.Purple:GrayboxUI.Yellow,GrayboxUI.Panel,.6f));}
            GrayboxUI.Text(body,"Semilla: "+seed+"   ·   Mano inicial: "+Game.State.rules.initialHand+"   ·   Robo automático",26,506,980,34,19,GrayboxUI.Muted);
            GrayboxUI.Button(body,"Cambiar semilla",26,550,230,42,()=>{seed=(seed*31+17)&0x7fffffff;ShowSetup();});
            GrayboxUI.Button(body,"COMENZAR PARTIDA",26,620,510,64,()=>StartMatch(false),Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.55f));
            GrayboxUI.Button(body,"Cargar escenario de pruebas",563,620,510,64,()=>StartMatch(true));
            GrayboxUI.Text(body,"El escenario prepara combate, biomas, ceniza y vías rápidas para comprobar reglas. La partida normal usa los mazos barajados.",26,711,1040,53,16,GrayboxUI.Muted);
        }
        void StartMatch(bool scenario)
        {
            CloseModal();ClearAction();lastActor=-1;viewedPlayer=0;focus=-1;page=0;handFilter="Todas";useResources=false;board.Zoom=1;board.Pan=Vector2.zero;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules());
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
                ClearAction();viewedPlayer=state.activePlayer;lastActor=-1;focus=-1;page=0;Game.Restore(state);
            }catch(Exception e){Game.Notify("No se pudo cargar: "+e.Message);}
        }
    }
}
