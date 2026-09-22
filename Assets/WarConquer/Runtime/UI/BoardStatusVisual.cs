using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public sealed class BoardStatusVisual:MonoBehaviour
    {
        Transform poison,sleep;Transform[] bubbles=new Transform[3];TextMesh attack,sleepText;
        Camera cameraView;int applications;float pulse;
        sealed class Badge { public Transform root,icon;public TextMesh label;public string value;public float appeared; }
        readonly Dictionary<string,Badge> badges=new Dictionary<string,Badge>();
        BoardMeshFactory factory;TextMesh healthChange;Transform healthParticles;
        readonly List<Transform> sparks=new List<Transform>();
        bool initializedHealth;int previousHealth,previousMaximum;float healthPulse;
        public void Initialize(BoardMeshFactory mesh,Camera camera)
        {
            factory=mesh;cameraView=camera;Color toxic=new Color(.28f,.035f,.43f),dark=new Color(.035f,.01f,.055f);
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
            healthParticles=mesh.Group(transform,"Cambio de vida · partículas",Vector3.zero);
            for(int i=0;i<6;i++)sparks.Add(mesh.Part(healthParticles,"Destello",mesh.Sphere,Vector3.zero,Vector3.one*.085f,Color.green).transform);
            healthChange=mesh.Label(healthParticles,"Cambio de vida",new Vector3(0,2.45f,0),.075f,Color.green);
            healthParticles.gameObject.SetActive(false);
        }
        public void Sync(GameManager game,Piece p)
        {
            poison.gameObject.SetActive(p.poison>0);sleep.gameObject.SetActive(game.IsSleeping(p));
            if(applications!=p.poisonApplications){applications=p.poisonApplications;pulse=1;}
            attack.gameObject.SetActive(!game.Data(p).IsStructure);
            bool ready=CombatManager.Targets(game,p).Count>0;
            attack.text=game.IsSleeping(p)?"DORMIDO":!FactionCardRules.CanAttack(game,p)?"NO ATACA AL ENTRAR":p.attacked?"ATQ USADO":ready?"ATQ LISTO":game.CanTakeTurnAction(TurnStage.Assault)&&p.owner==game.ActingPlayerId?"ATQ —":"ESPERA";
            attack.color=ready?GrayboxUI.Green:game.IsSleeping(p)?new Color(.58f,.79f,1):GrayboxUI.Muted;
            var active=PieceStatusPresentation.Collect(game,p).Where(s=>s.id!="poison"&&s.id!="sleep").ToArray();
            var previouslyActive=new HashSet<string>(badges.Where(b=>b.Value.root.gameObject.activeSelf).Select(b=>b.Key));
            foreach(var badge in badges.Values)badge.root.gameObject.SetActive(false);
            for(int i=0;i<active.Length;i++)
            {
                var indicator=active[i];if(!badges.TryGetValue(indicator.id,out var badge)){badge=CreateBadge(indicator);badges.Add(indicator.id,badge);}
                if(!previouslyActive.Contains(indicator.id)||badge.value!=indicator.label){badge.appeared=Time.unscaledTime;badge.value=indicator.label;}
                badge.root.gameObject.SetActive(true);badge.label.text=indicator.label;
                int columns=Mathf.Min(3,active.Length),row=i/3;
                badge.root.localPosition=new Vector3((i%3-(columns-1)*.5f)*.58f,2.85f+row*.48f,0);
            }
            int maximum=game.MaxHealth(p);
            int change=initializedHealth?(p.health-previousHealth)-(maximum-previousMaximum):0;
            if(change!=0)
            {
                healthPulse=1;var color=change>0?new Color(.3f,1,.5f):new Color(1,.28f,.24f);
                healthChange.text=(change>0?"+":"")+change+" HP";healthChange.color=color;healthParticles.gameObject.SetActive(true);
                foreach(var spark in sparks)factory.Paint(spark.GetComponent<Renderer>(),color);
            }
            previousHealth=p.health;previousMaximum=maximum;initializedHealth=true;
        }
        Badge CreateBadge(StatusIndicator indicator)
        {
            var root=factory.Group(transform,"Estado · "+indicator.id,Vector3.zero);
            var icon=factory.Group(root,"Símbolo",Vector3.zero);var color=indicator.color;
            factory.Part(root,"Fondo",factory.Sphere,new Vector3(0,0,.025f),new Vector3(.42f,.4f,.065f),new Color(.05f,.07f,.11f));
            var label=factory.Label(root,"Estado",new Vector3(0,-.26f,-.04f),.027f,color);
            void Bar(string name,Vector3 position,Vector3 size,float angle=0)
            {var part=factory.Part(icon,name,factory.Cube,position,size,color);part.transform.localRotation=Quaternion.Euler(0,0,angle);}
            switch(indicator.id)
            {
                case "slow":
                    var upper=factory.Part(icon,"Reloj superior",factory.Cone,new Vector3(0,.1f,-.045f),new Vector3(.3f,.12f,.09f),color);upper.transform.localRotation=Quaternion.Euler(0,0,180);
                    factory.Part(icon,"Reloj inferior",factory.Cone,new Vector3(0,-.12f,-.045f),new Vector3(.3f,.12f,.09f),color);break;
                case "health":case "no-heal":
                    Bar("Cruz horizontal",new Vector3(0,0,-.05f),new Vector3(.26f,.075f,.05f));Bar("Cruz vertical",new Vector3(0,0,-.05f),new Vector3(.075f,.26f,.05f));
                    if(indicator.id=="no-heal")Bar("Curación prohibida",new Vector3(0,0,-.1f),new Vector3(.37f,.045f,.05f),-45);break;
                case "movement":
                    for(int i=0;i<2;i++){Bar("Flecha derecha",new Vector3(.055f,i*.12f-.07f,-.06f),new Vector3(.19f,.05f,.05f),-45);Bar("Flecha izquierda",new Vector3(-.055f,i*.12f-.07f,-.06f),new Vector3(.19f,.05f,.05f),45);}break;
                case "attack":
                    Bar("Espada",new Vector3(0,.025f,-.06f),new Vector3(.055f,.28f,.05f),-25);Bar("Empuñadura",new Vector3(-.045f,-.08f,-.06f),new Vector3(.18f,.05f,.05f),-25);break;
                case "guard":case "guard-used":
                    var shield=factory.Part(icon,"Escudo",factory.Hex,new Vector3(0,0,-.055f),new Vector3(.2f,.1f,.23f),color);shield.transform.localRotation=Quaternion.Euler(90,0,0);
                    if(indicator.id=="guard-used")Bar("Protección gastada",new Vector3(0,0,-.13f),new Vector3(.35f,.035f,.04f),-45);break;
                case "evolved":
                    for(int i=0;i<4;i++)Bar("Estrella",new Vector3(0,0,-.065f),new Vector3(.33f,.045f,.045f),45*i);break;
                case "growing":
                    Bar("Tallo",new Vector3(0,-.025f,-.06f),new Vector3(.04f,.27f,.04f));
                    for(int i=0;i<2;i++)factory.Part(icon,"Hoja",factory.Sphere,new Vector3(i==0?-.08f:.08f,.055f,-.06f),new Vector3(.17f,.1f,.04f),color);break;
                default:
                    Bar("Aviso",new Vector3(0,.04f,-.06f),new Vector3(.065f,.19f,.05f));factory.Part(icon,"Punto",factory.Sphere,new Vector3(0,-.115f,-.06f),Vector3.one*.07f,color);break;
            }
            return new Badge{root=root,icon=icon,label=label,appeared=Time.unscaledTime};
        }
        void Update()
        {
            if(cameraView==null)return;poison.rotation=sleep.rotation=attack.transform.rotation=cameraView.transform.rotation;
            pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);poison.localScale=Vector3.one*(1+pulse*.6f);
            for(int i=0;i<3;i++)bubbles[i].localPosition=new Vector3(i*.12f,i*.22f+Mathf.Repeat(Time.unscaledTime*.28f+i*.17f,.5f),0);
            foreach(var pair in badges)
            {
                var b=pair.Value;if(!b.root.gameObject.activeSelf)continue;b.root.rotation=cameraView.transform.rotation;
                float entry=Mathf.Clamp01(1-(Time.unscaledTime-b.appeared));b.root.localScale=Vector3.one*(1+entry*.25f+.035f*Mathf.Sin(Time.unscaledTime*3));
                b.icon.localRotation=Quaternion.Euler(0,0,pair.Key=="slow"?Mathf.Sin(Time.unscaledTime)*18:Mathf.Sin(Time.unscaledTime*2)*4);
            }
            if(healthPulse>0)
            {
                healthPulse=Mathf.Max(0,healthPulse-Time.unscaledDeltaTime);float progress=1-healthPulse;
                healthChange.transform.rotation=cameraView.transform.rotation;healthChange.transform.localPosition=new Vector3(0,2.05f+progress*.9f,0);
                foreach(var spark in sparks){float angle=sparks.IndexOf(spark)*Mathf.PI/3+progress;spark.localPosition=new Vector3(Mathf.Cos(angle)*(.2f+progress*.4f),.7f+progress*1.2f,Mathf.Sin(angle)*(.2f+progress*.4f));spark.localScale=Vector3.one*.085f*healthPulse;}
                if(healthPulse==0)healthParticles.gameObject.SetActive(false);
            }
        }
    }
}
