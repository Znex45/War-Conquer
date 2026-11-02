using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // Visual corrections sit outside animated FBX paths so the idle cannot overwrite them.
    public sealed class LeaderPoseCorrection : MonoBehaviour
    {
        string leader;
        Transform staffAttachment,upperArm,forearm,gripHand;
        Quaternion staffRotation;
        public void Initialize(string value){leader=value;}
        void Start()
        {
            var animator=GetComponentInChildren<Animator>();if(animator!=null){animator.Rebind();animator.Update(0);}
            var nodes=GetComponentsInChildren<Transform>();
            if(leader=="XIL'THAR")
            {
                string[] names={"Cube.013","Cube.014","Cube.017","Cube.044"};
                for(int i=0;i<names.Length;i++)
                {
                    var piece=nodes.FirstOrDefault(t=>t.name==names[i]);if(piece==null)continue;
                    var filter=piece.GetComponent<MeshFilter>();if(filter==null)continue;
                    piece.SetParent(transform,false);
                    float side=i%2==0?-1f:1f;bool upper=i<2;
                    piece.localRotation=Quaternion.Euler(0,0,side*(upper?-22f:22f))*Quaternion.Euler(315,270,0);
                    float size=Mathf.Max(filter.sharedMesh.bounds.size.y,filter.sharedMesh.bounds.size.z);
                    piece.localScale=Vector3.one*((upper?.56f:.46f)/(size*1.414214f));
                    var center=new Vector3(side*(upper?.48f:.43f),upper?1.32f:.68f,.27f);
                    piece.localPosition=center-piece.localRotation*Vector3.Scale(filter.sharedMesh.bounds.center,piece.localScale);
                }
                return;
            }
            string shaftName=leader=="ZUKGROK"?"Cube.002":"Circle.004";
            string tipName=leader=="ZUKGROK"?"Cube.005":"Cube.045";
            var shaft=nodes.FirstOrDefault(t=>t.name==shaftName);var tip=nodes.FirstOrDefault(t=>t.name==tipName);
            var hand=nodes.FirstOrDefault(t=>t.name==(leader=="ZUKGROK"?"Bone.016":"hand_r"));
            if(shaft==null||tip==null||hand==null)return;
            var mesh=shaft.GetComponent<MeshFilter>();if(mesh==null)return;
            var bounds=mesh.sharedMesh.bounds;
            Vector3 axis=bounds.size.x>bounds.size.y&&bounds.size.x>bounds.size.z?Vector3.right:bounds.size.y>bounds.size.z?Vector3.up:Vector3.forward;
            Vector3 centerWorld=shaft.TransformPoint(bounds.center),direction=shaft.TransformVector(axis).normalized;
            Vector3 palm=hand.position;
            if(leader=="FAUNAR")
            {
                var finger=nodes.FirstOrDefault(t=>t.name=="middle_01_r");
                if(finger!=null)palm=Vector3.Lerp(hand.position,finger.position,.55f);
            }
            // Cross-section centers measured on the actual imported shaft, not its curved bounding box.
            Vector3 grip=shaft.TransformPoint(leader=="ZUKGROK"?new Vector3(.00167213f,-.00049391f,-.00036230f):new Vector3(-.00000002f,-.00000018f,.25570990f));
            var attachment=new GameObject("Agarre del bastón").transform;attachment.SetParent(hand,false);attachment.position=palm;
            Vector3 correction=palm-grip;
            foreach(var part in new[]{shaft,tip}){part.position+=correction;part.SetParent(attachment,true);}
            if(leader=="ZUKGROK")
            {
                staffAttachment=attachment;gripHand=hand;
                upperArm=nodes.FirstOrDefault(t=>t.name=="Bone.008");forearm=nodes.FirstOrDefault(t=>t.name=="Bone.009");
                if(Vector3.Dot(direction,tip.GetComponent<Renderer>().bounds.center-grip)<0)direction=-direction;
                var localAxis=transform.InverseTransformDirection(direction);
                staffRotation=Quaternion.FromToRotation(localAxis,new Vector3(.6f,1f,.3f).normalized)*Quaternion.Inverse(transform.rotation)*attachment.rotation;
            }
        }
        void LateUpdate()
        {
            if(staffAttachment==null||upperArm==null||forearm==null||gripHand==null)return;
            Vector3 target=transform.TransformPoint(new Vector3(.62f,.93f,-.12f));
            Vector3 shoulder=upperArm.position;float a=Vector3.Distance(shoulder,forearm.position),b=Vector3.Distance(forearm.position,gripHand.position);
            Vector3 delta=target-shoulder;float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.0001f,a+b-.0001f);Vector3 forward=delta.normalized;
            float along=(a*a-b*b+d*d)/(2f*d),height=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            Vector3 pole=Vector3.ProjectOnPlane(-transform.forward,forward).normalized;
            Vector3 elbow=shoulder+forward*along+pole*height;Quaternion handRotation=gripHand.rotation;
            upperArm.rotation=Quaternion.FromToRotation(forearm.position-shoulder,elbow-shoulder)*upperArm.rotation;
            forearm.rotation=Quaternion.FromToRotation(gripHand.position-forearm.position,shoulder+forward*d-forearm.position)*forearm.rotation;
            gripHand.rotation=handRotation;
            staffAttachment.rotation=transform.rotation*staffRotation;
        }
    }
}
