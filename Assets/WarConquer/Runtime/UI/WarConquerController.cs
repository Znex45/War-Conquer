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
    public partial class WarConquerController : MonoBehaviour
    {
        public GameManager Game { get; private set; }
        RectTransform root,matchRoot,header,left,leaderPanel,boardPanel,inspector,hand,piles,modal,tooltip;
        bool setupVisible,scoreDetails;
        BoardView board;
        string mode="inspect",handFilter="Todas";
        int viewedPlayer,focus=-1,selectedCard=-1,page,lastActor=-1;
        [NonSerialized] Piece selectedPiece;
        readonly List<int> targets=new List<int>();
        bool useResources;
        Biome chosenBiome=Biome.Forest;
        string[] leaders={"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"};
        int seed=2026;
        int humanPlayers=1;
        readonly Color edge=new Color32(48,61,79,255);
        string SavePath=>Path.Combine(Application.persistentDataPath,"war-conquer-save.json");
        void Awake()
        {
            ClearAction();focus=-1;viewedPlayer=0;page=0;lastActor=-1;useResources=false;
            Game=new GameManager(CardCatalog.Load());CreateUI();Game.Changed+=Render;
            Game.NewGame(leaders,seed,CardCatalog.LoadRules(),4,false);ShowSetup();
        }
        void OnDestroy(){if(Game!=null)Game.Changed-=Render;board?.Dispose();}
        void CreateUI()
        {
            if(FindAnyObjectByType<EventSystem>()==null)
            {
                var events=new GameObject("WarConquer EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(transform);
            }
            var canvasObject=new GameObject("WarConquer Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.transform.SetParent(transform);
            canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=Mathf.Min(Screen.width/1600f,Screen.height/1000f);
            var background=canvasObject.GetComponent<RectTransform>();background.gameObject.AddComponent<Image>().color=GrayboxUI.Background;
            root=GrayboxUI.Rect(background,"Graybox",0,0,1600,1000);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            matchRoot=GrayboxUI.Rect(root,"Interfaz de partida",0,0,1600,1000);
            matchRoot.gameObject.AddComponent<CanvasGroup>();
            header=GrayboxUI.Rect(matchRoot,"Header",0,0,1600,66);
            left=GrayboxUI.Box(matchRoot,"Acciones de etapa",16,76,238,326,GrayboxUI.Panel);
            leaderPanel=GrayboxUI.Box(matchRoot,"Líder independiente",16,412,238,312,GrayboxUI.Panel);
            boardPanel=GrayboxUI.Box(matchRoot,"Board",266,76,982,648,new Color32(17,29,38,255));boardPanel.gameObject.AddComponent<RectMask2D>();board=new BoardView(boardPanel,this);
            inspector=GrayboxUI.Box(matchRoot,"Puntuación y selección",1260,76,324,648,GrayboxUI.Panel);
            hand=GrayboxUI.Box(matchRoot,"Hand",16,736,1268,248,GrayboxUI.Panel);
            piles=GrayboxUI.Box(matchRoot,"Mazo y descarte",1296,736,288,248,GrayboxUI.Panel);
        }
        void Update()
        {
            if(root==null)return;var canvas=root.GetComponentInParent<CanvasScaler>();float scale=Mathf.Min(Screen.width/1600f,Screen.height/1000f);
            if(Mathf.Abs(canvas.scaleFactor-scale)>.001f)canvas.scaleFactor=scale;
            board?.Tick();
            matchRoot.GetComponent<CanvasGroup>().interactable=!board.World.IsAnimating;
            UpdateAI();
        }
        public void Render()
        {
            if(Game?.State==null)return;
            SetMatchVisible(!setupVisible&&Game.State.phase!=Phase.Setup);
            int actor=Game.ActingPlayerId;
            if(actor!=lastActor){lastActor=actor;viewedPlayer=actor;page=0;handFilter="Todas";useResources=Game.ActingPlayer.isAI&&AiPlayer.UseResources;ClearAction();focus=-1;nextAiAction=Time.unscaledTime+1;}
            if(selectedPiece!=null&&!BoardManager.Pieces(Game.State).Contains(selectedPiece))ClearAction();
            HideTooltip();foreach(var panel in new[]{header,left,leaderPanel,inspector,hand,piles})GrayboxUI.Clear(panel);
            var s=Game.State;GrayboxUI.PlayerLeaders=s.players.Select(p=>p.leader).ToArray();var player=s.players[viewedPlayer];
            DrawHeader();DrawTurnActions();DrawLeader(player);DrawScoreboard();DrawSelection();DrawHand(player);DrawPiles(player);
            board.Render(Game,new HashSet<int>(ValidTargets()),new HashSet<int>(targets),focus);
            var chrome=board.Overlay;
            GrayboxUI.Text(chrome,"TABLERO",16,16,200,22,12,GrayboxUI.Muted);
            GrayboxUI.Button(chrome,board.World.ShowLabels?"Ocultar datos":"Ver datos",573,11,107,30,()=>{board.World.ShowLabels=!board.World.ShowLabels;Render();});
            GrayboxUI.Button(chrome,"Girar -",690,11,72,30,()=>board.Rotate(-30));GrayboxUI.Button(chrome,"Girar +",771,11,72,30,()=>board.Rotate(30));
            GrayboxUI.Button(chrome,"−",854,11,33,30,()=>{board.Zoom=Mathf.Max(1,board.Zoom-.25f);Render();});
            GrayboxUI.Button(chrome,"+",892,11,33,30,()=>{board.Zoom=Mathf.Min(5f,board.Zoom+.25f);Render();});
            GrayboxUI.Button(chrome,"1:1",930,11,42,30,()=>{board.Reset();Render();});
            if(board.Zoom>1)
            {
                GrayboxUI.Button(chrome,"←",840,48,30,28,()=>{board.Pan+=new Vector2(80,0);Render();});GrayboxUI.Button(chrome,"→",875,48,30,28,()=>{board.Pan-=new Vector2(80,0);Render();});
                GrayboxUI.Button(chrome,"↑",910,48,30,28,()=>{board.Pan+=new Vector2(0,80);Render();});GrayboxUI.Button(chrome,"↓",945,48,30,28,()=>{board.Pan-=new Vector2(0,80);Render();});
            }
            GrayboxUI.Text(chrome,"Arrastrar: desplazar · Botón derecho: girar · Rueda: acercar · Borde verde: objetivo válido",16,620,950,20,12,GrayboxUI.Muted);
            if(s.battle!=null)
            {
                var b=s.battle;var banner=GrayboxUI.Box(chrome,"Batalla",210,42,570,46,new Color32(85,49,65,255));
                GrayboxUI.Text(banner,"BATALLA  J"+(b.attackerOwner+1)+" → J"+(b.defenderOwner+1)+"   ·   RESPONDE J"+(b.priorityPlayer+1),12,10,546,28,18,GrayboxUI.Ink,FontStyle.Bold);
            }
            if(s.pendingChoices.Count>0&&s.pendingChoices[0].kind=="Discard"&&!Game.ActingPlayer.isAI)ShowDiscardChoice();
            if(s.phase==Phase.Finished)ShowVictory();
        }
        void DrawHeader()
        {
            var s=Game.State;var actor=Game.ActingPlayer;
            GrayboxUI.Text(header,"WAR & CONQUER",20,12,300,34,26,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(header,s.phase==Phase.Setup?"PREPARACIÓN · PARTIDA EN PAUSA":"RONDA "+s.round+" · TURNO J"+(s.activePlayer+1)+(s.Active.isAI?" (IA)":"")+" · "+TimingRules.StageName(s.stage),340,12,595,27,18,GrayboxUI.PlayerColor(s.activePlayer),FontStyle.Bold);
            GrayboxUI.Text(header,"J"+(actor.id+1)+" · ENERGÍA "+actor.currentEnergy+" / "+actor.maxEnergy,960,10,325,28,21,GrayboxUI.PlayerColor(actor.id),FontStyle.Bold);
            GrayboxUI.Text(header,string.Join(" · ",actor.resources.Where(r=>r.amount>0).Select(r=>Names.Biomes[(int)r.biome]+" "+r.amount))+"  Esporas "+actor.spores,960,39,380,20,11,GrayboxUI.Muted);
            GrayboxUI.Button(header,"Menú",1402,14,182,36,ShowMatchMenu);
        }
        List<int> ValidTargets()
        {
            if(!Game.CanAct||Game.ActingPlayer.isAI)return new List<int>();
            if(Game.State.pendingChoices.Count>0)return Game.State.pendingChoices[0].kind=="Discard"?new List<int>():ChoiceManager.Targets(Game,Game.State.pendingChoices[0]);
            if(viewedPlayer!=Game.ActingPlayerId)return new List<int>();
            switch(mode)
            {
                case "card": var instance=Game.ActingPlayer.hand.Find(c=>c.instanceId==selectedCard);return instance!=null&&Game.CardBlockReason(instance,useResources)==""?Game.CardTargets(Game.Catalog[instance.cardId],targets):new List<int>();
                case "move": return MovementManager.Paths(Game,selectedPiece).Keys.ToList();
                case "attack":return CombatManager.Targets(Game,selectedPiece);
                case "terraform":if(!Game.CanTakeTurnAction(TurnStage.Terraforming)||Game.ActingPlayer.currentEnergy<Game.TerraformCost(chosenBiome))return new List<int>();return Game.State.tiles.Where(t=>Game.TerraformTarget(t.id)&&t.biome!=chosenBiome).Select(t=>t.id).ToList();
                case "ash":if(!Game.CanTakeTurnAction(TurnStage.Terraforming)||Game.ActingPlayer.currentEnergy<Game.AshCost())return new List<int>();return Game.State.tiles.Where(t=>t.owner==Game.ActingPlayerId&&TerrainManager.Normal(t)&&Game.TerraformTarget(t.id)).Select(t=>t.id).ToList();
                case "leader":return AbilityManager.LeaderTargets(Game).Except(targets).ToList();
                case "ability":return AbilityManager.Targets(Game,selectedPiece).Except(targets).ToList();
                case "step":return selectedPiece==null?new List<int>():Game.State.tiles[selectedPiece.tileId].neighbors.Where(n=>!Game.State.tiles[n].IsOccupied&&!Game.State.tiles[n].blocked&&Game.State.tiles[n].baseOwner<0).ToList();
                default:return new List<int>();
            }
        }
        string ModeName()=>mode=="inspect"?"INFORMACIÓN":mode=="card"?"JUGAR CARTA":mode=="terraform"?"TERRAFORMAR":mode=="ash"?"TIERRA CENIZA":mode=="move"?"MOVER":mode=="attack"?"ATACAR":"HABILIDAD";
        void Begin(string action){mode=action;selectedCard=-1;targets.Clear();if(action=="terraform"||action=="ash"||action=="leader")selectedPiece=null;Game.Notify("Modo "+ModeName()+". Selecciona las casillas resaltadas.");}
        void ClearAction(){mode="inspect";selectedCard=-1;selectedPiece=null;targets.Clear();}
        public void TileClick(int id)
        {
            if(!Game.CanAct||Game.ActingPlayer.isAI||board.World.IsAnimating)return;
            if(Game.State.pendingChoices.Count>0){if(ValidTargets().Contains(id))ChoiceManager.Resolve(Game,new[]{id});return;}
            if(ValidTargets().Contains(id))
            {
                bool done=false;
                switch(mode)
                {
                    case "card":case "leader":case "ability":
                        targets.Add(id);
                        int count=mode=="leader"?(Game.ActingPlayer.leader=="SAHRIA"?2:1):mode=="ability"?AbilityManager.TargetCount(Game.Data(selectedPiece)):MaxCardTargets();
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
            if(selectedPiece!=null&&selectedPiece.owner==Game.ActingPlayerId&&viewedPlayer==Game.ActingPlayerId&&Game.CanTakeTurnAction(TurnStage.Assault)&&!Game.Data(selectedPiece).IsStructure)mode="move";
            Render();
        }
        int MaxCardTargets()
        {
            var inst=Game.ActingPlayer.hand.Find(c=>c.instanceId==selectedCard);if(inst==null)return 1;var card=Game.Catalog[inst.cardId];return GenericCardRules.MaxTargets(card);
        }
        void Confirm()
        {
            if(!Game.CanAct||Game.ActingPlayer.isAI)return;
            bool ok=mode=="card"?Game.Play(selectedCard,targets,useResources,chosenBiome):mode=="leader"?AbilityManager.Leader(Game,targets):AbilityManager.Activate(Game,selectedPiece,targets);
            if(ok)ClearAction();Render();
        }
    }
}
