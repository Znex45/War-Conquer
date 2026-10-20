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
        public readonly Board3DScene World;
        public RectTransform Overlay { get; }
        public float Zoom {get=>World.Zoom;set=>World.Zoom=value;}
        public Vector2 Pan {get=>World.Pan;set=>World.Pan=value;}
        public BoardView(RectTransform parent,WarConquerController controller)
        {
            root=parent;
            var world=new GameObject("WAR & CONQUER · TABLERO 3D");world.transform.SetParent(controller.transform,false);World=world.AddComponent<Board3DScene>();World.Initialize();
            var imageRoot=GrayboxUI.Rect(parent,"Vista del tablero 3D",0,0,982,648);viewport=imageRoot.gameObject.AddComponent<RawImage>();viewport.color=Color.white;
            var input=imageRoot.gameObject.AddComponent<BoardViewportInput>();input.Click=position=>{int id=World.Pick(position);if(id>=0)controller.TileClick(id);};
            input.Drag=(delta,orbit)=>{if(orbit){World.Yaw+=delta.x*.35f;World.Pitch-=delta.y*.22f;}else World.Pan+=new Vector2(delta.x,-delta.y);World.UpdateCamera();};
            input.Scroll=amount=>{Zoom=Mathf.Clamp(Zoom+amount*.15f,1,5f);World.UpdateCamera();};
            Overlay=GrayboxUI.Rect(parent,"Información sobre el tablero",0,0,982,648);
            Tick();
        }
        public void Render(GameManager game,HashSet<int> valid,HashSet<int> selected,int focus)
        {GrayboxUI.Clear(Overlay);World.Sync(game,valid,selected,focus);Tick();}
        public void Tick()
        {
            var canvas=root.GetComponentInParent<Canvas>();float scale=canvas!=null?canvas.scaleFactor:1;
            World.ResizeTexture(Mathf.RoundToInt(root.rect.width*scale),Mathf.RoundToInt(root.rect.height*scale));
            if(viewport.texture!=World.Texture)viewport.texture=World.Texture;
        }
        public void Rotate(float degrees){World.Yaw+=degrees;World.UpdateCamera();}
        public void Reset(){Zoom=1;Pan=Vector2.zero;World.Yaw=0;World.Pitch=53;World.UpdateCamera();}
        public void Dispose(){if(viewport!=null)viewport.texture=null;if(World!=null)BoardMeshFactory.Release(World.gameObject);}
    }
    public sealed class BoardViewportInput : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IBeginDragHandler,IDragHandler,IScrollHandler
    {
        public Action<Vector2> Click;
        public Action<Vector2,bool> Drag;
        public Action<float> Scroll;
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
            var canvas=GetComponentInParent<Canvas>();float scale=canvas!=null?canvas.scaleFactor:1;
            Drag?.Invoke(data.delta/scale,data.button==PointerEventData.InputButton.Right);
        }
        public void OnScroll(PointerEventData data){Scroll?.Invoke(data.scrollDelta.y);}
    }
}
