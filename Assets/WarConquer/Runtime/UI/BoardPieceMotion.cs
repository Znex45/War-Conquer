using System;
using System.Collections.Generic;
using UnityEngine;
namespace WarConquer
{
    public sealed class BoardPieceMotion : MonoBehaviour
    {
        struct Step {public Vector3 position;public int tile;}
        readonly Queue<Step> route=new Queue<Step>();
        Vector3 start,end,restScale;float elapsed,travel;bool moving,landed;int tile;
        public Action<int> Landed;
        public bool IsMoving=>moving||route.Count>0;
        public bool SuspendForCombat {get;set;}
        public void Enqueue(Vector3 point,int destination)
        {
            route.Enqueue(new Step{position=point,tile=destination});if(!moving){restScale=transform.localScale;Next();}
        }
        void Next()
        {
            if(route.Count==0){moving=false;transform.localRotation=Quaternion.identity;transform.localScale=restScale;return;}
            start=transform.localPosition;var step=route.Dequeue();end=step.position;tile=step.tile;elapsed=0;landed=false;
            travel=Mathf.Clamp(Vector3.Distance(start,end)*.085f,.15f,.32f);moving=true;
        }
        void Update()=>Tick(Time.unscaledDeltaTime);
        public void Tick(float dt)
        {
            if(!moving||SuspendForCombat)return;
            elapsed+=Mathf.Max(0,dt);
            var axis=Vector3.Cross(Vector3.up,(end-start).normalized);
            if(elapsed<.42f)
            {
                float t=elapsed/.42f;transform.localPosition=start+Vector3.up*(.65f*t*t*(3-2*t));
                transform.localRotation=Quaternion.AngleAxis(-9f*Mathf.Sin(t*Mathf.PI*.5f),axis);
            }
            else if(elapsed<.42f+travel)
            {
                float t=(elapsed-.42f)/travel;transform.localPosition=Vector3.Lerp(start,end,t*t*(3-2*t))+Vector3.up*.65f;
                transform.localRotation=Quaternion.AngleAxis(Mathf.Lerp(-9,10,t),axis);
            }
            else if(elapsed<.55f+travel)
            {
                float t=(elapsed-.42f-travel)/.13f;transform.localPosition=end+Vector3.up*(.65f*(1-t*t));
                transform.localRotation=Quaternion.AngleAxis(10*(1-t),axis);
            }
            else
            {
                if(!landed){landed=true;Landed?.Invoke(tile);}
                float t=Mathf.Clamp01((elapsed-.55f-travel)/.1f);transform.localPosition=end;transform.localRotation=Quaternion.identity;
                float impact=Mathf.Sin(t*Mathf.PI);transform.localScale=Vector3.Scale(restScale,new Vector3(1+.06f*impact,1-.1f*impact,1+.06f*impact));
                if(t>=1){transform.localScale=restScale;Next();}
            }
        }
    }
}
