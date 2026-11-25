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

            bool SculptSwampUnit()
            {
                switch (id)
                {
                    case "larva-micelial": MycelialLarva(); return true;
                    case "bestia-micelial": MycelialBoar(); return true;
                    case "ciervo-de-esporas": AntleredSporeDeer(); return true;
                    case "espora": MycelialSeed(false); return true;
                    case "mushroom-token": MycelialSeed(true); return true;
                    case "frog-token": PondFrog(); return true;
                    case "firefly-token": MarshFirefly(); return true;
                    case "lushroom": ScarletLushroom(); return true;
                    case "alligator-revengeful-bite": SwampAlligator(true); return true;
                    case "alligator-token": SwampAlligator(false); return true;
                    default: return false;
                }
            }

            void MycelialLarva()
            {
                Color skinColor=Pigment(0x8FA963), darker=Pigment(0x576F53), pale=Pigment(0xC7CB82), antenna=Pigment(0xE3D283);
                var body=G(model,"Idle:Body",Vector3.zero);
                Sweep(body,"Vientre orgánico de la larva",darker,Knot(-.07f,.16f,.31f,.024f),
                    Knot(-.035f,.183f,.223f,.079f),Knot(.038f,.171f,.076f,.107f),Knot(.049f,.155f,-.085f,.105f),
                    Knot(.021f,.151f,-.233f,.066f));
                for(int i=0;i<5;i++)
                {
                    float z=.224f-i*.094f;
                    float x=Mathf.Sin(i*.8f-.6f)*.053f;
                    float h=.169f+Mathf.Sin(i*.65f)*.027f;
                    var shell=Ball(body,"Anillo del dorso",V(x,h+.044f,z),
                        V(.189f+i*.022f,.215f+i*.008f,.146f),i%2==0?skinColor:pale);
                    shell.localRotation=Quaternion.Euler(8-i*4,16-i*7,0);
                    if(i%2==0)
                        for(int side=-1;side<=1;side+=2)
                            Ball(body,"Mancha verde dorsal",V(x+side*.066f,h+.141f,z+.013f),V(.055f,.03f,.061f),darker);
                    for(int side=-1;side<=1;side+=2)
                        Ball(body,"Pata blanda de larva",V(x+side*.096f,.065f,z-.018f),V(.082f,.093f,.098f),darker);
                }
                var head=G(body,"Idle:Head",V(.035f,.167f,-.27f));
                Ball(head,"Cabeza ovalada de larva",Vector3.zero,V(.256f,.22f,.213f),Pigment(0x9F7951));
                Ball(head,"Hocico cálido",V(0,-.037f,-.084f),V(.185f,.103f,.112f),Pigment(0xC09B6A));
                MiniatureEyes(head,.067f,.011f,-.089f,.064f,Pigment(0x70552F),Pigment(0xDFCC9A));
                for(int side=-1;side<=1;side+=2)
                {
                    Sweep(head,"Antena redondeada "+side,darker,Knot(side*.061f,.071f,.001f,.022f),
                        Knot(side*.089f,.145f,-.027f,.016f),Knot(side*.125f,.207f,-.035f,.011f));
                    Ball(head,"Espora sensorial",V(side*.126f,.213f,-.037f),V(.078f,.084f,.078f),antenna);
                }
            }

            void MycelialBoar()
            {
                Color rind=Pigment(0x435D4F), shoulders=Pigment(0x597556), cap=Pigment(0xA15F77);
                Color moss=Pigment(0x7D8D5A), bone=Pigment(0xDFCB9B);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Lomo en arco",V(0,.381f,.052f),V(.382f,.405f,.582f),rind);
                Ball(body,"Espaldar micelial",V(0,.482f,-.051f),V(.403f,.229f,.478f),shoulders);
                for(int i=0;i<3;i++)
                {
                    var p=V((i-1)*.143f,.474f+i%2*.092f,.059f+i%2*.071f);
                    Mushroom(body,p,.091f+i%2*.062f,.231f+i%2*.094f,i==1?cap:Pigment(0x784B66));
                }
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<2;i++)
                    {
                        float z=i==0?-.139f:.227f;
                        Vector3 foot=V(side*.163f,.059f,z+side*.05f);
                        CurvedLimb(body,"Pata de jabalí "+side+"/"+i,rind,V(side*.108f,.321f,z),
                            V(side*.172f,.173f,z-.034f),foot,.093f,.062f,.054f);
                        Ball(body,"Pezuña partida",foot+V(0,0,-.043f),V(.135f,.114f,.151f),Pigment(0x3A443D));
                    }
                    Plate(body,"Musgo del lomo",V(side*.155f,.471f,-.121f),V(.179f,.259f,.086f),moss,
                        V(63,side*27,-side*21),"leaf",LeafEdge);
                }
                var head=G(body,"Idle:Head",V(0,.306f,-.271f));
                head.localRotation=Quaternion.Euler(-6,11,4);
                Ball(head,"Cráneo de jabalí",V(0,.039f,.007f),V(.287f,.265f,.274f),rind);
                Ball(head,"Hocico plano",V(0,-.032f,-.141f),V(.22f,.153f,.204f),shoulders);
                Ball(head,"Morro de cuero",V(0,-.014f,-.225f),V(.157f,.104f,.06f),Pigment(0x3B4035));
                for(int side=-1;side<=1;side+=2)
                {
                    Sweep(head,"Colmillo curvo "+side,bone,Knot(side*.087f,-.063f,-.145f,.051f),
                        Knot(side*.153f,-.034f,-.188f,.038f),Knot(side*.172f,.041f,-.198f,.024f),
                        Knot(side*.145f,.094f,-.202f,.006f));
                    Plate(head,"Oreja del jabalí",V(side*.114f,.153f,.018f),V(.117f,.154f,.081f),rind,V(-12,0,-side*33),"leaf",LeafEdge);
                    Ball(head,"Ojo dorado",V(side*.099f,.06f,-.112f),V(.048f,.041f,.04f),Pigment(0xBFCA61));
                }
                var tail=G(body,"Idle:Tail",V(0,.356f,.28f));
                Sweep(tail,"Cola del jabalí",rind,Knot(0,0,0,.044f),Knot(.039f,-.014f,.121f,.038f),
                    Knot(.086f,.04f,.183f,.025f),Knot(.121f,.091f,.153f,.008f));
            }

            void AntleredSporeDeer()
            {
                Color fur=Pigment(0xA37751), belly=Pigment(0xCFAD77), antler=Pigment(0xE0CDA1), darkFur=Pigment(0x70583F);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Lomo estrecho del ciervo",V(0,.482f,.039f),V(.264f,.276f,.513f),fur);
                Ball(body,"Pecho claro",V(0,.466f,-.15f),V(.211f,.265f,.253f),belly);
                Sweep(body,"Cuello del ciervo",fur,Knot(0,.469f,-.133f,.117f),Knot(0,.583f,-.187f,.102f),
                    Knot(.008f,.686f,-.193f,.067f),Knot(.011f,.772f,-.234f,.061f));
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<2;i++)
                    {
                        float z=i==0?-.139f:.238f;
                        Vector3 hoof=V(side*.123f,.049f,z+(i==0?side*.058f:side*.076f));
                        CurvedLimb(body,"Pata fina del ciervo "+side+"/"+i,fur,V(side*.08f,.471f,z),
                            V(side*.124f,.253f,z+(i==0?-.019f:.054f)),hoof,.064f,.039f,.028f);
                        Ball(body,"Pezuña del ciervo",hoof+V(0,-.012f,-.016f),V(.083f,.075f,.103f),darkFur);
                    }
                    for(int i=0;i<3;i++)
                        Ball(body,"Mota de pelaje",V(side*.126f,.537f-i%2*.039f,.06f+i*.065f),
                            V(.030f,.041f,.037f),belly);
                }
                Plate(body,"Cola levantada",V(0,.513f,.297f),V(.116f,.157f,.072f),belly,V(-53,0,5),"leaf",LeafEdge);
                var head=G(body,"Idle:Head",V(.012f,.781f,-.246f));
                head.localRotation=Quaternion.Euler(-6,-13,4);
                Ball(head,"Cabeza del ciervo",V(0,0,-.013f),V(.198f,.225f,.215f),fur);
                Ball(head,"Morro largo",V(0,-.05f,-.106f),V(.151f,.112f,.155f),belly);
                Ball(head,"Nariz del ciervo",V(0,-.04f,-.185f),V(.075f,.06f,.045f),darkFur);
                for(int side=-1;side<=1;side+=2)
                {
                    Plate(head,"Oreja abierta",V(side*.098f,.067f,.031f),V(.085f,.171f,.069f),fur,
                        V(-7,side*15,-side*62),"leaf",LeafEdge);
                    Sweep(head,"Asta ramificada "+side,antler,Knot(side*.048f,.092f,.036f,.033f),
                        Knot(side*.116f,.208f,.039f,.029f),Knot(side*.176f,.299f,.066f,.025f),
                        Knot(side*.208f,.39f,.051f,.019f),Knot(side*.187f,.457f,.034f,.006f));
                    Sweep(head,"Rama exterior "+side,antler,Knot(side*.151f,.27f,.052f,.027f),
                        Knot(side*.224f,.304f,.091f,.019f),Knot(side*.269f,.348f,.10f,.006f));
                    Sweep(head,"Rama interior "+side,antler,Knot(side*.203f,.369f,.052f,.021f),
                        Knot(side*.139f,.393f,.011f,.017f),Knot(side*.107f,.429f,-.01f,.005f));
                    Ball(head,"Espora del asta",V(side*.268f,.348f,.10f),V(.058f,.065f,.058f),Pigment(0xB9D981));
                    Ball(head,"Ojo lateral",V(side*.076f,.038f,-.078f),V(.055f,.054f,.042f),darkFur);
                }
                P(body,"Sombrero pequeño del ciervo",m.MushroomCap,V(0,.588f,.146f),V(.203f,.196f,.175f),Pigment(0x65845F));
            }

            void MycelialSeed(bool mushroom)
            {
                Color cap=Pigment(mushroom?0xA67557:0x9D78B4), shade=Pigment(mushroom?0x805A4C:0x725681);
                Color fiber=Pigment(mushroom?0xD0BE96:0xC3B2A7), glowColor=Pigment(mushroom?0xB5C981:0xE2C2ED);
                var body=G(model,"Idle:Body",V(0,mushroom?0:.079f,0));
                Ball(body,mushroom?"Tallo compacto":"Cuerpo de espora",V(0,.263f,0),V( .215f,.313f,.199f),fiber);
                var head=G(body,"Idle:Head",V(0,.352f,-.035f));
                head.localRotation=Quaternion.Euler(-10,mushroom?10:-6,mushroom?-8:11);
                Ball(head,"Rostro bajo las laminillas",V(0,-.031f,-.073f),V(.194f,.166f,.141f),fiber);
                MiniatureEyes(head,.055f,-.019f,-.127f,.052f,shade,Pigment(0xE9D9AC));
                float wide=mushroom?.355f:.28f;
                P(head,"Sombrero de brote",m.MushroomCap,V(0,.051f,.033f),V(wide,mushroom?.265f:.375f,wide*.9f),cap);
                P(head,"Laminillas orgánicas",m.MushroomCap,V(0,.028f,.033f),V(wide*.91f,.155f,wide*.85f),shade);
                for(int j=0;j<5;j++)
                {
                    var p=V(0,.051f,.033f)+Rad(j,5,wide*.23f,mushroom?.057f:.077f);
                    Ball(head,"Mancha de micelio",p,V(.052f,.03f,.046f),glowColor);
                }
                for(int side=-1;side<=1;side+=2)
                {
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.081f,.292f,.009f));
                    CurvedLimb(arm,"Raicilla del brote "+side,fiber,Vector3.zero,V(side*.059f,-.073f,-.027f),
                        V(side*.071f,-.131f,-.071f),.039f,.029f,.018f);
                    Ball(arm,"Extremo de raíz",V(side*.071f,-.135f,-.075f),V(.062f,.061f,.069f),fiber);
                    if(mushroom)
                    {
                        Flesh(body,"Pie unido al tallo",V(side*.061f,.173f,.013f),V(side*.095f,.045f,-.037f),.084f,fiber);
                        Ball(body,"Pie de hongo",V(side*.097f,.046f,-.042f),V(.102f,.093f,.137f),shade);
                    }
                    else
                        Sweep(body,"Filamento de espora "+side,shade,Knot(side*.056f,.194f,.049f,.031f),
                            Knot(side*.092f,.107f,.098f,.025f),Knot(side*.073f,.058f,.122f,.019f),
                            Knot(side*.035f,.034f,.081f,.006f));
                }
            }

            void PondFrog()
            {
                Color green=Pigment(0x74A24B), yellow=Pigment(0xBAD071), moss=Pigment(0x507548), foot=Pigment(0x81AE64);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso agazapado del sapo",V(0,.186f,.061f),V(.359f,.309f,.38f),green);
                Ball(body,"Buche amarillo",V(0,.205f,-.109f),V(.286f,.223f,.184f),yellow);
                for (int side=-1;side<=1;side+=2)
                {
                    Ball(body,"Anca plegada del sapo",V(side*.159f,.16f,.126f),V(.199f,.251f,.247f),moss);
                    Ball(body,"Tibia plegada",V(side*.214f,.085f,.121f),V(.142f,.124f,.206f),green)
                        .localRotation=Quaternion.Euler(5,side*30,0);
                    var palm=V(side*.145f,.039f,-.179f);
                    CurvedLimb(body,"Brazo fino de sapo "+side,moss,V(side*.102f,.206f,-.081f),
                        V(side*.139f,.107f,-.13f),palm,.054f,.041f,.033f);
                    Plate(body,"Membrana de pie palmeado",palm+V(0,.001f,-.054f),V(.108f,.132f,.028f),
                        foot,V(91,0,-side*11),"wing",WingEdge);
                    for(int toe=-1;toe<=1;toe++)
                        Sweep(body,"Dedo de sapo "+side+"/"+toe,foot,Knot(palm.x,palm.y,palm.z,.026f),
                            Knot(palm.x+toe*.036f,.03f,palm.z-.068f,.017f),
                            Knot(palm.x+toe*.058f,.026f,palm.z-.108f,.008f));
                }
                var head=G(body,"Idle:Head",V(0,.307f,-.081f));
                head.localRotation=Quaternion.Euler(-5,9,3);
                Ball(head,"Cabeza ancha del sapo",Vector3.zero,V(.336f,.182f,.238f),green);
                Sweep(head,"Comisura de sapo",moss,Knot(-.126f,-.029f,-.062f,.014f),
                    Knot(-.066f,-.035f,-.116f,.016f),Knot(.061f,-.035f,-.117f,.016f),Knot(.137f,-.025f,-.064f,.012f));
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Ojo elevado",V(side*.117f,.071f,-.015f),V(.143f,.147f,.131f),yellow);
                    Ball(head,"Párpado del sapo",V(side*.127f,.106f,-.005f),V(.127f,.068f,.108f),green);
                    Ball(head,"Pupila anfibia horizontal",V(side*.123f,.072f,-.079f),V(.083f,.047f,.035f),ink);
                    Ball(head,"Reflejo del ojo",V(side*.123f-.015f,.086f,-.095f),Vector3.one*.017f,ivory);
                    Ball(body,"Mota moteada del sapo",V(side*.131f,.29f,.11f),V(.052f,.034f,.059f),moss);
                }
            }

            void MarshFirefly()
            {
                Color shell=Pigment(0x445E4F), ridge=Pigment(0x607D58), wingTint=Pigment(0xCAC69B);
                var body=G(model,"Idle:Body",V(0,.535f,0));
                Ball(body,"Abdomen verde oscuro",V(0,0,.116f),V(.231f,.215f,.363f),shell);
                Ball(body,"Linterna del abdomen",V(0,-.009f,.239f),V(.188f,.204f,.187f),Pigment(0xF8CA61));
                Ball(body,"Corazón de la linterna",V(0,-.022f,.289f),V(.107f,.146f,.11f),Pigment(0xFFE49B));
                Ball(body,"Tórax",V(0,.025f,-.077f),V(.209f,.181f,.217f),ridge);
                var head=G(body,"Idle:Head",V(0,.026f,-.196f));
                Ball(head,"Cráneo del insecto",Vector3.zero,V(.188f,.161f,.165f),shell);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Ojo brillante",V(side*.066f,.025f,-.06f),V(.067f,.073f,.063f),Pigment(0x685A38));
                    Ball(head,"Reflejo dorado",V(side*.067f,.037f,-.091f),V(.024f,.025f,.016f),Pigment(0xEAD690));
                    Sweep(head,"Antena de luciérnaga "+side,ridge,Knot(side*.041f,.052f,-.012f,.021f),
                        Knot(side*.086f,.103f,-.063f,.015f),Knot(side*.133f,.156f,-.119f,.006f));
                    for(int i=0;i<3;i++)
                    {
                        float z=i*.095f-.112f;
                        CurvedLimb(body,"Pata suspendida "+side+"/"+i,ridge,V(side*.067f,-.022f,z),
                            V(side*.158f,-.057f,z+.043f),V(side*.184f,-.159f,z+.023f),.024f,.016f,.008f);
                    }
                    var wing=G(body,side<0?"Idle:WingL":"Idle:WingR",V(side*.068f,.08f,-.035f));
                    wing.localRotation=Quaternion.Euler(27,-side*23,-side*62);
                    var membrane=Plate(wing,"Ala de nácar",V(0,.159f,.017f),V(.178f,.401f,.036f),wingTint,
                        V(9,side*11,0),"wing",WingEdge);
                    Bone(membrane,"Nervadura",V(0,-.37f,-.53f),V(.13f,.39f,-.54f),.048f,ridge);
                    Plate(wing,"Élitro abierto",V(-side*.073f,.078f,.085f),V(.105f,.239f,.072f),shell,V(-34,0,side*21),"feather",FeatherEdge);
                }
            }

            void ScarletLushroom()
            {
                Color moss=Pigment(0x728F56), pale=Pigment(0xA6AA74), wood=Pigment(0x455F57), shale=Pigment(0x527765);
                Color scarlet=Pigment(0xAF3B35), glowColor=Pigment(0x64C9C4), yellow=Pigment(0xF3C68C);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso ancho de raíces",V(0,.365f,.037f),V(.359f,.39f,.325f),wood);
                Ball(body,"Vientre musgoso redondo",V(0,.303f,-.095f),V(.331f,.284f,.238f),moss);
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 foot=V(side*.144f,.066f,side*.058f-.058f);
                    CurvedLimb(body,"Raíz de pierna "+side,wood,V(side*.101f,.31f,.049f),
                        V(side*.154f,.168f,.029f),foot,.101f,.086f,.068f);
                    Ball(body,"Pie redondeado",foot,V(.17f,.117f,.193f),wood);
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.16f,.432f,.011f));
                    Vector3 hand=V(side*.109f,-.209f,-.083f);
                    CurvedLimb(arm,"Brazo fibroso "+side,wood,Vector3.zero,V(side*.089f,-.122f,.035f),hand,.102f,.085f,.074f);
                    Ball(arm,"Nudo del hombro",V(side*.036f,-.015f,.02f),V(.21f,.198f,.183f),shale);
                    for(int i=0;i<3;i++)
                        Sweep(arm,"Dedo de raíz "+side+"/"+i,moss,Knot(hand.x+(i-1)*.045f,hand.y+.026f,hand.z,.04f),
                            Knot(hand.x+(i-1)*.055f,hand.y-.032f,hand.z-.013f,.031f),
                            Knot(hand.x+(i-1)*.052f,hand.y-.047f,hand.z-.053f,.012f));
                    Sweep(body,"Fibra del pecho "+side,shale,Knot(side*.09f,.468f,-.137f,.019f),
                        Knot(side*.124f,.361f,-.204f,.022f),Knot(side*.119f,.266f,-.175f,.019f),Knot(side*.087f,.212f,-.136f,.009f));
                }
                var head=G(body,"Idle:Head",V(0,.63f,-.045f));
                head.localRotation=Quaternion.Euler(-4,3,6);
                Ball(head,"Cara envejecida",V(0,-.026f,-.116f),V(.267f,.218f,.211f),pale);
                Ball(head,"Mandíbula del hongo",V(0,-.113f,-.118f),V(.206f,.116f,.18f),moss);
                for(int side=-1;side<=1;side+=2)
                    Ball(head,"Arruga de la frente",V(side*.064f,.038f,-.205f),V(.104f,.035f,.046f),moss)
                        .localRotation=Quaternion.Euler(0,0,side*16);
                MiniatureEyes(head,.059f,.018f,-.215f,.056f,Pigment(0x4E6247),Pigment(0xD9D395),7);
                var cap=G(head,"Sombrero carmesí de Lushroom",V(0,.132f,.081f));
                cap.localRotation=Quaternion.Euler(-3,0,-6);
                P(cap,"Láminas turquesa",m.MushroomCap,V(0,-.034f,0),V(.823f,.493f,.727f),glowColor);
                P(cap,"Cúpula carmesí",m.MushroomCap,V(0,.005f,0),V(.87f,.792f,.763f),scarlet);
                for(int i=0;i<10;i++)
                {
                    float a=i*Mathf.PI/5f;
                    var center=V(Mathf.Cos(a)*(.175f+i%2*.129f),i%2==0?.225f:.147f,Mathf.Sin(a)*(.144f+i%2*.126f));
                    Ball(cap,"Mota brillante",center,V(.061f,.037f,.052f),i%3==0?glowColor:yellow);
                    if(i%2==0)
                    {
                        var edge=V(Mathf.Cos(a+.21f)*.365f,.038f,Mathf.Sin(a+.21f)*.308f);
                        var ridge = Vector3.Lerp(center, edge, .68f) + V(0,.055f,0);
                        Sweep(cap,"Filigrana del sombrero "+i,glowColor,Knot(center.x,center.y,center.z,.011f),
                            Knot(ridge.x,ridge.y,ridge.z,.017f),Knot(edge.x,edge.y,edge.z,.009f));
                    }
                }
            }

            void SwampAlligator(bool adult)
            {
                Color back=Pigment(adult?0x5D813E:0x749C4D), plates=Pigment(adult?0x3F653E:0x4E7848);
                Color belly=Pigment(adult?0xC4AD72:0xD3C286), armor=Pigment(0x76583B), throat=Pigment(0x663D3C);
                float size=adult?1:.84f;
                var body=G(model,"Idle:Body",Vector3.zero);
                body.localScale=Vector3.one*size;
                Ball(body,"Cuerpo de caimán",V(0,.275f,.056f),V(.365f,adult?.334f:.274f,.569f),back)
                    .localRotation=Quaternion.Euler(adult?15:4,0,-3);
                Ball(body,"Vientre escamado",V(0,.228f,-.111f),V(.3f,.226f,.393f),belly);
                for(int row=0;row<4;row++)
                    for(int side=-1;side<=1;side+=2)
                    {
                        var plate=V(side*.062f,.416f-row*.016f,-.115f+row*.119f);
                        Gem(body,"Escama dorsal",plate,V(.111f,.12f,.17f),row%2==0?plates:back)
                            .localRotation=Quaternion.Euler(11,side*19,side*26);
                    }
                for(int i=0;i<2;i++)
                    for(int side=-1;side<=1;side+=2)
                    {
                        float z=i==0?-.137f:.21f;
                        Vector3 foot=V(side*(adult?.255f:.211f),.056f+(adult&&side>0&&i==0?.11f:0),z+(i==0?-.073f:.055f));
                        CurvedLimb(body,"Brazo de caimán "+side+"/"+i,back,V(side*.1f,.277f,z),
                            V(side*.195f,.151f,z+.017f),foot,.098f,.075f,.052f);
                        Ball(body,"Mano de caimán",foot+V(0,0,-.031f),V(.139f,.082f,.132f),plates);
                        for(int finger=-1;finger<=1;finger++)
                            Gem(body,"Uña clara",foot+V(finger*.035f,-.008f,-.095f),V(.041f,.082f,.036f),belly)
                                .localRotation=Quaternion.Euler(-59,finger*14,0);
                    }
                if(adult)
                {
                    for(int side=-1;side<=1;side+=2)
                        Plate(body,"Protección de cuero",V(side*.164f,.372f,-.062f),V(.179f,.238f,.088f),armor,
                            V(18,side*47,side*16),"shield",ShieldEdge);
                    Bone(body,"Correa de cuero",V(-.164f,.32f,-.18f),V(.161f,.391f,-.082f),.063f,armor);
                    Box(body,"Hebilla bronce",V(.081f,.378f,-.168f),V(.075f,.074f,.048f),Pigment(0xB89051));
                }

                var head=G(body,"Idle:Head",V(0,adult?.387f:.233f,-.223f));
                head.localRotation=Quaternion.Euler(adult?16:2,adult?-16:6,-3);
                Flesh(body,"Nuca conectada",V(0,.283f,-.127f),head.localPosition,.243f,back);
                Ball(head,"Cráneo de escamas",V(0,.02f,.012f),V(.3f,.217f,.242f),back);
                Ball(head,"Fondo de la boca",V(0,-.026f,-.068f),V(.259f,.141f,.219f),throat);
                var upper=G(head,"Hocico superior",V(0,adult?.060f:.013f,-.057f));
                upper.localRotation=Quaternion.Euler(adult?19:4,0,0);
                Ball(upper,"Morro escamado",V(0,.021f,-.143f),V(.299f,.12f,.352f),back);
                Ball(upper,"Paladar",V(0,-.029f,-.139f),V(.259f,.041f,.322f),throat);
                var lower=G(head,"Mandíbula inferior",V(0,adult?-.096f:-.049f,-.056f));
                lower.localRotation=Quaternion.Euler(adult?-26:-2,0,0);
                Ball(lower,"Mandíbula crema",V(0,-.01f,-.145f),V(.272f,.105f,.334f),belly);
                if(adult) Ball(lower,"Lengua de la boca abierta",V(0,.035f,-.134f),V(.155f,.046f,.232f),Pigment(0xB77262));
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Ceja de escama",V(side*.094f,.091f,-.045f),V(.143f,.078f,.103f),plates);
                    Ball(head,"Ojo del caimán",V(side*.106f,.064f,-.094f),V(.084f,.07f,.056f),Pigment(0xD7BC57));
                    Ball(head,"Pupila del caimán",V(side*.108f,.066f,-.122f),V(.029f,.052f,.016f),ink);
                    Ball(upper,"Nariz del caimán",V(side*.081f,.069f,-.242f),V(.046f,.052f,.037f),plates);
                    for(int tooth=0;tooth<(adult?5:3);tooth++)
                    {
                        float z=-.073f-tooth*(adult?.045f:.072f);
                        float width=side*(.12f-Mathf.Max(0,tooth-2)*.013f);
                        var point=Gem(upper,"Diente superior",V(width,-.037f,z),V(.036f,.083f-tooth*.005f,.037f),ivory);
                        point.localRotation=Quaternion.Euler(0,0,side*13);
                        if(adult) Gem(lower,"Diente inferior",V(width*.9f,.026f,z-.013f),V(.032f,.072f-tooth*.006f,.033f),ivory);
                    }
                }
                var tail=G(body,"Idle:Tail",V(0,.258f,.28f));
                Sweep(tail,"Cola larga de caimán",back,Knot(0,0,0,.091f),Knot(.067f,-.068f,.111f,.079f),
                    Knot(.151f,-.121f,.192f,.058f),Knot(.252f,-.135f,.246f,.041f),Knot(.326f,-.105f,.264f,.024f),
                    Knot(.373f,-.059f,.232f,.008f));
                for(int i=0;i<3;i++)
                    Gem(tail,"Engarce de la cola",V(.048f+i*.069f,-.015f-i*.035f,.082f+i*.057f),
                        V(.107f-i*.012f,.086f-i*.008f,.118f),plates).localRotation=Quaternion.Euler(16,32,0);
            }
        }
    }
}

