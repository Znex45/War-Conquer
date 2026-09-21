using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    public sealed class BoardDiceVisual:MonoBehaviour
    {
        readonly Queue<DiceRoll> pending=new Queue<DiceRoll>();
        Board3DScene world;BoardMeshFactory mesh;GameState state;Transform die;TextMesh result;
        DiceRoll current;int lastId;float elapsed;Vector3 start,end;Quaternion settled;
        public bool IsAnimating=>current!=null||pending.Count>0;
        public void Initialize(BoardMeshFactory factory,Board3DScene scene){mesh=factory;world=scene;}
        public void Sync(GameManager game)
        {
            if(state!=game.State){state=game.State;pending.Clear();current=null;lastId=state.nextRollId-1;if(die!=null)BoardMeshFactory.Release(die.gameObject);if(result!=null)BoardMeshFactory.Release(result.gameObject);die=null;result=null;}
            foreach(var roll in state.diceRolls)if(roll.id>lastId){pending.Enqueue(roll);lastId=roll.id;}
        }
        void Build()
        {
            die=mesh.Group(transform,"Dado 3D",Vector3.zero);
            mesh.Part(die,"Cubo del dado",mesh.Cube,Vector3.zero,Vector3.one*.72f,new Color(.95f,.94f,.86f));
            var normals=new[]{Vector3.up,Vector3.right,Vector3.forward,Vector3.back,Vector3.left,Vector3.down};
            for(int face=1;face<=6;face++)
            {
                var n=normals[face-1];var right=n==Vector3.up||n==Vector3.down?Vector3.right:Vector3.Cross(n,Vector3.up);var up=Vector3.Cross(right,n);
                var dots=new List<Vector2>();if(face%2==1)dots.Add(Vector2.zero);
                if(face>=2){dots.Add(new Vector2(-.17f,-.17f));dots.Add(new Vector2(.17f,.17f));}
                if(face>=4){dots.Add(new Vector2(-.17f,.17f));dots.Add(new Vector2(.17f,-.17f));}
                if(face==6){dots.Add(new Vector2(-.17f,0));dots.Add(new Vector2(.17f,0));}
                foreach(var p in dots)mesh.Part(die,"Punto cara "+face,mesh.Sphere,n*.365f+right*p.x+up*p.y,Vector3.one*.09f,new Color(.025f,.035f,.055f));
            }
            if(result!=null)BoardMeshFactory.Release(result.gameObject);
            result=mesh.Label(transform,"Resultado del dado",Vector3.zero,.09f,Color.white);
        }
        void Update()
        {
            if(current==null&&pending.Count>0)
            {
                current=pending.Dequeue();if(die!=null)BoardMeshFactory.Release(die.gameObject);Build();elapsed=0;
                end=Board3DScene.Position(state.tiles[current.tileId])+new Vector3(.15f,BoardMeshFactory.Surface+.39f,-.2f);
                start=end+new Vector3(-2.3f,2.5f,1.2f);
                var normals=new[]{Vector3.up,Vector3.right,Vector3.forward,Vector3.back,Vector3.left,Vector3.down};
                settled=Quaternion.FromToRotation(normals[current.value-1],Vector3.up);
            }
            if(current==null)return;elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/1.6f);
            var position=Vector3.Lerp(start,end,t);position.y=end.y+Mathf.Abs(Mathf.Cos(t*Mathf.PI*3.5f))*(1-t)*2.5f;
            die.localPosition=position;die.localRotation=t<1?Quaternion.Slerp(Quaternion.Euler(720*t,540*t,360*t),settled,Mathf.Pow(t,6)):settled;
            result.transform.position=world.BoardCamera.ViewportToWorldPoint(new Vector3(.5f,.89f,12));result.transform.rotation=world.BoardCamera.transform.rotation;
            result.characterSize=world.BoardCamera.orthographicSize*.012f;
            result.text=t<1?"TIRANDO D6":("D6  "+current.value+" / "+current.threshold+"+\n"+(current.value>=current.threshold?"SUPERA":"FALLA"));
            result.color=t<1?Color.white:current.value>=current.threshold?GrayboxUI.Green:GrayboxUI.Yellow;
            if(elapsed>=3){current=null;BoardMeshFactory.Release(die.gameObject);BoardMeshFactory.Release(result.gameObject);die=null;result=null;}
        }
    }
}
