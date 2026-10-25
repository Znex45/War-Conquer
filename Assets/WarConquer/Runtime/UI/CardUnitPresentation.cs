using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    // Presentation only: neither the piece root nor game data is animated here.
    // Idle uses saved local poses, so repeated Sync, movement and undo cannot accumulate drift.
    public sealed class CardUnitPresentation : MonoBehaviour
    {
        struct Joint { public Transform node;public Vector3 position;public Quaternion rotation;public string role; }
        readonly List<Joint> joints=new List<Joint>();
        Transform sculpture,arrival;Transform[] motes;
        Vector3 restPosition,restScale;Quaternion restRotation;
        BoardPieceMotion motion;
        float clock,phase,elapsed;bool flying,voidBeing;
        public const float ArrivalDuration=.95f;
        public bool IsSpawning {get;private set;}
        public Transform Sculpture=>sculpture;
        public void Initialize(BoardMeshFactory mesh,CardData card,int identity,bool spawn)
        {
            if(card.IsStructure||card.category==Category.Spell){enabled=false;return;}
            sculpture=transform.Find("Escultura de carta");if(sculpture==null){enabled=false;return;}
            restPosition=sculpture.localPosition;restRotation=sculpture.localRotation;restScale=sculpture.localScale;
            motion=GetComponent<BoardPieceMotion>();flying=card.movementType=="Flying";voidBeing=CardPieceModels.Habitat(card)==Biome.Void;
            phase=(identity*137.508f%360)*Mathf.Deg2Rad;
            foreach(var node in sculpture.GetComponentsInChildren<Transform>())if(node.name.StartsWith("Idle:"))
                joints.Add(new Joint{node=node,position=node.localPosition,rotation=node.localRotation,role=node.name.Substring(5)});
            if(!spawn)return;
            IsSpawning=true;elapsed=0;
            arrival=mesh.Group(transform,"Aparición de unidad",Vector3.up*.13f);motes=new Transform[12];
            Biome habitat=CardPieceModels.Habitat(card);
            Color tint=voidBeing?new Color(.26f,.72f,1):habitat==Biome.Desert?new Color(.95f,.74f,.32f):habitat==Biome.Wasteland?new Color(1,.4f,.12f):new Color(.48f,.87f,.57f);
            for(int i=0;i<motes.Length;i++)
                motes[i]=mesh.Part(arrival,"Partícula de invocación",voidBeing?mesh.Crystal:habitat==Biome.Desert?mesh.Carved:mesh.Petal,Vector3.zero,Vector3.one*.05f,tint).transform;
            ApplyArrival(0);
        }
        void ResetPose()
        {
            sculpture.localPosition=restPosition;sculpture.localRotation=restRotation;sculpture.localScale=restScale;
            foreach(var j in joints){j.node.localPosition=j.position;j.node.localRotation=j.rotation;}
        }
        void CompleteArrival()
        {
            IsSpawning=false;ResetPose();if(arrival!=null)BoardMeshFactory.Release(arrival.gameObject);arrival=null;motes=null;
        }
        void ApplyArrival(float t)
        {
            float reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.7f));
            float settle=t>.7f?Mathf.Sin((t-.7f)/.3f*Mathf.PI)*.045f:0;
            sculpture.localScale=restScale*Mathf.Max(.015f,reveal);
            sculpture.localPosition=restPosition+Vector3.up*(-.22f*(1-reveal)+settle);
            sculpture.localRotation=restRotation*Quaternion.Euler(0,(1-reveal)*(voidBeing?65:18),0);
            for(int i=0;i<motes.Length;i++)
            {
                float a=i*Mathf.PI*2/motes.Length+t*(voidBeing?5:2);
                float radius=.21f+Mathf.Sin(t*Mathf.PI)*.21f;
                motes[i].localPosition=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(Mathf.PI*t)*(.25f+i%3*.12f),Mathf.Sin(a)*radius);
                motes[i].localScale=Vector3.one*(Mathf.Sin(t*Mathf.PI)*(.04f+i%3*.012f));
                motes[i].localRotation=Quaternion.Euler(i*31+t*180,i*43+t*120,30);
            }
        }
        void LateUpdate()
        {
            if(sculpture==null)return;
            clock+=Time.unscaledDeltaTime;
            if(motion!=null&&motion.IsMoving)
            {if(IsSpawning)CompleteArrival();else ResetPose();return;}
            if(IsSpawning)
            {
                elapsed+=Time.unscaledDeltaTime;float t=elapsed/ArrivalDuration;
                if(t>=1)CompleteArrival();else ApplyArrival(t);return;
            }
            ResetPose();float wave=Mathf.Sin(clock*2.1f+phase);
            // Grounded feet stay seated. Hovering pieces and articulated joints move independently.
            if(flying)sculpture.localPosition=restPosition+Vector3.up*(.025f*wave);
            else sculpture.localScale=new Vector3(restScale.x,restScale.y*(1+wave*.012f),restScale.z);
            foreach(var j in joints)
            {
                Vector3 angle=Vector3.zero;
                switch(j.role)
                {
                    case "Head":angle=new Vector3(wave*2,Mathf.Sin(clock*.85f+phase)*6,0);break;
                    case "Tail":angle=new Vector3(0,Mathf.Sin(clock*2.7f+phase)*12,wave*3);break;
                    case "ArmL":angle=new Vector3(wave*3,0,wave*2);break;
                    case "ArmR":angle=new Vector3(-wave*3,0,-wave*2);break;
                    case "WingL":angle=new Vector3(0,0,Mathf.Sin(clock*9+phase)*18);break;
                    case "WingR":angle=new Vector3(0,0,-Mathf.Sin(clock*9+phase)*18);break;
                    case "Body":angle=new Vector3(0,voidBeing?wave*5:wave*1.5f,0);break;
                }
                j.node.localRotation=j.rotation*Quaternion.Euler(angle);
            }
        }
        void OnDisable(){if(sculpture!=null){ResetPose();if(IsSpawning)CompleteArrival();}}
    }
}
