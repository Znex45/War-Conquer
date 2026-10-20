using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // A deterministic idle pose: every frame starts from the sampled pose, never the previous frame.
    public sealed class LeaderIdlePresentation : MonoBehaviour
    {
        sealed class Pose
        {
            public Transform node;
            public Vector3 position;
            public Quaternion rotation;
            public Pose(Transform t){node=t;position=t.localPosition;rotation=t.localRotation;}
            public void Restore(){node.localPosition=position;node.localRotation=rotation;}
        }
        string leader;
        readonly Dictionary<string,Transform> nodes=new Dictionary<string,Transform>();
        Pose[] pose;
        Transform[] staff,rocks;
        Vector3 origin;
        public void Initialize(string name){leader=name;}
        void Start()
        {
            var animator=GetComponentInChildren<Animator>();
            if(animator!=null)
            {
                animator.Rebind();animator.Update(0);
                // These two idles are driven here so imported arm motion cannot fight the relaxed pose.
                animator.enabled=false;
            }
            pose=GetComponentsInChildren<Transform>().Where(t=>t!=transform).Select(t=>new Pose(t)).ToArray();
            foreach(var p in pose)nodes[p.node.name]=p.node;
            string[] staffNames={"Circle.016","Circle.017","Circle.018","Circle.019","Circle.020","Circle.021","Circle.022","Circle.023","Circle.027","Cube.064"};
            staff=staffNames.Where(nodes.ContainsKey).Select(n=>nodes[n]).ToArray();
            string[] rockNames={"Cube.018","Cube.019","Cube.021","Cube.022","Cube.023","Cube.024","Cube.025"};
            rocks=rockNames.Where(nodes.ContainsKey).Select(n=>nodes[n]).ToArray();
            origin=transform.localPosition;
        }
        void Tilt(string name,Vector3 angles)
        {if(nodes.TryGetValue(name,out var bone))bone.localRotation*=Quaternion.Euler(angles);}
        void Aim(string boneName,string childName,Vector3 direction)
        {
            if(!nodes.TryGetValue(boneName,out var bone)||!nodes.TryGetValue(childName,out var child))return;
            bone.rotation=Quaternion.FromToRotation(child.position-bone.position,direction)*bone.rotation;
        }
        void LateUpdate()
        {
            if(pose==null)return;
            foreach(var p in pose)p.Restore();
            float t=Time.time,breath=Mathf.Sin(t*1.45f),sway=Mathf.Sin(t*.71f);
            if(leader=="SAHRIA")
            {
                Tilt("columna",new Vector3(breath*.7f,0,sway*.6f));
                Tilt("columna2",new Vector3(breath*.9f,sway*.8f,0));
                Tilt("cabeza",new Vector3(Mathf.Sin(t*.93f)*1.2f,Mathf.Sin(t*.47f)*2.5f,0));
                // The aligned Sahria mesh faces local -Z, so her anatomical right is -X.
                // Use that same frame for both arms and the staff, independent of her board corner.
                Vector3 forward=-transform.forward,up=transform.up;
                Vector3 right=Vector3.Cross(up,forward).normalized;
                Aim("brazo.R","antebrazo.R",-up+right*.24f+forward*.08f);
                Aim("antebrazo.R","mano.R",-up+forward*(.25f+breath*.025f));
                Aim("brazo.L","antebrazo.L",-up-right*.45f+forward*.12f);
                Aim("antebrazo.L","mano.L",forward*.65f+up*.12f-right*.28f);
                for(int i=1;i<=8;i++)Tilt("cadera."+i.ToString("000"),new Vector3(Mathf.Sin(t*1.1f-i*.4f)*1.1f,0,0));
                // Rotate all staff pieces around the grip together, with the ornament above the hand.
                if(nodes.TryGetValue("mano.L",out var hand)&&nodes.TryGetValue("Circle.016",out var shaft)&&nodes.TryGetValue("Cube.064",out var crown))
                {
                    var shaftRenderer=shaft.GetComponent<Renderer>();var crownRenderer=crown.GetComponent<Renderer>();
                    if(shaftRenderer!=null&&crownRenderer!=null)
                    {
                        Vector3 axis=crownRenderer.bounds.center-shaftRenderer.bounds.center;
                        Quaternion turn=Quaternion.FromToRotation(axis,up+right*.06f+forward*(.035f+breath*.012f));
                        foreach(var part in staff){part.position=hand.position+turn*(part.position-hand.position);part.rotation=turn*part.rotation;}
                    }
                }
            }
            else
            {
                transform.localPosition=origin+Vector3.up*(.065f*Mathf.Sin(t*1.05f));
                Tilt("Empty.007",new Vector3(Mathf.Sin(t*.8f)*1.1f,Mathf.Sin(t*.53f)*2f,0));
                Tilt("Empty.002",new Vector3(0,0,Mathf.Sin(t*1.05f+.5f)*2f));
                Tilt("Empty.004",new Vector3(0,0,Mathf.Sin(t*1.05f+2f)*2f));
                for(int i=0;i<rocks.Length;i++)
                {
                    float phase=i*.9f;
                    rocks[i].position+=transform.up*(.045f*Mathf.Sin(t*1.3f+phase));
                    rocks[i].localRotation*=Quaternion.Euler(Mathf.Sin(t*.8f+phase)*5f,Mathf.Sin(t*.6f+phase)*8f,0);
                }
            }
        }
    }
}
