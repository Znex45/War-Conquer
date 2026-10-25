using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // Only the model parts move; the structure's logical tile, collider and status UI stay fixed.
    public sealed class BoardStructureAssembly : MonoBehaviour
    {
        sealed class Part
        {
            public Transform node;public Renderer renderer;public Vector3 position;public Quaternion rotation;
            public bool visible;public float delay;
        }
        Part[] parts;float elapsed;
        public bool IsBuilding {get;private set;}
        public void Begin()
        {
            int count=GetComponentsInChildren<MeshRenderer>().Length;
            float stagger=Mathf.Min(.11f,.9f/Mathf.Max(1,count-1));
            parts=GetComponentsInChildren<MeshRenderer>().OrderBy(r=>r.transform.localPosition.y)
                .Select((r,i)=>new Part{node=r.transform,renderer=r,position=r.transform.localPosition,
                    rotation=r.transform.localRotation,visible=r.enabled,delay=i*stagger}).ToArray();
            elapsed=0;IsBuilding=parts.Length>0;
            foreach(var part in parts){part.renderer.enabled=false;part.node.localPosition=part.position+Vector3.up*6f;}
            enabled=IsBuilding;
        }
        void Update()
        {
            elapsed+=Time.unscaledDeltaTime;bool unfinished=false;
            foreach(var part in parts)
            {
                float time=elapsed-part.delay;
                if(time<0){unfinished=true;continue;}
                part.renderer.enabled=part.visible;
                if(time<.42f)
                {
                    float t=time/.42f;
                    part.node.localPosition=part.position+Vector3.up*(6f*(1f-t*t));
                    part.node.localRotation=part.rotation*Quaternion.Euler(0,(1f-t)*18f,0);unfinished=true;
                }
                else if(time<.58f)
                {
                    float t=(time-.42f)/.16f;
                    part.node.localPosition=part.position+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.065f*(1f-t));
                    part.node.localRotation=part.rotation;unfinished=true;
                }
                else{part.node.localPosition=part.position;part.node.localRotation=part.rotation;}
            }
            IsBuilding=unfinished;if(!unfinished)enabled=false;
        }
    }
}
