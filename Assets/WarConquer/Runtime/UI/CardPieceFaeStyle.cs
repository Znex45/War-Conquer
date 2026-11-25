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

            bool SculptWoodlandUnit()
            {
                switch (id)
                {
                    case "charming-mongoose": WoodlandDuelist(false); return true;
                    case "ferret-eluding-hunter": WoodlandDuelist(true); return true;
                    case "bober-reinforcer": WoodlandBuilder(); return true;
                    case "panther-stealthy-assassin": WoodlandStalker(); return true;
                    default: return false;
                }
            }

            void WoodlandDuelist(bool ferret)
            {
                Color fur = Pigment(ferret ? 0x966643 : 0xC89463);
                Color cream = Pigment(ferret ? 0xE9CB9A : 0xF6DEAE);
                Color leather = Pigment(ferret ? 0x533A32 : 0x704936);
                Color clothColor = Pigment(ferret ? 0x4A655C : 0x687744);
                Color brass = Pigment(ferret ? 0xAA8D61 : 0xC79B57);
                var body = G(model, "Idle:Body", Vector3.zero);
                var torso = Ball(body, "Jubón entallado", V(0, .457f, .022f), V(ferret ? .29f : .362f, ferret ? .432f : .39f, .275f), clothColor);
                torso.localRotation = Quaternion.Euler(ferret ? -9 : -2, 0, ferret ? 9 : -5);
                Ball(body, "Pechera clara", V(0, .611f, -.083f), V(.195f, .266f, .185f), cream);
                Bone(body, "Correa diagonal", V(-.156f, .612f, -.132f), V(.137f, .366f, -.132f), .052f, leather);
                Bone(body, "Cinturón con hebilla", V(-.155f, .369f, -.112f), V(.149f, .418f, -.112f), .061f, leather);
                Box(body, "Hebilla de latón", V(.025f, .417f, -.158f), V(.078f, .077f, .036f), brass);
                for (int side = -1; side <= 1; side += 2)
                {
                    float pawZ = (ferret ? side : -side) * .075f;
                    CurvedLimb(body, "Pierna del duelista " + side, leather, V(side * .086f, .368f, .018f),
                        V(side * .146f, .215f, pawZ * .65f), V(side * .151f, .089f, pawZ), .071f, .064f, .051f);
                    Ball(body, "Pata adelantada", V(side * .152f, .046f, pawZ - .044f), V(.139f, .091f, .184f), fur);
                    Plate(body, "Faldón de cuero", V(side * .106f, .302f, -.087f), V(.158f, .235f, .083f), clothColor,
                        V(-9, 0, side * 14), "leaf", LeafEdge);
                    Bone(body, "Costura del faldón", V(side*.083f,.339f,-.136f), V(side*.123f,.235f,-.129f),.015f,brass);

                    var arm = G(body, side < 0 ? "Idle:ArmL" : "Idle:ArmR", V(side * .142f, .594f, -.008f));
                    Vector3 hand = side < 0 ? V(-.113f, ferret ? -.144f : -.122f, -.102f) : V(.093f, -.119f, -.149f);
                    CurvedLimb(arm, "Brazo revestido " + side, leather, Vector3.zero, V(side*.083f,-.064f,.003f), hand, .075f, .061f, .058f);
                    Plate(arm, "Hombrera de capas", V(side*.033f,-.017f,-.052f), V(.169f,.142f,.127f),
                        clothColor, V(16, 0, -side * 23), "shield", ShieldEdge);
                    Ball(arm, "Puño con brazal", hand, V(.117f,.111f,.119f), fur);
                    if (side < 0)
                    {
                        var weapon = G(arm, "Espada de guardabosques", hand);
                        weapon.localRotation = Quaternion.Euler(ferret ? -20 : 7, 12, ferret ? 61 : 34);
                        Bone(weapon, "Puño envuelto", V(0,-.065f,0), V(0,.07f,0), .037f, leather);
                        Box(weapon, "Guarda de latón", V(0,.058f,0), V(.141f,.034f,.056f), brass);
                        Plate(weapon, "Hoja forjada", V(.006f,.239f,0), V(ferret?.083f:.097f,.371f,.041f),
                            Pigment(0xCEDAD6), Vector3.zero, "blade", BladeEdge);
                        Bone(weapon, "Nervio de hoja", V(0,.119f,-.024f), V(.031f,.35f,-.024f), .012f, Pigment(0x93AAA4));
                    }
                    else
                    {
                        var shield = G(arm, "Escudo de madera", hand + V(.026f,.02f,-.028f));
                        shield.localRotation = Quaternion.Euler(ferret ? 17 : 8, -17, ferret ? -18 : -7);
                        Vector3 plateSize = V(ferret ? .259f : .305f, ferret ? .376f : .413f, .078f);
                        Plate(shield, "Borde metálico", Vector3.zero, plateSize, brass, Vector3.zero, "shield", ShieldEdge);
                        Plate(shield, "Tablas pintadas", V(0,0,-.044f), Vector3.Scale(plateSize,V(.825f,.865f,.43f)),
                            clothColor, Vector3.zero, "shield", ShieldEdge);
                        Plate(shield, "Hoja de heráldica", V(0,.001f,-.066f), V(.122f,.185f,.03f), brass, V(0,0,-14), "leaf", LeafEdge);
                        Bone(shield, "Nervadura grabada", V(-.038f,-.058f,-.087f), V(.028f,.058f,-.087f), .012f, leather);
                    }
                }

                var head = G(body, "Idle:Head", V(ferret ? .012f : 0, ferret ? .799f : .762f, -.021f));
                head.localRotation = Quaternion.Euler(ferret ? -12 : -7, ferret ? 12 : -8, ferret ? -6 : 4);
                Ball(head, "Cráneo de mustélido", V(0,.018f,.012f), V(ferret?.284f:.346f,ferret?.296f:.29f,.262f), fur);
                Ball(head, "Mejillas claras", V(0,-.058f,-.105f), V(ferret?.195f:.257f,.155f,ferret?.201f:.17f), cream);
                Ball(head, "Puente del hocico", V(0,-.021f,-.136f), V(ferret?.115f:.128f,.121f,ferret?.182f:.14f), fur);
                Ball(head, "Nariz", V(0,-.041f,ferret?-.23f:-.204f), V(.062f,.044f,.038f), leather);
                for (int side = -1; side <= 1; side += 2)
                {
                    Ball(head, "Oreja exterior", V(side * (ferret?.117f:.135f), .124f, .018f), V(.121f,.125f,.096f), fur);
                    Ball(head, "Interior rosado", V(side * (ferret?.117f:.135f), .129f, -.022f), V(.074f,.077f,.04f), Pigment(0xDAA08A));
                    if (ferret)
                        Ball(head, "Máscara de hurón", V(side*.079f,.032f,-.110f), V(.142f,.096f,.098f), leather);
                    else
                        Ball(head, "Mejilla de mangosta", V(side*.103f,-.068f,-.081f), V(.135f,.144f,.161f), cream);
                }
                MiniatureEyes(head, ferret ? .068f : .078f, .042f, -.121f, ferret ? .073f : .086f,
                    Pigment(0x684428), cream, ferret ? 5 : 0);
                var tail = G(body, "Idle:Tail", V(.019f,.233f,.126f));
                Sweep(tail, "Cola curvada del duelista", fur, Knot(0,0,0,.092f),
                    Knot(.027f,-.059f,.14f,.088f), Knot(.107f,-.072f,.225f,.076f),
                    Knot(.225f,-.019f,.244f,.064f), Knot(ferret?.292f:.253f,.099f,.224f,.054f),
                    Knot(ferret?.285f:.232f,ferret?.264f:.173f,.139f,.035f),
                    Knot(ferret?.255f:.197f,ferret?.30f:.191f,.079f,.005f));
                if (ferret)
                    Sweep(tail,"Punta oscura del hurón",leather,Knot(.289f,.17f,.195f,.045f),
                        Knot(.286f,.253f,.145f,.039f),Knot(.255f,.303f,.078f,.007f));
            }

            void WoodlandBuilder()
            {
                Color fur = Pigment(0xAD643D), cream = Pigment(0xEDBC86), leather = Pigment(0x604030);
                Color hardhat = Pigment(0xF4B945), copper = Pigment(0xB78748), steel = Pigment(0x777478);
                var body = G(model, "Idle:Body", Vector3.zero);
                Ball(body, "Torso compacto", V(0,.378f,.025f), V(.469f,.537f,.382f), fur);
                Ball(body, "Peto del constructor", V(0,.383f,-.131f), V(.34f,.324f,.166f), Pigment(0xC08253));
                Bone(body,"Tirante para herramientas",V(-.164f,.58f,-.126f),V(.157f,.264f,-.169f),.063f,leather);
                Bone(body,"Cinturón reforzado",V(-.227f,.263f,-.137f),V(.228f,.263f,-.137f),.075f,leather);
                Box(body,"Hebilla cuadrada",V(.053f,.267f,-.189f),V(.113f,.09f,.053f),hardhat);
                Box(body,"Hueco de la hebilla",V(.054f,.267f,-.22f),V(.058f,.041f,.018f),leather);
                for (int side=-1; side<=1; side+=2)
                {
                    Vector3 boot = V(side*.132f,.059f,-.054f+side*.041f);
                    CurvedLimb(body,"Pierna del castor "+side,fur,V(side*.102f,.243f,.042f),V(side*.154f,.123f,boot.z),boot,.097f,.085f,.081f);
                    Ball(body,"Bota de trabajo",boot,V(.196f,.121f,.222f),leather);
                    Box(body,"Bolsa lateral",V(side*.201f,.245f,.054f),V(.145f,.172f,.177f),leather);
                    Leaf(body,"Solapa de bolsa",V(side*.215f,.281f,-.007f),V(.111f,.105f,.045f),copper,V(0,side*28,0));
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.197f,.463f,.013f));
                    Vector3 hand = side<0 ? V(-.097f,.124f,-.138f) : V(.023f,-.13f,-.149f);
                    CurvedLimb(arm,"Brazo robusto "+side,fur,Vector3.zero,V(side*.079f,side<0?.011f:-.074f,-.07f),hand,.106f,.091f,.082f);
                    Ball(arm,"Puño",hand,V(.144f,.139f,.144f),fur);
                    if (side>=0) continue;
                    var hammer=G(arm,"Mazo de construcción",hand);
                    hammer.localRotation=Quaternion.Euler(14,-9,22);
                    Bone(hammer,"Mango de madera",V(0,-.111f,0),V(0,.234f,0),.058f,leather);
                    P(hammer,"Mazo facetado",m.SculptureBlock,V(0,.237f,0),V(.264f,.148f,.157f),steel);
                    Box(hammer,"Cincha del mazo",V(0,.238f,0),V(.061f,.154f,.166f),Pigment(0x5D595C));
                }
                var head=G(body,"Idle:Head",V(0,.669f,-.045f));
                head.localRotation=Quaternion.Euler(-7,8,4);
                Ball(head,"Cabeza redonda de castor",V(0,.012f,0),V(.366f,.303f,.285f),fur);
                Ball(head,"Carrillos",V(0,-.071f,-.118f),V(.271f,.161f,.183f),cream);
                Ball(head,"Nariz oscura",V(0,-.031f,-.203f),V(.089f,.059f,.051f),leather);
                for (int side=-1; side<=1; side+=2)
                {
                    Ball(head,"Oreja de castor",V(side*.148f,.086f,.012f),V(.11f,.108f,.087f),fur);
                    Box(head,"Incisivo",V(side*.029f,-.129f,-.188f),V(.053f,.087f,.056f),ivory);
                }
                MiniatureEyes(head,.085f,.025f,-.138f,.071f,Pigment(0x694328),cream,3);
                P(head,"Copa del casco",m.MushroomCap,V(0,.103f,.008f),V(.382f,.284f,.345f),hardhat);
                Ball(head,"Visera ancha",V(0,.104f,-.097f),V(.392f,.062f,.261f),hardhat);
                for(int i=-1;i<=1;i++)
                    Bone(head,"Nervio del casco",V(i*.085f,.161f,-.119f),V(i*.074f,.179f,.092f),.020f,Pigment(0xFFD36A));

                var tail=G(body,"Idle:Tail",V(.011f,.161f,.164f));
                var paddle=Ball(tail,"Cola ancha de castor",V(.082f,-.027f,.179f),V(.276f,.088f,.401f),leather);
                paddle.localRotation=Quaternion.Euler(-10,22,-9);
                for(int i=-1;i<=1;i++)
                {
                    Bone(paddle,"Tallado diagonal",V(-.26f,.42f,i*.19f-.17f),V(.24f,.42f,i*.19f+.12f),.038f,copper);
                    Bone(paddle,"Tallado cruzado",V(-.24f,.43f,i*.19f+.11f),V(.23f,.43f,i*.19f-.14f),.038f,copper);
                }
            }

            void WoodlandStalker()
            {
                Color shadow=Pigment(0x27233B), plum=Pigment(0x403658), muzzle=Pigment(0x59506C);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso felino bajo",V(0,.335f,.042f),V(.291f,.272f,.613f),shadow);
                Ball(body,"Pecho en acecho",V(0,.302f,-.181f),V(.291f,.279f,.335f),plum);
                Flesh(body,"Nuca extendida",V(0,.359f,-.16f),V(0,.291f,-.264f),.253f,shadow);
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 front=V(side*.165f,.047f,-.292f+side*.095f);
                    Vector3 back=V(side*.164f,.048f,.285f-side*.041f);
                    CurvedLimb(body,"Pata de acecho "+side,plum,V(side*.096f,.3f,-.194f),V(side*.162f,.158f,-.23f+side*.045f),front,.078f,.061f,.048f);
                    CurvedLimb(body,"Pata posterior "+side,shadow,V(side*.102f,.334f,.183f),V(side*.175f,.171f,.277f),back,.101f,.063f,.046f);
                    Ball(body,"Almohadilla delantera",front+V(0,0,-.04f),V(.135f,.094f,.168f),plum);
                    Ball(body,"Almohadilla posterior",back+V(0,0,-.043f),V(.136f,.095f,.172f),plum);
                }
                var head=G(body,"Idle:Head",V(0,.287f,-.31f));
                head.localRotation=Quaternion.Euler(10,-17,3);
                Ball(head,"Cabeza de pantera",Vector3.zero,V(.28f,.239f,.257f),shadow);
                Ball(head,"Mandíbula felina",V(0,-.064f,-.106f),V(.211f,.121f,.152f),muzzle);
                Ball(head,"Nariz felina",V(0,-.032f,-.175f),V(.094f,.041f,.06f),ink);
                for(int side=-1;side<=1;side+=2)
                {
                    Plate(head,"Oreja redondeada",V(side*.113f,.097f,.024f),V(.102f,.143f,.105f),plum,
                        V(-9,-side*19,-side*26),"leaf",LeafEdge);
                    Ball(head,"Mejilla de pantera",V(side*.101f,-.037f,-.096f),V(.125f,.114f,.119f),plum);
                    Ball(head,"Ceja de acecho",V(side*.068f,.073f,-.125f),V(.106f,.044f,.057f),shadow)
                        .localRotation=Quaternion.Euler(0,-side*19,side*15);
                }
                MiniatureEyes(head,.069f,.038f,-.117f,.068f,Pigment(0xB9AA35),Pigment(0xF8D477),13);
                var tail=G(body,"Idle:Tail",V(0,.346f,.253f));
                Sweep(tail,"Cola felina arqueada",shadow,Knot(0,0,0,.075f),Knot(.029f,-.025f,.139f,.06f),
                    Knot(.137f,.008f,.245f,.05f),Knot(.272f,.052f,.229f,.046f),Knot(.343f,.143f,.157f,.038f),
                    Knot(.287f,.235f,.094f,.032f),Knot(.197f,.247f,.128f,.021f),Knot(.186f,.215f,.174f,.006f));

                // The layered leaf mantle is deliberately readable from above the board.
                for(int row=0;row<4;row++)
                    for(int side=-1;side<=1;side+=2)
                    {
                        var spot=V(side*(.061f+row*.013f),.435f-row*.004f,-.195f+row*.113f);
                        var mantle=Plate(body,"Hoja superpuesta del manto",spot,V(.147f,.232f,.064f),
                            Pigment(row%2==0?0x6E9448:0x4C7544),V(72,side*29,side*36),"leaf",LeafEdge);
                        Bone(mantle,"Nervadura",V(0,-.28f,-.54f),V(0,.34f,-.54f),.047f,Pigment(0xBCC66B));
                    }
            }

            void SculptBurrowRabbit()
            {
                Color wool=Pigment(0xD9CC9C), cream=Pigment(0xEFE6BC), pink=Pigment(0xC28F86);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Lomo agazapado de conejo",V(0,.205f,.08f),V(.279f,.319f,.466f),wool);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(body,"Anca plegada",V(side*.11f,.155f,.16f),V(.178f,.239f,.254f),wool);
                    Ball(body,"Pata plegada",V(side*.13f,.052f,-.023f),V(.117f,.101f,.183f),cream);
                    Ball(body,"Pata delantera",V(side*.062f,.056f,-.195f),V(.103f,.104f,.159f),cream);
                }
                Ball(body,"Rabo de algodón",V(0,.238f,.294f),V(.149f,.138f,.129f),cream);
                var head=G(body,"Idle:Head",V(0,.275f,-.136f));
                head.localRotation=Quaternion.Euler(-8,11,-7);
                Ball(head,"Cabeza de conejo",Vector3.zero,V(.249f,.248f,.23f),wool);
                Ball(head,"Hocico corto",V(0,-.047f,-.096f),V(.194f,.131f,.154f),cream);
                Ball(head,"Nariz rosada",V(0,-.022f,-.171f),V(.052f,.045f,.026f),pink);
                MiniatureEyes(head,.066f,.03f,-.106f,.058f,Pigment(0x544037),cream);
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 ear=V(side*.08f,.235f,.027f+side*.02f);
                    Vector3 lean=V(-13,side*17,-side*14);
                    Plate(head,"Oreja de conejo",ear,V(.095f,.311f+side*.035f,.054f),wool,lean,"leaf",LeafEdge);
                    Plate(head,"Interior de oreja",ear-V(0,.009f,.034f),V(.054f,.225f+side*.025f,.018f),pink,lean,"leaf",LeafEdge);
                }
            }

            bool SculptMycelialFolk()
            {
                switch (id)
                {
                    case "recolector-de-esporas": MycelialWarden(FolkRole.Collector); return true;
                    case "hongo-explorador": MycelialWarden(FolkRole.Scout); return true;
                    case "cazador-de-esporas": MycelialWarden(FolkRole.Hunter); return true;
                    case "chaman-micelial": MycelialWarden(FolkRole.Shaman); return true;
                    case "coloso-de-esporas": MycelialWarden(FolkRole.Colossus); return true;
                    case "matriarca-micelial": MycelialWarden(FolkRole.Matriarch); return true;
                    case "guardian-del-bosque": MycelialTree(false); return true;
                    case "ancestro-micelial": MycelialTree(true); return true;
                    default: return false;
                }
            }

            enum FolkRole { Collector, Scout, Hunter, Shaman, Colossus, Matriarch }

            void MycelialWarden(FolkRole role)
            {
                bool strong=role==FolkRole.Colossus, queen=role==FolkRole.Matriarch, hunter=role==FolkRole.Hunter;
                bool scout=role==FolkRole.Scout, collector=role==FolkRole.Collector, shaman=role==FolkRole.Shaman;
                Color fur=Pigment(strong?0x9C9366:0xC4A77D), face=Pigment(strong?0xB1A47B:0xEDC999), wood=Pigment(0x694D3B);
                Color cap=Pigment(collector?0xA76A50:scout?0x368279:hunter?0x576D40:shaman?0x8051A4:strong?0x406765:0xAF5A80);
                Color clothColor=Pigment(collector?0x637D51:scout?0x416963:hunter?0x5C6448:shaman?0x59406D:strong?0x526B56:0x82455F);
                Color accent=Pigment(collector?0xE4B75A:scout?0x8DD5AF:hunter?0xC5CF75:shaman?0xD6ACEC:strong?0xADD99B:0xE5A8B9);
                float width=strong?.475f:queen?.352f:scout?.275f:.302f;
                float height=strong?1.11f:queen?1.02f:scout?1.055f:1;
                var body=G(model,"Idle:Body",Vector3.zero);
                body.localScale=V(1,height,1);
                float lean=scout?13:hunter?-7:collector?7:0;
                Ball(body,"Cuerpo micelial",V(0,.406f,.012f),V(width,strong?.471f:.399f,strong?.334f:.269f),clothColor)
                    .localRotation=Quaternion.Euler(lean*.36f,0,lean*.31f);
                Ball(body,"Cuello de pelaje",V(0,.564f,-.031f),V(width*.66f,.218f,.21f),fur);
                if(strong)
                {
                    Ball(body,"Pecho de corteza",V(0,.442f,-.119f),V(.394f,.357f,.156f),Pigment(0x445959));
                    for(int i=-1;i<=1;i++)
                        Sweep(body,"Fisura verde del coloso "+i,accent,Knot(i*.102f,.32f,-.183f,.012f),
                            Knot(i*.081f+.025f,.422f,-.206f,.01f),Knot(i*.106f,.522f,-.175f,.015f),
                            Knot(i*.122f+.014f,.574f,-.145f,.006f));
                }
                else
                {
                    Bone(body,"Correa micelial",V(-width*.407f,.491f,-.114f),V(width*.428f,.363f,-.119f),.043f,wood);
                    if(!queen)
                    {
                        Plate(body,"Solapa del cinturón",V(.093f,.382f,-.146f),V(.106f,.123f,.071f),accent,V(8,0,9),"shield",ShieldEdge);
                        Box(body,"Broche de cuero",V(.078f,.403f,-.195f),V(.049f,.051f,.022f),wood);
                    }
                }
                if(queen||scout||shaman)
                {
                    Plate(body,queen?"Manto de guardiana":shaman?"Manto del bosque dormido":"Manto de exploración",
                        V(0,.369f,.118f),V(queen?.442f:shaman?.369f:.313f,.437f,.099f),
                        cap,V(-22,0,scout?17:-8),"cloth",ClothEdge);
                }

                for(int side=-1;side<=1;side+=2)
                {
                    float z=(scout?side:hunter?-side:side)*.051f;
                    var paw=V(side*(strong?.169f:.122f),.067f,z-.059f);
                    CurvedLimb(body,"Raíz de apoyo "+side,wood,V(side*width*.35f,.29f,.011f),
                        V(side*width*.411f,.147f,z-.025f),paw,strong?.092f:.063f,strong?.078f:.050f,.048f);
                    Ball(body,"Pata con fibras",paw+V(0,-.01f,-.023f),V(strong?.19f:.147f,.118f,.168f),wood);
                    Ball(body,"Fibra de la pata",paw+V(0,-.012f,-.073f),V(strong?.145f:.102f,.079f,.098f),fur);
                    if(queen)
                        Ball(body,"Vaina de cría",V(side*.125f,.263f,-.133f),V(.149f,.198f,.15f),accent);

                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*width*.48f,.55f,.005f));
                    float handY=strong?-.202f:shaman&&side<0?-.11f:hunter?-.139f:scout&&side>0?-.26f:-.187f;
                    Vector3 hand=V(side*(strong?.119f:.087f),handY,side<0?-.102f:-.077f);
                    CurvedLimb(arm,"Brazo de fibra "+side,fur,Vector3.zero,V(side*.071f,handY*.45f,.011f),hand,
                        strong?.106f:.065f,strong?.084f:.052f,strong?.082f:.046f);
                    Ball(arm,"Mano de hongo",hand,V(strong?.168f:.102f,strong?.163f:.102f,strong?.152f:.109f),face);
                    if(strong)
                    {
                        P(arm,"Hombrera de hongo leñoso",m.MushroomCap,V(side*.035f,-.01f,.026f),
                            V(.297f,.31f,.299f),Pigment(0x739178));
                        Ball(arm,"Nudillos de micelio",hand+V(0,-.01f,-.056f),V(.129f,.102f,.067f),fur);
                    }
                    else if(hunter)
                        Plate(arm,"Hombrera de hojas",V(side*.029f,-.017f,-.042f),V(.168f,.149f,.108f),cap,
                            V(19,0,side*31),"leaf",LeafEdge);
                    if (collector && side>0) CollectorVial(arm,hand,accent);
                    if (scout && side>0) ScoutLamp(arm,hand,wood,accent);
                    if (hunter && side>0) MycelialBow(arm,hand,wood,accent);
                    if (shaman && side<0) ShamanStaff(arm,hand,wood,accent);
                }

                if(collector) SporeBasket(body,wood,cap,accent);
                if(scout)
                {
                    Bone(body,"Rollo cartográfico",V(-.171f,.504f,.168f),V(.161f,.536f,.171f),.132f,Pigment(0xD9C69A));
                    Bone(body,"Cordón del mapa",V(-.082f,.459f,.161f),V(-.068f,.586f,.176f),.033f,wood);
                }
                if(hunter)
                {
                    Stem(body,"Carcaj de esporas",V(-.174f,.325f,.168f),.119f,.241f,wood)
                        .localRotation=Quaternion.Euler(6,0,-19);
                    for(int i=0;i<3;i++)
                    {
                        Vector3 shaft=V(-.231f+i*.031f,.64f-i*.022f,.191f);
                        Bone(body,"Flecha guardada",shaft-V(-.035f,.161f,.002f),shaft,.014f,Pigment(0xD8BC85));
                        Plate(body,"Pluma del carcaj",shaft,V(.040f,.084f,.019f),accent,V(0,0,-14),"leaf",LeafEdge);
                    }
                }
                if(queen)
                {
                    Ball(body,"Vaina de cría central",V(0,.223f,-.181f),V(.15f,.208f,.139f),Pigment(0xBD798D));
                    Gem(body,"Broche maternal",V(0,.518f,-.152f),V(.079f,.104f,.044f),Pigment(0xE7C89B));
                }

                var head=G(body,"Idle:Head",V(0,strong?.718f:.659f,-.017f));
                head.localRotation=Quaternion.Euler(scout?-11:collector?5:-5,scout?-16:hunter?13:5,shaman?-9:5);
                Ball(head,"Rostro de miceliano",V(0,-.02f,-.064f),V(strong?.296f:.239f,.225f,.236f),fur);
                Ball(head,"Mejillas de pelaje",V(0,-.055f,-.151f),V(strong?.223f:.196f,.149f,.139f),face);
                Ball(head,"Nariz de miceliano",V(0,-.03f,-.222f),V(.057f,.046f,.036f),wood);
                MiniatureEyes(head,strong?.071f:.061f,.007f,-.167f,strong?.070f:.069f,
                    Pigment(shaman?0x9C67BD:0x586C44),Pigment(0xECDFB6),hunter?10:0);
                for(int side=-1;side<=1;side+=2)
                {
                    var place=V(side*(strong?.179f:.145f),.029f,.018f);
                    var tilt=V(-15,-side*14,-side*(scout?59:66));
                    Plate(head,"Oreja puntiaguda",place,V(.095f,.21f,.079f),fur,tilt,"leaf",LeafEdge);
                    Plate(head,"Interior de oreja",place+V(0,0,-.047f),V(.053f,.153f,.026f),
                        Pigment(0xAF766A),tilt,"leaf",LeafEdge);
                }

                float capWidth=strong?.59f:queen?.636f:scout?.373f:hunter?.454f:shaman?.503f:.433f;
                float capHeight=scout?.43f:strong?.364f:queen?.329f:shaman?.575f:.403f;
                Vector3 brim=V(scout?.029f:0,.094f,shaman?.029f:.016f);
                var hood=P(head,"Sombrero con láminas",m.MushroomCap,brim,V(capWidth,capHeight,capWidth*.91f),cap);
                hood.localRotation=Quaternion.Euler(shaman?-10:4,0,scout?-14:hunter?12:queen?-9:6);
                P(head,"Laminillas ambarinas",m.MushroomCap,brim-V(0,.022f,0),V(capWidth*.94f,capHeight*.57f,capWidth*.86f),face);
                for (int i=0;i<(strong?7:5);i++)
                {
                    var p=brim+Rad(i,strong?7:5,capWidth*.27f,capHeight*.199f);
                    Ball(head,"Mota nacarada",p,V(capWidth*.153f,.037f,capWidth*.136f),accent);
                }
                if(scout)
                    Plate(head,"Pluma de explorador",brim+V(.136f,.136f,.032f),V(.091f,.248f,.055f),
                        accent,V(-20,3,-37),"feather",FeatherEdge);
                if(queen)
                    for(int side=-1;side<=1;side+=2)
                        P(head,"Sombrero pequeño de la matriarca",m.MushroomCap,brim+V(side*.244f,.016f,.089f),
                            V(.23f,.265f,.198f),side<0?accent:cap).localRotation=Quaternion.Euler(-12,0,side*26);
            }

            void SporeBasket(Transform body, Color wood, Color cap, Color accent)
            {
                var basket=G(body,"Cesto de recolección",V(-.174f,.336f,.168f));
                basket.localRotation=Quaternion.Euler(-7,-19,-24);
                Ball(basket,"Trenzado del cesto",V(0,-.012f,0),V(.232f,.273f,.221f),wood);
                Ring(basket,"Borde de vara",V(0,.088f,0),.11f,.024f,accent,8);
                for(int i=0;i<7;i++)
                    Bone(basket,"Vara de cestería",Rad(i,7,.083f,-.097f),Rad(i,7,.105f,.085f),.017f,accent);
                Mushroom(basket,V(.022f,.072f,-.032f),.066f,.142f,cap);
                Ball(basket,"Esporas recolectadas",V(-.047f,.107f,.033f),V(.084f,.091f,.086f),accent);
            }

            void CollectorVial(Transform handRoot, Vector3 hand, Color accent)
            {
                Stem(handRoot,"Vial",hand+V(.024f,-.116f,-.073f),.086f,.149f,Pigment(0x755989));
                Stem(handRoot,"Tapón de madera",hand+V(.024f,.031f,-.073f),.068f,.038f,accent);
                Gem(handRoot,"Polen del vial",hand+V(.024f,-.033f,-.12f),V(.069f,.092f,.029f),Pigment(0xF0B8DA));
            }

            void ScoutLamp(Transform arm,Vector3 hand,Color wood,Color accent)
            {
                var handle=G(arm,"Farol de sendero",hand+V(.026f,-.092f,-.035f));
                Sweep(handle,"Asa del farol",wood,Knot(-.043f,-.032f,0,.015f),Knot(-.039f,.05f,0,.014f),
                    Knot(.015f,.071f,0,.014f),Knot(.06f,.024f,0,.012f),Knot(.047f,-.035f,0,.011f));
                Gem(handle,"Cámara luminosa",V(0,-.059f,0),V(.109f,.125f,.097f),accent);
                P(handle,"Caperuza del farol",m.MushroomCap,V(0,-.005f,0),V(.12f,.08f,.12f),wood);
            }

            void MycelialBow(Transform arm,Vector3 hand,Color wood,Color accent)
            {
                var bow=G(arm,"Arco de raíz",hand);
                bow.localRotation=Quaternion.Euler(18,-19,-13);
                Sweep(bow,"Arco tensado",wood,Knot(-.023f,-.229f,.032f,.013f),Knot(.067f,-.157f,.008f,.025f),
                    Knot(.099f,-.002f,-.008f,.036f),Knot(.081f,.155f,.014f,.023f),Knot(.013f,.25f,.04f,.013f));
                Bone(bow,"Cuerda tensa",V(-.023f,-.229f,.032f),V(.013f,.25f,.04f),.012f,Pigment(0xE2C99A));
                Bone(bow,"Flecha dirigida",V(-.002f,-.016f,.115f),V(.079f,-.013f,-.145f),.017f,wood);
                Gem(bow,"Espora de la flecha",V(.08f,-.013f,-.163f),V(.056f,.052f,.09f),accent);
            }

            void ShamanStaff(Transform arm,Vector3 hand,Color wood,Color accent)
            {
                var staff=G(arm,"Bastón de sueños",hand+V(-.04f,-.008f,.039f));
                staff.localRotation=Quaternion.Euler(-10,-19,13);
                Sweep(staff,"Madera torneada del bastón",wood,Knot(0,-.416f,0,.025f),Knot(.012f,-.139f,.009f,.034f),
                    Knot(0,.092f,0,.038f),Knot(-.042f,.273f,.006f,.029f),Knot(0,.414f,0,.022f));
                Ring(staff,"Atrapasueños violeta",V(-.006f,.327f,-.022f),.108f,.030f,accent,8,Quaternion.Euler(90,0,0));
                for(int i=-1;i<=1;i++)
                {
                    Bone(staff,"Hilo de amuleto",V(i*.057f,.269f,-.025f),V(i*.072f,.163f-Mathf.Abs(i)*.024f,-.03f),.013f,wood);
                    Gem(staff,"Gota de sueño",V(i*.072f,.151f-Mathf.Abs(i)*.024f,-.03f),
                        V(.039f,.071f,.031f),accent);
                }
            }

            void MycelialTree(bool ancient)
            {
                Color wood=Pigment(ancient?0x71624D:0x90654B), bark=Pigment(ancient?0x9A8968:0xA77956);
                Color moss=Pigment(ancient?0x668663:0x5D8250), sap=Pigment(ancient?0xBDD5A1:0x8ACC72);
                var body=G(model,"Idle:Body",Vector3.zero);
                float trunk=ancient?.743f:.656f;
                Sweep(body,"Tronco vivo",wood,Knot(0,.263f,.016f,.152f),Knot(.022f,.443f,.013f,ancient?.128f:.166f),
                    Knot(-.023f,.558f,.038f,.148f),Knot(0,trunk,.037f,.115f));
                for(int i=-1;i<=1;i++)
                    Sweep(body,"Veta longitudinal "+i,bark,Knot(i*.086f,.223f,-.099f,.019f),Knot(i*.078f+.024f,.404f,-.124f,.018f),
                        Knot(i*.072f-.011f,.563f,-.092f,.016f),Knot(i*.063f,.691f,-.065f,.009f));
                for(int side=-1;side<=1;side+=2)
                {
                    var foot=V(side*.148f,.071f,side*.054f-.018f);
                    CurvedLimb(body,"Pata radicular "+side,wood,V(side*.07f,.33f,.023f),V(side*.159f,.158f,.03f),foot,.092f,.071f,.061f);
                    for(int i=-1;i<=1;i++)
                        Sweep(body,"Raíz de apoyo "+side+"/"+i,bark,Knot(foot.x,foot.y,foot.z,.045f),
                            Knot(foot.x+i*.044f,.045f,foot.z-.061f,.034f),Knot(foot.x+i*.09f,.021f,foot.z-.144f,.008f));
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.128f,trunk-.13f,.02f));
                    var hand=V(side*(ancient?.213f:.103f),ancient?-.205f:-.262f,-.077f);
                    CurvedLimb(arm,"Rama articulada "+side,wood,Vector3.zero,V(side*.118f,-.085f,.021f),hand,.083f,.071f,.059f);
                    Ball(arm,"Nudo de la mano",hand,V(.136f,.132f,.114f),bark);
                    Plate(arm,"Capa de musgo de hombro",V(side*.041f,.02f,.021f),V(.212f,.256f,.089f),moss,
                        V(55,side*35,side*30),"leaf",LeafEdge);
                    if(!ancient && side<0)
                    {
                        var shield=G(arm,"Escudo de corteza viva",hand+V(-.032f,.126f,-.065f));
                        shield.localRotation=Quaternion.Euler(5,17,-9);
                        Plate(shield,"Tabla de corteza",Vector3.zero,V(.354f,.506f,.127f),bark,Vector3.zero,"shield",ShieldEdge);
                        for(int i=-1;i<=1;i++)
                            Bone(shield,"Hendidura de madera",V(i*.10f,-.172f+Mathf.Abs(i)*.032f,-.074f),
                                V(i*.095f,.19f,-.075f),.021f,wood);
                        Plate(shield,"Hoja protectora",V(0,.019f,-.089f),V(.13f,.299f,.066f),sap,V(0,0,-22),"leaf",LeafEdge);
                    }
                }
                var head=G(body,"Idle:Head",V(0,trunk+.079f,-.029f));
                Ball(head,"Nudo facial",V(0,-.011f,.013f),V(.273f,.281f,.235f),bark);
                Ball(head,"Nariz de madera",V(0,-.019f,-.121f),V(.071f,.101f,.083f),wood);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Hendidura del ojo",V(side*.067f,-.003f,-.107f),V(.082f,.067f,.046f),Pigment(0x423D34));
                    Ball(head,"Ojo de resina",V(side*.067f,-.004f,-.134f),V(.051f,.047f,.034f),sap);
                    Sweep(head,"Rama de la corona "+side,wood,Knot(side*.083f,.088f,.034f,.054f),Knot(side*.187f,.238f,.056f,.046f),
                        Knot(side*(ancient?.276f:.212f),.29f,.102f,.033f),Knot(side*(ancient?.312f:.246f),.421f,.06f,.006f));
                    Plate(head,"Racimo de hojas",V(side*(ancient?.271f:.202f),.31f,.045f),V(.155f,.228f,.103f),moss,
                        V(17,side*24,-side*30),"leaf",LeafEdge);
                    if(ancient)
                    {
                        Sweep(head,"Rama joven "+side,bark,Knot(side*.186f,.228f,.079f,.039f),Knot(side*.196f,.339f,.044f,.029f),
                            Knot(side*.114f,.407f,.004f,.009f));
                        Plate(head,"Hoja antigua",V(side*.165f,.372f,.02f),V(.102f,.173f,.055f),sap,
                            V(23,side*35,side*34),"leaf",LeafEdge);
                    }
                }
                if(ancient)
                    for(int i=-1;i<=1;i++)
                        Plate(head,"Barba de liquen",V(i*.077f,-.153f,-.088f),V(.073f,.277f-Mathf.Abs(i)*.047f,.065f),
                            Pigment(0xB8A883),V(15,0,i*20),"feather",FeatherEdge);
            }
        }
    }
}
