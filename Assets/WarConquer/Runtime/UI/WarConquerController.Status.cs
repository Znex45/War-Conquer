using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public partial class WarConquerController
    {
        CanvasGroup selectionActionGroup;
        void RefreshSelectionActions()
        {if(selectionActionGroup!=null)selectionActionGroup.interactable=board?.World==null||!board.World.IsAnimating;}
        void DrawEffectDisplayControl(Transform parent,float x,float y,float width,float height)
        {
            string label=board.World.EffectDisplay==BoardEffectDisplay.Compact?"Efectos: compacto":board.World.EffectDisplay==BoardEffectDisplay.SelectedOnly?"Efectos: selección":"Efectos: ocultos";
            GrayboxUI.Button(parent,label,x,y,width,height,()=>
            {board.World.EffectDisplay=(BoardEffectDisplay)(((int)board.World.EffectDisplay+1)%3);Render();});
        }
        void SelectOccupant(Piece piece)
        {
            selectedPiece=piece;selectedCard=-1;mode="inspect";targets.Clear();Render();
        }
        void DrawScoreboard()
        {
            var s=Game.State;GrayboxUI.Text(inspector,"CONQUISTA · 10 PC",12,12,220,26,16,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Button(inspector,scoreDetails?"Menos":"Detalles",235,10,78,28,()=>{scoreDetails=!scoreDetails;Render();});
            for(int i=0;i<4;i++)
            {
                int id=i;var p=s.players[i];float y=46+i*(scoreDetails?70:52);var color=GrayboxUI.PlayerColor(i);
                var row=GrayboxUI.Box(inspector,"Puntuación J"+(i+1),10,y,304,scoreDetails?65:47,i==viewedPlayer?Color.Lerp(color,GrayboxUI.Panel,.88f):Color.Lerp(edge,GrayboxUI.Panel,.65f));
                if(p.inactive){GrayboxUI.Text(row,"PUESTO "+(i+1)+" · SIN JUGADOR",8,9,287,27,12,GrayboxUI.Muted);continue;}
                var b=row.gameObject.AddComponent<UnityEngine.UI.Button>();b.onClick.AddListener(()=>{viewedPlayer=id;page=0;handFilter="Todas";ClearAction();Render();});
                GrayboxUI.Text(row,"J"+(i+1)+" "+p.leader+(p.isAI?" · IA":"")+(p.eliminated?" · FUERA":""),8,4,209,22,13,color,FontStyle.Bold);
                GrayboxUI.Text(row,p.conquestPoints+" / 10",220,3,82,25,18,color,FontStyle.Bold);
                string control=string.Join("  ",new[]{"B1","B2","B3","B4","C"}.Select((name,n)=>name+":"+s.tiles.Count(t=>t.territory==n&&t.owner==id)));
                GrayboxUI.Text(row,"Vida "+p.leaderHealth+" · +"+ConquestManager.Income(s,id)+" PC/turno"+(scoreDetails?" · faltan "+Mathf.Max(0,10-p.conquestPoints):""),8,25,290,18,11,GrayboxUI.Muted);
                if(scoreDetails)GrayboxUI.Text(row,control,8,43,290,17,11,GrayboxUI.Muted);
                GrayboxUI.Box(row,"Progreso",0,scoreDetails?62:44,304*Mathf.Clamp01(p.conquestPoints/10f),3,color);
            }
            if(scoreDetails)GrayboxUI.Text(inspector,"B1/B2/B3/B4/C: bases y zona central",13,330,296,18,10,GrayboxUI.Muted);
        }
        void DrawSelection()
        {
            selectionActionGroup=null;
            var s=Game.State;var box=GrayboxUI.Rect(inspector,"Detalle de selección",12,scoreDetails?358:280,300,274);
            var focused=focus>=0?s.tiles[focus]:null;
            if(focused?.unit!=null&&focused.structure!=null&&selectedPiece!=null)
            {
                GrayboxUI.Button(box,"Unidad",0,-2,101,26,()=>SelectOccupant(focused.unit),selectedPiece==focused.unit?GrayboxUI.Green:(Color?)null);
                GrayboxUI.Button(box,"Estructura",108,-2,103,26,()=>SelectOccupant(focused.structure),selectedPiece==focused.structure?GrayboxUI.Green:(Color?)null);
            }
            else GrayboxUI.Text(box,ModeName(),0,0,209,23,14,GrayboxUI.Muted,FontStyle.Bold);
            if(selectedCard>=0||selectedPiece!=null||mode!="inspect")GrayboxUI.Button(box,"Cancelar",219,-2,81,26,()=>{ClearAction();Render();});
            var instance=s.players[viewedPlayer].hand.Find(c=>c.instanceId==selectedCard);
            var card=instance!=null?Game.Catalog[instance.cardId]:selectedPiece!=null?Game.Data(selectedPiece):null;
            if(card!=null)
            {
                GrayboxUI.Text(box,card.name,0,32,300,45,21,GrayboxUI.PlayerColor(selectedPiece?.owner??viewedPlayer),FontStyle.Bold);
                var quote=EnergyManager.Quote(s.players[selectedPiece?.owner??viewedPlayer],card,useResources);
                string stats="E "+quote.EnergyLabel+" · HP "+(selectedPiece!=null?selectedPiece.health+"/"+Game.MaxHealth(selectedPiece):card.health.ToString())+" · ATK "+(selectedPiece!=null?CombatManager.AttackValue(Game,selectedPiece):card.attack)+" · MOV "+(selectedPiece?.remainingMovement??card.movement);
                var effects=selectedPiece!=null?PieceStatusPresentation.Collect(Game,selectedPiece):null;
                string summary=effects==null?"":effects.Count==0?"Sin estados activos":string.Join(" · ",effects.Take(2).Select(e=>e.label))+(effects.Count>2?" · +"+(effects.Count-2):"");
                GrayboxUI.Text(box,stats,0,80,300,35,14);GrayboxUI.Text(box,instance!=null?quote.Detail:Names.Categories[(int)card.category]+" · ALC "+card.range+(selectedPiece!=null?"\n"+summary:""),0,116,300,31,12,GrayboxUI.Muted);
                GrayboxUI.Button(box,"Ver carta completa",0,153,190,28,()=>ShowCardModal(card,s.players[selectedPiece?.owner??viewedPlayer]));
                if(selectedPiece!=null)GrayboxUI.Button(box,"Estados ("+effects.Count+")",199,153,101,28,()=>ShowPieceStates(selectedPiece),GrayboxUI.Purple);
                if(selectedPiece!=null&&!Game.ActingPlayer.isAI&&Game.CanTakeTurnAction(TurnStage.Assault)&&selectedPiece.owner==Game.ActingPlayerId&&!(targets.Count>0&&(mode=="card"||mode=="leader"||mode=="ability")))
                {
                    var actions=GrayboxUI.Rect(box,"Acciones de la unidad",0,187,300,29);selectionActionGroup=actions.gameObject.AddComponent<CanvasGroup>();
                    GrayboxUI.Button(actions,"Mover",0,0,92,29,()=>{if(!board.World.IsAnimating)Begin("move");},null,MovementManager.Paths(Game,selectedPiece).Count>0);
                    GrayboxUI.Button(actions,"Atacar",101,0,92,29,()=>{if(!board.World.IsAnimating)Begin("attack");},null,CombatManager.Targets(Game,selectedPiece).Count>0);
                    GrayboxUI.Button(actions,"+1 paso",202,0,98,29,()=>{if(!board.World.IsAnimating)Begin("step");},null,Game.ActingPlayer.freeSteps>0);
                }
                else if(instance!=null)GrayboxUI.Text(box,viewedPlayer!=Game.ActingPlayerId?"Consulta de otra mano.":Game.CardBlockReason(instance,useResources),0,188,300,38,12,GrayboxUI.Yellow);
                if(selectedPiece!=null)
                {
                    string availability=Game.IsSleeping(selectedPiece)?"DORMIDO · No puede actuar":card.IsStructure?AbilityManager.HasActive(card)?selectedPiece.abilityUsed?"HABILIDAD YA USADA":AbilityManager.CanActivate(Game,selectedPiece)?"HABILIDAD DISPONIBLE":"HABILIDAD NO DISPONIBLE":"ESTRUCTURA · EFECTOS PASIVOS":!FactionCardRules.CanAttack(Game,selectedPiece)?"NO ATACA EL TURNO EN QUE SE JUEGA":selectedPiece.attacked&&!XiltharRules.CanSecondAttack(Game,selectedPiece)?"ATAQUE YA USADO":CombatManager.Targets(Game,selectedPiece).Count>0?XiltharRules.CanSecondAttack(Game,selectedPiece)?"SEGUNDO ATAQUE · REEMPLAZA MOV":"PUEDE ATACAR": "SIN ATAQUE DISPONIBLE";
                    GrayboxUI.Text(box,availability,0,218,300,18,11,Game.IsSleeping(selectedPiece)?GrayboxUI.Yellow:GrayboxUI.Green);
                }
            }
            else if(mode=="terraform")
            {
                GrayboxUI.Text(box,"Elige bioma y casilla",0,33,300,34,21,GrayboxUI.Ink,FontStyle.Bold);
                var biomes=new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland,Biome.Void};
                for(int i=0;i<biomes.Length;i++){var biome=biomes[i];GrayboxUI.Button(box,Names.Biomes[(int)biome],(i%2)*153,76+(i/2)*35,147,30,()=>{chosenBiome=biome;Render();},chosenBiome==biome?GrayboxUI.BiomeColor(biome):edge);}
                GrayboxUI.Text(box,"Coste: "+Game.TerraformCost(chosenBiome)+" E"+(Game.TerraformCost(chosenBiome)<s.rules.terraformCost?" ("+s.rules.terraformCost+" original)":""),0,190,300,25,13,GrayboxUI.Muted);
            }
            else
            {
                var tile=focus>=0?s.tiles[focus]:null;
                GrayboxUI.Text(box,tile==null?"Selecciona una carta\no una unidad.":"HEX "+(tile.id+1)+" · "+Names.Biomes[(int)tile.biome],0,34,300,57,21,GrayboxUI.Ink,FontStyle.Bold);
                string info=tile==null?"Consulta los detalles aquí.\nLas reglas y controles están en Menú → Ayuda.":"Control: "+(tile.owner<0?"Nadie":"J"+(tile.owner+1))+"\nZona: "+(tile.territory==4?"Centro":"J"+(tile.territory+1))+" · Recurso +"+tile.resource+(tile.specialEffect!=null?"\nTerreno inestable: "+tile.specialEffect.threshold+"+":"");
                GrayboxUI.Text(box,info,0,102,300,96,14,GrayboxUI.Muted);
            }
            if(targets.Count>0&&(mode=="card"||mode=="leader"||mode=="ability"))
            {
                var resolve=GrayboxUI.Button(box,"Resolver selección ("+targets.Count+")",0,184,300,31,()=>{if(!board.World.IsAnimating)Confirm();},Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.6f));
                selectionActionGroup=resolve.gameObject.AddComponent<CanvasGroup>();
            }
            if(mode=="card"&&card?.id=="terraform")GrayboxUI.Button(box,Names.Biomes[(int)chosenBiome],199,153,101,28,()=>{var list=new[]{Biome.Forest,Biome.Swamp,Biome.Tundra,Biome.Volcanic,Biome.Desert,Biome.Wasteland,Biome.Void};chosenBiome=list[(System.Array.IndexOf(list,chosenBiome)+1)%list.Length];Render();});
            if(mode=="card"&&card!=null&&card.effects.Any(e=>e.biome=="ChooseForestSwamp"))GrayboxUI.Button(box,Names.Biomes[(int)chosenBiome],199,153,101,28,()=>{chosenBiome=chosenBiome==Biome.Forest?Biome.Swamp:Biome.Forest;Render();});
            GrayboxUI.Text(box,Game.LastMessage,0,232,300,45,12,GrayboxUI.Yellow);
            RefreshSelectionActions();
        }
        void ShowPieceStates(Piece piece)
        {
            if(piece==null||!BoardManager.Pieces(Game.State).Contains(piece))return;
            HideTooltip();var effects=PieceStatusPresentation.Collect(Game,piece);var card=Game.Data(piece);
            var body=OpenModal("ESTADOS, EFECTOS Y MEJORAS");
            GrayboxUI.Text(body,card.name+" · J"+(piece.owner+1)+" · HEX "+(piece.tileId+1),24,70,1078,35,24,GrayboxUI.PlayerColor(piece.owner),FontStyle.Bold);
            GrayboxUI.Text(body,"Vida "+piece.health+" / "+Game.MaxHealth(piece)+"    Ataque "+CombatManager.AttackValue(Game,piece)+"    Movimiento disponible "+piece.remainingMovement+"    ·    "+effects.Count+" efectos activos",24,109,1078,30,16,GrayboxUI.Ink);
            var viewport=GrayboxUI.Box(body,"Lista de estados",24,153,1064,616,GrayboxUI.Background);
            viewport.gameObject.AddComponent<RectMask2D>();
            var heights=effects.Select(e=>Mathf.Max(132,80+Mathf.CeilToInt(e.description.Length/100f)*22)).ToArray();
            var content=GrayboxUI.Rect(viewport,"Contenido de estados",0,0,1064,Mathf.Max(616,heights.Sum()+effects.Count*8+12));
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=42;
            var track=GrayboxUI.Box(body,"Desplazamiento de estados",1094,153,10,616,GrayboxUI.Disabled);
            var handle=GrayboxUI.Box(track,"Control",0,0,10,100,GrayboxUI.Muted);
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=handle.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            if(effects.Count==0)GrayboxUI.Text(content,"Esta carta no tiene estados ni mejoras activos.\nLos valores mostrados corresponden a su situación actual en el tablero.",24,26,1016,90,20,GrayboxUI.Muted);
            float rowY=8;
            for(int i=0;i<effects.Count;i++)
            {
                var effect=effects[i];var row=GrayboxUI.Box(content,"Detalle · "+effect.id,10,rowY,1044,heights[i],GrayboxUI.Panel);
                GrayboxUI.Box(row,"Color del efecto",0,0,5,heights[i],effect.color).GetComponent<Image>().raycastTarget=false;
                GrayboxUI.Text(row,effect.label,20,9,1000,27,20,effect.color,FontStyle.Bold);
                GrayboxUI.Text(row,effect.duration,20,40,1000,22,14,GrayboxUI.Muted);
                GrayboxUI.Text(row,effect.description,20,68,1000,heights[i]-74,17,GrayboxUI.Ink);
                rowY+=heights[i]+8;
            }
        }
        void ShowVictory()
        {
            if(modal!=null&&modal.name=="Victoria")return;CloseModal();
            var s=Game.State;modal=GrayboxUI.Box(root,"Victoria",0,0,1600,1000,new Color(0,0,0,.9f));
            var box=GrayboxUI.Box(modal,"Ganador",325,255,950,490,GrayboxUI.Panel);
            GrayboxUI.Text(box,"JUGADOR "+(s.winner+1)+" HA GANADO",45,50,860,75,40,GrayboxUI.PlayerColor(s.winner),FontStyle.Bold);
            GrayboxUI.Text(box,ConquestManager.Reason(s.victoryReason),45,155,860,65,29,GrayboxUI.Ink,FontStyle.Bold);
            GrayboxUI.Text(box,s.players[s.winner].leader+" · "+s.players[s.winner].conquestPoints+" Puntos de Conquista",45,245,860,52,23,GrayboxUI.Muted);
            GrayboxUI.Button(box,"Nueva partida",45,361,860,64,ShowSetup,Color.Lerp(GrayboxUI.PlayerColor(s.winner),GrayboxUI.Panel,.5f));
        }
    }
}
