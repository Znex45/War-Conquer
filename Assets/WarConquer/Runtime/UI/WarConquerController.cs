using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace WarConquer
{
    public class WarConquerController : MonoBehaviour
    {
        public GameManager Game { get; private set; }
        RectTransform root,header,left,boardPanel,inspector,hand,modal;
        BoardView board;
        string mode="inspect";
        int viewedPlayer,focus=-1,selectedCard=-1,page;
        [NonSerialized] Piece selectedPiece;
        readonly List<int> targets=new List<int>();
        bool useResources=true;
        Biome chosenBiome=Biome.Forest;
        string[] leaders={"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"};
        int seed=2026;
        readonly Color edge=new Color32(48,61,79,255);
        string SavePath=>Path.Combine(Application.persistentDataPath,"war-conquer-save.json");
        void Awake()
        {
            // Unity restores private serializable fields on editor reload; selection belongs to this game only.
            ClearAction();focus=-1;viewedPlayer=0;page=0;
            Game=new GameManager(CardCatalog.Load());CreateUI();Game.Changed+=Render;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules());
        }
        void OnDestroy(){if(Game!=null)Game.Changed-=Render;}
        void CreateUI()
        {
            if(FindAnyObjectByType<EventSystem>()==null)
            {
                var events=new GameObject("WarConquer EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform);
            }
            var canvasObject=new GameObject("WarConquer Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.transform.SetParent(transform);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor=Mathf.Min(Screen.width/1600f,Screen.height/1000f);
            var background=canvasObject.GetComponent<RectTransform>();background.gameObject.AddComponent<Image>().color=GrayboxUI.Background;
            root=GrayboxUI.Rect(background,"Graybox",0,0,1600,1000);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            header=GrayboxUI.Rect(root,"Header",0,0,1600,66);
            left=GrayboxUI.Box(root,"Control",16,76,222,674,GrayboxUI.Panel);
            boardPanel=GrayboxUI.Box(root,"Board",250,76,986,674,new Color32(17,29,38,255));boardPanel.gameObject.AddComponent<RectMask2D>();board=new BoardView(boardPanel,this);
            inspector=GrayboxUI.Box(root,"Inspector",1248,76,336,674,GrayboxUI.Panel);
            hand=GrayboxUI.Box(root,"Hand",16,762,1568,222,GrayboxUI.Panel);
        }
        void Update()
        {
            var canvas=root.GetComponentInParent<CanvasScaler>();float scale=Mathf.Min(Screen.width/1600f,Screen.height/1000f);
            if(Mathf.Abs(canvas.scaleFactor-scale)>.001f)canvas.scaleFactor=scale;
        }
        public void Render()
        {
            if(Game?.State==null)return;
            GrayboxUI.Clear(header);GrayboxUI.Clear(left);GrayboxUI.Clear(inspector);GrayboxUI.Clear(hand);
            var s=Game.State;var active=s.Active;var p=s.players[viewedPlayer];GrayboxUI.PlayerLeaders=s.players.Select(pl=>pl.leader).ToArray();
            GrayboxUI.Text(header,"WAR & CONQUER",20,13,300,34,27,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(header,"GRAYBOX  /  4 JUGADORES LOCALES",328,22,370,26,14,GrayboxUI.Muted);
            GrayboxUI.Text(header,"RONDA "+s.round+"   ·   J"+(s.activePlayer+1)+"  "+active.leader+"   ·   "+(s.phase==Phase.Finished?"FIN":"ACCIONES"),775,22,490,28,17,GrayboxUI.PlayerColor(s.activePlayer),FontStyle.Bold);
            GrayboxUI.Button(header,"Nueva partida",1394,15,188,36,ShowSetup);
            GrayboxUI.Text(left,"JUGADORES",14,12,190,25,14,GrayboxUI.Muted,FontStyle.Bold);
            for(int i=0;i<4;i++)
            {
                int id=i;var pl=s.players[i];var color=Color.Lerp(GrayboxUI.PlayerColor(i),GrayboxUI.Panel,.78f);
                var b=GrayboxUI.Button(left,(i==s.activePlayer?"> ":"")+"J"+(i+1)+" · "+pl.leader+(pl.eliminated?" [FUERA]":""),12,42+i*43,198,36,()=>{viewedPlayer=id;page=0;ClearAction();Render();},i==viewedPlayer?color:edge);
                if(i==viewedPlayer)b.GetComponent<Image>().color=color;
            }
            GrayboxUI.Text(left,"J"+(p.id+1)+"  "+p.leader,14,226,198,25,20,GrayboxUI.PlayerColor(p.id),FontStyle.Bold);
            GrayboxUI.Text(left,"Vida del Líder  "+p.leaderHealth+" / "+s.rules.leaderHealth,14,258,194,26,16);
            GrayboxUI.Text(left,"ENERGÍA  "+p.currentEnergy+" / "+p.maxEnergy,14,292,194,29,22,GrayboxUI.PlayerColor(p.id),FontStyle.Bold);
            GrayboxUI.Text(left,"Mazo "+p.deck.Count+"   Mano "+p.hand.Count+"   Desc. "+p.discardPile.Count,14,330,198,28,15);
            GrayboxUI.Text(left,"Esporas: "+p.spores+"   Centro: "+p.centerScore,14,361,198,24,15);
            string resources=string.Join("  ·  ",p.resources.Where(r=>r.amount>0).Select(r=>Names.Biomes[(int)r.biome]+" "+r.amount));
            GrayboxUI.Text(left,"RECURSOS\n"+(resources.Length>0?resources:"Sin recursos de bioma"),14,395,194,66,13,GrayboxUI.Muted);
            GrayboxUI.Button(left,useResources?"Recursos compatibles: SÍ":"Recursos compatibles: NO",12,472,198,32,()=>{useResources=!useResources;Render();});
            GrayboxUI.Button(left,"Ver mazo / descarte",12,514,198,32,()=>ShowDeck(p));
            GrayboxUI.Button(left,"Habilidad del Líder",12,557,198,36,()=>Begin("leader"),Color.Lerp(GrayboxUI.PlayerColor(p.id),GrayboxUI.Panel,.5f),viewedPlayer==s.activePlayer&&Game.CanAct);
            GrayboxUI.Button(left,"FINALIZAR TURNO  >",12,614,198,44,()=>{ClearAction();Game.EndTurn();viewedPlayer=s.activePlayer;page=0;Render();},Color.Lerp(GrayboxUI.PlayerColor(s.activePlayer),GrayboxUI.Panel,.4f),Game.CanAct);
            var valid=ValidTargets();board.Render(Game,new HashSet<int>(valid),new HashSet<int>(targets),focus);
            GrayboxUI.Text(boardPanel,"87 HEXÁGONOS  ·  MAPA CONTINUO  ·  SIN PUENTES",16,12,600,22,12,GrayboxUI.Muted);
            GrayboxUI.Button(boardPanel,"−",854,11,33,30,()=>{board.Zoom=Mathf.Max(1,board.Zoom-.25f);Render();});
            GrayboxUI.Button(boardPanel,"+",892,11,33,30,()=>{board.Zoom=Mathf.Min(2,board.Zoom+.25f);Render();});
            GrayboxUI.Button(boardPanel,"1:1",930,11,42,30,()=>{board.Zoom=1;board.Pan=Vector2.zero;Render();});
            if(board.Zoom>1)
            {
                GrayboxUI.Button(boardPanel,"←",840,48,30,28,()=>{board.Pan.x+=80;Render();});GrayboxUI.Button(boardPanel,"→",875,48,30,28,()=>{board.Pan.x-=80;Render();});
                GrayboxUI.Button(boardPanel,"↑",910,48,30,28,()=>{board.Pan.y+=80;Render();});GrayboxUI.Button(boardPanel,"↓",945,48,30,28,()=>{board.Pan.y-=80;Render();});
            }
            GrayboxUI.Text(boardPanel,"Círculo: unidad · Cuadrado: estructura · Borde verde: objetivo · R: vía rápida · V: veneno · Zz: sueño",16,641,965,24,12,GrayboxUI.Muted);
            DrawInspector();DrawHand(p);
        }
        List<int> ValidTargets()
        {
            if(viewedPlayer!=Game.State.activePlayer||!Game.CanAct)return new List<int>();
            switch(mode)
            {
                case "card": var instance=Game.State.Active.hand.Find(c=>c.instanceId==selectedCard);return instance!=null&&Game.CardBlockReason(instance,useResources)==""?Game.CardTargets(Game.Catalog[instance.cardId],targets):new List<int>();
                case "move": return MovementManager.Paths(Game,selectedPiece).Keys.ToList();
                case "attack":return CombatManager.Targets(Game,selectedPiece);
                case "terraform":return Game.State.tiles.Where(t=>Game.TerraformTarget(t.id)&&t.biome!=chosenBiome).Select(t=>t.id).ToList();
                case "ash":return Game.State.tiles.Where(t=>t.owner==Game.State.activePlayer&&TerrainManager.Normal(t)).Select(t=>t.id).ToList();
                case "leader":return AbilityManager.LeaderTargets(Game).Except(targets).ToList();
                case "ability":return AbilityManager.Targets(Game,selectedPiece).Except(targets).ToList();
                case "step":return selectedPiece==null?new List<int>():Game.State.tiles[selectedPiece.tileId].neighbors.Where(n=>!Game.State.tiles[n].IsOccupied&&!Game.State.tiles[n].blocked&&Game.State.tiles[n].baseOwner<0).ToList();
                default:return new List<int>();
            }
        }
        void DrawInspector()
        {
            var s=Game.State;
            GrayboxUI.Text(inspector,"INSPECTOR  /  "+ModeName(),14,12,308,26,14,GrayboxUI.Muted,FontStyle.Bold);
            CardData card=null;var inst=s.players[viewedPlayer].hand.Find(c=>c.instanceId==selectedCard);if(inst!=null)card=Game.Catalog[inst.cardId];
            var tile=focus>=0?s.tiles[focus]:null;
            if(card==null&&selectedPiece!=null)card=Game.Data(selectedPiece);
            if(card!=null)
            {
                GrayboxUI.Text(inspector,card.name,14,48,307,57,24,GrayboxUI.PlayerColor(viewedPlayer),FontStyle.Bold);
                GrayboxUI.Text(inspector,Names.Categories[(int)card.category]+" · "+card.factionTag+"\n"+string.Join(" / ",card.subtypes),14,106,307,42,14,GrayboxUI.Muted);
                GrayboxUI.Text(inspector,"E "+card.energyCost+"   VIDA "+(selectedPiece!=null?selectedPiece.health:card.health)+"   ATQ "+(selectedPiece!=null?CombatManager.AttackValue(Game,selectedPiece):card.attack)+"\nMOV "+(selectedPiece!=null?selectedPiece.remainingMovement:card.movement)+"   ALC "+card.range+"   "+(card.movementType=="Flying"?"Voladora":card.movementType=="Fixed"?"Fija":"Terrestre"),14,154,307,47,16);
                GrayboxUI.Text(inspector,"Biomas: "+string.Join(" / ",card.biomes.Select(b=>Names.Biomes[(int)b])),14,207,306,40,14,GrayboxUI.Muted);
                GrayboxUI.Text(inspector,card.description,14,253,307,122,16);
                GrayboxUI.Text(inspector,string.Join(" · ",card.terrainTags),14,381,307,46,12,GrayboxUI.Muted);
                if(inst!=null)
                {
                    string why=viewedPlayer!=s.activePlayer?"Solo J"+(s.activePlayer+1)+" puede actuar.":Game.CardBlockReason(inst,useResources);
                    GrayboxUI.Text(inspector,why.Length>0?why:"Selecciona las casillas resaltadas.",14,432,307,43,15,why.Length>0?GrayboxUI.Yellow:GrayboxUI.Green);
                }
                else if(selectedPiece!=null)
                {
                    bool own=selectedPiece.owner==s.activePlayer&&Game.CanAct;
                    GrayboxUI.Button(inspector,"Mover",14,433,98,34,()=>Begin("move"),null,own&&!card.IsStructure);
                    GrayboxUI.Button(inspector,"Atacar",120,433,98,34,()=>Begin("attack"),null,own&&!card.IsStructure);
                    GrayboxUI.Button(inspector,"+1 paso",225,433,98,34,()=>Begin("step"),null,own&&s.Active.freeSteps>0&&!card.IsStructure);
                    GrayboxUI.Button(inspector,AbilityManager.Label(card),14,474,309,33,()=>{if(AbilityManager.TargetCount(card)==0)AbilityManager.Activate(Game,selectedPiece,new List<int>());else Begin("ability");},null,own&&AbilityManager.HasActive(card)&&!selectedPiece.abilityUsed&&!Game.IsSleeping(selectedPiece));
                }
            }
            else
            {
                GrayboxUI.Text(inspector,tile==null?"Cinco tierras.\nUn mundo por conquistar.":"HEX "+(tile.id+1)+" · "+Names.Biomes[(int)tile.biome],14,48,307,84,24,GrayboxUI.Ink,FontStyle.Bold);
                GrayboxUI.Text(inspector,tile==null?"Todas las casillas empiezan sin bioma. Selecciona una carta de tu mano o una unidad del tablero.":"Territorio: "+(tile.territory==4?"Centro":"J"+(tile.territory+1))+"\nControl: "+(tile.owner<0?"Nadie":"J"+(tile.owner+1))+"\nRecurso estratégico: +"+tile.resource+"\n"+(tile.specialEffect!=null?"Terreno inestable: d6 "+tile.specialEffect.threshold+"+":"Sin efecto de terreno"),14,137,307,107,16,GrayboxUI.Muted);
                GrayboxUI.Text(inspector,"TERRAFORMACIÓN",14,260,307,22,14,GrayboxUI.Muted,FontStyle.Bold);
                var biomes=new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland};
                for(int i=0;i<biomes.Length;i++){var b=biomes[i];GrayboxUI.Button(inspector,Names.Biomes[(int)b],14+(i%2)*158,294+(i/2)*39,150,33,()=>{chosenBiome=b;Begin("terraform");},chosenBiome==b?GrayboxUI.BiomeColor(b):edge);}
                GrayboxUI.Text(inspector,"Coste base: "+s.rules.terraformCost+" Energía.\nRevelar ceniza: "+s.rules.revealAshCost+" Energía.",14,421,307,41,14,GrayboxUI.Muted);
                GrayboxUI.Button(inspector,"Revelar Tierra Ceniza",14,474,309,33,()=>Begin("ash"));
            }
            if(mode=="card"&&card!=null&&card.effects.Any(e=>e.biome=="ChooseForestSwamp"))
                GrayboxUI.Button(inspector,"Bioma: "+Names.Biomes[(int)chosenBiome],14,474,309,33,()=>{chosenBiome=chosenBiome==Biome.Forest?Biome.Swamp:Biome.Forest;Render();});
            if(targets.Count>0&&(mode=="card"||mode=="leader"||mode=="ability"))GrayboxUI.Button(inspector,"Resolver selección ("+targets.Count+")",14,516,199,36,Confirm,Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.55f));
            GrayboxUI.Button(inspector,"Cancelar",221,516,102,36,()=>{ClearAction();Render();});
            GrayboxUI.Text(inspector,Game.LastMessage,14,565,307,85,16,GrayboxUI.Yellow);
        }
        string ModeName()=>new Dictionary<string,string>{{"inspect","INFORMACIÓN"},{"card","JUGAR CARTA"},{"move","MOVER"},{"attack","ATACAR"},{"terraform","TERRAFORMAR"},{"ash","TIERRA CENIZA"},{"ability","HABILIDAD"},{"leader","LÍDER"},{"step","PASO EXTRA"}}[mode];
        void DrawHand(Player player)
        {
            GrayboxUI.Text(hand,"MANO J"+(player.id+1)+" · "+player.hand.Count+" cartas",14,10,330,23,15,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
            GrayboxUI.Button(hand,"Registro",960,5,107,29,ShowLog);GrayboxUI.Button(hand,"Guardar",1075,5,107,29,Save);GrayboxUI.Button(hand,"Cargar",1190,5,107,29,Load);
            GrayboxUI.Button(hand,"<",1370,5,40,29,()=>{page=Math.Max(0,page-1);Render();});GrayboxUI.Text(hand,(page+1).ToString(),1424,10,30,20,14);GrayboxUI.Button(hand,">",1462,5,40,29,()=>{page=Math.Min(Math.Max(0,(player.hand.Count-1)/8),page+1);Render();});
            int i=0;
            foreach(var instance in player.hand.Skip(page*8).Take(8))
            {
                var c=Game.Catalog[instance.cardId];int id=instance.instanceId;bool chosen=id==selectedCard;var color=Color.Lerp(GrayboxUI.PlayerColor(player.id),GrayboxUI.Panel,chosen?.45f:.85f);
                var rect=GrayboxUI.Box(hand,c.name,14+i*193,41,181,167,color);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=rect.GetComponent<Image>();button.onClick.AddListener(()=>{ClearAction();selectedCard=id;mode="card";chosenBiome=Biome.Forest;Game.Notify(viewedPlayer==Game.State.activePlayer?"Selecciona un objetivo válido.":"Consulta de J"+(viewedPlayer+1)+". Las acciones corresponden al jugador activo.");});
                GrayboxUI.Text(rect,c.energyCost.ToString(),11,6,30,34,26,GrayboxUI.PlayerColor(player.id),FontStyle.Bold);
                GrayboxUI.Text(rect,Names.Categories[(int)c.category].ToUpperInvariant(),44,14,129,22,11,GrayboxUI.Muted,FontStyle.Bold);
                GrayboxUI.Text(rect,c.name,11,45,160,48,18,GrayboxUI.Ink,FontStyle.Bold);
                GrayboxUI.Text(rect,c.category==Category.Spell?c.description:((c.IsStructure?"ESTRUCTURA":"ATQ "+c.attack+"  MOV "+c.movement)+"\nVIDA "+c.health+"  ALC "+c.range),11,99,158,44,12,GrayboxUI.Muted);
                GrayboxUI.Text(rect,c.requiresAshLand?"REQUIERE TIERRA CENIZA":string.Join(" / ",c.biomes.Select(b=>Names.Biomes[(int)b])),11,146,160,18,10,GrayboxUI.PlayerColor(player.id));i++;
            }
            if(player.hand.Count==0)GrayboxUI.Text(hand,"Mano vacía. Robarás automáticamente al comenzar tu próximo turno.",18,90,1100,35,20,GrayboxUI.Muted);
        }
        void Begin(string action){mode=action;selectedCard=-1;targets.Clear();if(action=="terraform"||action=="ash"||action=="leader")selectedPiece=null;Game.Notify("Modo "+ModeName()+". Selecciona las casillas resaltadas.");}
        void ClearAction(){mode="inspect";selectedCard=-1;selectedPiece=null;targets.Clear();}
        public void TileClick(int id)
        {
            if(ValidTargets().Contains(id))
            {
                bool done=false;
                switch(mode)
                {
                    case "card":case "leader":case "ability":
                        targets.Add(id);
                        int count=mode=="leader"?(Game.State.Active.leader=="SAHRIA"?2:1):mode=="ability"?AbilityManager.TargetCount(Game.Data(selectedPiece)):MaxCardTargets();
                        if(targets.Count>=count)Confirm();else{Game.Notify("Selección: "+string.Join(", ",targets.Select(t=>t+1))+". Puedes añadir objetivos o resolver.");}return;
                    case "move":done=MovementManager.Move(Game,selectedPiece,id);break;
                    case "attack":done=CombatManager.Attack(Game,selectedPiece,id);break;
                    case "terraform":done=Game.Terraform(id,chosenBiome);break;
                    case "ash":done=Game.RevealAsh(id);break;
                    case "step":done=MovementManager.FreeStep(Game,selectedPiece,id);break;
                }
                if(done){ClearAction();focus=id;}Render();return;
            }
            if(mode!="inspect"&&mode!="move") {Game.Notify("Casilla no válida. Cancela la acción para inspeccionarla.");return;}
            ClearAction();focus=id;selectedPiece=Game.State.tiles[id].unit??Game.State.tiles[id].structure;
            if(selectedPiece!=null&&selectedPiece.owner==Game.State.activePlayer&&viewedPlayer==Game.State.activePlayer&&!Game.Data(selectedPiece).IsStructure)mode="move";
            Render();
        }
        int MaxCardTargets()
        {
            var inst=Game.State.Active.hand.Find(c=>c.instanceId==selectedCard);if(inst==null)return 1;var card=Game.Catalog[inst.cardId];return card.category==Category.Spell?card.effects[0].count*(card.effects[0].operation=="March"?2:1):1;
        }
        void Confirm()
        {
            bool ok=mode=="card"?Game.Play(selectedCard,targets,useResources,chosenBiome):mode=="leader"?AbilityManager.Leader(Game,targets):AbilityManager.Activate(Game,selectedPiece,targets);
            if(ok)ClearAction();Render();
        }
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
            GrayboxUI.Button(body,"COMENZAR PARTIDA",26,620,510,64,()=>{CloseModal();ClearAction();viewedPlayer=0;focus=-1;page=0;board.Zoom=1;board.Pan=Vector2.zero;Game.NewGame(leaders,seed,CardCatalog.LoadRules());},Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.55f));
            GrayboxUI.Button(body,"Cargar escenario de pruebas",563,620,510,64,()=>{CloseModal();ClearAction();viewedPlayer=0;focus=-1;page=0;Game.NewGame(leaders,seed,CardCatalog.LoadRules());PrototypeScenario.Load(Game);Game.Notify("Escenario de pruebas: biomas y unidades preparadas. No es el inicio normal de partida.");});
            GrayboxUI.Text(body,"El escenario prepara combate, biomas, ceniza y vías rápidas para comprobar reglas. La partida normal usa los mazos barajados.",26,711,1040,53,16,GrayboxUI.Muted);
        }
        void ShowLog(){var body=OpenModal("Registro de la partida");GrayboxUI.Text(body,string.Join("\n",Game.State.log.Skip(Math.Max(0,Game.State.log.Count-29))),26,80,1070,684,19,GrayboxUI.Muted);}
        void ShowDeck(Player player)
        {
            var body=OpenModal("J"+(player.id+1)+" · "+player.leader+" · Main Deck 50");
            var cards=Game.Catalog.All.Where(c=>c.leader==player.leader&&c.quantity>0).ToList();
            for(int i=0;i<cards.Count;i++)
            {var c=cards[i];int col=i/16,row=i%16;GrayboxUI.Text(body,c.quantity+"× "+c.name+"  · E"+c.energyCost,26+col*541,80+row*35,520,31,17,GrayboxUI.Muted);}
            GrayboxUI.Text(body,"Mazo restante: "+player.deck.Count+"  ·  Mano: "+player.hand.Count+"  ·  Descarte: "+player.discardPile.Count,26,655,1050,31,18);
            GrayboxUI.Text(body,"Descarte: "+string.Join(", ",player.discardPile.Select(c=>Game.Catalog[c.cardId].name)),26,700,1070,73,15,GrayboxUI.Muted);
        }
        void Save(){try{File.WriteAllText(SavePath,GamePersistence.Serialize(Game.State));Game.Notify("Partida guardada localmente.");}catch(Exception e){Game.Notify("No se pudo guardar: "+e.Message);}}
        void Load()
        {
            try
            {
                if(!File.Exists(SavePath)){Game.Notify("Todavía no hay una partida guardada.");return;}
                var state=GamePersistence.Deserialize(File.ReadAllText(SavePath));
                string error=StateValidator.Validate(state,Game.Catalog);if(error.Length>0){Game.Notify("Guardado no válido: "+error);return;}
                ClearAction();viewedPlayer=state.activePlayer;focus=-1;page=0;Game.Restore(state);
            }catch(Exception e){Game.Notify("No se pudo cargar: "+e.Message);}
        }
    }
}
