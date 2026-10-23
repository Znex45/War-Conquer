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
    public sealed class BoardViewportInput : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IBeginDragHandler,IDragHandler,IScrollHandler
    {
        public Action<Vector2> Click;
        public Action<Vector2,Vector2,bool> Drag;
        public Action<float,Vector2> Scroll;
        bool dragged;
        public void OnPointerDown(PointerEventData data){dragged=false;}
        public void OnBeginDrag(PointerEventData data){dragged=true;}
        public static Vector2 Normalize(RectTransform rect,Vector2 local)=>new Vector2((local.x-rect.rect.xMin)/rect.rect.width,(local.y-rect.rect.yMin)/rect.rect.height);
        public void OnPointerClick(PointerEventData data)
        {
            if(dragged||data.button!=PointerEventData.InputButton.Left)return;
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var local))Click?.Invoke(Normalize(rect,local));
        }
        public void OnDrag(PointerEventData data)
        {
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position-data.delta,data.pressEventCamera,out var from)&&
               RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var to))
                Drag?.Invoke(Normalize(rect,from),Normalize(rect,to),data.button==PointerEventData.InputButton.Right);
        }
        public void OnScroll(PointerEventData data)
        {
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.enterEventCamera,out var local))
                Scroll?.Invoke(data.scrollDelta.y,Normalize(rect,local));
        }
    }
}
