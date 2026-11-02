#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarConquer.Editor
{
    // Exercises the input path independently of game decisions and rendering effects.
    public static class BoardSelectionTests
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}

        [MenuItem("War & Conquer/Comprobar selección del tablero")]
        public static void Run()
        {
            Geometry();PointerGestures();CardRaycasts();
            Debug.Log("BOARD_SELECTION_PASSED: hex interiors through occluders, valid targets at six rotations/three pitches/two zooms, click jitter, drag/orbit, phase badge raycast isolation and no game mutation.");
        }

        public static void RunBatch()
        {
            try{Run();EditorApplication.Exit(0);}
            catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
        }

        static void Geometry()
        {
            var game=new GameManager(CardCatalog.Load());
            game.NewGame(CardCatalog.Leaders,2026,CardCatalog.LoadRules(),4);
            var tile=game.State.tiles.First(t=>t.q==0&&t.r==0);
            var neighbor=game.State.tiles[tile.neighbors[0]];
            string before=GamePersistence.Serialize(game.State);
            var root=new GameObject("Selection geometry fixture");
            try
            {
                var world=root.AddComponent<Board3DScene>();world.Initialize();world.BoardCamera.aspect=982f/648;
                world.Sync(game,new HashSet<int>{tile.id},new HashSet<int>(),-1);
                // A tall neighboring miniature overlaps this tile in screen space.
                var overlap=new GameObject("Overlapping neighboring miniature",typeof(BoxCollider),typeof(Hex3DTarget));
                overlap.layer=BoardMeshFactory.Layer;overlap.transform.SetParent(root.transform,false);
                overlap.transform.position=Board3DScene.Position(tile)+Vector3.up*2.5f;
                overlap.GetComponent<BoxCollider>().size=new Vector3(1.4f,4.8f,1.4f);
                overlap.GetComponent<Hex3DTarget>().tileId=neighbor.id;
                // Decoration with a collider but no target must not swallow the ray.
                var decoration=new GameObject("Untargeted decoration",typeof(BoxCollider));
                decoration.layer=BoardMeshFactory.Layer;decoration.transform.SetParent(root.transform,false);
                decoration.transform.position=Board3DScene.Position(tile)+Vector3.up*4;
                decoration.GetComponent<BoxCollider>().size=new Vector3(2,1,2);
                Physics.SyncTransforms();
                foreach(float yaw in new[]{0f,60f,120f,180f,240f,300f})
                foreach(float pitch in new[]{35f,53f,78f})
                foreach(float zoom in new[]{1f,2f})
                {
                    world.Yaw=yaw;world.Pitch=pitch;world.Zoom=zoom;world.UpdateCamera();
                    foreach(var offset in new[]{Vector3.zero,new Vector3(.55f,0,0),new Vector3(-.55f,0,0),new Vector3(0,0,.5f),new Vector3(0,0,-.5f)})
                    {
                        var point=world.BoardCamera.WorldToViewportPoint(Board3DScene.Position(tile)+Vector3.up*BoardMeshFactory.Surface+offset);
                        Check(world.Pick(new Vector2(point.x,point.y))==tile.id,"Valid hex hidden by collider: yaw="+yaw+", pitch="+pitch+", zoom="+zoom+", offset="+offset);
                    }
                }
                UnityEngine.Object.DestroyImmediate(overlap);
                world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);Physics.SyncTransforms();
                Check(world.Pick(world.ViewportOf(tile.id))==tile.id,"Untargeted collider swallowed ordinary tile selection.");
                Check(world.Pick(new Vector2(-.01f,.5f))==-1&&world.Pick(new Vector2(.5f,1.01f))==-1,"Outside viewport click accepted.");
                world.Hover(tile.id);world.Hover(-1);
                Check(GamePersistence.Serialize(game.State)==before,"Picking or hover mutated game data.");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }

        public static void PointerGestures()
        {
            var previousEvents=EventSystem.current;
            var eventRoot=new GameObject("Selection event fixture",typeof(EventSystem));
            var viewport=new GameObject("Input fixture",typeof(RectTransform),typeof(BoardViewportInput));
            try
            {
                var events=eventRoot.GetComponent<EventSystem>();if(Application.isPlaying)EventSystem.current=events;events.pixelDragThreshold=5;
                var rect=viewport.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(982,648);
                var input=viewport.GetComponent<BoardViewportInput>();int clicks=0,drags=0,orbits=0;
                input.Click=_=>clicks++;input.Drag=(a,b,orbit)=>{drags++;if(orbit)orbits++;};
                Action<PointerEventData.InputButton,Vector2,bool> gesture=(button,delta,dragStarted)=>
                {
                    var data=new PointerEventData(events){button=button,position=new Vector2(100,100),pressPosition=new Vector2(100,100),eligibleForClick=true};
                    input.OnPointerDown(data);
                    if(dragStarted)
                    {
                        data.position+=delta;data.delta=delta;data.eligibleForClick=false;
                        input.OnBeginDrag(data);input.OnDrag(data);
                        ((IEndDragHandler)input).OnEndDrag(data);
                    }
                    input.OnPointerClick(data);
                };
                gesture(PointerEventData.InputButton.Left,Vector2.zero,false);
                Check(clicks==1&&drags==0,"Ordinary click lost.");
                gesture(PointerEventData.InputButton.Left,new Vector2(7,3),true);
                Check(clicks==2&&drags==0,"Hand jitter lost or doubled click / moved camera.");
                gesture(PointerEventData.InputButton.Left,new Vector2(40,15),true);
                Check(clicks==2&&drags==1&&orbits==0,"Intentional pan produced action or failed.");
                gesture(PointerEventData.InputButton.Right,new Vector2(7,3),true);
                Check(clicks==2&&drags==1,"Right jitter executed a tile action.");
                gesture(PointerEventData.InputButton.Right,new Vector2(40,15),true);
                Check(clicks==2&&drags==2&&orbits==1,"Right drag did not remain an orbit.");
                if(Application.isPlaying)
                {
                    events.pixelDragThreshold=10;
                    gesture(PointerEventData.InputButton.Left,new Vector2(15,0),true);
                    Check(clicks==3&&drags==2,"System drag threshold was ignored.");
                }
            }
            finally
            {
                if(Application.isPlaying&&previousEvents!=null)EventSystem.current=previousEvents;
                UnityEngine.Object.DestroyImmediate(viewport);UnityEngine.Object.DestroyImmediate(eventRoot);
            }
        }

        static void CardRaycasts()
        {
            var game=new GameManager(CardCatalog.Load());game.NewGame(CardCatalog.Leaders,2026,CardCatalog.LoadRules(),4);
            var root=new GameObject("Readonly card fixture",typeof(RectTransform),typeof(CanvasGroup));
            try
            {
                var card=CardPresentation.Draw(root.transform,game,game.Catalog.All.First(),game.State.Active,0,0,198,201);
                var badge=card.Find("Etapa de uso");var group=badge.GetComponent<CanvasGroup>();
                Check(group!=null&&group.ignoreParentGroups&&!group.blocksRaycasts&&!group.interactable,"Phase badge bypasses readonly/animation input filters.");
                root.GetComponent<CanvasGroup>().blocksRaycasts=false;
                Check(!card.GetComponent<Image>().Raycast(Vector2.zero,null),"Readonly card root intercepts input.");
                foreach(var graphic in badge.GetComponentsInChildren<Graphic>())
                    Check(!graphic.raycastTarget||!graphic.Raycast(Vector2.zero,null),"Readonly card phase graphic intercepts input: "+graphic.name);
                root.GetComponent<CanvasGroup>().blocksRaycasts=true;
                Check(card.GetComponent<Image>().Raycast(Vector2.zero,null),"Card root lost its normal click surface.");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}
#endif
