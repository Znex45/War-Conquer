using UnityEngine;

namespace WarConquer
{
    // Presentation only. The BASE root retains its gameplay target and collider.
    public static class LeaderModels
    {
        public static string AssetName(string leader)
        {
            switch(leader)
            {
                case "XIL'THAR": return "Xilthar";
                case "ZUKGROK": return "Zukgrok";
                case "FAUNAR": return "Faunar";
                case "SAHRIA": return "Sahria";
                default: return null;
            }
        }
        public static bool Spawn(Transform parent,string leader)
        {
            string name=AssetName(leader);if(name==null)return false;
            var prefab=Resources.Load<GameObject>("LeaderModels/"+name+"_Leader");
            if(prefab==null){Debug.LogError("Falta el modelo del líder "+leader);return false;}
            var visual=Object.Instantiate(prefab,parent,false);visual.name="Líder 3D · "+leader;
            foreach(var child in visual.GetComponentsInChildren<Transform>(true))child.gameObject.layer=BoardMeshFactory.Layer;
            foreach(var animator in visual.GetComponentsInChildren<Animator>(true))animator.applyRootMotion=false;
            return true;
        }
    }
}
