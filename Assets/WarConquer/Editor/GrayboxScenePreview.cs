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
            var previousColor=Handles.color;
            var label=new GUIStyle(EditorStyles.boldLabel){alignment=TextAnchor.MiddleCenter};label.normal.textColor=Color.white;
            foreach(var tile in preview.tiles)
            {
                var center=new Vector3(tile.x,tile.y,0);
                var polygon=new Vector3[6];
                for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;polygon[i]=center+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*.8f;}
                Handles.color=GrayboxUI.Background;Handles.DrawAAConvexPolygon(polygon);
                for(int i=0;i<6;i++)polygon[i]=center+(polygon[i]-center)*.94f;
                Handles.color=GrayboxUI.TerritoryColor(tile.territory);Handles.DrawAAConvexPolygon(polygon);
                if(tile.baseOwner>=0)Handles.Label(center,"J"+(tile.baseOwner+1),label);
                else if(tile.territory==4&&tile.q==0&&tile.r==0)Handles.Label(center,"CENTRO",label);
            }
            Handles.color=previousColor;
        }

        [MenuItem("War & Conquer/Ver mapa en escena")]
        public static void FrameMap()
        {
            var controller=Object.FindAnyObjectByType<WarConquerController>();
            if(controller==null){GrayboxProject.Open();controller=Object.FindAnyObjectByType<WarConquerController>();}
            if(controller==null)return;
            Selection.activeGameObject=controller.gameObject;
            var view=SceneView.lastActiveSceneView??EditorWindow.GetWindow<SceneView>();
            view.drawGizmos=true;view.LookAt(Vector3.zero,Quaternion.identity,9.5f,true);view.Focus();view.Repaint();
        }
    }
}
#endif
