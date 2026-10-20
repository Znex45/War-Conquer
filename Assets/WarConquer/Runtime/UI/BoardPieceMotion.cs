using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    // Presentation only: the rules resolve immediately; the piece follows the resolved route.
    public sealed class BoardPieceMotion : MonoBehaviour
    {
        readonly Queue<Vector3> route=new Queue<Vector3>();
        Vector3 start,end;float elapsed,duration;bool moving;
        public bool IsMoving=>moving||route.Count>0;
        public void Enqueue(Vector3 point){route.Enqueue(point);if(!moving)Next();}
        void Next()
        {
            if(route.Count==0){moving=false;transform.localRotation=Quaternion.identity;return;}
            start=transform.localPosition;end=route.Dequeue();elapsed=0;
            duration=Mathf.Clamp(Vector3.Distance(start,end)*.22f,.28f,.85f);moving=true;
        }
        void Update()
        {
            if(!moving)return;
            elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/duration);
            float eased=t*t*(3f-2f*t),lift=Mathf.Sin(t*Mathf.PI);
            transform.localPosition=Vector3.Lerp(start,end,eased)+Vector3.up*(lift*.13f);
            var direction=end-start;direction.y=0;
            var axis=Vector3.Cross(Vector3.up,direction.normalized);
            transform.localRotation=Quaternion.AngleAxis(Mathf.Sin(t*Mathf.PI*2)*5f,axis);
            if(t>=1){transform.localPosition=end;transform.localRotation=Quaternion.identity;Next();}
        }
    }
}
