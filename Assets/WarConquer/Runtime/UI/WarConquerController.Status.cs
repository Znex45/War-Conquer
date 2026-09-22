using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public partial class WarConquerController
    {
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
            var s=Game.State;var box=GrayboxUI.Rect(inspector,"Detalle de selección",12,scoreDetails?358:280,300,274);
            GrayboxUI.Text(box,ModeName(),0,0,209,23,14,GrayboxUI.Muted,FontStyle.Bold);
            if(selectedCard>=0||selectedPiece!=null||mode!="inspect")GrayboxUI.Button(box,"Cancelar",219,-2,81,26,()=>{ClearAction();Render();});
            var instance=s.players[viewedPlayer].hand.Find(c=>c.instanceId==selectedCard);
            var card=instance!=null?Game.Catalog[instance.cardId]:selectedPiece!=null?Game.Data(selectedPiece):null;
            if(card!=null)
            {
                GrayboxUI.Text(box,card.name,0,32,300,45,21,GrayboxUI.PlayerColor(viewedPlayer),FontStyle.Bold);
                var quote=EnergyManager.Quote(s.players[viewedPlayer],card,useResources);
                string stats="E "+quote.EnergyLabel+" · VIDA "+(selectedPiece?.health??card.health)+" · ATQ "+(selectedPiece!=null?CombatManager.AttackValue(Game,selectedPiece):card.attack)+" · MOV "+(selectedPiece?.remainingMovement??card.movement);
                GrayboxUI.Text(box,stats,0,80,300,35,14);GrayboxUI.Text(box,instance!=null?quote.Detail:Names.Categories[(int)card.category]+" · ALC "+card.range,0,116,300,29,12,GrayboxUI.Muted);
                GrayboxUI.Button(box,"Ver carta completa",0,153,190,28,()=>ShowCardModal(card,s.players[viewedPlayer]));
                if(selectedPiece!=null&&!Game.ActingPlayer.isAI&&Game.CanTakeTurnAction(TurnStage.Assault)&&selectedPiece.owner==Game.ActingPlayerId)
                {
                    GrayboxUI.Button(box,"Mover",0,187,92,29,()=>Begin("move"),null,MovementManager.Paths(Game,selectedPiece).Count>0);
                    GrayboxUI.Button(box,"Atacar",101,187,92,29,()=>Begin("attack"),null,CombatManager.Targets(Game,selectedPiece).Count>0);
                    GrayboxUI.Button(box,"+1 paso",202,187,98,29,()=>Begin("step"),null,Game.ActingPlayer.freeSteps>0);
                }
                else if(instance!=null)GrayboxUI.Text(box,viewedPlayer!=Game.ActingPlayerId?"Consulta de otra mano.":Game.CardBlockReason(instance,useResources),0,188,300,38,12,GrayboxUI.Yellow);
                if(selectedPiece!=null)GrayboxUI.Text(box,Game.IsSleeping(selectedPiece)?"DORMIDO · No puede mover ni atacar":selectedPiece.attacked?"ATAQUE YA USADO":CombatManager.Targets(Game,selectedPiece).Count>0?"PUEDE ATACAR": "SIN ATAQUE DISPONIBLE",0,218,300,18,11,Game.IsSleeping(selectedPiece)?GrayboxUI.Yellow:GrayboxUI.Green);
            }
            else if(mode=="terraform")
            {
                GrayboxUI.Text(box,"Elige bioma y casilla",0,33,300,34,21,GrayboxUI.Ink,FontStyle.Bold);
                var biomes=new[]{Biome.Forest,Biome.Swamp,Biome.Desert,Biome.Tundra,Biome.Volcanic,Biome.Wasteland};
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
            if(targets.Count>0&&(mode=="card"||mode=="leader"||mode=="ability"))GrayboxUI.Button(box,"Resolver selección ("+targets.Count+")",0,189,300,31,Confirm,Color.Lerp(GrayboxUI.Green,GrayboxUI.Panel,.6f));
            if(mode=="card"&&card?.id=="terraform")GrayboxUI.Button(box,Names.Biomes[(int)chosenBiome],199,153,101,28,()=>{var list=new[]{Biome.Forest,Biome.Swamp,Biome.Tundra,Biome.Volcanic,Biome.Desert,Biome.Wasteland};chosenBiome=list[(System.Array.IndexOf(list,chosenBiome)+1)%list.Length];Render();});
            if(mode=="card"&&card!=null&&card.effects.Any(e=>e.biome=="ChooseForestSwamp"))GrayboxUI.Button(box,Names.Biomes[(int)chosenBiome],199,153,101,28,()=>{chosenBiome=chosenBiome==Biome.Forest?Biome.Swamp:Biome.Forest;Render();});
            GrayboxUI.Text(box,Game.LastMessage,0,232,300,45,12,GrayboxUI.Yellow);
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
