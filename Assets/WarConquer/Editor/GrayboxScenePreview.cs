#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    // Shows the same lattice in Scene view before Play, without adding gameplay objects or modifying the scene.
    public static class GrayboxScenePreview
    {
        static GameState preview;
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        static void Draw(WarConquerController controller, GizmoType type)
        {
            if(Application.isPlaying)return;
            if(preview==null){preview=new GameState();BoardManager.Create(preview);}
            var previousColor=Handles.color;var previousDepth=Handles.zTest;Handles.zTest=UnityEngine.Rendering.CompareFunction.LessEqual;
            var label=new GUIStyle(EditorStyles.boldLabel){alignment=TextAnchor.MiddleCenter};label.normal.textColor=Color.white;
            foreach(var tile in preview.tiles)
            {
                var center=Board3DScene.Position(tile);
                var polygon=new Vector3[6];
                for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;polygon[i]=center+new Vector3(Mathf.Cos(angle)*.8f,BoardMeshFactory.Surface,Mathf.Sin(angle)*.8f);}
                Handles.color=Color.Lerp(GrayboxUI.TerritoryColor(tile.territory),Color.black,.5f);
                for(int i=0;i<6;i++)Handles.DrawAAConvexPolygon(polygon[i],polygon[(i+1)%6],polygon[(i+1)%6]-Vector3.up*BoardMeshFactory.Surface,polygon[i]-Vector3.up*BoardMeshFactory.Surface);
                for(int i=0;i<6;i++)polygon[i]=center+Vector3.up*BoardMeshFactory.Surface+(polygon[i]-center-Vector3.up*BoardMeshFactory.Surface)*.955f;
                Handles.color=GrayboxUI.TerritoryColor(tile.territory);Handles.DrawAAConvexPolygon(polygon);
                if(tile.baseOwner>=0){Handles.color=GrayboxUI.PlayerColor(tile.baseOwner);Handles.CubeHandleCap(0,center+Vector3.up*.67f,Quaternion.identity,.55f,EventType.Repaint);Handles.Label(center+Vector3.up*1.05f,"J"+(tile.baseOwner+1),label);}
                else if(tile.conquestSite)Handles.Label(center+Vector3.up*.4f,"OBJETIVO +1 PC",label);
            }
            Handles.color=previousColor;Handles.zTest=previousDepth;
        }

        [MenuItem("War & Conquer/Ver mapa en escena")]
        public static void FrameMap()
        {
            var controller=Object.FindAnyObjectByType<WarConquerController>();
            if(controller==null){GrayboxProject.Open();controller=Object.FindAnyObjectByType<WarConquerController>();}
            if(controller==null)return;
            Selection.activeGameObject=controller.gameObject;
            var view=SceneView.lastActiveSceneView??EditorWindow.GetWindow<SceneView>();
            view.in2DMode=false;view.drawGizmos=true;view.LookAt(Vector3.zero,Quaternion.Euler(53,0,0),19f,true);view.Focus();view.Repaint();
        }
    }
}
#endif
