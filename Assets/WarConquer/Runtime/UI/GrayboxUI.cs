using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WarConquer
{
    public static class GrayboxUI
    {
        public static readonly Color Background=new Color32(15,19,27,255), Panel=new Color32(25,32,44,255), Muted=new Color32(158,175,193,255), Ink=new Color32(234,240,248,255);
        public static readonly Color Purple=new Color32(184,119,244,255), Yellow=new Color32(245,203,76,255), Green=new Color32(104,220,171,255);
        static Font font;
        public static Font Font => font ? font : (font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        public static string[] PlayerLeaders={"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"};
        public static Color PlayerColor(int id)=>PlayerLeaders[id]=="ZUKGROK"?Purple:Yellow;
        public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static RectTransform Box(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var r=Rect(parent,name,x,y,w,h);r.gameObject.AddComponent<Image>().color=color;return r;
        }
        public static Text Text(Transform parent,string text,float x,float y,float w,float h,int size=16,Color? color=null,FontStyle style=FontStyle.Normal)
        {
            var r=Rect(parent,"Text",x,y,w,h);var label=r.gameObject.AddComponent<Text>();label.font=Font;label.text=text;label.fontSize=size;label.color=color??Ink;label.fontStyle=style;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;label.raycastTarget=false;return label;
        }
        public static Button Button(Transform parent,string label,float x,float y,float w,float h,Action click,Color? color=null,bool enabled=true)
        {
            var r=Box(parent,label,x,y,w,h,color??new Color32(44,55,72,255));var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(()=>click());b.interactable=enabled;
            var colors=b.colors;colors.highlightedColor=new Color(1.25f,1.25f,1.25f);colors.disabledColor=new Color(.55f,.55f,.55f,.7f);b.colors=colors;
            var t=Text(r,label,5,3,w-10,h-6,15);t.alignment=TextAnchor.MiddleCenter;return b;
        }
        public static void Clear(Transform root){for(int i=root.childCount-1;i>=0;i--){var go=root.GetChild(i).gameObject;go.SetActive(false);UnityEngine.Object.Destroy(go);}}
        public static Color BiomeColor(Biome b)=>new[]{new Color32(65,74,84,255),new Color32(44,99,74,255),new Color32(53,83,91,255),new Color32(116,162,179,255),new Color32(141,67,52,255),new Color32(151,124,59,255),new Color32(83,49,102,255),new Color32(99,93,88,255)}[(int)b];
    }
    public class HexGraphic : MaskableGraphic, IPointerClickHandler
    {
        public Color border=Color.gray;
        public Action Click;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var center=r.center;float radius=Mathf.Min(r.width,r.height)/2;
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                Vector2 pa=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,pb=center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius;
                Vector2 ia=center+(pa-center)*.88f,ib=center+(pb-center)*.88f;
                int n=vh.currentVertCount;vh.AddVert(center,color,Vector2.zero);vh.AddVert(ia,color,Vector2.zero);vh.AddVert(ib,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);
                n=vh.currentVertCount;vh.AddVert(pa,border,Vector2.zero);vh.AddVert(pb,border,Vector2.zero);vh.AddVert(ib,border,Vector2.zero);vh.AddVert(ia,border,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
        public override bool Raycast(Vector2 screenPoint,Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint,eventCamera,out var point);
            var p=point-rectTransform.rect.center;float rad=rectTransform.rect.width/2;
            return Mathf.Abs(p.x)<=rad&&Mathf.Abs(p.y)<=.86603f*rad&&.86603f*Mathf.Abs(p.x)+.5f*Mathf.Abs(p.y)<=.86603f*rad;
        }
        public void OnPointerClick(PointerEventData data){if(data.button==PointerEventData.InputButton.Left)Click?.Invoke();}
    }
    public class PieceGraphic : MaskableGraphic
    {
        public bool square;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;int count=square?4:20;var center=r.center;float rad=r.width/2;
            for(int i=0;i<count;i++)
            {
                float a=2*Mathf.PI*i/count+(square?Mathf.PI/4:0),b=2*Mathf.PI*(i+1)/count+(square?Mathf.PI/4:0);int n=vh.currentVertCount;
                vh.AddVert(center,color,Vector2.zero);vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);vh.AddVert(center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*rad,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);
            }
        }
    }
}
