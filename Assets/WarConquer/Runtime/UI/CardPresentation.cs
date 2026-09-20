using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WarConquer
{
    public static class CardPresentation
    {
        public static RectTransform Draw(Transform parent,GameManager game,CardData card,Player player,float x,float y,float w,float h,bool resources=false,bool large=false,Action click=null,Action enter=null,Action exit=null,Piece piece=null)
        {
            var color=GrayboxUI.PlayerColor(player.id);var quote=EnergyManager.Quote(player,card,resources);
            var root=GrayboxUI.Box(parent,card.name,x,y,w,h,Color.Lerp(color,GrayboxUI.Panel,.83f));
            GrayboxUI.Box(root,"Faction stripe",0,0,w,3,color);
            int valueSize=large?24:18;float statY=large?215:75;
            Stat(root,"ENERGÍA",quote.EnergyLabel,9,8,w*.56f,valueSize,color);
            Stat(root,"VIDA",card.category==Category.Spell?"—":(piece?.health??card.health).ToString(),w-66,8,58,valueSize,GrayboxUI.Ink,TextAnchor.UpperRight);
            var name=GrayboxUI.Text(root,card.name,10,large?78:42,w-20,large?78:32,large?26:14,GrayboxUI.Ink,FontStyle.Bold);name.alignment=TextAnchor.MiddleCenter;
            if(large){var glyph=GrayboxUI.Text(root,card.category==Category.Spell?"✦":card.IsStructure?"◇":"●",0,158,w,48,36,color);glyph.alignment=TextAnchor.MiddleCenter;}
            string attack=card.category==Category.Spell?"—":(piece!=null?CombatManager.AttackValue(game,piece):card.attack).ToString();
            string movement=card.category==Category.Spell||card.IsStructure?"—":(piece?.remainingMovement??card.movement).ToString();
            Stat(root,"FUERZA",attack,9,statY,82,valueSize,GrayboxUI.Ink);
            Stat(root,"MOV",movement,w-66,statY,58,valueSize,GrayboxUI.Ink,TextAnchor.UpperRight);
            float footer=large?271:112;GrayboxUI.Box(root,"Separador",8,footer,w-16,1,color);
            GrayboxUI.Text(root,Names.Categories[(int)card.category]+" · "+string.Join(" / ",card.subtypes),9,footer+5,w-18,large?29:17,large?14:10,color,FontStyle.Bold);
            if(large)
            {
                GrayboxUI.Text(root,card.description,12,312,w-24,116,16,GrayboxUI.Ink);
                string metadata="ALC "+card.range+" · "+card.movementType+"\n"+card.factionTag+" · "+string.Join(" · ",card.terrainTags)+"\nBiomas: "+string.Join(" / ",card.biomes.Select(b=>Names.Biomes[(int)b]))+"\n"+quote.Detail+(card.requiresAshLand?"\nCondición: Tierra Ceniza propia y libre.":"");
                GrayboxUI.Text(root,metadata,12,435,w-24,98,13,GrayboxUI.Muted);
                GrayboxUI.Text(root,"VENTANAS DE USO\n"+TimingRules.Description(card),12,539,w-24,h-545,13,color);
            }
            else
            {
                GrayboxUI.Text(root,card.description,9,footer+24,w-18,h-footer-58,11,GrayboxUI.Ink);
                GrayboxUI.Text(root,card.factionTag+" · "+string.Join(" / ",card.biomes.Select(b=>Names.Biomes[(int)b])),9,h-31,w-18,14,9,color);
                GrayboxUI.Text(root,string.Join(" / ",(card.allowedPhases??Array.Empty<TurnStage>()).Select(TimingRules.StageName)),9,h-16,w-18,13,9,GrayboxUI.Muted);
            }
            if(click!=null){var button=root.gameObject.AddComponent<Button>();button.targetGraphic=root.GetComponent<Image>();button.onClick.AddListener(()=>click());}
            if(enter!=null||exit!=null){var hover=root.gameObject.AddComponent<CardHover>();hover.enter=enter;hover.exit=exit;}
            return root;
        }
        static void Stat(Transform root,string label,string value,float x,float y,float w,int size,Color color,TextAnchor align=TextAnchor.UpperLeft)
        {
            var title=GrayboxUI.Text(root,label,x,y,w,13,9,GrayboxUI.Muted);title.alignment=align;
            var number=GrayboxUI.Text(root,value,x,y+13,w,30,size,color,FontStyle.Bold);number.alignment=align;
        }
    }
    public class CardHover : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Action enter,exit;
        public void OnPointerEnter(PointerEventData e){enter?.Invoke();}
        public void OnPointerExit(PointerEventData e){exit?.Invoke();}
        void OnDisable(){exit?.Invoke();}
    }
}
