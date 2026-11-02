using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Two related woodland cultures: Faunar's animal adventurers and village craft,
            // and Zukgrok's broad-eared mushroom folk. Original forms use the shared meshes.
            void ApplyFaunarStyle()
            {
                Color timber=new Color(.44f,.25f,.13f),honey=new Color(.95f,.68f,.26f),mint=new Color(.31f,.68f,.48f),berry=new Color(.56f,.27f,.51f);
                if(card.IsStructure)
                {
                    FaePaint(timber,"Pilar de madera","Mirador","Poste de vigilancia","Larguero de escalera","Rama del arbusto","Poste de huerto");
                    FaePaint(honey,"Peldaño","Barandilla del mirador","Valla de bayas","Aro de canalización","Aro de sello");
                    FaePaint(mint,"Follaje tallado","Teja de hoja","Nenúfar");
                    switch(id)
                    {
                        case "resting-camp":
                            URemove("Tienda de campaña");
                            FaeCottage(model,new Vector3(0,.07f,.075f),.53f,.43f,new Color(.25f,.51f,.53f),timber,honey);
                            break;
                        case "vigilant-tower":
                            URemove("Cubierta de hojas");
                            P(model,"Cubierta de bellota del vigía",m.MushroomCap,new Vector3(0,1.0f,0),new Vector3(.73f,.63f,.67f),new Color(.23f,.48f,.45f));
                            for(int j=0;j<4;j++)
                            {
                                var p=Rad(j,4,.22f,1.055f);Leaf(model,"Teja curvada del mirador",p,new Vector3(.22f,.35f,.06f),j%2==0?mint:new Color(.22f,.51f,.36f),new Vector3(53,j*90,0));
                            }
                            Bone(model,"Rama curva del farol",new Vector3(.22f,.8f,-.19f),new Vector3(.28f,.96f,-.21f),.035f,timber);
                            Bone(model,"Gancho del farol",new Vector3(.28f,.96f,-.21f),new Vector3(.34f,.92f,-.21f),.028f,timber);
                            Ball(model,"Farol de savia",new Vector3(.34f,.86f,-.21f),new Vector3(.095f,.15f,.085f),honey);
                            break;
                        case "berry-bush":
                            FaePaint(new Color(.75f,.22f,.4f),"Baya");
                            Ball(model,"Cesto grande del huerto",new Vector3(.2f,.12f,-.27f),new Vector3(.2f,.2f,.18f),honey);
                            for(int j=0;j<3;j++)Bone(model,"Trenza de cesto",new Vector3(.13f+j*.065f,.055f,-.347f),new Vector3(.13f+j*.065f,.19f,-.347f),.012f,timber);
                            break;
                        case "mana-pool":
                            FaePaint(new Color(.24f,.72f,.75f),"Lente de maná");
                            for(int s=-1;s<=1;s+=2)
                            {
                                Flesh(model,"Raíz del manantial feérico",new Vector3(s*.29f,.06f,.11f),new Vector3(s*.27f,.36f,.1f),.055f,timber);
                                Flesh(model,"Rama del manantial",new Vector3(s*.27f,.31f,.1f),new Vector3(s*.1f,.48f,.11f),.04f,timber);
                                Leaf(model,"Pétalo de maná",new Vector3(s*.17f,.41f,.1f),new Vector3(.2f,.28f,.06f),mint,new Vector3(35,0,-s*35));
                            }
                            break;
                        case "valiant-seal":FaeSealPetals(berry,honey);break;
                        case "courage-seal":FaeSealPetals(mint,honey);break;
                        case "emergency-seal":FaeSealPetals(new Color(.34f,.44f,.7f),honey);break;
                    }
                    return;
                }
                var head=UFind("Idle:Head");if(head==null)return;
                var torso=UFind("Torso animal");if(torso==null)return;
                float h=torso.localPosition.y;
                Color fabric=id=="charming-mongoose"?new Color(.84f,.39f,.18f):id=="ferret-eluding-hunter"?new Color(.29f,.54f,.58f):id=="bober-reinforcer"?new Color(.61f,.35f,.19f):new Color(.15f,.35f,.3f);
                // Tailored collars and petal cloaks follow the animal's chest rather than hiding its species.
                for(int side=-1;side<=1;side+=2)
                    Flesh(model,"Cuello de capa del aventurero",new Vector3(side*.105f,h+.1f,-.17f),new Vector3(side*.1f,h-.04f,-.22f),.055f,fabric);
                Ball(model,"Nudo de pañuelo",new Vector3(0,h+.005f,-.235f),new Vector3(.11f,.1f,.06f),honey);
                Leaf(model,"Caída del pañuelo",new Vector3(.04f,h-.08f,-.25f),new Vector3(.1f,.2f,.035f),fabric,new Vector3(0,0,-20));
                if(id=="panther-stealthy-assassin")
                {
                    for(int side=-1;side<=1;side+=2)Leaf(model,"Capa de pétalo del acechador",new Vector3(side*.135f,h+.13f,.025f),new Vector3(.22f,.47f,.055f),fabric,new Vector3(-72,0,side*14));
                    FaePaint(new Color(.12f,.3f,.28f),"Hombrera de follaje");
                }
                else
                {
                    P(head,"Gorro del caminante",m.MushroomCap,new Vector3(0,.145f,.025f),new Vector3(.32f,.28f,.3f),id=="bober-reinforcer"?new Color(.27f,.55f,.51f):fabric);
                    Leaf(head,"Pluma del caminante",new Vector3(-.11f,.24f,.045f),new Vector3(.08f,.22f,.035f),honey,new Vector3(-10,0,23));
                }
                if(id=="bober-reinforcer")
                {
                    Box(model,"Petaca del carpintero",new Vector3(.205f,.29f,.05f),new Vector3(.095f,.2f,.19f),fabric);
                    Bone(model,"Mango de mazo del carpintero",new Vector3(.21f,.23f,.08f),new Vector3(.21f,.46f,.08f),.03f,timber);
                    Box(model,"Cabeza de mazo del carpintero",new Vector3(.21f,.46f,.08f),new Vector3(.15f,.075f,.08f),honey);
                }
                else if(id=="ferret-eluding-hunter")
                    Leaf(model,"Capa corta del hurón",new Vector3(0,h+.145f,.1f),new Vector3(.34f,.36f,.045f),fabric,new Vector3(-75,0,0));
            }
            void ApplyZukgrokStyle()
            {
                Color fur=new Color(.77f,.65f,.46f),rose=new Color(.63f,.35f,.43f),plum=new Color(.46f,.28f,.49f),teal=new Color(.23f,.48f,.41f),wood=new Color(.43f,.29f,.17f),warm=new Color(.93f,.72f,.35f);
                if(card.IsStructure){ZukVillageStructures(wood,warm,teal,plum);return;}
                var head=UFind("Idle:Head");if(head==null)return;
                var torso=UFind("Torso micelial");var trunk=UFind("Tronco tallado");
                if(torso!=null||trunk!=null)
                {
                    var body=UFind("Idle:Body");bool tree=trunk!=null;
                    Color face=tree?new Color(.48f,.33f,.2f):fur;
                    // A broad furry face and long ears change the silhouette of the mushroom people.
                    FaePaint(face,"Rostro del bosque","Mano","Fibra del antebrazo","Brazo de raíz","Unión del cuello");
                    FaePaint(new Color(.31f,.22f,.19f),"Raíz del pie");
                    if(!tree)
                    {
                        torso.localScale=new Vector3(torso.localScale.x*1.08f,torso.localScale.y,torso.localScale.z*1.08f);
                        FaePaint(id=="chaman-micelial"||id=="rey-micelial"?plum:teal,"Torso micelial","Placa de musgo");
                    }
                    var cap=UFind("Capucha de laminillas");
                    if(cap!=null)FaePaint(id=="chaman-micelial"?plum:id=="matriarca-micelial"?new Color(.69f,.35f,.47f):id=="rey-micelial"?new Color(.38f,.33f,.5f):new Color(.3f,.52f,.43f),"Capucha de laminillas");
                    for(int side=-1;side<=1;side+=2)
                    {
                        Flesh(head,"Oreja larga de los micelianos",new Vector3(side*.1f,.005f,.015f),new Vector3(side*.255f,.115f,.015f),.095f,face);
                        Leaf(head,"Punta de oreja miceliana",new Vector3(side*.245f,.115f,.018f),new Vector3(.115f,.24f,.065f),face,new Vector3(0,0,-side*52));
                        Leaf(head,"Interior rosado de oreja",new Vector3(side*.232f,.105f,-.006f),new Vector3(.065f,.16f,.024f),rose,new Vector3(0,0,-side*52));
                        Ball(head,"Mejilla de pelo suave",new Vector3(side*.1f,-.045f,-.087f),new Vector3(.13f,.12f,.13f),face);
                        if(!tree)Leaf(head,"Mechón de mejilla",new Vector3(side*.15f,-.063f,-.012f),new Vector3(.095f,.16f,.055f),face,new Vector3(0,0,side*55));
                    }
                    FaePaint(new Color(.19f,.25f,.23f),"Órbita");
                    foreach(var eye in head.GetComponentsInChildren<Transform>().Where(t=>t.name=="Pupila"||t.name=="Reflejo ocular"))
                    {var scale=eye.localScale;scale.x*=1.16f;scale.y*=1.7f;eye.localScale=scale;}
                    if(!tree)
                    {
                        // Seat a broad forward face below the cap's brim, so the eyes remain
                        // readable from the elevated board camera rather than hiding in shadow.
                        foreach(var eye in head.GetComponentsInChildren<Transform>().Where(t=>t.name=="Órbita"||t.name=="Pupila"||t.name=="Reflejo ocular").ToArray())BoardMeshFactory.Release(eye.gameObject);
                        Ball(head,"Rostro adelantado miceliano",new Vector3(0,-.017f,-.11f),new Vector3(.245f,.195f,.17f),face);
                        for(int side=-1;side<=1;side+=2)
                        {
                            Ball(head,"Esclerótica miceliana",new Vector3(side*.064f,-.017f,-.189f),new Vector3(.067f,.073f,.022f),new Color(.95f,.9f,.77f));
                            Ball(head,"Iris miceliano",new Vector3(side*.064f,-.017f,-.201f),new Vector3(.041f,.05f,.015f),new Color(.18f,.42f,.37f));
                            Ball(head,"Pupila miceliana",new Vector3(side*.064f,-.017f,-.208f),new Vector3(.017f,.036f,.008f),ink);
                            Ball(head,"Brillo de ojo miceliano",new Vector3(side*.064f-.009f,-.005f,-.213f),Vector3.one*.009f,ivory);
                        }
                    }
                    Ball(head,"Nariz miceliana",new Vector3(0,tree?-.005f:-.04f,tree?-.139f:-.215f),new Vector3(.058f,.043f,.052f),new Color(.29f,.19f,.17f));
                    Bone(head,"Sonrisa miceliana",new Vector3(-.032f,tree?-.073f:-.085f,tree?-.116f:-.18f),new Vector3(.032f,tree?-.073f:-.085f,tree?-.116f:-.18f),.009f,new Color(.35f,.22f,.19f));
                    if(!tree)
                    {
                        float y=torso.localPosition.y;
                        Box(body,"Petaca de tejido miceliano",new Vector3(.12f,y-.025f,-torso.localScale.z*.43f),new Vector3(.13f,.13f,.05f),warm);
                        for(int j=0;j<3;j++)Bone(body,"Costura de chaleco",new Vector3(-.022f,y-.08f+j*.065f,-torso.localScale.z*.47f),new Vector3(.022f,y-.065f+j*.065f,-torso.localScale.z*.47f),.012f,warm);
                    }
                    return;
                }
                if(id=="larva-micelial")
                {
                    FaePaint(new Color(.57f,.69f,.39f),"Segmento de larva");
                    for(int side=-1;side<=1;side+=2)Ball(head,"Mejilla de larva feérica",new Vector3(side*.11f,-.025f,-.075f),new Vector3(.09f,.075f,.07f),rose);
                }
                else if(id=="bestia-micelial"||id=="ciervo-de-esporas")
                {
                    FaePaint(new Color(.4f,.52f,.34f),"Coraza micelial bestial","Musgo del lomo");
                    Ball(model,"Nudo de guarda del bosque",new Vector3(0,id=="ciervo-de-esporas"?.63f:.39f,-.255f),new Vector3(.12f,.13f,.075f),warm);
                }
                else if(id=="frog-token")FaePaint(new Color(.5f,.66f,.37f),"Cabeza ancha","Muslo posterior");
                else if(id.Contains("alligator"))FaePaint(new Color(.31f,.48f,.35f),"Hocico","Lomo escamado","Escama lateral de caimán");
                else if(id=="firefly-token")FaePaint(warm,"Órgano luminiscente");
            }
            void FaePaint(Color color,params string[] names)
            {foreach(var name in names)UColor(name,color);}
            void FaeSealPetals(Color color,Color gold)
            {
                FaePaint(color,"Sello de piedra","Relieve del coraje");
                for(int j=0;j<4;j++)Leaf(model,"Hoja cincelada del sello feérico",Rad(j,4,.2f,.107f),new Vector3(.08f,.14f,.018f),gold,new Vector3(90,j*90,25));
            }
            void FaeCottage(Transform p,Vector3 pos,float width,float height,Color roof,Color wall,Color lamp)
            {
                Stem(p,"Casa del claro",pos,width,height,wall);
                P(p,"Tejado abombado del claro",m.MushroomCap,pos+Vector3.up*(height-.025f),new Vector3(width*1.36f,width*.9f,width*1.36f),roof);
                Ball(p,"Puerta redonda del claro",pos+new Vector3(0,height*.29f,-width*.47f),new Vector3(width*.38f,height*.55f,.035f),new Color(.17f,.24f,.2f));
                Ball(p,"Pomo de bellota",pos+new Vector3(width*.09f,height*.26f,-width*.5f),Vector3.one*.035f,lamp);
                for(int side=-1;side<=1;side+=2)
                {
                    var q=pos+new Vector3(side*width*.38f,height*.65f,-width*.29f);
                    Ball(p,"Ventana cálida del claro",q,new Vector3(width*.21f,height*.24f,.045f),lamp);
                    Bone(p,"Parteluz de ventana",q+new Vector3(0,-height*.1f,-.025f),q+new Vector3(0,height*.1f,-.025f),.012f,wall);
                }
            }
            void ZukVillageStructures(Color wood,Color warm,Color teal,Color plum)
            {
                FaePaint(wood,"Tronco del santuario","Tronco de Árbol Madre","Torre hueca","Bulbo de raíz","Bancal de cultivo","Trenza del borde del nido","Cestería del nido");
                FaePaint(new Color(.29f,.54f,.42f),"Copa maternal","Copa protectora");
                FaePaint(warm,"Resina de raíz","Nexo de la red","Farol de luciérnaga","Semilla madre");
                switch(id)
                {
                    case "torre-de-esporas":
                        FaePaint(plum,"Corona esporífera");
                        FaeRoundDoor(model,new Vector3(0,.2f,-.125f),.17f,.29f,wood,warm);
                        for(int j=0;j<2;j++)
                        {
                            Ball(model,"Ventana de la torre miceliana",new Vector3(.08f,.57f+j*.25f,-.105f),new Vector3(.1f,.13f,.035f),warm);
                            Box(model,"Repisa de ventana miceliana",new Vector3(.07f,.5f+j*.25f,-.13f),new Vector3(.17f,.035f,.1f),teal);
                        }
                        break;
                    case "semillero-micelial":
                        for(int side=-1;side<=1;side+=2)
                        {
                            Flesh(model,"Arco de rama del vivero",new Vector3(side*.28f,.12f,.22f),new Vector3(side*.25f,.51f,.22f),.045f,wood);
                            Flesh(model,"Bóveda de rama del vivero",new Vector3(side*.25f,.48f,.22f),new Vector3(0,.64f,.22f),.04f,wood);
                            Leaf(model,"Dosel del vivero",new Vector3(side*.13f,.57f,.15f),new Vector3(.32f,.42f,.06f),teal,new Vector3(60,0,side*45));
                        }
                        break;
                    case "jardin-micelial":
                        for(int j=0;j<6;j++)
                        {
                            var plank=Box(model,"Borde de huerto comunal",Rad(j,6,.31f,.09f),new Vector3(.3f,.085f,.045f),j%2==0?plum:wood);plank.localRotation=Quaternion.Euler(0,-j*60+90,0);
                        }
                        break;
                    case "santuario-del-bosque":
                        for(int j=0;j<3;j++)Leaf(model,"Teja de pétalo del santuario",new Vector3((j-1)*.17f,.81f,.06f),new Vector3(.3f,.46f,.07f),j==1?plum:teal,new Vector3(73,0,(j-1)*20));
                        break;
                    case "arbol-madre":
                        FaeRoundDoor(model,new Vector3(0,.24f,-.125f),.17f,.3f,wood,warm);
                        Ball(model,"Ventana de la casa madre",new Vector3(.085f,.63f,-.096f),new Vector3(.115f,.15f,.045f),warm);
                        P(model,"Alero de casa madre",m.MushroomCap,new Vector3(0,.4f,-.11f),new Vector3(.3f,.2f,.24f),plum);
                        break;
                    case "bosque-eterno":
                        FaeCottage(model,new Vector3(0,.045f,-.21f),.23f,.22f,plum,wood,warm);
                        break;
                    case "micelium-root":
                        FaeRoundDoor(model,new Vector3(0,.17f,-.18f),.16f,.24f,wood,warm);
                        FaePaint(teal,"Sombrero esculpido");
                        break;
                    case "santuario-del-sueno":
                        FaePaint(plum,"Sombrero esculpido");
                        break;
                    case "nido-de-esporas":
                        FaePaint(new Color(.75f,.53f,.61f),"Saco de espora");
                        FaePaint(warm,"Huevo central");
                        break;
                    case "firefly-nest":
                        FaePaint(teal,"Junco arqueado");
                        FaePaint(warm,"Farol de luciérnaga");break;
                    case "red-micelial":FaePaint(plum,"Nodo hexagonal");break;
                    case "corazon-del-hongo":FaePaint(new Color(.58f,.25f,.43f),"Ventrículo izquierdo","Ventrículo derecho");break;
                }
            }
            void FaeRoundDoor(Transform p,Vector3 pos,float width,float height,Color wood,Color warm)
            {
                Ball(p,"Marco de puerta miceliana",pos,new Vector3(width*1.25f,height*1.1f,.045f),warm);
                Ball(p,"Puerta miceliana",pos+Vector3.back*.015f,new Vector3(width,height,.035f),new Color(.22f,.19f,.22f));
                Ball(p,"Tirador de puerta miceliana",pos+new Vector3(width*.25f,-height*.08f,-.036f),Vector3.one*.03f,wood);
            }
        }
    }
}
