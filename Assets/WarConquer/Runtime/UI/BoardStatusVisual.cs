using UnityEngine;

namespace WarConquer
{
    public sealed class BoardStatusVisual:MonoBehaviour
    {
        Transform poison,sleep;Transform[] bubbles=new Transform[3];TextMesh attack,sleepText;
        Camera cameraView;int applications;float pulse;
        public void Initialize(BoardMeshFactory mesh,Camera camera)
        {
            cameraView=camera;Color toxic=new Color(.28f,.035f,.43f),dark=new Color(.035f,.01f,.055f);
            poison=mesh.Group(transform,"Veneno · calavera y huesos",new Vector3(-.35f,2.25f,0));
            mesh.Part(poison,"Contorno del cráneo",mesh.Sphere,new Vector3(0,.13f,.02f),new Vector3(.42f,.38f,.12f),new Color(.72f,.43f,.9f));
            mesh.Part(poison,"Cráneo",mesh.Sphere,new Vector3(0,.13f,0),new Vector3(.38f,.34f,.12f),toxic);
            mesh.Part(poison,"Mandíbula",mesh.Cube,new Vector3(0,-.015f,0),new Vector3(.23f,.14f,.10f),toxic);
            for(int i=0;i<2;i++)mesh.Part(poison,"Cuenca",mesh.Sphere,new Vector3(i==0?-.08f:.08f,.15f,-.065f),new Vector3(.1f,.1f,.045f),dark);
            for(int i=0;i<2;i++){var bone=mesh.Part(poison,"Hueso cruzado",mesh.Cube,new Vector3(0,-.19f,0),new Vector3(.54f,.065f,.065f),toxic);bone.transform.localRotation=Quaternion.Euler(0,0,i==0?27:-27);}
            sleep=mesh.Group(transform,"Sueño · burbujas y Zzz",new Vector3(.4f,1.45f,0));
            for(int i=0;i<3;i++)bubbles[i]=mesh.Part(sleep,"Burbuja",mesh.Sphere,new Vector3(i*.12f,i*.22f,0),Vector3.one*(.12f+i*.025f),new Color(.53f,.82f,1)).transform;
            sleepText=mesh.Label(sleep,"Zzz",new Vector3(.2f,.8f,0),.09f,new Color(.73f,.88f,1));sleepText.text="Zzz";
            attack=mesh.Label(transform,"Disponibilidad de ataque",new Vector3(0,.12f,-.5f),.043f,Color.white);
        }
        public void Sync(GameManager game,Piece p)
        {
            poison.gameObject.SetActive(p.poison>0);sleep.gameObject.SetActive(game.IsSleeping(p));
            if(applications!=p.poisonApplications){applications=p.poisonApplications;pulse=1;}
            attack.gameObject.SetActive(!game.Data(p).IsStructure);
            bool ready=CombatManager.Targets(game,p).Count>0;
            attack.text=game.IsSleeping(p)?"DORMIDO":p.attacked?"ATQ USADO":ready?"ATQ LISTO":game.CanTakeTurnAction(TurnStage.Assault)&&p.owner==game.ActingPlayerId?"ATQ —":"ESPERA";
            attack.color=ready?GrayboxUI.Green:game.IsSleeping(p)?new Color(.58f,.79f,1):GrayboxUI.Muted;
        }
        void Update()
        {
            if(cameraView==null)return;poison.rotation=sleep.rotation=attack.transform.rotation=cameraView.transform.rotation;
            pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);poison.localScale=Vector3.one*(1+pulse*.6f);
            for(int i=0;i<3;i++)bubbles[i].localPosition=new Vector3(i*.12f,i*.22f+Mathf.Repeat(Time.unscaledTime*.28f+i*.17f,.5f),0);
        }
    }
}
