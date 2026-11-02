using System;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Broad silhouettes and functional equipment distinguish names at board distance.
            // Everything remains under the original rig, before its idle poses are captured.
            void EnhanceUnitIdentity()
            {
                G(model,"Identidad · "+card.name,Vector3.zero);
                switch(id)
                {
                    case "recolector-de-esporas":CollectorIdentity();break;
                    case "hongo-explorador":ExplorerIdentity();break;
                    case "larva-micelial":LarvaIdentity();break;
                    case "bestia-micelial":FungalBeastIdentity();break;
                    case "guardian-del-bosque":TreeIdentity(false);break;
                    case "cazador-de-esporas":HunterIdentity();break;
                    case "chaman-micelial":ShamanIdentity();break;
                    case "coloso-de-esporas":ColossusIdentity();break;
                    case "matriarca-micelial":MatriarchIdentity();break;
                    case "ancestro-micelial":TreeIdentity(true);break;
                    case "ciervo-de-esporas":DeerIdentity();break;
                    case "rey-micelial":KingIdentity();break;
                    case "nomada-de-arena":NomadIdentity();break;
                    case "escorpion-solar":ScorpionIdentity();break;
                    case "guardian-del-obelisco":ObeliskGuardIdentity();break;
                    case "sacerdote-solar":PriestIdentity();break;
                    case "bestia-de-las-dunas":DuneBeastIdentity();break;
                    case "avatar-del-sol":SunAvatarIdentity();break;
                    case "guardian-del-oasis":OasisGuardianIdentity();break;
                    case "halconero-solar":FalconerIdentity();break;
                    case "escarabajo-de-cobre":BeetleIdentity();break;
                    case "emperador-de-las-dunas":EmperorIdentity();break;
                    case "titan-de-ceniza":AshIdentity(true);break;
                    case "espora":SporeIdentity(false);break;
                    case "charming-mongoose":MongooseIdentity();break;
                    case "ferret-eluding-hunter":FerretIdentity();break;
                    case "bober-reinforcer":BeaverIdentity();break;
                    case "panther-stealthy-assassin":PantherIdentity();break;
                    case "rabbit-token":RabbitIdentity();break;
                    case "battle-ant-token":AntIdentity();break;
                    case "sandy-ember":AshIdentity(false);break;
                    case "dune-cammel":CamelIdentity();break;
                    case "old-mummy":MummyIdentity();break;
                    case "wandering-beast":WandererIdentity();break;
                    case "baby-phoenix":PhoenixIdentity();break;
                    case "frog-token":FrogIdentity();break;
                    case "mushroom-token":SporeIdentity(true);break;
                    case "firefly-token":FireflyIdentity(false);break;
                    case "lushroom":LushroomIdentity();break;
                    case "alligator-revengeful-bite":AlligatorIdentity(true);break;
                    case "alligator-token":AlligatorIdentity(false);break;
                    case "crystal-minion":CrystalMinionIdentity();break;
                    case "dark-fly":FireflyIdentity(true);break;
                    case "the-eternal-eye":EternalEyeIdentity();break;
                    case "void-hole":VoidHoleIdentity();break;
                    case "sprinting-sprite":SprintIdentity();break;
                }
            }
            Transform UFind(string name,Transform from=null)=>(from??model).GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name==name);
            void URemove(params string[] names)
            {
                foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>names.Contains(t.name)).ToArray())if(t!=null)BoardMeshFactory.Release(t.gameObject);
            }
            void UColor(string name,Color color)
            {foreach(var t in model.GetComponentsInChildren<Transform>().Where(t=>t.name==name)){var r=t.GetComponent<Renderer>();if(r!=null)m.Paint(r,color);}}
            void UBand(Transform p,string name,Vector3 center,float radius,float width,Color color)
                =>Ring(p,name,center,radius,width,color,8);
            void UCurve(Transform p,string name,Vector3[] points,float width,Color color)
            {for(int i=1;i<points.Length;i++)Bone(p,name,points[i-1],points[i],width,color);}
            void UCapSpots(Transform head,Color color,float radius=.19f,float y=.22f)
            {
                for(int j=0;j<5;j++)Ball(head,"Motas grandes del sombrero",Rad(j,5,radius,y),new Vector3(.09f,.027f,.075f),color);
            }
            void CollectorIdentity()
            {
                var body=UFind("Idle:Body");URemove("Cesta de esporas");
                Ball(body,"Cesta lateral del recolector",new Vector3(-.28f,.31f,.07f),new Vector3(.27f,.34f,.28f),trim);
                UBand(body,"Borde de cesta",new Vector3(-.28f,.45f,.07f),.125f,.026f,ivory);
                for(int j=0;j<4;j++)Bone(body,"Tejido de la cesta",new Vector3(-.39f+j*.072f,.17f,-.065f),new Vector3(-.39f+j*.072f,.43f,-.065f),.022f,ivory);
                Mushroom(body,new Vector3(-.28f,.43f,.07f),.055f,.22f,new Color(.61f,.29f,.31f));
                Bone(body,"Correa de cosecha",new Vector3(-.28f,.36f,-.08f),new Vector3(.1f,.57f,-.16f),.035f,skin);
                var arm=UFind("Idle:ArmR");Stem(arm,"Frasco de esporas",new Vector3(.13f,-.29f,-.09f),.13f,.14f,glow);Stem(arm,"Tapón del frasco",new Vector3(.13f,-.15f,-.09f),.15f,.035f,trim);
            }
            void ExplorerIdentity()
            {
                URemove("Arco orgánico","Cuerda de arco");var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                Leaf(body,"Capa de hoja del explorador",new Vector3(0,.37f,.18f),new Vector3(.53f,.56f,.09f),new Color(.16f,.34f,.32f),new Vector3(-12,0,0));
                Bone(body,"Rollo cartográfico",new Vector3(-.15f,.49f,.23f),new Vector3(.15f,.49f,.23f),.14f,ivory);
                Bone(body,"Banda del explorador",new Vector3(-.16f,.52f,-.12f),new Vector3(.16f,.3f,-.16f),.035f,trim);
                var arm=UFind("Idle:ArmR");UCurve(arm,"Asa de farol",new[]{new Vector3(.13f,-.2f,-.06f),new Vector3(.2f,-.25f,-.07f),new Vector3(.2f,-.34f,-.07f)},.018f,trim);
                Ball(arm,"Farol de luciérnaga",new Vector3(.2f,-.39f,-.07f),new Vector3(.15f,.15f,.15f),glow);Stem(arm,"Base del farol",new Vector3(.2f,-.48f,-.07f),.16f,.035f,trim);
                Leaf(head,"Pluma de explorador",new Vector3(.18f,.27f,.02f),new Vector3(.09f,.29f,.04f),ivory,new Vector3(0,0,-35));
            }
            void LarvaIdentity()
            {
                var segments=model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Segmento de larva").ToArray();
                for(int j=0;j<segments.Length;j++){m.Paint(segments[j].GetComponent<Renderer>(),j%2==0?new Color(.48f,.64f,.31f):ivory);if(j%2==0)Mushroom(model,segments[j].localPosition+Vector3.up*.11f,.045f,.15f,cloth);}
                var head=UFind("Idle:Head");Leaf(head,"Mandíbula larval",new Vector3(0,-.05f,-.15f),new Vector3(.11f,.06f,.055f),trim,new Vector3(90,0,0));
            }
            void FungalBeastIdentity()
            {
                Ball(model,"Coraza micelial bestial",new Vector3(0,.49f,.04f),new Vector3(.48f,.29f,.6f),new Color(.19f,.33f,.3f));
                for(int j=0;j<3;j++)Mushroom(model,new Vector3((j-1)*.14f,.56f,.05f),.07f+j%2*.08f,.25f,new Color(.44f,.29f,.48f));
                var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)Gem(head,"Defensa micelial",new Vector3(s*.17f,.06f,-.06f),new Vector3(.1f,.27f,.11f),ivory);
                for(int s=-1;s<=1;s+=2)Bone(model,"Tendón de hiedra",new Vector3(s*.17f,.48f,-.18f),new Vector3(s*.22f,.17f,-.22f),.055f,leaf);
            }
            void HunterIdentity()
            {
                URemove("Arco orgánico","Cuerda de arco");var arm=UFind("Idle:ArmR");
                var ends=new[]{new Vector3(.06f,-.34f,-.1f),new Vector3(.17f,-.2f,-.13f),new Vector3(.19f,.04f,-.13f),new Vector3(.08f,.2f,-.1f)};
                UCurve(arm,"Arco curvado de cazador",ends,.045f,skin);Bone(arm,"Cuerda tensa de cazador",ends[0],ends[3],.012f,ivory);
                Bone(arm,"Flecha de espora",new Vector3(.08f,-.08f,.04f),new Vector3(.08f,-.08f,-.29f),.018f,trim);Gem(arm,"Punta de espora",new Vector3(.08f,-.08f,-.31f),new Vector3(.06f,.045f,.11f),glow);
                var body=UFind("Idle:Body");Stem(body,"Carcaj del cazador",new Vector3(-.22f,.25f,.12f),.15f,.28f,skin);
                for(int j=0;j<3;j++)Bone(body,"Flecha del carcaj",new Vector3(-.26f+j*.035f,.35f,.12f),new Vector3(-.28f+j*.035f,.67f,.13f),.018f,ivory);
                Leaf(body,"Camuflaje de cazador",new Vector3(-.13f,.43f,.15f),new Vector3(.36f,.45f,.08f),cloth,new Vector3(-15,0,10));
            }
            void ShamanIdentity()
            {
                var body=UFind("Idle:Body");var arm=UFind("Idle:ArmL");
                Bone(arm,"Bastón ritual corto",new Vector3(-.12f,-.36f,-.08f),new Vector3(-.12f,.3f,-.08f),.055f,skin);
                Ring(arm,"Atrapasueños micelial",new Vector3(-.12f,.28f,-.08f),.14f,.025f,ivory,8,Quaternion.Euler(90,0,0));
                for(int j=0;j<3;j++){Bone(arm,"Hilo del atrapasueños",new Vector3(-.21f+j*.09f,.18f,-.08f),new Vector3(-.21f+j*.09f,.04f,-.08f),.012f,trim);Gem(arm,"Talismán de sueño",new Vector3(-.21f+j*.09f,.025f,-.08f),new Vector3(.04f,.09f,.04f),glow);}
                for(int j=0;j<5;j++)Ball(body,"Collar chamánico",new Vector3((j-2)*.06f,.53f+Mathf.Abs(j-2)*.018f,-.15f),Vector3.one*.065f,j%2==0?ivory:glow);
                UCapSpots(UFind("Idle:Head"),new Color(.72f,.57f,.93f));
            }
            void ColossusIdentity()
            {
                var body=UFind("Idle:Body");var torso=UFind("Torso micelial");torso.localScale=new Vector3(.62f,.76f,.41f);torso.localPosition=new Vector3(0,.52f,0);
                for(int s=-1;s<=1;s+=2)
                {
                    var arm=UFind(s<0?"Idle:ArmL":"Idle:ArmR");Ball(arm,"Hombrera de coloso",new Vector3(s*.025f,.035f,.02f),new Vector3(.32f,.29f,.38f),stone);
                    Ball(arm,"Puño de coloso",new Vector3(s*.13f,-.22f,-.06f),new Vector3(.23f,.25f,.24f),skin);
                    Mushroom(body,new Vector3(s*.25f,.83f,.05f),.08f,.33f,new Color(.29f,.43f,.44f));
                }
                for(int j=0;j<3;j++)Gem(body,"Fisura luminosa del coloso",new Vector3((j-1)*.1f,.53f,-.21f),new Vector3(.025f,.32f,.025f),glow);
            }
            void MatriarchIdentity()
            {
                var head=UFind("Idle:Head");var cap=UFind("Capucha de laminillas");cap.localScale=new Vector3(.68f,.8f,.62f);
                for(int s=-1;s<=1;s+=2)Mushroom(head,new Vector3(s*.25f,.04f,.04f),.035f,.28f,new Color(.6f,.26f,.44f));
                var body=UFind("Idle:Body");for(int j=0;j<3;j++)Ball(body,"Vaina de cría",new Vector3((j-1)*.15f,.16f,-.13f),new Vector3(.13f,.21f,.14f),j%2==0?new Color(.76f,.55f,.69f):glow);
                Leaf(body,"Manto maternal",new Vector3(0,.32f,.18f),new Vector3(.63f,.49f,.12f),new Color(.46f,.26f,.4f),new Vector3(-10,0,0));
            }
            void TreeIdentity(bool elder)
            {
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                for(int j=0;j<3;j++)Bone(body,elder?"Surco ancestral":"Veta de corteza",new Vector3((j-1)*.1f,.22f,-.17f),new Vector3((j-1)*.09f,elder?.72f:.48f,-.17f),.027f,dark);
                if(elder)
                {
                    for(int s=-1;s<=1;s+=2){Bone(head,"Rama ancestral",new Vector3(s*.15f,.17f,.03f),new Vector3(s*.32f,.4f,.05f),.055f,skin);Leaf(head,"Copa ancestral",new Vector3(s*.32f,.37f,.06f),new Vector3(.26f,.3f,.16f),new Color(.38f,.57f,.3f),new Vector3(0,0,s*25));}
                    for(int j=0;j<4;j++)Leaf(head,"Barba de raíces",new Vector3((j-1.5f)*.055f,-.13f,-.12f),new Vector3(.055f,.29f,.045f),ivory,new Vector3(0,0,(j-1.5f)*7));
                }
                else
                {
                    var arm=UFind("Idle:ArmL");Ball(arm,"Escudo de corteza",new Vector3(-.09f,-.12f,-.13f),new Vector3(.34f,.47f,.12f),skin);
                    for(int j=0;j<3;j++)Bone(arm,"Veta del escudo",new Vector3(-.19f+j*.1f,-.31f,-.2f),new Vector3(-.19f+j*.1f,.07f,-.2f),.025f,dark);
                    Leaf(arm,"Emblema del bosque",new Vector3(-.09f,-.1f,-.22f),new Vector3(.16f,.25f,.04f),leaf,Vector3.zero);
                }
            }
            void DeerIdentity()
            {
                var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)
                {
                    Bone(head,"Asta bifurcada",new Vector3(s*.21f,.32f,.07f),new Vector3(s*.17f,.49f,.02f),.027f,ivory);
                    Bone(head,"Asta exterior",new Vector3(s*.22f,.32f,.07f),new Vector3(s*.35f,.44f,.08f),.027f,ivory);
                    Ball(head,"Espora del asta",new Vector3(s*.35f,.44f,.08f),Vector3.one*.07f,glow);
                }
                for(int j=0;j<5;j++)Ball(model,"Motas del ciervo",new Vector3(j%2==0?-.16f:.16f,.56f,j*.08f-.11f),new Vector3(.04f,.035f,.045f),ivory);
            }
            void KingIdentity()
            {
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                for(int s=-1;s<=1;s+=2){Leaf(body,"Manto real lobulado",new Vector3(s*.22f,.57f,.18f),new Vector3(.27f,.71f,.08f),new Color(.41f,.21f,.43f),new Vector3(-10,0,s*14));Ball(body,"Broche regio de ámbar",new Vector3(s*.17f,.66f,-.14f),Vector3.one*.11f,trim);}
                for(int j=-1;j<=1;j++)Leaf(head,"Cresta del rey",new Vector3(j*.1f,.34f,.03f),new Vector3(.12f,.3f,.05f),trim,new Vector3(0,0,-j*15));
                Gem(body,"Corazón del reino micelial",new Vector3(0,.6f,-.2f),new Vector3(.14f,.22f,.06f),glow);
            }
            void SporeIdentity(bool mushroom)
            {
                var head=UFind("Idle:Head");UColor("Capucha de laminillas",mushroom?new Color(.24f,.55f,.48f):new Color(.5f,.36f,.64f));
                UCapSpots(head,mushroom?ivory:glow,.13f,.245f);
                if(!mushroom)for(int s=-1;s<=1;s+=2)Leaf(head,"Lóbulo de espora",new Vector3(s*.17f,.15f,.02f),new Vector3(.15f,.17f,.11f),new Color(.55f,.42f,.7f),new Vector3(0,0,s*55));
            }
            void LushroomIdentity()
            {
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                UColor("Capucha de laminillas",new Color(.24f,.56f,.38f));
                for(int s=-1;s<=1;s+=2){Leaf(body,"Faldón frondoso",new Vector3(s*.13f,.33f,.1f),new Vector3(.22f,.38f,.13f),leaf,new Vector3(-15,0,s*27));Mushroom(head,new Vector3(s*.18f,.2f,.02f),.06f,.18f,new Color(.66f,.41f,.55f));}
                Gem(head,"Gota de rocío",new Vector3(0,.34f,-.11f),new Vector3(.09f,.14f,.07f),glow);
            }
            void MongooseIdentity()
            {
                var body=UFind("Torso animal");body.localScale=new Vector3(.29f,.36f,.5f);body.localPosition+=new Vector3(0,.025f,-.015f);
                var head=UFind("Idle:Head");UColor("Máscara de pelaje",new Color(.3f,.18f,.1f));
                for(int s=-1;s<=1;s+=2)Leaf(head,"Mejilla de mangosta",new Vector3(s*.11f,-.04f,.02f),new Vector3(.12f,.24f,.13f),ivory,new Vector3(0,0,s*23));
                Bone(model,"Cinta del guardabosques",new Vector3(-.12f,.44f,-.21f),new Vector3(.13f,.39f,-.21f),.035f,cloth);
                Leaf(model,"Broche de hoja",new Vector3(0,.39f,-.23f),new Vector3(.08f,.14f,.03f),trim,Vector3.zero);
            }
            void FerretIdentity()
            {
                var head=UFind("Idle:Head");head.localScale=new Vector3(.9f,.9f,1.07f);
                UColor("Máscara de pelaje",new Color(.19f,.12f,.1f));UColor("Pechera de pelaje",new Color(.85f,.74f,.53f));
                var tail=UFind("Idle:Tail");for(int j=0;j<4;j++)
                {var band=Stem(tail,"Anillo de cola del hurón",new Vector3(.03f,.025f,.06f+j*.05f),.096f,.026f,j%2==0?ivory:dark);band.localRotation=Quaternion.Euler(90,0,0);}
                Leaf(model,"Capucha ligera del escurridizo",new Vector3(0,.4f,.16f),new Vector3(.36f,.22f,.05f),cloth,new Vector3(-70,0,0));
            }
            void BeaverIdentity()
            {
                var head=UFind("Idle:Head");head.localPosition+=Vector3.down*.035f;
                var tail=UFind("Idle:Tail");var paddle=UFind("Cola",tail);paddle.localScale=new Vector3(.31f,.065f,.39f);paddle.localPosition=new Vector3(0,.015f,.15f);
                Bone(model,"Tronco del reforzador",new Vector3(-.22f,.2f,-.36f),new Vector3(.22f,.2f,-.36f),.17f,skin);
                for(int s=-1;s<=1;s+=2)
                {
                    var end=P(model,"Anillo de tronco",m.Cylinder,new Vector3(s*.225f,.2f,-.36f),new Vector3(.14f,.014f,.14f),ivory);end.localRotation=Quaternion.Euler(0,0,s*90);
                    Bone(model,"Brazo del constructor",new Vector3(s*.14f,.34f,-.1f),new Vector3(s*.16f,.25f,-.34f),.075f,skin);
                }
                Bone(model,"Correa de herramientas",new Vector3(-.17f,.4f,-.12f),new Vector3(.18f,.25f,-.17f),.04f,cloth);
            }
            void PantherIdentity()
            {
                UFind("Idle:Head").localPosition+=Vector3.down*.055f;
                UColor("Hocico",new Color(.33f,.37f,.43f));UColor("Cabeza animal",new Color(.075f,.1f,.16f));
                var torso=UFind("Torso animal");torso.localScale=new Vector3(.37f,.28f,.67f);
                for(int s=-1;s<=1;s+=2)
                {
                    Ball(model,"Hombro felino",new Vector3(s*.15f,.36f,-.15f),new Vector3(.18f,.27f,.22f),new Color(.09f,.13f,.18f));
                    for(int j=0;j<3;j++)Gem(model,"Garra de asesino",new Vector3(s*.16f+(j-1)*.025f,.065f,-.315f),new Vector3(.025f,.035f,.095f),ivory);
                }
                var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)Bone(head,"Ceja de depredador",new Vector3(s*.045f,.095f,-.13f),new Vector3(s*.12f,.07f,-.12f),.025f,dark);
            }
            void RabbitIdentity()
            {
                var head=UFind("Idle:Head");
                for(int s=-1;s<=1;s+=2)Leaf(head,"Interior de oreja",new Vector3(s*.105f,.2f,-.035f),new Vector3(.055f,.29f,.028f),new Color(.7f,.4f,.44f),new Vector3(-8,0,-s*15));
                Gem(model,"Zanahoria del conejo",new Vector3(.19f,.16f,-.28f),new Vector3(.1f,.22f,.1f),new Color(.86f,.36f,.1f));
                Leaf(model,"Hojas de zanahoria",new Vector3(.19f,.29f,-.28f),new Vector3(.11f,.16f,.06f),leaf,new Vector3(0,0,-20));
            }
            void CamelIdentity()
            {
                for(int s=-1;s<=1;s+=2)
                {
                    Box(model,"Alforja de caravana",new Vector3(s*.235f,.53f,.05f),new Vector3(.15f,.24f,.3f),cloth);
                    Bone(model,"Correa de alforja",new Vector3(s*.25f,.68f,-.09f),new Vector3(s*.25f,.4f,-.09f),.025f,trim);
                    var bottle=Stem(model,"Odre de la caravana",new Vector3(s*.25f,.42f,.22f),.1f,.16f,trim);Ball(model,"Tapón del odre",bottle.localPosition+Vector3.up*.18f,Vector3.one*.055f,dark);
                }
                var head=UFind("Idle:Head");Bone(head,"Cabezada del camello",new Vector3(-.095f,-.025f,-.19f),new Vector3(.095f,-.025f,-.19f),.026f,cloth);
                Bone(model,"Rienda de caravana",new Vector3(.1f,.9f,-.42f),new Vector3(.18f,.62f,-.06f),.016f,trim);
            }
            void DuneBeastIdentity()
            {
                var head=UFind("Idle:Head");
                Gem(head,"Cuerno de bestia de dunas",new Vector3(0,.07f,-.17f),new Vector3(.15f,.32f,.13f),ivory).localRotation=Quaternion.Euler(-30,0,0);
                for(int s=-1;s<=1;s+=2)Ball(model,"Placa de hombro de arena",new Vector3(s*.22f,.47f,-.13f),new Vector3(.2f,.33f,.32f),stone);
                for(int j=0;j<4;j++)Leaf(model,"Cresta de dunas",new Vector3(0,.62f,j*.11f-.11f),new Vector3(.09f,.2f,.13f),trim,new Vector3(-25,0,0));
            }
            void WandererIdentity()
            {
                var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)
                {
                    UCurve(head,"Cuerno erosionado",new[]{new Vector3(s*.13f,.1f,.03f),new Vector3(s*.23f,.19f,.07f),new Vector3(s*.25f,.28f,.01f)},.065f,stone);
                    Leaf(head,"Melena errante",new Vector3(s*.15f,-.03f,.06f),new Vector3(.18f,.32f,.14f),dark,new Vector3(0,0,s*20));
                }
                Bone(model,"Fardo del errante",new Vector3(-.21f,.61f,.12f),new Vector3(.21f,.61f,.12f),.16f,cloth);
                Bone(model,"Atadura del fardo",new Vector3(0,.7f,.12f),new Vector3(0,.48f,.21f),.035f,trim);
            }
            void FrogIdentity()
            {
                for(int j=0;j<5;j++)Ball(model,"Mancha del sapo",new Vector3((j%2==0?-1:1)*.12f,.36f,.03f+j*.025f),new Vector3(.075f,.025f,.065f),dark);
                var head=UFind("Idle:Head");Bone(head,"Sonrisa anfibia",new Vector3(-.14f,-.045f,-.13f),new Vector3(.14f,-.045f,-.13f),.018f,dark);
                Leaf(model,"Hoja del estanque",new Vector3(0,.4f,.1f),new Vector3(.2f,.22f,.035f),leaf,new Vector3(70,0,0));
            }
            void AlligatorIdentity(bool adult)
            {
                var head=UFind("Idle:Head");
                if(adult)
                {
                    var snout=UFind("Hocico");snout.localScale=new Vector3(.32f,.16f,.4f);
                    for(int s=-1;s<=1;s+=2){Gem(head,"Colmillo vengativo",new Vector3(s*.11f,.015f,-.21f),new Vector3(.04f,.12f,.04f),ivory);Ball(model,"Escudo de escama",new Vector3(s*.12f,.33f,.06f),new Vector3(.13f,.14f,.31f),dark);}
                    Bone(head,"Cicatriz del vengador",new Vector3(-.1f,.13f,-.16f),new Vector3(-.04f,.12f,-.22f),.018f,ivory);
                }
                else{UColor("Cresta dorsal",leaf);for(int j=0;j<3;j++)Ball(model,"Motas de caimán joven",new Vector3(.14f,.28f,j*.12f-.08f),Vector3.one*.045f,trim);}
            }
            void ScorpionIdentity()
            {
                // Eight walking legs, in addition to its two pedipalps.
                URemove("Fémur de insecto","Tibia de insecto","Apertura de pinza");
                for(int s=-1;s<=1;s+=2)for(int j=0;j<4;j++)
                {
                    float z=-.17f+j*.1f;var knee=new Vector3(s*(.24f+j%2*.02f),.16f,z);
                    Bone(model,"Fémur de escorpión",new Vector3(s*.1f,.2f,z),knee,.035f,trim);Bone(model,"Pata de escorpión",knee,new Vector3(s*.36f,.025f,z+.02f),.027f,skin);
                }
                for(int s=-1;s<=1;s+=2)
                {
                    Gem(model,"Dedo fijo de pinza",new Vector3(s*.26f,.2f,-.48f),new Vector3(.07f,.08f,.18f),trim).localRotation=Quaternion.Euler(0,s*18,0);
                    Gem(model,"Dedo móvil de pinza",new Vector3(s*.35f,.2f,-.46f),new Vector3(.07f,.07f,.16f),ivory).localRotation=Quaternion.Euler(0,-s*24,0);
                }
                var tail=UFind("Idle:Tail");Gem(tail,"Gota de veneno solar",new Vector3(0,.47f,.005f),new Vector3(.06f,.08f,.06f),new Color(.95f,.61f,.16f));
            }
            void AntIdentity()
            {
                var abdomen=UFind("Abdomen segmentado");abdomen.localPosition=new Vector3(0,.24f,.2f);abdomen.localScale=new Vector3(.3f,.31f,.35f);
                var thorax=UFind("Tórax");thorax.localScale=new Vector3(.17f,.21f,.19f);thorax.localPosition=new Vector3(0,.24f,-.07f);
                Ball(model,"Pecíolo de hormiga",new Vector3(0,.23f,.03f),new Vector3(.11f,.14f,.13f),trim);
                var head=UFind("Idle:Head");head.localScale=Vector3.one*1.12f;
                for(int s=-1;s<=1;s+=2){Gem(head,"Mandíbula de hormiga",new Vector3(s*.085f,-.045f,-.12f),new Vector3(.07f,.07f,.16f),ivory).localRotation=Quaternion.Euler(0,-s*25,0);}
                UBand(model,"Banda abdominal de hormiga",new Vector3(0,.25f,.2f),.15f,.025f,trim);
            }
            void BeetleIdentity()
            {
                URemove("Sutura del caparazón","Engarce del caparazón");
                var abdomen=UFind("Abdomen segmentado");abdomen.localScale=new Vector3(.48f,.35f,.47f);abdomen.localPosition=new Vector3(0,.26f,.1f);
                for(int s=-1;s<=1;s+=2)Ball(model,"Élitro de cobre",new Vector3(s*.105f,.35f,.1f),new Vector3(.21f,.2f,.38f),s<0?new Color(.8f,.49f,.2f):new Color(.61f,.31f,.11f));
                Bone(model,"Sutura de élitros",new Vector3(0,.45f,-.055f),new Vector3(0,.45f,.27f),.022f,trim);
                var head=UFind("Idle:Head");Gem(head,"Cuerno de escarabajo",new Vector3(0,.17f,-.025f),new Vector3(.1f,.3f,.1f),trim).localRotation=Quaternion.Euler(-24,0,0);
                for(int s=-1;s<=1;s+=2)Bone(head,"Mandíbula del escarabajo",new Vector3(s*.08f,-.035f,-.1f),new Vector3(s*.11f,-.01f,-.21f),.035f,ivory);
            }
            void FireflyIdentity(bool abyssal)
            {
                var abdomen=UFind("Abdomen segmentado");abdomen.localScale=new Vector3(.3f,.26f,.45f);
                if(abyssal)
                {
                    UColor("Cabeza de insecto",dark);UColor("Abdomen segmentado",new Color(.14f,.09f,.25f));
                    for(int s=-1;s<=1;s+=2)Gem(model,"Placa de quitina abisal",new Vector3(s*.11f,.61f,.13f),new Vector3(.18f,.2f,.32f),cloth);
                    var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)Gem(head,"Mandíbula de mosca oscura",new Vector3(s*.09f,-.065f,-.1f),new Vector3(.07f,.1f,.15f),glow).localRotation=Quaternion.Euler(0,-s*25,0);
                }
                else
                {
                    UColor("Órgano luminiscente",new Color(.83f,.94f,.29f));
                    for(int j=0;j<3;j++)Ball(model,"Segmento de luciérnaga",new Vector3(0,.5f,.14f+j*.06f),new Vector3(.24f-j*.025f,.18f,.095f),j%2==0?trim:glow);
                }
            }
            void PhoenixIdentity()
            {
                var head=UFind("Idle:Head");
                for(int j=-1;j<=1;j++)Leaf(head,"Cresta de fénix",new Vector3(j*.055f,.12f,.035f),new Vector3(.08f,.25f,.04f),j==0?ivory:trim,new Vector3(-10,0,-j*20));
                var tail=UFind("Idle:Tail");for(int j=-1;j<=1;j++)Leaf(tail,"Llama caudal del fénix",new Vector3(j*.07f,-.1f,.2f),new Vector3(.07f,.46f,.04f),j==0?new Color(1,.72f,.21f):new Color(.96f,.4f,.09f),new Vector3(-60,0,j*14));
                for(int s=-1;s<=1;s+=2)
                {
                    var wing=UFind(s<0?"Idle:WingL":"Idle:WingR");
                    Leaf(wing,"Remera dorada del fénix",new Vector3(s*.3f,-.06f,.13f),new Vector3(.09f,.34f,.04f),new Color(1,.72f,.22f),new Vector3(15,0,s*62));
                }
            }
            void UCurvedSabre(Transform arm,float scale=1)
            {
                foreach(var t in arm.GetComponentsInChildren<Transform>().Where(t=>t.name=="Hoja de khopesh").ToArray())if(t!=null)BoardMeshFactory.Release(t.gameObject);
                var curve=new[]{new Vector3(.08f,-.1f,-.08f),new Vector3(.1f,.015f,-.08f),new Vector3(.2f,.1f,-.08f),new Vector3(.23f,.2f,-.08f),new Vector3(.19f,.25f,-.08f)};
                for(int j=0;j<curve.Length;j++)curve[j]=new Vector3(.08f,-.1f,-.08f)+(curve[j]-new Vector3(.08f,-.1f,-.08f))*scale;
                UCurve(arm,"Filo curvo del khopesh",curve,.045f,ivory);Gem(arm,"Punta de khopesh",curve[curve.Length-1],new Vector3(.045f,.09f,.03f),trim).localRotation=Quaternion.Euler(0,0,24);
            }
            void NomadIdentity()
            {
                URemove("Hombrera","Pectoral de bronce","Casquete");var head=UFind("Idle:Head");var body=UFind("Idle:Body");
                Ball(head,"Turbante del nómada",new Vector3(0,.13f,.015f),new Vector3(.32f,.2f,.3f),cloth);
                Stem(head,"Vuelta del turbante",new Vector3(0,.12f,0),.31f,.045f,ivory);
                Box(head,"Velo contra la arena",new Vector3(0,-.07f,-.13f),new Vector3(.24f,.13f,.035f),cloth);
                for(int s=-1;s<=1;s+=2)Leaf(head,"Caída del turbante",new Vector3(s*.11f,-.08f,.08f),new Vector3(.13f,.38f,.05f),cloth,new Vector3(-8,0,s*13));
                Bone(body,"Bandolera de nómada",new Vector3(-.18f,.8f,-.11f),new Vector3(.18f,.52f,-.16f),.06f,skin);
                Ball(body,"Bolsa de travesía",new Vector3(-.23f,.44f,.06f),new Vector3(.18f,.22f,.16f),trim);
                UCurvedSabre(UFind("Idle:ArmR"));
            }
            void ObeliskGuardIdentity()
            {
                URemove("Escudo tallado","Insignia del escudo","Hoja de khopesh","Empuñadura","Guarda");
                var arm=UFind("Idle:ArmL");Box(arm,"Escudo torre del obelisco",new Vector3(-.09f,-.12f,-.15f),new Vector3(.34f,.57f,.09f),stone);
                Box(arm,"Borde del escudo torre",new Vector3(-.09f,-.12f,-.205f),new Vector3(.3f,.53f,.025f),trim);
                Box(arm,"Campo del escudo torre",new Vector3(-.09f,-.12f,-.225f),new Vector3(.25f,.46f,.025f),cloth);
                for(int j=0;j<3;j++)Gem(arm,"Glifo del guardián",new Vector3(-.09f,-.25f+j*.13f,-.245f),new Vector3(.07f,.08f,.025f),glow);
                var right=UFind("Idle:ArmR");Bone(right,"Asta de guardián",new Vector3(.1f,-.49f,-.09f),new Vector3(.1f,.53f,-.09f),.035f,dark);
                Gem(right,"Lanza de obelisco",new Vector3(.1f,.63f,-.09f),new Vector3(.12f,.27f,.05f),ivory);
                Bone(right,"Travesaño de lanza",new Vector3(.025f,.5f,-.09f),new Vector3(.175f,.5f,-.09f),.035f,trim);
            }
            void PriestIdentity()
            {
                URemove("Hombrera");UColor("Túnica modelada",ivory);var body=UFind("Idle:Body");
                for(int s=-1;s<=1;s+=2)Leaf(body,"Estola solar del sacerdote",new Vector3(s*.1f,.61f,-.2f),new Vector3(.09f,.53f,.035f),cloth,Vector3.zero);
                var arm=UFind("Idle:ArmL");Bone(arm,"Pergamino sagrado",new Vector3(-.23f,-.2f,-.08f),new Vector3(.025f,-.2f,-.08f),.12f,ivory);
                for(int j=0;j<3;j++)Box(arm,"Escritura del pergamino",new Vector3(-.16f+j*.055f,-.15f,-.11f),new Vector3(.025f,.015f,.04f),trim);
                var head=UFind("Idle:Head");Stem(head,"Banda sacerdotal",new Vector3(0,.12f,0),.32f,.05f,trim);Gem(head,"Piedra del rito solar",new Vector3(0,.14f,-.16f),new Vector3(.08f,.11f,.04f),glow);
            }
            void SunAvatarIdentity()
            {
                URemove("Casquete","Hombrera","Bota");UColor("Rostro",new Color(.98f,.8f,.37f));UColor("Brazo",trim);UColor("Pierna",trim);UColor("Túnica modelada",ivory);
                var body=UFind("Idle:Body");var center=new Vector3(0,.96f,.14f);
                for(int j=0;j<12;j++)
                {
                    float a=j*Mathf.PI*2/12;var radial=new Vector3(Mathf.Sin(a),Mathf.Cos(a),0);
                    var ray=Leaf(body,"Rayo del avatar solar",center+radial*.385f,new Vector3(.065f,j%2==0?.18f:.13f,.045f),j%2==0?trim:ivory,new Vector3(0,0,-a*Mathf.Rad2Deg));
                }
                for(int s=-1;s<=1;s+=2)Leaf(body,"Cola de luz del avatar",new Vector3(s*.11f,.21f,.08f),new Vector3(.13f,.38f,.05f),trim,new Vector3(-15,0,s*15));
            }
            void FalconerIdentity()
            {
                var left=UFind("Idle:ArmL");var hawk=G(left,"Halcón compañero",new Vector3(-.08f,-.18f,-.08f));
                for(int s=-1;s<=1;s+=2)Bone(hawk,"Garra del halcón",new Vector3(s*.025f,.005f,0),new Vector3(s*.025f,-.075f,-.035f),.017f,trim);
                Ball(hawk,"Torso del halcón",new Vector3(0,.07f,0),new Vector3(.14f,.2f,.17f),new Color(.47f,.31f,.19f));
                Ball(hawk,"Cabeza del halcón",new Vector3(0,.18f,-.045f),Vector3.one*.115f,ivory);
                Gem(hawk,"Pico ganchudo del halcón",new Vector3(0,.15f,-.12f),new Vector3(.045f,.065f,.095f),trim);
                for(int s=-1;s<=1;s+=2)
                {
                    Ball(hawk,"Ojo del halcón",new Vector3(s*.037f,.19f,-.09f),Vector3.one*.022f,ink);
                    var wing=UFind(s<0?"Idle:WingL":"Idle:WingR");
                    foreach(Transform child in wing.Cast<Transform>().ToArray())if(child!=null)BoardMeshFactory.Release(child.gameObject);
                    wing.SetParent(hawk,false);wing.localPosition=new Vector3(s*.045f,.08f,0);wing.localRotation=Quaternion.identity;
                    for(int j=0;j<3;j++)Leaf(wing,"Pluma del halcón compañero",new Vector3(s*(.04f+j*.035f),-.02f*j,.015f*j),new Vector3(.055f,.18f,.025f),j%2==0?ivory:cloth,new Vector3(5,0,s*65));
                }
                Leaf(hawk,"Cola del halcón",new Vector3(0,-.015f,.1f),new Vector3(.08f,.19f,.025f),cloth,new Vector3(-60,0,0));
                UCurvedSabre(UFind("Idle:ArmR"),.73f);
                var body=UFind("Idle:Body");Leaf(body,"Capa de cetrero",new Vector3(0,.55f,.17f),new Vector3(.45f,.62f,.05f),new Color(.29f,.36f,.39f),new Vector3(-12,0,0));
            }
            void EmperorIdentity()
            {
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");UColor("Manto ceremonial",new Color(.55f,.17f,.18f));
                for(int s=-1;s<=1;s+=2)Leaf(body,"Manto imperial amplio",new Vector3(s*.18f,.6f,.15f),new Vector3(.31f,.85f,.06f),new Color(.55f,.17f,.18f),new Vector3(-13,0,s*16));
                for(int j=-1;j<=1;j++)Leaf(head,"Cresta imperial",new Vector3(j*.1f,.27f,.04f),new Vector3(.12f,.33f,.055f),trim,new Vector3(0,0,-j*20));
                URemove("Hoja de khopesh","Empuñadura","Guarda");var arm=UFind("Idle:ArmR");Bone(arm,"Asta del estandarte imperial",new Vector3(.1f,-.46f,0),new Vector3(.1f,.57f,0),.035f,trim);
                Gem(arm,"Remate del estandarte",new Vector3(.1f,.63f,0),new Vector3(.09f,.17f,.06f),glow);
                Box(arm,"Estandarte del emperador",new Vector3(.21f,.37f,.02f),new Vector3(.22f,.3f,.035f),new Color(.55f,.17f,.18f));
                Gem(arm,"Sol heráldico",new Vector3(.21f,.38f,-.01f),new Vector3(.1f,.15f,.025f),trim);
            }
            void AshIdentity(bool titan)
            {
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                if(titan)
                {
                    for(int j=-1;j<=1;j++)Leaf(body,"Llama de chimenea del titán",new Vector3(j*.14f,1.18f,.14f),new Vector3(.11f,.27f,.06f),j==0?ivory:new Color(1,.48f,.08f),new Vector3(0,0,j*10));
                    for(int s=-1;s<=1;s+=2)Ball(body,"Contrafuerte del titán",new Vector3(s*.24f,.44f,.07f),new Vector3(.18f,.45f,.27f),stone);
                    Box(body,"Placa solar quebrada",new Vector3(0,.56f,-.19f),new Vector3(.34f,.08f,.04f),trim);
                }
                else
                {
                    for(int j=-1;j<=1;j++)Leaf(head,"Llama de brasa viva",new Vector3(j*.065f,.23f,0),new Vector3(.09f,.31f,.065f),j==0?ivory:new Color(1,.49f,.12f),new Vector3(0,0,-j*15));
                    for(int s=-1;s<=1;s+=2)Ball(body,"Costra de arena fundida",new Vector3(s*.15f,.45f,-.1f),new Vector3(.17f,.24f,.16f),new Color(.61f,.39f,.19f));
                }
            }
            void OasisGuardianIdentity()
            {
                for(int s=-1;s<=1;s+=2)Gem(model,"Placa turquesa del oasis",new Vector3(s*.16f,.48f,.03f),new Vector3(.1f,.15f,.12f),glow);
                Stem(model,"Palmera del caparazón",new Vector3(0,.48f,.16f),.045f,.19f,skin);
                for(int j=0;j<4;j++)Leaf(model,"Fronda del oasis",Rad(j,4,.09f,.68f)+new Vector3(0,0,.16f),new Vector3(.08f,.23f,.045f),leaf,new Vector3(50,j*90,0));
                var head=UFind("Idle:Head");for(int s=-1;s<=1;s+=2)Gem(head,"Escama guardiana",new Vector3(s*.1f,.1f,.015f),new Vector3(.07f,.14f,.09f),trim);
            }
            void MummyIdentity()
            {
                var skirt=UFind("Túnica modelada");skirt.localScale=new Vector3(.43f,.25f,.36f);skirt.localPosition=new Vector3(0,.31f,0);
                var left=UFind("Idle:ArmL");left.localRotation=Quaternion.Euler(-15,0,-18);
                var right=UFind("Idle:ArmR");right.localRotation=Quaternion.Euler(-12,0,12);
                for(int s=-1;s<=1;s+=2)
                {
                    Leaf(model,"Venda suelta de momia",new Vector3(s*.18f,.61f,-.1f),new Vector3(.06f,.38f,.025f),ivory,new Vector3(-12,0,s*30));
                    Bone(model,"Venda cruzada del pecho",new Vector3(s*.13f,.77f,-.14f),new Vector3(-s*.1f,.58f,-.14f),.026f,stone);
                }
                Gem(model,"Amuleto funerario",new Vector3(0,.55f,-.19f),new Vector3(.1f,.16f,.04f),glow);
            }
            void CrystalMinionIdentity()
            {
                var body=UFind("Idle:Body");
                for(int s=-1;s<=1;s+=2)
                {
                    Gem(body,"Corteza de geoda",new Vector3(s*.15f,.02f,.035f),new Vector3(.22f,.42f,.31f),stone).localRotation=Quaternion.Euler(0,0,-s*18);
                    var arm=G(body,s<0?"Idle:ArmL":"Idle:ArmR",new Vector3(s*.22f,.02f,-.035f));
                    Bone(arm,"Brazo del esbirro cristalino",Vector3.zero,new Vector3(s*.1f,-.15f,-.09f),.1f,trim);
                    Gem(arm,"Garra de geoda",new Vector3(s*.11f,-.19f,-.11f),new Vector3(.14f,.18f,.15f),glow);
                }
                Gem(body,"Fractura del núcleo",new Vector3(0,.015f,-.16f),new Vector3(.09f,.32f,.025f),ivory);
            }
            void EternalEyeIdentity()
            {
                var head=UFind("Idle:Head");
                UCurve(head,"Párpado superior abisal",new[]{new Vector3(-.22f,0,-.17f),new Vector3(-.14f,.14f,-.17f),new Vector3(0,.2f,-.17f),new Vector3(.14f,.14f,-.17f),new Vector3(.22f,0,-.17f)},.04f,cloth);
                UCurve(head,"Párpado inferior abisal",new[]{new Vector3(-.22f,0,-.17f),new Vector3(-.12f,-.15f,-.17f),new Vector3(.12f,-.15f,-.17f),new Vector3(.22f,0,-.17f)},.035f,stone);
                for(int j=-1;j<=1;j++)Gem(head,"Pestaña mineral del ojo eterno",new Vector3(j*.13f,.29f-Mathf.Abs(j)*.045f,-.1f),new Vector3(.05f,.14f,.05f),trim).localRotation=Quaternion.Euler(0,0,-j*25);
                for(int s=-1;s<=1;s+=2)UCurve(head,"Zarcillo del ojo",new[]{new Vector3(s*.16f,-.16f,.06f),new Vector3(s*.24f,-.34f,.06f),new Vector3(s*.14f,-.46f,.12f)},.035f,cloth);
            }
            void VoidHoleIdentity()
            {
                for(int j=0;j<6;j++)
                {
                    var p=Rad(j,6,.29f,.055f);var shard=Gem(model,"Borde fracturado del agujero",p,new Vector3(.17f,.12f,.15f),stone);shard.localRotation=Quaternion.Euler(0,j*60,22);
                }
                for(int j=0;j<3;j++)
                {
                    var a=Rad(j,3,.2f,.09f);var b=Rad(j,3,.27f,.3f);var c=Rad(j,3,.17f,.34f);
                    UCurve(model,"Garra del horizonte de sucesos",new[]{a,b,c},.033f,cloth);Gem(model,"Luz atrapada",c,new Vector3(.035f,.07f,.035f),glow);
                }
            }
            void SprintIdentity()
            {
                var body=UFind("Idle:Body");URemove("Pierna de sombra");
                for(int s=-1;s<=1;s+=2)
                {
                    var knee=new Vector3(s*.1f,-.23f,s<0?.13f:-.1f);var foot=new Vector3(s*.15f,-.43f,s<0?.24f:-.19f);
                    Bone(body,"Muslo del sprite corredor",new Vector3(s*.07f,-.13f,0),knee,.055f,trim);Bone(body,"Pierna del sprite corredor",knee,foot,.05f,trim);Gem(body,"Pie del corredor",foot,new Vector3(.08f,.06f,.15f),glow);
                }
                for(int j=-1;j<=1;j++)Leaf(body,"Bufanda del sprite veloz",new Vector3(j*.065f,.11f,.2f),new Vector3(.07f,.39f-j*.035f,.035f),j==0?glow:cloth,new Vector3(-65,0,j*12));
                var head=UFind("Idle:Head");Gem(head,"Cresta aerodinámica del sprite",new Vector3(0,.11f,.055f),new Vector3(.08f,.18f,.17f),trim).localRotation=Quaternion.Euler(-35,0,0);
            }
        }
    }
}
