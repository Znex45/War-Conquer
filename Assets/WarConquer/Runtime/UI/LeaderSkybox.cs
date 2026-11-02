using UnityEngine;
namespace WarConquer
{
    // Camera-local skybox avoids changing menus, previews or other scene cameras.
    public sealed class LeaderSkybox:MonoBehaviour
    {
        Skybox sky;string current;
        public string CurrentLeader=>current;
        public static int PriorityOwner(GameState s)=>s.pendingChoices.Count>0?s.pendingChoices[0].owner:s.battle!=null?s.battle.priorityPlayer:s.responsePlayer>=0?s.responsePlayer:s.activePlayer;
        public void Sync(GameManager game,Camera camera)
        {
            if(camera==null||game.State.players.Count==0)return;
            if(sky==null)sky=camera.GetComponent<Skybox>();
            if(sky==null)sky=camera.gameObject.AddComponent<Skybox>();
            var s=game.State;
            int owner=PriorityOwner(s);
            if(owner<0||owner>=s.players.Count)return;
            string leader=s.players[owner].leader;
            if(current==leader)return;
            string key=leader=="XIL'THAR"?"Xilthar":leader=="ZUKGROK"?"Zukgrok":leader=="FAUNAR"?"Faunar":leader=="SAHRIA"?"Sahria":null;
            var material=key==null?null:Resources.Load<Material>("Environment/Skyboxes/"+key);
            sky.material=material;sky.enabled=material!=null;
            camera.clearFlags=material!=null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;current=leader;
        }
    }
}

