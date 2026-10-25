using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Model-space details are assembled before the idle/arrival rigs capture their poses.
            Transform Flesh(Transform p,string name,Vector3 a,Vector3 b,float width,Color color)
            {
                var t=Ball(p,name,(a+b)*.5f,new Vector3(width,(b-a).magnitude*1.26f,width),color);
                t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return t;
            }
            void FinishUnitSculpture()
            {
                var head=UFind("Idle:Head");var body=UFind("Idle:Body");
                if(id=="larva-micelial"||id=="escorpion-solar")
                {
                    // Seat contact feet on the habitat surface, including the rounded mesh profile.
                    foreach(Transform child in model)child.localPosition+=Vector3.down*(id=="larva-micelial"?.026f:.016f);
                }
                EmbedDetail("Corazón del reino micelial",-.16f);
                EmbedDetail("Fractura del núcleo",-.09f);
                if(UFind("Torso animal")!=null){FinishAnimal(head);return;}
                if(UFind("Torso")!=null)
                {
                    Color flesh=id=="old-mummy"?ivory:id=="avatar-del-sol"?trim:skin;
                    Flesh(body,"Cuello anatómico",new Vector3(0,.79f,0),new Vector3(0,.94f,0),.135f,flesh);
                    for(int side=-1;side<=1;side+=2)
                    {
                        var arm=UFind(side<0?"Idle:ArmL":"Idle:ArmR");
                        var elbow=new Vector3(side*.055f,-.13f,-.025f);var hand=new Vector3(side*.08f,-.25f,-.08f);
                        var old=UFind("Brazo",arm);if(old!=null)BoardMeshFactory.Release(old.gameObject);
                        Flesh(arm,"Bíceps esculpido",Vector3.zero,elbow,.105f,flesh);
                        Flesh(arm,"Antebrazo esculpido",elbow,hand,.085f,flesh);
                        Ball(arm,"Codo",elbow,Vector3.one*.086f,flesh);
                        Ball(head,"Oreja",new Vector3(side*.12f,-.005f,.01f),new Vector3(.055f,.085f,.045f),flesh);
                        Bone(head,"Ceja esculpida",new Vector3(side*.035f,.056f,-.113f),new Vector3(side*.094f,.05f,-.103f),.018f,dark);
                        for(int j=0;j<2;j++)Bone(body,"Pliegue de túnica",new Vector3(side*(.07f+j*.045f),.29f,-.17f),new Vector3(side*(.04f+j*.03f),.46f,-.142f),.012f,Color.Lerp(cloth,dark,.25f));
                    }
                    Bone(head,"Boca",new Vector3(-.035f,-.074f,-.105f),new Vector3(.035f,-.074f,-.105f),.012f,dark);
                    if(id=="old-mummy")UColor("Oreja",ivory);
                }
                if(UFind("Torso micelial")!=null||UFind("Tronco tallado")!=null)
                {
                    bool tree=UFind("Tronco tallado")!=null;
                    float neck=head.localPosition.y;
                    Flesh(body,"Unión del cuello",new Vector3(0,neck-.2f,0),new Vector3(0,neck-.045f,0),.19f,tree?skin:ivory);
                    if(!tree)
                    {
                        var cap=UFind("Capucha de laminillas");
                        if(cap!=null)for(int j=0;j<6;j++)
                        {
                            float a=(j*30+15)*Mathf.Deg2Rad;float r=cap.localScale.x*.36f;
                            Bone(head,"Laminilla radial",new Vector3(Mathf.Cos(a)*.12f,.052f,-Mathf.Sin(a)*.09f),new Vector3(Mathf.Cos(a)*r,.052f,-Mathf.Sin(a)*cap.localScale.z*.36f),.012f,Color.Lerp(ivory,cloth,.22f));
                        }
                    }
                    foreach(var arm in body.GetComponentsInChildren<Transform>().Where(t=>t.name=="Idle:ArmL"||t.name=="Idle:ArmR").ToArray())
                    {
                        int side=arm.name.EndsWith("L")?-1:1;var end=new Vector3(side*.13f,-.2f,-.05f);
                        Flesh(arm,"Fibra del antebrazo",new Vector3(side*.055f,-.09f,-.02f),end,tree?.082f:.065f,skin);
                        for(int j=0;j<2;j++)Bone(arm,"Dedos de raíz",end+new Vector3((j-.5f)*.035f,-.01f,-.015f),end+new Vector3((j-.5f)*.035f,-.055f,-.035f),.023f,tree?skin:ivory);
                    }
                }
                if(UFind("Coraza de roca")!=null)
                {
                    Flesh(body,"Nexo cervical de basalto",new Vector3(0,.84f,0),new Vector3(0,.98f,0),.18f,stone);
                    for(int side=-1;side<=1;side+=2)
                    {
                        var arm=UFind(side<0?"Idle:ArmL":"Idle:ArmR");
                        for(int j=0;j<3;j++)Box(arm,"Nudillo de escoria",new Vector3(side*.08f+(j-1)*.046f,-.37f,-.137f),new Vector3(.043f,.07f,.045f),stone);
                        Bone(head,"Arco superciliar de basalto",new Vector3(side*.035f,.065f,-.123f),new Vector3(side*.12f,.04f,-.1f),.038f,stone);
                    }
                }
                if(id=="baby-phoenix")
                {
                    Flesh(model,"Cuello del fénix",new Vector3(0,.65f,-.035f),new Vector3(0,.79f,-.08f),.14f,trim);
                    for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++)Bone(model,"Garra de ave",new Vector3(side*.085f,.3f,-.04f),new Vector3(side*.085f+(j-1)*.025f,.28f,-.1f),.015f,dark);
                }
                if(id.Contains("alligator"))
                {
                    for(int j=0;j<4;j++)for(int side=-1;side<=1;side+=2)Ball(model,"Escama lateral de caimán",new Vector3(side*.135f,.26f,-.15f+j*.12f),new Vector3(.065f,.075f,.1f),Color.Lerp(skin,leaf,.5f));
                    for(int side=-1;side<=1;side+=2)Ball(head,"Orificio nasal",new Vector3(side*.075f,.113f,-.27f),new Vector3(.028f,.016f,.036f),dark);
                }
                if(id=="escorpion-solar"||id=="escarabajo-de-cobre"||id=="battle-ant-token")
                {
                    // Continuous ventral joints tie all walking limbs into the thorax.
                    for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++)Ball(model,"Coxa del artrópodo",new Vector3(side*.105f,.2f,-.16f+j*.13f),new Vector3(.08f,.07f,.075f),skin);
                }
                if(id=="sprinting-sprite")for(int side=-1;side<=1;side+=2)
                {
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",new Vector3(side*.08f,.06f,0));
                    var elbow=new Vector3(side*.06f,-.11f,side*.07f);var hand=new Vector3(side*.065f,-.15f,-side*.08f);
                    Flesh(arm,"Brazo del corredor",Vector3.zero,elbow,.06f,trim);
                    Flesh(arm,"Antebrazo del corredor",elbow,hand,.05f,trim);
                    Gem(arm,"Mano del corredor",hand,new Vector3(.065f,.09f,.07f),glow);
                }
            }
            void EmbedDetail(string name,float z)
            {
                foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name==name))
                {var p=t.localPosition;p.z=z;t.localPosition=p;}
            }
            void FinishAnimal(Transform head)
            {
                bool deer=Has("ciervo"),camel=Has("cammel"),rabbit=Has("rabbit"),panther=Has("panther"),ferret=Has("ferret"),beaver=Has("bober");
                Color fur=panther?new Color(.075f,.1f,.16f):rabbit?new Color(.8f,.76f,.63f):Has("bestia-micelial")?new Color(.27f,.38f,.3f):skin;
                var torso=UFind("Torso animal");float h=torso.localPosition.y;
                // The rear and chest masses overlap the torso; limbs are never attached to thin air.
                Ball(model,"Pecho animal",new Vector3(0,h,-.14f),new Vector3(torso.localScale.x*.92f,torso.localScale.y*.98f,.29f),fur);
                Ball(model,"Pelvis animal",new Vector3(0,h,.2f),new Vector3(torso.localScale.x,torso.localScale.y*1.02f,.3f),fur);
                if(rabbit)
                {
                    URemove("Cola","Curva de cola");Ball(UFind("Idle:Tail"),"Cola de algodón del conejo",new Vector3(0,.015f,.075f),Vector3.one*.16f,ivory);
                    for(int side=-1;side<=1;side+=2)Ball(model,"Muslo del conejo",new Vector3(side*.135f,.22f,.19f),new Vector3(.22f,.3f,.27f),fur);
                }
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Músculo mandibular",new Vector3(side*.065f,-.077f,-.065f),new Vector3(.12f,.09f,.16f),fur);
                    if(!deer&&!camel)for(int j=0;j<2;j++)Bone(head,"Bigote fino",new Vector3(side*.055f,-.05f,-.2f),new Vector3(side*.145f,-.035f-j*.025f,-.205f),.006f,ivory);
                    if(deer||camel)Bone(model,"Hendidura de pezuña",new Vector3(side*.16f,.024f,-.315f),new Vector3(side*.16f,.075f,-.315f),.009f,ivory);
                    else if(!beaver)for(int j=0;j<2;j++)Bone(model,"Dedos delanteros",new Vector3(side*.16f+(j-.5f)*.035f,.035f,-.27f),new Vector3(side*.16f+(j-.5f)*.035f,.035f,-.315f),.012f,panther?ivory:fur);
                }
                if(ferret)
                {
                    // Long low flexible body and a tapered tail, rather than a second upright mongoose.
                    URemove("Anillo de cola del hurón","Cola");var tail=UFind("Idle:Tail");
                    Flesh(tail,"Cola ahusada de hurón",new Vector3(0,0,0),new Vector3(.055f,.015f,.245f),.095f,fur);
                    Flesh(tail,"Punta de cola de hurón",new Vector3(.045f,.014f,.18f),new Vector3(.1f,.055f,.285f),.057f,dark);
                }
                if(camel)
                {
                    URemove("Alforja tejida");
                    Flesh(head,"Labio de camello",new Vector3(-.065f,-.075f,-.2f),new Vector3(.065f,-.075f,-.2f),.075f,fur);
                }
                if(beaver)for(int j=0;j<4;j++)Bone(UFind("Idle:Tail"),"Escama de paleta",new Vector3(-.1f,.049f,.04f+j*.055f),new Vector3(.1f,.049f,.095f+j*.055f),.009f,trim);
            }
            void FinishStructureSculpture()
            {
                EmbedDetail("Fisura de ruptura",-.09f);
                EmbedDetail("Canal de ascua",-.19f);
                EmbedDetail("Glifo de umbral",-.075f);
                switch(id)
                {
                    case "santuario-del-sueno":Stem(model,"Pedúnculo de flor del sueño",new Vector3(0,.065f,-.13f),.04f,.11f,leaf);break;
                    case "void-beacon":
                        for(int side=-1;side<=1;side+=2)Bone(model,"Engarce de aleta direccional",new Vector3(side*.09f,.86f,0),new Vector3(side*.22f,.86f,0),.025f,trim);
                        break;
                    case "bosque-eterno":
                        foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Copa de bosque eterno").ToArray())BoardMeshFactory.Release(t.gameObject);
                        foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Tronco del bosque eterno").ToArray())
                        {var size=t.localScale;size.y*=.82f;t.localScale=size;}
                        UColor("Tronco del bosque eterno",Color.Lerp(skin,ivory,.12f));
                        UColor("Rama petrificada",Color.Lerp(skin,ivory,.12f));
                        for(int j=0;j<3;j++)
                        {
                            var p=Rad(j,3,.21f,.025f);float h=j==1?.94f:.69f;
                            for(int level=0;level<3;level++)P(model,"Piso de ramas perennes",m.Cone,p+Vector3.up*(h*(.35f+level*.2f)),new Vector3(.36f-level*.075f,h*.43f,.36f-level*.075f),j==1?leaf:cloth);
                        }
                        break;
                    case "trono-solar":
                        for(int side=-1;side<=1;side+=2)for(int z=-1;z<=1;z+=2)Box(model,"Pata del trono",new Vector3(side*.14f,.28f,z*.115f),new Vector3(.065f,.2f,.065f),trim);
                        var rug=UFind("Alfombra solar");rug.localPosition=new Vector3(0,.197f,-.17f);
                        break;
                    case "vigilant-tower":
                        for(int side=-1;side<=1;side+=2)
                        {
                            Bone(model,"Larguero de escalera",new Vector3(side*.125f,.08f,-.25f),new Vector3(side*.125f,.81f,-.25f),.027f,skin);
                            Bone(model,"Barandilla del mirador",new Vector3(-.25f,.98f,side*.19f),new Vector3(.25f,.98f,side*.19f),.03f,trim);
                        }
                        break;
                    case "sandy-anthill":
                        foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Cono del hormiguero").ToArray())BoardMeshFactory.Release(t.gameObject);
                        // A broad eroded mound joins its chimneys and seats both tunnel mouths.
                        Ball(model,"Zócalo continuo de hormiguero",new Vector3(0,.095f,.03f),new Vector3(.67f,.23f,.5f),stone);
                        for(int j=0;j<3;j++)
                        {
                            var p=new Vector3((j-1)*.14f,0,j%2*.12f);float h=.48f+j*.1f;
                            Ball(model,"Montículo erosionado",p+Vector3.up*h*.44f,new Vector3(.37f,h,.36f),stone);
                            Ball(model,"Chimenea de tierra",p+Vector3.up*h*.82f,new Vector3(.18f,h*.42f,.18f),stone);
                            P(model,"Respiradero del hormiguero",m.Cylinder,p+Vector3.up*h*1.01f,new Vector3(.065f,.008f,.065f),dark);
                        }
                        for(int side=-1;side<=1;side+=2)Ball(model,"Umbral erosionado",new Vector3(side*.13f,.07f,-.21f),new Vector3(.18f,.075f,.15f),trim);
                        break;
                    case "ciudad-sepultada":
                    case "templo-de-ceniza":
                    case "santuario-de-arena":
                    case "obelisco-de-arena":
                    case "obelisco-solar":
                        FinishMasonry();break;
                    case "campamento-nomada":
                    case "med-camp":
                    case "resting-camp":FinishTent();break;
                    case "arbol-madre":
                        for(int j=0;j<4;j++)Flesh(model,"Estría del tronco madre",new Vector3(Mathf.Cos(j*1.57f)*.1f,.12f,Mathf.Sin(j*1.57f)*.1f),new Vector3(Mathf.Cos(j*1.57f)*.085f,.65f,Mathf.Sin(j*1.57f)*.085f),.035f,dark);
                        break;
                    case "nido-de-esporas":Ring(model,"Trenza del borde del nido",Vector3.up*.21f,.27f,.045f,trim,10);break;
                    case "semillero-micelial":
                        for(int side=-1;side<=1;side+=2)Box(model,"Cierre lateral del bancal",new Vector3(side*.3f,.12f,0),new Vector3(.055f,.09f,.52f),trim);
                        break;
                    case "red-micelial":
                        for(int j=0;j<3;j++)StructureRoot(new Vector3(0,.1f,0),Rad(j,3,.31f,.035f),.035f,ivory);break;
                    case "frogpit":
                        for(int j=0;j<3;j++)Ball(model,"Huevos visibles de rana",new Vector3(.09f+j*.025f,.165f,-.1f),Vector3.one*.028f,dark);break;
                    case "oasis-de-cristal":Ring(model,"Ondulación del oasis",new Vector3(-.035f,.064f,-.04f),.11f,.008f,ivory,10);break;
                    case "mana-pool":Ring(model,"Oleaje arcano",Vector3.up*.104f,.19f,.009f,glow,10);break;
                }
            }
            void FinishMasonry()
            {
                // Shallow joints are attached to each real wall, with its own transform/size.
                foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Torre de ciudad sepultada"||t.name=="Santuario de basalto"||t.name=="Dintel del santuario").ToArray())
                {
                    var size=t.localScale;var p=t.localPosition;
                    for(int j=1;j<=2;j++)Bone(model,"Junta de sillares",p+new Vector3(-size.x*.44f,-size.y*.5f+size.y*j/3,-size.z*.501f),p+new Vector3(size.x*.44f,-size.y*.5f+size.y*j/3,-size.z*.501f),.009f,Color.Lerp(stone,dark,.45f));
                }
            }
            void FinishTent()
            {
                var tent=UFind("Tienda de campaña");if(tent==null)return;
                foreach(var t in tent.GetComponentsInChildren<Transform>().Where(t=>t.name=="Lona de tienda").ToArray())BoardMeshFactory.Release(t.gameObject);
                for(int side=-1;side<=1;side+=2)
                {
                    // Continuous ridge-to-foot panels; no unsupported roof edge or gap at the peak.
                    var panel=Box(tent,"Lona tensada",new Vector3(side*.14f,.23f,0),new Vector3(.035f,Mathf.Sqrt(.28f*.28f+.46f*.46f),.52f),cloth);
                    panel.localRotation=Quaternion.Euler(0,0,side*31.33f);
                    Bone(tent,"Costura de lona",new Vector3(0,.46f,.24f),new Vector3(side*.28f,0,.24f),.015f,trim);
                    Bone(tent,"Piqueta",new Vector3(side*.3f,-.07f,.2f),new Vector3(side*.3f,.005f,.2f),.025f,skin);
                    Bone(tent,"Viento de tienda",new Vector3(side*.2f,.12f,.24f),new Vector3(side*.3f,-.045f,.2f),.01f,ivory);
                }
            }
        }
    }
}

