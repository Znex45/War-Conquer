using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarConquer
{
    // A live 3D camera inside the existing card HUD.
    public sealed class BoardView : IDisposable
    {
        readonly RectTransform root;
        readonly RawImage viewport;
        readonly Canvas canvas;
        public readonly Board3DScene World;
        public RectTransform Overlay { get; }
        public float Zoom {get=>World.Zoom;set=>World.Zoom=value;}
        public Vector2 Pan {get=>World.Pan;set=>World.Pan=value;}
        public BoardView(RectTransform parent,WarConquerController controller)
        {
            root=parent;canvas=root.GetComponentInParent<Canvas>();
            var world=new GameObject("WAR & CONQUER · TABLERO 3D");world.transform.SetParent(controller.transform,false);World=world.AddComponent<Board3DScene>();World.Initialize();
            var imageRoot=GrayboxUI.Rect(parent,"Vista del tablero 3D",0,0,982,648);viewport=imageRoot.gameObject.AddComponent<RawImage>();viewport.color=Color.white;
            var input=imageRoot.gameObject.AddComponent<BoardViewportInput>();input.Click=position=>{int id=World.Pick(position);if(id>=0)controller.TileClick(id);};
            input.Drag=(from,to,orbit)=>{
                if(orbit){var delta=to-from;World.Yaw=Mathf.Repeat(World.Yaw+delta.x*220f,360f);World.Pitch-=delta.y*130f;World.UpdateCamera();}
                else World.DragCamera(from,to);
            };
            input.Scroll=(amount,cursor)=>World.ZoomAt(amount,cursor);
            input.Hover=position=>World.Hover(position.HasValue?World.Pick(position.Value):-1);
            Overlay=GrayboxUI.Rect(parent,"Información sobre el tablero",0,0,982,648);
            Tick();
        }
        public void Render(GameManager game,HashSet<int> valid,HashSet<int> selected,int focus)
        {GrayboxUI.Clear(Overlay);World.Sync(game,valid,selected,focus);Tick();}
        public void Tick()
        {
            float scale=canvas!=null?canvas.scaleFactor:1;
            World.ResizeTexture(Mathf.RoundToInt(root.rect.width*scale),Mathf.RoundToInt(root.rect.height*scale));
            if(viewport.texture!=World.Texture)viewport.texture=World.Texture;
        }
        public void Rotate(float degrees){World.Yaw+=degrees;World.UpdateCamera();}
        public void Nudge(Vector2 direction){World.DragCamera(Vector2.one*.5f,Vector2.one*.5f-direction*.08f);}
        public void Reset(){Zoom=Board3DScene.DefaultZoom;Pan=Vector2.zero;World.PanHeight=0;World.Yaw=0;World.Pitch=53;World.UpdateCamera();}
        public void Dispose(){if(viewport!=null)viewport.texture=null;if(World!=null)BoardMeshFactory.Release(World.gameObject);}
    }
    public sealed class BoardViewportInput : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IScrollHandler,IPointerMoveHandler,IPointerExitHandler
    {
        public Action<Vector2> Click;
        public Action<Vector2,Vector2,bool> Drag;
        public Action<float,Vector2> Scroll;
        public Action<Vector2?> Hover;
        public const float IntentionalDragPixels=12f;
        bool dragged,clicked;Vector2 pressed;
        public void OnPointerDown(PointerEventData data){dragged=false;clicked=false;pressed=data.position;}
        public void OnBeginDrag(PointerEventData data){}
        public static Vector2 Normalize(RectTransform rect,Vector2 local)=>new Vector2((local.x-rect.rect.xMin)/rect.rect.width,(local.y-rect.rect.yMin)/rect.rect.height);
        public void OnPointerClick(PointerEventData data)
        {
            if(dragged||clicked||data.button!=PointerEventData.InputButton.Left)return;
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var local))
            {clicked=true;Click?.Invoke(Normalize(rect,local));}
        }
        public void OnDrag(PointerEventData data)
        {
            if(data.button!=PointerEventData.InputButton.Left&&data.button!=PointerEventData.InputButton.Right)return;
            bool beginning=!dragged;
            if(beginning)
            {
                float threshold=Mathf.Max(IntentionalDragPixels,EventSystem.current!=null?EventSystem.current.pixelDragThreshold*2:0);
                if((data.position-pressed).sqrMagnitude<threshold*threshold)return;
                dragged=true;Hover?.Invoke(null);
            }
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,beginning?pressed:data.position-data.delta,data.pressEventCamera,out var from)&&
               RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var to))
                Drag?.Invoke(Normalize(rect,from),Normalize(rect,to),data.button==PointerEventData.InputButton.Right);
        }
        // The input module may begin a drag before our intentional threshold. Treat
        // that small gesture as a click on release, once, rather than losing it.
        public void OnEndDrag(PointerEventData data){if(!dragged)OnPointerClick(data);}
        public void OnPointerMove(PointerEventData data)
        {
            if(data.dragging)return;
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.enterEventCamera,out var local))Hover?.Invoke(Normalize(rect,local));
        }
        public void OnPointerExit(PointerEventData data){Hover?.Invoke(null);}
        public void OnScroll(PointerEventData data)
        {
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.enterEventCamera,out var local))
                Scroll?.Invoke(data.scrollDelta.y,Normalize(rect,local));
        }
    }
}
