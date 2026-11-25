using System;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Original solar-imperial miniatures: stepped sandstone, falcon-like armour,
            // linen and lapis accents. Card anatomy and existing animation pivots remain intact.
            readonly Color saGold=new Color(.94f,.66f,.23f),saShade=new Color(.52f,.31f,.12f);
            readonly Color saSand=new Color(.83f,.69f,.47f),saLinen=new Color(.96f,.9f,.72f);
            readonly Color saTeal=new Color(.075f,.34f,.4f),saLapis=new Color(.14f,.67f,.77f);
            void ApplySahriaStyle()
            {
                if(card.leader!="SAHRIA")return;
                foreach(var r in model.GetComponentsInChildren<MeshRenderer>())
                {
                    string n=r.name;
                    if(n.IndexOf("Túnica",StringComparison.Ordinal)>=0||n.IndexOf("Manto",StringComparison.Ordinal)>=0||n.IndexOf("Capa",StringComparison.Ordinal)>=0||n.IndexOf("Alforja",StringComparison.Ordinal)>=0||n=="Lona tensada"||n=="Campo del escudo torre")m.Paint(r,saTeal);
                    else if(n.IndexOf("Capitel",StringComparison.Ordinal)>=0||n.IndexOf("Cinturón",StringComparison.Ordinal)>=0||n.IndexOf("Pectoral",StringComparison.Ordinal)>=0||n.IndexOf("Rayo",StringComparison.Ordinal)>=0||n.IndexOf("Corona",StringComparison.Ordinal)>=0||n.IndexOf("Cresta imperial",StringComparison.Ordinal)>=0)m.Paint(r,saGold);
                    else if(n.IndexOf("Escalón",StringComparison.Ordinal)>=0||n.IndexOf("Columna del santuario",StringComparison.Ordinal)>=0||n.IndexOf("Dintel del santuario",StringComparison.Ordinal)>=0||n.IndexOf("Torre de ciudad",StringComparison.Ordinal)>=0||n.IndexOf("Obelisco emergente",StringComparison.Ordinal)>=0)m.Paint(r,saSand);
                    else if(n.IndexOf("Glifo",StringComparison.Ordinal)>=0||n=="Cristal de rango"||n=="Gema del trono")m.Paint(r,saLapis);
                }
                if(card.IsStructure){SahriaArchitecture();return;}
                var body=UFind("Idle:Body");var head=UFind("Idle:Head");
                if(UFind("Torso")!=null)SahriaRetainer(body,head);
                else if(UFind("Torso animal")!=null)SahriaBeast(head);
                else if(UFind("Coraza de roca")!=null)SahriaAshGuard(body);
                else if(id=="escorpion-solar"||id=="escarabajo-de-cobre"||id=="battle-ant-token")SahriaCarapace();
                else if(id=="guardian-del-oasis")
                {
                    UColor("Caparazón del oasis",saTeal);UColor("Placa del caparazón",saGold);
                    Box(model,"Sahria: silla del manantial",new Vector3(0,.49f,-.025f),new Vector3(.21f,.07f,.22f),saGold);
                    Gem(model,"Sahria: lapislázuli del oasis",new Vector3(0,.55f,-.09f),new Vector3(.1f,.16f,.07f),saLapis);
                }
                else if(id=="baby-phoenix")
                {
                    UColor("Torso de ave",new Color(.8f,.29f,.09f));UColor("Cabeza del fénix",saGold);
                    for(int side=-1;side<=1;side+=2)Leaf(head,"Sahria: penacho del fénix",new Vector3(side*.07f,.13f,.01f),new Vector3(.06f,.23f,.05f),saGold,new Vector3(-10,0,-side*18));
                }
            }
            void SahriaRetainer(Transform body,Transform head)
            {
                bool nomad=id=="nomada-de-arena",priest=id=="sacerdote-solar",mummy=id=="old-mummy",avatar=id=="avatar-del-sol",emperor=id=="emperador-de-las-dunas";
                if(!nomad&&!mummy)
                {
                    URemove("Hombrera","Pectoral de bronce");
                    Box(body,"Sahria: peto laminar",new Vector3(0,.72f,-.12f),new Vector3(.35f,.24f,.105f),saGold);
                    Box(body,"Sahria: faldón de lino",new Vector3(0,.36f,-.165f),new Vector3(.155f,.26f,.035f),saLinen);
                    for(int side=-1;side<=1;side+=2)
                    {
                        var arm=UFind(side<0?"Idle:ArmL":"Idle:ArmR");
                        for(int row=0;row<2;row++)
                            Leaf(arm,"Sahria: hombrera alada",new Vector3(side*(.005f+row*.04f),.015f-row*.05f,-.055f),new Vector3(.16f,.25f-row*.025f,.085f),row==0?saGold:saShade,new Vector3(0,0,side*65));
                    }
                    Gem(body,"Sahria: gema del peto",new Vector3(0,.73f,-.184f),new Vector3(.08f,.12f,.04f),saLapis);
                }
                if(nomad)
                {
                    UColor("Turbante del nómada",saLinen);UColor("Caída del turbante",saLinen);UColor("Velo contra la arena",saTeal);
                    var arm=UFind("Idle:ArmL");Box(arm,"Sahria: brazal del caminante",new Vector3(-.055f,-.16f,-.05f),new Vector3(.11f,.14f,.13f),saGold);
                    Leaf(body,"Sahria: capa de arena",new Vector3(-.13f,.57f,.15f),new Vector3(.28f,.57f,.07f),saLinen,new Vector3(-12,0,-12));
                }
                else if(mummy)
                {
                    for(int side=-1;side<=1;side+=2)Box(body,"Sahria: cuello funerario",new Vector3(side*.105f,.8f,-.1f),new Vector3(.15f,.07f,.12f),saGold);
                    UColor("Amuleto funerario",saLapis);
                }
                else if(priest)
                {
                    URemove("Casquete");UColor("Túnica modelada",saLinen);
                    Ball(head,"Sahria: tocado sacerdotal",new Vector3(0,.12f,.025f),new Vector3(.3f,.2f,.28f),saLinen);
                    for(int side=-1;side<=1;side+=2)Leaf(head,"Sahria: corona del rito",new Vector3(side*.1f,.25f,.035f),new Vector3(.09f,.29f,.08f),saGold,new Vector3(-5,0,-side*12));
                }
                else
                {
                    URemove("Casquete","Corona de guardia","Cresta imperial");
                    Ball(head,"Sahria: casco solar",new Vector3(0,.13f,.015f),new Vector3(.31f,.22f,.29f),saGold);
                    for(int side=-1;side<=1;side+=2)
                    {
                        Box(head,"Sahria: carrillera del halcón",new Vector3(side*.115f,-.025f,-.065f),new Vector3(.055f,.2f,.14f),saGold);
                        Leaf(head,"Sahria: ala del casco",new Vector3(side*.135f,.12f,.04f),new Vector3(.08f,.29f,.085f),saGold,new Vector3(-20,0,-side*15));
                        var brow=Box(head,"Sahria: visera angular",new Vector3(side*.065f,.061f,-.11f),new Vector3(.14f,.065f,.105f),saGold);
                        brow.localRotation=Quaternion.Euler(0,-side*15,-side*9);
                    }
                    Gem(head,"Sahria: nasal del halcón",new Vector3(0,-.015f,-.155f),new Vector3(.045f,.15f,.085f),saGold);
                    if(emperor||avatar)
                    {
                        Leaf(head,"Sahria: cresta de mando",new Vector3(0,.28f,.04f),new Vector3(.12f,.33f,.08f),saGold,new Vector3(-12,0,0));
                        UColor("Manto imperial amplio",saLinen);UColor("Manto ceremonial",saTeal);UColor("Estandarte del emperador",saTeal);
                    }
                }
            }
            void SahriaBeast(Transform head)
            {
                var torso=UFind("Torso animal");float h=torso.localPosition.y;
                // Fitted barding follows the existing shoulder and back, never replaces animal anatomy.
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(model,"Sahria: barda imperial",new Vector3(side*torso.localScale.x*.42f,h+.065f,-.12f),new Vector3(.1f,.22f,.29f),saGold);
                    Bone(model,"Sahria: arnés de pecho",new Vector3(side*.12f,h+.12f,-.2f),new Vector3(side*.15f,h-.075f,-.2f),.035f,saTeal);
                }
                if(id=="dune-cammel")UColor("Alforja de caravana",saTeal);
                else
                {
                    Box(head,"Sahria: testera de bestia",new Vector3(0,.06f,-.055f),new Vector3(.14f,.12f,.19f),saGold);
                    Gem(head,"Sahria: gema de testera",new Vector3(0,.07f,-.155f),new Vector3(.065f,.09f,.035f),saLapis);
                }
            }
            void SahriaAshGuard(Transform body)
            {
                UColor("Puño tallado",saShade);UColor("Placa solar quebrada",saGold);
                for(int side=-1;side<=1;side+=2)
                {
                    var arm=UFind(side<0?"Idle:ArmL":"Idle:ArmR");
                    Leaf(arm,"Sahria: placa de ascua",new Vector3(side*.04f,.015f,-.1f),new Vector3(.21f,.31f,.1f),saShade,new Vector3(0,0,side*55));
                }
                Gem(body,"Sahria: sello solar forjado",new Vector3(0,.7f,-.195f),new Vector3(.16f,.17f,.06f),saGold);
            }
            void SahriaCarapace()
            {
                UColor("Abdomen segmentado",saShade);UColor("Tórax",saGold);
                if(id=="escarabajo-de-cobre")UColor("Élitro de cobre",new Color(.8f,.46f,.16f));
                else for(int row=0;row<3;row++)
                {
                    float y=id=="battle-ant-token"?.375f:.285f;
                    Ball(model,"Sahria: placa de quitina solar",new Vector3(0,y,.045f+row*.065f),new Vector3(.27f,.095f,.12f),row%2==0?saGold:saShade);
                }
            }
            void SahriaArchitecture()
            {
                var sculpture=model;model=UFind("Identidad: "+id)??model;
                try
                {
                    switch(id)
                    {
                        case "santuario-de-arena":
                            URemove("Cúpula de arena");
                            Box(model,"Sahria: cornisa del santuario",new Vector3(0,.855f,.06f),new Vector3(.68f,.075f,.42f),saGold);
                            Box(model,"Sahria: terraza de santuario",new Vector3(0,.92f,.06f),new Vector3(.52f,.07f,.33f),saSand);
                            for(int side=-1;side<=1;side+=2)Leaf(model,"Sahria: capitel de palma",new Vector3(side*.24f,.73f,.06f),new Vector3(.18f,.19f,.16f),saGold,new Vector3(0,0,-side*25));
                            break;
                        case "templo-de-ceniza":
                            URemove("Pirámide de escoria");
                            for(int level=0;level<3;level++)Box(model,"Sahria: pirámide escalonada",new Vector3(0,.9f+level*.075f,.07f),new Vector3(.57f-level*.11f,.085f,.47f-level*.075f),level==0?saGold:saShade);
                            for(int side=-1;side<=1;side+=2)Box(model,"Sahria: marco de pórtico",new Vector3(side*.115f,.43f,-.145f),new Vector3(.04f,.4f,.06f),saGold);
                            break;
                        case "obelisco-solar":
                        case "obelisco-de-arena":
                            float baseY=id=="obelisco-solar"?.2f:.05f;
                            for(int side=-1;side<=1;side+=2)
                            {
                                var buttress=Box(model,"Sahria: contrafuerte de obelisco",new Vector3(side*.12f,baseY+.17f,.02f),new Vector3(.095f,.35f,.25f),saGold);
                                buttress.localRotation=Quaternion.Euler(0,0,-side*8);
                            }
                            break;
                        case "obelisco-de-ruptura":
                            for(int side=-1;side<=1;side+=2)Box(model,"Sahria: zócalo de obelisco roto",new Vector3(side*.14f,.23f,0),new Vector3(.22f,.09f,.29f),saGold);
                            break;
                        case "trono-solar":
                            for(int side=-1;side<=1;side+=2)
                                Leaf(model,"Sahria: ala del respaldo",new Vector3(side*.19f,.83f,.16f),new Vector3(.15f,.48f,.095f),saGold,new Vector3(0,0,-side*15));
                            UColor("Respaldo del emperador",saTeal);break;
                        case "ciudad-sepultada":
                            foreach(var tower in model.GetComponentsInChildren<Transform>().Where(t=>t.name=="Torre de ciudad sepultada").ToArray())
                                Box(model,"Sahria: cornisa de ruina",tower.localPosition+Vector3.up*(tower.localScale.y*.42f),new Vector3(.23f,.065f,.24f),saGold);
                            break;
                        case "campamento-nomada":
                            var tent=UFind("Tienda de campaña");
                            if(tent!=null)for(int side=-1;side<=1;side+=2)
                                Bone(tent,"Sahria: ribete de lino",new Vector3(0,.46f,-.25f),new Vector3(side*.28f,0,-.25f),.027f,saLinen);
                            break;
                        case "sandy-anthill":
                            UColor("Montículo erosionado",saSand);UColor("Chimenea de tierra",saSand);UColor("Zócalo continuo de hormiguero",saSand);
                            for(int side=-1;side<=1;side+=2)Box(model,"Sahria: dintel del hormiguero",new Vector3(side*.13f,.235f,-.18f),new Vector3(.17f,.045f,.07f),saGold);
                            break;
                        case "oasis-de-cristal":
                            Box(model,"Sahria: ruina del oasis",new Vector3(-.23f,.07f,-.12f),new Vector3(.12f,.12f,.14f),saSand);break;
                        case "arena-profunda":
                        case "dunas-movedizas":
                            var fragment=Box(model,"Sahria: friso semienterrado",new Vector3(.18f,.06f,.16f),new Vector3(.2f,.07f,.12f),saGold);fragment.localRotation=Quaternion.Euler(0,-20,-8);break;
                    }
                }
                finally{model=sculpture;}
            }

            bool SculptDesertUnit()
            {
                switch (id)
                {
                    case "nomada-de-arena": DesertSoldier(DesertRole.Nomad); return true;
                    case "guardian-del-obelisco": DesertSoldier(DesertRole.Guard); return true;
                    case "sacerdote-solar": DesertSoldier(DesertRole.Priest); return true;
                    case "halconero-solar": DesertSoldier(DesertRole.Falconer); return true;
                    case "emperador-de-las-dunas": DesertSoldier(DesertRole.Emperor); return true;
                    case "avatar-del-sol": SolarApparition(); return true;
                    case "escorpion-solar": SolarScorpion(); return true;
                    case "escarabajo-de-cobre": CopperScarab(); return true;
                    case "battle-ant-token": ColonyAnt(); return true;
                    case "bestia-de-las-dunas": DuneRhinoceros(); return true;
                    case "guardian-del-oasis": OasisTortoise(); return true;
                    case "sandy-ember": EmberFennec(); return true;
                    case "dune-cammel": PackCamel(); return true;
                    case "old-mummy": DesertMummy(); return true;
                    case "wandering-beast": WastelandCrawler(); return true;
                    case "baby-phoenix": PhoenixChick(); return true;
                    default: return false;
                }
            }

            enum DesertRole { Nomad, Guard, Priest, Falconer, Emperor }

            void DesertSoldier(DesertRole role)
            {
                bool amberArmor = role == DesertRole.Guard || role == DesertRole.Emperor;
                bool nomad = role == DesertRole.Nomad;
                bool priest = role == DesertRole.Priest;
                bool emperor = role == DesertRole.Emperor;
                bool falconer = role == DesertRole.Falconer;
                Color hide=Pigment(0x79513C), linen=Pigment(nomad?0xBD8C60:0xE9D2A1), bronze=Pigment(0xB77F36), gilded=Pigment(0xE3AB50);
                Color outfit=Pigment(nomad?0x9E5742:priest?0xE8D3A7:emperor?0x9B443A:falconer?0x647267:0x34646E);
                Color flesh=Pigment(0xAC744C), faceShade=Pigment(0x815139);
                var body=G(model,"Idle:Body",Vector3.zero);
                if(nomad) body.localRotation=Quaternion.Euler(-2,-4,-4);
                float shoulderY=priest?.738f:emperor?.785f:.745f;
                Ball(body,"Torso del viajero del desierto",V(0,.613f,-.002f),V(amberArmor?.357f:.302f,.33f,.235f),outfit);
                for(int side=-1;side<=1;side+=2)
                {
                    float forward = (nomad || falconer ? side : -side) * .066f;
                    VaryDesertLeg(body,side,forward,linen,hide);
                    float skirtTop=priest?.444f:.405f;
                    var skirt=Plate(body,"Paño plisado",V(side*.083f,skirtTop,-.071f),V(.194f,priest?.454f:.319f,.183f),outfit,
                        V(11,-side*20,-side*13),"cloth",ClothEdge);
                    Bone(skirt,"Borde tejido",V(-.33f,-.38f,-.51f),V(.31f,-.38f,-.51f),.053f,bronze);
                    if (amberArmor)
                    {
                        Plate(body,"Guardafalda de placas",V(side*.131f,.421f,-.032f),V(.131f,.224f,.171f),bronze,
                            V(13,-side*24,-side*12),"shield",ShieldEdge);
                    }
                }
                Box(body,"Cinturón de cuero",V(0,.519f,-.099f),V(.321f,.065f,.119f),hide);
                Gem(body,"Hebilla de lapislázuli",V(.014f,.517f,-.166f),V(.087f,.079f,.038f),Pigment(0x51BBC6));
                if (amberArmor)
                {
                    Plate(body,"Coraza facetada",V(0,.671f,-.101f),V(.342f,.267f,.146f),bronze,
                        V(0,0,0),"shield",ShieldEdge);
                    Plate(body,"Collar solar",V(0,.739f,-.165f),V(.309f,.135f,.057f),gilded,
                        V(8,0,0),"shield",ShieldEdge);
                    for(int n=-1;n<=1;n++)
                        Gem(body,"Incrustación de lapislázuli",V(n*.073f,.702f-Mathf.Abs(n)*.018f,-.203f),
                            V(.043f,.085f,.027f),Pigment(0x4DBBC8));
                }
                else
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        Vector3 start=V(side*.127f,shoulderY,-.087f), end=V(side*(priest?.064f:.15f),.50f,-.127f);
                        Bone(body,priest?"Estola color lapislázuli":"Tirante de cuero",start,end,priest?.049f:.041f,priest?Pigment(0x517D82):hide);
                    }
                }
                if (nomad || falconer)
                {
                    Bone(body,"Bandolera de viaje",V(-.146f,.709f,-.106f),V(.16f,.47f,-.130f),.05f,hide);
                    Ball(body,"Odre del viajero",V(.176f,.452f,.055f),V(.161f,.214f,.154f),Pigment(0x985E40));
                    Stem(body,"Cuello del odre",V(.179f,.541f,.042f),.068f,.085f,linen);
                }
                if (emperor || priest || falconer)
                {
                    var cape = Plate(body,"Manto ceremonial",V(0, .489f,.129f),
                        V(emperor?.455f:priest?.339f:.317f,emperor?.663f:.436f,.097f),
                        emperor?Pigment(0x8F3841):priest?linen:Pigment(0x6F7D68),V(-14,-11,-9),"cloth",ClothEdge);
                    for(int i=-1;i<=1;i++)
                        Bone(cape,"Pliegue de manto",V(i*.22f,.4f,.44f),V(i*.29f,-.36f,.47f),.034f,bronze);
                }

                var head=G(body,"Idle:Head",V(0,shoulderY+.202f,-.005f));
                head.localRotation=Quaternion.Euler(nomad?-7:0,falconer?-17:nomad?10:0,emperor?4:0);
                Ball(head,"Cráneo",V(0,-.012f,.004f),V(.235f,.269f,.24f),flesh);
                Ball(head,"Mandíbula",V(0,-.081f,-.041f),V(.182f,.134f,.185f),faceShade);
                if (!nomad)
                    P(head,"Nariz tallada",m.Carved,V(0,-.024f,-.122f),V(.056f,.084f,.047f),flesh);
                MiniatureEyes(head,.059f,.023f,-.112f,.049f,Pigment(0x534133),linen,role==DesertRole.Guard?8:0);
                if (amberArmor)
                {
                    P(head,"Casco de metal dorado",m.MushroomCap,V(0,.087f,.037f),V(.323f,.352f,.295f),gilded);
                    Box(head,"Visera angular",V(0,.067f,-.127f),V(.269f,.057f,.056f),bronze);
                    for(int side=-1;side<=1;side+=2)
                        Plate(head,"Carrillera del casco",V(side*.111f,-.039f,-.064f),
                            V(.079f,.207f,.089f),gilded,V(11,-side*23,side*9),"shield",ShieldEdge);
                    Gem(head,"Cresta grabada",V(0,.133f,-.106f),V(.079f,.16f,.054f),Pigment(0x6DB3B7));
                    if (emperor)
                        for(int i=-1;i<=1;i++)
                            Plate(head,"Cima de la corona",V(i*.082f,.215f,.012f),V(.083f,i==0?.243f:.181f,.079f),
                                gilded,V(-13,0,-i*21),"leaf",LeafEdge);
                }
                else
                {
                    Color wrap=nomad?outfit:falconer?Pigment(0x9D7957):linen;
                    Ball(head,"Tocado enrollado",V(0,.099f,.012f),V(.284f,.185f,.277f),wrap);
                    for(int i=0;i<3;i++)
                    {
                        var band=Stem(head,"Vuelta del tocado",V(0,.052f+i*.034f,.002f),.278f-i*.015f,.028f,i==1?linen:wrap);
                        band.localRotation=Quaternion.Euler(2,0,i%2==0?9:-8);
                    }
                    if (nomad)
                    {
                        Plate(head,"Velo del desierto",V(0,-.062f,-.125f),V(.233f,.141f,.071f),wrap,Vector3.zero,"cloth",ClothEdge);
                        Plate(head,"Extremo suelto del velo",V(.118f,-.079f,.086f),V(.145f,.355f,.049f),
                            wrap,V(24,21,36),"cloth",ClothEdge);
                    }
                    if (priest)
                        Gem(head,"Marca solar",V(0,.119f,-.144f),V(.072f,.116f,.032f),gilded);
                }

                for(int side=-1;side<=1;side+=2)
                {
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.157f,shoulderY,.009f));
                    float armRise=side<0 && (falconer||priest)?.012f:-.176f;
                    Vector3 hand=V(side*(falconer&&side<0?.224f:.106f),armRise,side<0?-.110f:-.066f);
                    CurvedLimb(arm,"Brazo humano "+side,linen,Vector3.zero,V(side*.095f,-.115f,-.012f),hand,.067f,.056f,.052f);
                    Ball(arm,"Mano",hand,V(.107f,.099f,.104f),flesh);
                    Plate(arm,"Brazal labrado",Vector3.Lerp(V(side*.095f,-.115f,-.012f),hand,.65f),
                        V(.113f,.098f,.094f),bronze,V(26,-side*12,13),"shield",ShieldEdge);
                    if (amberArmor)
                        Plate(arm,"Hombrera con filo",V(side*.023f,-.017f,-.044f),
                            V(emperor?.235f:.196f,.139f,.15f),gilded,V(22,0,side*14),"leaf",LeafEdge);
                    if (role == DesertRole.Guard)
                    {
                        if (side<0) ObeliskShield(arm,hand,bronze,gilded); else DesertSpear(arm,hand,gilded,hide);
                    }
                    else if (emperor)
                    {
                        if (side>0) DesertStandard(arm,hand,gilded,hide); else SolarRelic(arm,hand,gilded);
                    }
                    else if (priest)
                    {
                        if (side<0) SacredScroll(arm,hand,linen,bronze); else SolarRelic(arm,hand,gilded);
                    }
                    else if (falconer && side<0)
                        PerchedFalcon(arm,hand+V(0,.035f,0),gilded);
                    else if (side>0)
                        DesertKhopesh(arm,hand,bronze,hide,falconer?.79f:1);
                }
            }

            void VaryDesertLeg(Transform body, int side, float forward, Color linen, Color hide)
            {
                CurvedLimb(body,"Pierna del desierto "+side,linen,V(side*.095f,.435f,.012f),V(side*.105f,.234f,forward-.014f),
                    V(side*.134f,.084f,forward),.064f,.061f,.051f);
                Ball(body,"Bota",V(side*.136f,.058f,forward-.047f),V(.145f,.119f,.215f),hide);
                Bone(body,"Borde de bota",V(side*.134f,.111f,forward-.069f),V(side*.134f,.12f,forward+.041f),.091f,hide);
            }

            void DesertKhopesh(Transform arm, Vector3 hand, Color gilded, Color hide, float scale=1)
            {
                var blade=G(arm,"Khopesh",hand);
                blade.localRotation=Quaternion.Euler(13,16,-29);
                blade.localScale=Vector3.one*(scale*.91f);
                Bone(blade,"Empuñadura",V(0,-.105f,0),V(0,.06f,0),.045f,hide);
                Box(blade,"Guarda de bronce",V(0,.057f,0),V(.109f,.046f,.075f),gilded);
                Plate(blade,"Hoja curva de bronce",V(.047f,.229f,0),V(.181f,.367f,.048f),Pigment(0xD5B273),Vector3.zero,"blade",BladeEdge);
                Bone(blade,"Bisel claro",V(.063f,.142f,-.029f),V(.108f,.333f,-.027f),.018f,Pigment(0xF5D49A));
            }

            void ObeliskShield(Transform arm, Vector3 hand, Color bronze, Color gold)
            {
                var shield=G(arm,"Escudo obelisco",hand+V(-.027f,.019f,-.053f));
                shield.localRotation=Quaternion.Euler(9,13,-12);
                Plate(shield,"Bronce macizo",Vector3.zero,V(.278f,.529f,.098f),gold,Vector3.zero,"shield",ShieldEdge);
                Plate(shield,"Cara esmaltada",V(0,.002f,-.055f),V(.218f,.458f,.054f),Pigment(0x356E78),Vector3.zero,"shield",ShieldEdge);
                for(int i=0;i<3;i++)
                {
                    Gem(shield,"Runas del obelisco",V(0,.134f-i*.116f,-.091f),V(.060f,.09f,.026f),Pigment(0x9ADEDA));
                    for(int side=-1;side<=1;side+=2)
                        Box(shield,"Ribete grabado",V(side*.077f,.136f-i*.118f,-.086f),V(.026f,.039f,.025f),bronze);
                }
            }

            void DesertSpear(Transform arm, Vector3 hand, Color gold, Color hide)
            {
                var spear=G(arm,"Lanza de guardia",hand);
                spear.localRotation=Quaternion.Euler(11,4,-14);
                Bone(spear,"Asta de madera",V(0,-.49f,0),V(0,.51f,0),.04f,hide);
                Plate(spear,"Punta ceremonial",V(0,.57f,0),V(.118f,.251f,.057f),gold,Vector3.zero,"leaf",LeafEdge);
                Gem(spear,"Engaste turquesa",V(0,.487f,-.03f),V(.064f,.09f,.045f),Pigment(0x68C4C7));
            }

            void DesertStandard(Transform arm, Vector3 hand, Color gold, Color hide)
            {
                var pole=G(arm,"Estandarte del emperador",hand);
                pole.localRotation=Quaternion.Euler(2,12,9);
                Bone(pole,"Asta real",V(0,-.39f,0),V(0,.544f,0),.036f,hide);
                Plate(pole,"Pendón granate",V(.112f,.36f,0),V(.235f,.328f,.048f),Pigment(0xA64642),V(8,0,-12),"cloth",ClothEdge);
                Gem(pole,"Remate solar",V(0,.552f,0),V(.096f,.176f,.075f),gold);
                Gem(pole,"Sello del estandarte",V(.12f,.354f,-.04f),V(.103f,.135f,.027f),gold);
            }

            void SacredScroll(Transform arm, Vector3 hand, Color paper, Color bronze)
            {
                var scroll=G(arm,"Pergamino abierto",hand+V(-.047f,.019f,-.041f));
                scroll.localRotation=Quaternion.Euler(47,-12,11);
                Plate(scroll,"Página de marfil",Vector3.zero,V(.229f,.237f,.038f),paper,Vector3.zero,"cloth",ClothEdge);
                Bone(scroll,"Borde enrollado",V(-.112f,.106f,-.011f),V(.112f,.106f,-.011f),.058f,bronze);
                Bone(scroll,"Borde enrollado",V(-.093f,-.108f,-.011f),V(.106f,-.108f,-.011f),.058f,bronze);
                for(int i=0;i<3;i++) Bone(scroll,"Renglón de tinta",V(-.071f,.043f-i*.046f,-.028f),V(.044f-i*.012f,.038f-i*.046f,-.029f),.013f,Pigment(0x8B6246));
            }

            void SolarRelic(Transform arm, Vector3 hand, Color gold)
            {
                Vector3 center = hand + V(.022f,.046f,-.036f);
                Ball(arm,"Orbe de lapislázuli",center,V(.150f,.158f,.133f),Pigment(0x5CC3CC));
                Ring(arm,"Engaste de latón",center+V(0,0,-.03f),.079f,.023f,gold,8,Quaternion.Euler(90,0,0));
                Gem(arm,"Reflejo del orbe",center+V(-.018f,.025f,-.067f),V(.057f,.072f,.026f),Pigment(0xBBF0CE));
            }

            void PerchedFalcon(Transform arm, Vector3 perched, Color beak)
            {
                Color bronze=Pigment(0x87593D), cream=Pigment(0xE8C28E), shade=Pigment(0x51413C);
                var falcon=G(arm,"Halcón compañero",perched);
                falcon.localRotation=Quaternion.Euler(-1,28,-12);
                Ball(falcon,"Cuerpo del halcón",V(0,.10f,.014f),V(.127f,.194f,.129f),bronze);
                Ball(falcon,"Pechera clara",V(0,.105f,-.039f),V(.101f,.139f,.072f),cream);
                Ball(falcon,"Cabeza con máscara",V(0,.205f,-.036f),V(.119f,.119f,.113f),cream);
                Gem(falcon,"Pico ganchudo",V(0,.176f,-.108f),V(.058f,.065f,.073f),beak).localRotation=Quaternion.Euler(-38,0,0);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(falcon,"Ojo del ave",V(side*.037f,.21f,-.075f),V(.037f,.032f,.026f),shade);
                    var wing=G(falcon,side<0?"Idle:WingL":"Idle:WingR",V(side*.041f,.111f,.025f));
                    Plate(wing,"Ala plegada",V(side*.036f,-.033f,.048f),V(.094f,.196f,.034f),bronze,V(-31,side*58,side*22),"feather",FeatherEdge);
                    Plate(wing,"Plumas largas",V(side*.037f,-.052f,.071f),V(.065f,.154f,.027f),shade,V(-42,side*53,side*20),"feather",FeatherEdge);
                    Bone(falcon,"Garras sobre el guante",V(side*.032f,.042f,-.004f),V(side*.033f,-.012f,-.032f),.024f,beak);
                }
                Plate(falcon,"Cola",V(0,.003f,.094f),V(.103f,.166f,.036f),shade,V(-44,0,0),"leaf",LeafEdge);
            }

            void SolarApparition()
            {
                Color gold=Pigment(0xE8A83C), amber=Pigment(0xEA7938), white=Pigment(0xFFE4A2), bronze=Pigment(0x9F542E);
                var body=G(model,"Idle:Body",V(0,.151f,0));
                var halo=G(body,"Disco de sol",V(0,.865f,.146f));
                halo.localRotation=Quaternion.Euler(-13,0,0);
                Ring(halo,"Círculo de sol",Vector3.zero,.279f,.047f,gold,12,Quaternion.Euler(90,0,0));
                for(int i=0;i<10;i++)
                {
                    float angle=i*36*Mathf.Deg2Rad;
                    Plate(halo,"Rayo de corona",V(Mathf.Sin(angle)*.329f,Mathf.Cos(angle)*.329f,-.008f),V(.066f,i%2==0?.172f:.127f,.049f),
                        i%2==0?gold:white,V(0,0,-i*36),"leaf",LeafEdge);
                }
                Ball(body,"Torso radiante",V(0,.594f,.024f),V(.34f,.384f,.277f),bronze);
                Plate(body,"Coraza con brillo interior",V(0,.606f,-.113f),V(.33f,.345f,.142f),gold,V(7,0,0),"leaf",LeafEdge);
                Gem(body,"Núcleo solar",V(0,.624f,-.185f),V(.105f,.172f,.054f),white);
                var head=G(body,"Idle:Head",V(0,.858f,-.044f));
                Plate(head,"Máscara solar",Vector3.zero,V(.234f,.294f,.208f),gold,V(5,0,0),"shield",ShieldEdge);
                for(int side=-1;side<=1;side+=2)
                {
                    Leaf(head,"Abertura de luz",V(side*.057f,.026f,-.126f),V(.042f,.076f,.032f),white,V(0,0,-side*64));
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.139f,.65f,.018f));
                    Vector3 hand=V(side*.196f,-.061f,-.079f);
                    CurvedLimb(arm,"Brazo dorado "+side,bronze,Vector3.zero,V(side*.118f,-.016f,.014f),hand,.079f,.057f,.05f);
                    Plate(arm,"Borde de brazo",V(side*.053f,-.022f,-.045f),V(.123f,.228f,.079f),gold,V(0,side*10,side*65),"leaf",LeafEdge);
                    Ball(arm,"Luz en la palma",hand,V(.134f,.135f,.108f),white);
                    Sweep(body,"Cinta de ascua "+side,side<0?amber:gold,Knot(side*.094f,.47f,.071f,.072f),
                        Knot(side*.163f,.352f,.108f,.065f),Knot(side*.145f,.237f,.146f,.043f),
                        Knot(side*.109f,.158f,.152f,.025f),Knot(side*.19f,.106f,.154f,.009f));
                }
                Plate(body,"Paño suspendido",V(0,.354f,-.057f),V(.302f,.366f,.095f),white,V(11,0,7),"cloth",ClothEdge);
            }

            void SolarScorpion()
            {
                Color shell=Pigment(0xA96031), gold=Pigment(0xE5A54D), shade=Pigment(0x654237), tip=Pigment(0x573246);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Caparazón alargado",V(0,.218f,.079f),V(.326f,.263f,.48f),shell);
                Ball(body,"Tórax del escorpión",V(0,.211f,-.156f),V(.264f,.196f,.268f),shade);
                for(int i=0;i<4;i++)
                {
                    var plate=Ball(body,"Placa solar del abdomen",V(0,.307f-.018f*i,-.075f+i*.086f),
                        V(.302f-i*.019f,.151f,.126f),i%2==0?gold:shell);
                    plate.localRotation=Quaternion.Euler(i*7-5,0,0);
                }
                for(int side=-1;side<=1;side+=2)
                {
                    for(int row=0;row<4;row++)
                    {
                        float z=-.156f+row*.108f;
                        CurvedLimb(body,"Pata de escorpión "+side+"/"+row,shade,
                            V(side*.092f,.211f,z),V(side*(.256f+.022f*(row%2)),.151f,z-.048f),
                            V(side*(.369f-.015f*row),.023f,z+.035f),.038f,.023f,.01f);
                    }
                    var claw=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.092f,.205f,-.206f));
                    CurvedLimb(claw,"Brazo de pinza "+side,shell,Vector3.zero,V(side*.136f,.008f,-.088f),V(side*.209f,.019f,-.123f),.071f,.055f,.063f);
                    Ball(claw,"Pinza ancha",V(side*.217f,.026f,-.164f),V(.167f,.141f,.198f),shell);
                    Sweep(claw,"Dedo exterior "+side,gold,Knot(side*.267f,.022f,-.182f,.049f),Knot(side*.305f,.031f,-.274f,.041f),
                        Knot(side*.289f,.039f,-.337f,.027f),Knot(side*.230f,.04f,-.363f,.007f));
                    Sweep(claw,"Dedo interior "+side,shell,Knot(side*.161f,.014f,-.192f,.045f),Knot(side*.138f,.017f,-.264f,.034f),
                        Knot(side*.176f,.031f,-.322f,.022f),Knot(side*.217f,.041f,-.329f,.006f));
                    Ball(body,"Ojo de escorpión",V(side*.047f,.254f,-.277f),V(.046f,.043f,.025f),tip);
                }
                var tail=G(body,"Idle:Tail",V(0,.207f,.258f));
                Sweep(tail,"Cola articulada",shell,Knot(0,0,0,.078f),Knot(.016f,.047f,.132f,.079f),Knot(.028f,.171f,.165f,.067f),
                    Knot(.03f,.286f,.134f,.055f),Knot(.026f,.387f,.045f,.048f),Knot(.015f,.444f,-.058f,.042f));
                for (int i=0;i<4;i++)
                {
                    float fraction=i/3f;
                    var band=Ball(tail,"Anillo solar",V(.02f,.12f+fraction*.31f,.15f-fraction*fraction*.191f),
                        V(.158f-i*.019f,.09f,.155f-i*.019f),i%2==0?gold:shell);
                    band.localRotation=Quaternion.Euler(14+fraction*34,0,0);
                }
                Sweep(tail,"Aguijón curvo",tip,Knot(.016f,.443f,-.054f,.045f),Knot(.014f,.469f,-.13f,.039f),
                    Knot(.01f,.437f,-.18f,.022f),Knot(.01f,.393f,-.195f,.005f));
            }

            void CopperScarab()
            {
                Color shell=Pigment(0xBC774E), copper=Pigment(0xE3A073), shade=Pigment(0x6C493D), gilt=Pigment(0x9C653F);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Vientre del escarabajo",V(0,.212f,.092f),V(.371f,.27f,.494f),shade);
                for(int side=-1;side<=1;side+=2)
                {
                    var elytron=Ball(body,"Élitro de cobre",V(side*.105f,.304f,.102f),V(.238f,.26f,.476f),side<0?shell:copper);
                    elytron.localRotation=Quaternion.Euler(-7,side*8,side*11);
                    Sweep(body,"Estría del élitro "+side,gilt,Knot(side*.087f,.394f,-.057f,.012f),
                        Knot(side*.137f,.424f,.08f,.012f),Knot(side*.16f,.391f,.202f,.01f),Knot(side*.11f,.336f,.271f,.005f));
                    for(int i=0;i<3;i++)
                    {
                        float z=-.142f+.16f*i;
                        CurvedLimb(body,"Pata excavadora "+side+"/"+i,shade,V(side*.123f,.221f,z),V(side*.249f,.139f,z+.051f),
                            V(side*.35f,.029f,z-.047f),.043f,.033f,.017f);
                    }
                }
                Ball(body,"Tórax cobrizo",V(0,.283f,-.13f),V(.287f,.235f,.228f),gilt);
                var head=G(body,"Idle:Head",V(0,.211f,-.238f));
                Ball(head,"Cabeza mineral",Vector3.zero,V(.234f,.168f,.223f),shell);
                Sweep(head,"Cuerno del escarabajo",gilt,Knot(0,.015f,-.016f,.071f),Knot(0,.103f,-.091f,.061f),
                    Knot(0,.181f,-.148f,.04f),Knot(0,.222f,-.176f,.009f));
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Ojo negro",V(side*.083f,.030f,-.083f),V(.046f,.049f,.042f),ink);
                    Gem(head,"Mandíbula excavadora",V(side*.075f,-.047f,-.135f),V(.083f,.139f,.068f),gilt).localRotation=Quaternion.Euler(-51,0,side*22);
                }
                Ball(body,"Reflejo de cobre",V(.072f,.397f,-.055f),V(.067f,.038f,.042f),Pigment(0xF3C08E));
            }

            void ColonyAnt()
            {
                Color shell=Pigment(0x7D4A35), amber=Pigment(0xB07147), darkBrown=Pigment(0x4C3128);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Abdomen de la hormiga",V(0,.224f,.163f),V(.251f,.237f,.295f),shell).localRotation=Quaternion.Euler(-9,0,0);
                Ball(body,"Brillo del abdomen",V(0,.305f,.147f),V(.155f,.071f,.224f),amber);
                Ball(body,"Nudo de la cintura",V(0,.192f,.014f),V(.104f,.119f,.127f),darkBrown);
                Ball(body,"Tórax de la hormiga",V(0,.192f,-.073f),V(.161f,.157f,.171f),shell);
                var head=G(body,"Idle:Head",V(0,.201f,-.195f));
                Ball(head,"Cabeza triangular",Vector3.zero,V(.215f,.167f,.169f),shell);
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<3;i++)
                    {
                        float z=-.14f+.061f*i;
                        CurvedLimb(body,"Pata de hormiga "+side+"/"+i,darkBrown,V(side*.053f,.172f,z),
                            V(side*.151f,.128f,z-.043f),V(side*.246f,.023f,z+(i-1)*.075f),.022f,.017f,.009f);
                    }
                    Sweep(head,"Antena acodada "+side,darkBrown,Knot(side*.053f,.056f,-.011f,.019f),
                        Knot(side*.092f,.107f,-.077f,.016f),Knot(side*.139f,.121f,-.147f,.011f),Knot(side*.166f,.091f,-.20f,.006f));
                    Sweep(head,"Mandíbula de colonia "+side,amber,Knot(side*.075f,-.043f,-.043f,.038f),
                        Knot(side*.095f,-.044f,-.121f,.032f),Knot(side*.064f,-.034f,-.168f,.019f),Knot(side*.022f,-.032f,-.165f,.005f));
                    Ball(head,"Ojo lateral",V(side*.091f,.021f,-.061f),V(.043f,.052f,.032f),ink);
                }
            }

            void DuneRhinoceros()
            {
                Color skinColor=Pigment(0x977053), chest=Pigment(0xB88B5E), shield=Pigment(0xA7764D);
                Color lightHorn=Pigment(0xDEC397), toes=Pigment(0x70523C);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso pesado de las dunas",V(0,.376f,.082f),V(.473f,.451f,.593f),skinColor);
                Ball(body,"Joroba de arena",V(0,.511f,.038f),V(.441f,.381f,.521f),chest);
                for(int side=-1;side<=1;side+=2)
                {
                    for(int front=0;front<2;front++)
                    {
                        float z=front==0?-.145f:.253f;
                        Vector3 foot=V(side*.204f,.063f,z+side*.036f);
                        CurvedLimb(body,"Pata corpulenta "+side+"/"+front,skinColor,V(side*.132f,.378f,z),
                            V(side*.204f,.179f,z-.044f),foot,.12f,.099f,.083f);
                        Ball(body,"Pezuña excavadora",foot+V(0,0,-.049f),V(.188f,.117f,.195f),toes);
                        for(int claw=-1;claw<=1;claw++)
                            Ball(body,"Uña de pezuña",foot+V(claw*.044f,-.008f,-.133f),V(.044f,.056f,.046f),lightHorn);
                    }
                    var shoulder=Plate(body,"Placa de hombro de arenisca",V(side*.171f,.441f,-.065f),V(.218f,.284f,.104f),shield,
                        V(15,side*61,side*23),"shield",ShieldEdge);
                    Bone(shoulder,"Terrón de arena",V(-.26f,.16f,-.56f),V(.14f,.31f,-.49f),.042f,chest);
                }
                var head=G(body,"Idle:Head",V(0,.361f,-.256f));
                Ball(head,"Cráneo triangular pesado",V(0,.015f,-.027f),V(.33f,.289f,.307f),chest);
                Ball(head,"Hocico ancho",V(0,-.043f,-.135f),V(.252f,.164f,.203f),skinColor);
                Sweep(head,"Cuerno de arenisca",lightHorn,Knot(0,.00f,-.155f,.083f),Knot(0,.107f,-.198f,.069f),
                    Knot(0,.199f,-.206f,.044f),Knot(0,.244f,-.18f,.009f));
                for(int side=-1;side<=1;side+=2)
                {
                    Plate(head,"Oreja corta",V(side*.126f,.135f,.015f),V(.117f,.164f,.087f),shield,V(-21,0,-side*31),"leaf",LeafEdge);
                    Ball(head,"Ojo color arena",V(side*.108f,.044f,-.124f),V(.053f,.047f,.038f),toes);
                    Ball(head,"Pupila bronce",V(side*.11f,.047f,-.144f),V(.023f,.029f,.018f),Pigment(0xF0B956));
                }
                var tail=G(body,"Idle:Tail",V(0,.377f,.312f));
                Sweep(tail,"Cola pesada",skinColor,Knot(0,0,0,.073f),Knot(.066f,-.071f,.13f,.048f),
                    Knot(.114f,-.15f,.186f,.034f),Knot(.158f,-.148f,.218f,.012f));
            }

            void OasisTortoise()
            {
                Color bodyColor=Pigment(0x99835A), shell=Pigment(0x49716A), plates=Pigment(0x658F7E), sand=Pigment(0xDFB276);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Vientre de tortuga",V(0,.221f,.033f),V(.417f,.291f,.579f),bodyColor);
                Ball(body,"Caparazón del oasis",V(0,.371f,.038f),V(.58f,.423f,.646f),shell);
                for(int i=0;i<8;i++)
                {
                    float a=i*Mathf.PI/4f;
                    Vector3 p=V(Mathf.Cos(a)*.242f,.351f,.032f+Mathf.Sin(a)*.27f);
                    var scute=Ball(body,"Placa solapada del caparazón",p,V(.168f,.173f,.179f),i%2==0?plates:shell);
                    scute.localRotation=Quaternion.Euler(18,i*45,32);
                }
                for(int side=-1;side<=1;side+=2)
                    for(int i=0;i<2;i++)
                    {
                        Vector3 end=V(side*.271f,.068f,i==0?-.196f:.214f);
                        CurvedLimb(body,"Pata de guardián "+side+"/"+i,bodyColor,V(side*.126f,.195f,end.z),
                            V(side*.229f,.115f,end.z+.022f),end,.084f,.072f,.059f);
                        Ball(body,"Pata plana",end+V(0,-.018f,-.022f),V(.174f,.083f,.164f),bodyColor);
                        for(int digit=-1;digit<=1;digit++)
                            Ball(body,"Uña de tortuga",end+V(digit*.036f,-.007f,-.096f),V(.032f,.035f,.05f),sand);
                    }
                var head=G(body,"Idle:Head",V(0,.235f,-.328f));
                Ball(head,"Cabeza de tortuga",V(0,.002f,-.023f),V(.213f,.197f,.241f),bodyColor);
                Ball(head,"Pico de tortuga",V(0,-.052f,-.122f),V(.162f,.112f,.109f),sand);
                MiniatureEyes(head,.064f,.037f,-.108f,.055f,Pigment(0x735C2F),Pigment(0xDFC878));
                Ball(body,"Piedra clara en la cima",V(0,.563f,.054f),V(.273f,.089f,.281f),sand);
                Ball(body,"Espejo turquesa",V(-.027f,.60f,.02f),V(.176f,.043f,.174f),Pigment(0x69BDB0));
                Sweep(body,"Palma del caparazón",Pigment(0x846744),Knot(.078f,.58f,.088f,.036f),Knot(.101f,.667f,.116f,.027f),Knot(.138f,.748f,.105f,.014f));
                for(int i=0;i<4;i++)
                    Plate(body,"Fronda curvada",V(.138f,.753f,.105f)+Rad(i,4,.06f,0),V(.081f,.179f,.039f),
                        Pigment(0x79A15B),V(49,i*90+39,7),"leaf",LeafEdge);
            }

            void EmberFennec()
            {
                Color red=Pigment(0xB54C28), orange=Pigment(0xE97531), gold=Pigment(0xFBA842), fire=Pigment(0xFFD97D);
                var body=G(model,"Idle:Body",V(0,.048f,0));
                Ball(body,"Cuerpo en carrera",V(0,.306f,.043f),V(.278f,.289f,.476f),red).localRotation=Quaternion.Euler(-16,0,9);
                Ball(body,"Pecho incandescente",V(0,.335f,-.119f),V(.251f,.251f,.216f),orange);
                for(int side=-1;side<=1;side+=2)
                    for(int front=0;front<2;front++)
                    {
                        float z=front==0?-.105f:.193f;
                        var claw=V(side*.146f,front==0?.093f+side*.045f:.096f-side*.047f,z+(side<0?.095f:-.136f));
                        CurvedLimb(body,"Zancada de zorro "+side+"/"+front,red,V(side*.082f,.29f,z),
                            V(side*.156f,.176f,Mathf.Lerp(z,claw.z,.7f)+.038f),claw,.052f,.04f,.031f);
                        Ball(body,"Pata cobriza",claw+V(0,0,-.029f),V(.098f,.07f,.126f),orange);
                    }
                var head=G(body,"Idle:Head",V(0,.396f,-.202f));
                head.localRotation=Quaternion.Euler(-4,7,-7);
                Ball(head,"Cabeza de fénec",V(0,0,.008f),V(.284f,.234f,.25f),orange);
                Ball(head,"Hocico de ascuas",V(0,-.058f,-.106f),V(.192f,.117f,.17f),gold);
                Ball(head,"Nariz oscura",V(0,-.043f,-.194f),V(.066f,.043f,.05f),Pigment(0x693732));
                MiniatureEyes(head,.071f,.02f,-.116f,.078f,Pigment(0xA64526),fire,11);
                for(int side=-1;side<=1;side+=2)
                {
                    var ear=V(side*.109f,.17f,.021f);
                    var tilt=V(-17,-side*11,-side*29);
                    Plate(head,"Oreja flamígera de fénec",ear,V(.154f,.377f,.093f),red,tilt,"leaf",LeafEdge);
                    Plate(head,"Fuego dentro de la oreja",ear+V(0,.011f,-.051f),V(.098f,.278f,.039f),gold,tilt,"leaf",LeafEdge);
                    Sweep(body,"Grieta cálida "+side,fire,Knot(side*.092f,.374f,-.066f,.014f),
                        Knot(side*.138f,.386f,.008f,.017f),Knot(side*.126f,.409f,.07f,.012f),Knot(side*.13f,.382f,.122f,.006f));
                }
                Plate(head,"Mechón dorado",V(0,.155f,-.014f),V(.13f,.265f,.067f),gold,V(-12,0,18),"leaf",LeafEdge);
                var tail=G(body,"Idle:Tail",V(0,.313f,.241f));
                Sweep(tail,"Cola de brasa ondulante",red,Knot(0,0,0,.08f),Knot(.04f,.045f,.12f,.089f),Knot(.10f,.105f,.215f,.083f),
                    Knot(.118f,.21f,.247f,.061f),Knot(.153f,.29f,.216f,.04f),Knot(.175f,.33f,.171f,.006f));
                Plate(tail,"Llama exterior de cola",V(.10f,.212f,.201f),V(.212f,.343f,.125f),gold,
                    V(14,-33,-38),"feather",FeatherEdge);
                Plate(tail,"Llama clara de cola",V(.10f,.179f,.142f),V(.107f,.214f,.049f),fire,
                    V(12,-33,-36),"feather",FeatherEdge);
            }

            void PackCamel()
            {
                Color skinColor=Pigment(0xBA8553), shade=Pigment(0x946039), wool=Pigment(0xD1A46D);
                Color saddle=Pigment(0x7B5540), blanket=Pigment(0xAC6647), edge=Pigment(0xDCAB6E);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso del dromedario",V(0,.515f,.071f),V(.311f,.358f,.631f),skinColor);
                Ball(body,"Joroba bajo el equipaje",V(0,.664f,.056f),V(.289f,.306f,.355f),wool);
                Sweep(body,"Cuello curvo del dromedario",skinColor,Knot(0,.53f,-.162f,.122f),
                    Knot(0,.661f,-.205f,.095f),Knot(.014f,.766f,-.256f,.08f),
                    Knot(.015f,.913f,-.268f,.064f),Knot(.021f,.974f,-.303f,.07f));
                for(int side=-1;side<=1;side+=2)
                {
                    for(int front=0;front<2;front++)
                    {
                        float z=front==0?-.169f:.246f;
                        var foot=V(side*.142f,.055f,z+(front==0?side*.067f:-side*.035f));
                        var knee=V(side*.135f,.267f,z+(front==0?-.05f:.083f));
                        CurvedLimb(body,"Pierna larga "+side+"/"+front,skinColor,V(side*.105f,.516f,z),
                            knee,foot,.074f,.046f,.032f);
                        Ball(body,"Rodilla del dromedario",knee,V(.094f,.097f,.107f),shade);
                        Ball(body,"Pezuña del dromedario",foot+V(0,-.013f,-.031f),V(.11f,.084f,.155f),saddle);
                    }
                    Plate(body,"Manta de viaje",V(side*.15f,.58f,.087f),V(.365f,.287f,.072f),blanket,
                        V(0,side*80,7),"cloth",ClothEdge);
                    var bag=Box(body,"Alforja de cuero",V(side*.213f,.532f,.089f),V(.128f,.198f,.253f),saddle);
                    bag.localRotation=Quaternion.Euler(-9,0,-side*12);
                    Bone(body,"Correa de equipaje",V(side*.264f,.449f,.04f),V(side*.237f,.614f,.036f),.027f,edge);
                    Bone(body,"Borde de manta",V(side*.211f,.448f,-.086f),V(side*.22f,.461f,.235f),.035f,edge);
                }
                Bone(body,"Rollo de caravana",V(-.092f,.746f,.166f),V(.097f,.753f,.197f),.137f,wool);
                Bone(body,"Cinta del rollo",V(.002f,.715f,.127f),V(.002f,.788f,.221f),.041f,saddle);
                var head=G(body,"Idle:Head",V(.021f,.974f,-.31f));
                head.localRotation=Quaternion.Euler(-2,-12,-4);
                Ball(head,"Cabeza del dromedario",V(0,.012f,-.021f),V(.184f,.226f,.213f),skinColor);
                Ball(head,"Hocico sobresaliente",V(0,-.055f,-.111f),V(.165f,.102f,.174f),wool);
                Ball(head,"Labio hendido",V(0,-.091f,-.159f),V(.132f,.062f,.098f),shade);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(head,"Oreja pequeña",V(side*.076f,.116f,.038f),V(.069f,.127f,.076f),skinColor)
                        .localRotation=Quaternion.Euler(-15,0,-side*26);
                    Ball(head,"Ojo del dromedario",V(side*.076f,.039f,-.08f),V(.042f,.046f,.035f),saddle);
                    Bone(head,"Tira de la cabezada",V(side*.077f,.031f,-.05f),V(side*.079f,-.056f,-.135f),.022f,saddle);
                    Bone(body,"Riendas",V(side*.076f,.918f,-.39f),V(side*.14f,.687f,.03f),.018f,saddle);
                }
                var tail=G(body,"Idle:Tail",V(0,.489f,.355f));
                Sweep(tail,"Cola del dromedario",shade,Knot(0,0,0,.046f),Knot(.033f,-.093f,.106f,.03f),
                    Knot(.009f,-.222f,.126f,.028f),Knot(-.021f,-.276f,.146f,.01f));
            }

            void DesertMummy()
            {
                Color wrap=Pigment(0xAE9774), pale=Pigment(0xC7B08B), shadow=Pigment(0x534849), clothColor=Pigment(0x827369);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Torso encorvado",V(0,.562f,-.048f),V(.274f,.382f,.231f),shadow)
                    .localRotation=Quaternion.Euler(-22,0,11);
                Ball(body,"Pecho envuelto",V(0,.664f,-.077f),V(.29f,.219f,.211f),wrap);
                for(int i=0;i<5;i++)
                {
                    var band=Box(body,"Venda transversal",V(-.01f,.444f+i*.066f,-.162f-i*.008f),
                        V(.268f-i*.009f,.045f,.068f),i%2==0?pale:wrap);
                    band.localRotation=Quaternion.Euler(i*2,0,i%2==0?16:-16);
                }
                for(int side=-1;side<=1;side+=2)
                {
                    var foot=V(side*.132f,.072f,side*.097f-.027f);
                    var knee=V(side*.117f,.253f,side*.08f-.029f);
                    var legRoot=V(side*.078f,.442f,.001f);
                    CurvedLimb(body,"Pierna vendada "+side,shadow,legRoot,knee,foot,.059f,.046f,.046f);
                    Ball(body,"Pie con lino",foot+V(0,-.021f,-.062f),V(.113f,.1f,.207f),wrap);
                    DrawLinen(body,legRoot,knee,foot,5,.116f,pale,wrap,side);
                    Plate(body,"Faldón raído",V(side*.08f,.412f,-.112f),V(.162f,.256f,.093f),clothColor,V(9,side*18,side*11),"cloth",ClothEdge);

                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.127f,.679f,-.077f));
                    var elbow=V(side*.052f,-.142f,-.014f);
                    var hand=V(side*.087f,-.264f,-.065f+side*.034f);
                    CurvedLimb(arm,"Brazo de momia "+side,shadow,Vector3.zero,elbow,hand,.052f,.044f,.042f);
                    Ball(arm,"Mano de lino",hand,V(.089f,.111f,.085f),wrap);
                    DrawLinen(arm,Vector3.zero,elbow,hand,5,.099f,pale,wrap,side);
                    Plate(arm,"Venda que cuelga",elbow+V(side*.028f,-.073f,-.073f),V(.075f,.223f,.035f),pale,
                        V(-29,side*14,side*21),"cloth",ClothEdge);
                }
                var head=G(body,"Idle:Head",V(0,.818f,-.108f));
                head.localRotation=Quaternion.Euler(19,-12,7);
                Ball(head,"Cráneo envuelto",Vector3.zero,V(.205f,.263f,.205f),shadow);
                for(int j=0;j<5;j++)
                    Ball(head,"Vendas en la cabeza",V(.002f,-.081f+j*.04f,-.006f),V(.218f-(j==0?.045f:0),.043f,.213f),
                        j%2==0?wrap:pale).localRotation=Quaternion.Euler(-4,0,j%2==0?-11:9);
                Plate(head,"Lino en la coronilla",V(0,.101f,-.005f),V(.158f,.17f,.04f),wrap,V(86,0,15),"cloth",ClothEdge);
                Ball(head,"Cuenca oscura",V(0,.025f,-.104f),V(.18f,.056f,.055f),shadow);
                for(int side=-1;side<=1;side+=2)
                    Ball(head,"Ojo bajo la venda",V(side*.043f,.026f,-.135f),V(.034f,.031f,.02f),Pigment(0xE5C671));
            }

            void DrawLinen(Transform parent, Vector3 first, Vector3 joint, Vector3 last,
                int count, float width, Color light, Color shade, int side)
            {
                for(int i=0;i<count;i++)
                {
                    float t=(i+.65f)/(count+.2f);
                    Vector3 center=t<.5f?Vector3.Lerp(first,joint,t*2):Vector3.Lerp(joint,last,t*2-1);
                    Ball(parent,"Vuelta de lino",center,V(width*(1-t*.15f),.038f,width*(1-t*.12f)),
                        i%2==0?light:shade).localRotation=Quaternion.Euler(side*10,0,side*(i%2==0?22:-22));
                }
            }

            void WastelandCrawler()
            {
                Color rock=Pigment(0x8A6A55), ocher=Pigment(0xB08A64), darkRock=Pigment(0x51443E), horn=Pigment(0x745643);
                var body=G(model,"Idle:Body",Vector3.zero);
                Ball(body,"Vientre articulado",V(0,.31f,.07f),V(.398f,.332f,.581f),darkRock);
                Ball(body,"Caparazón arqueado de roca",V(0,.441f,.073f),V(.477f,.442f,.634f),rock)
                    .localRotation=Quaternion.Euler(-9,0,6);
                for(int i=0;i<4;i++)
                {
                    Vector3 pos=V(i%2==0?-.014f:.024f,.583f-i*.039f,-.115f+i*.136f);
                    P(body,"Estante del caparazón",m.Carved,pos,V(.389f-i*.026f,.174f,.219f),
                        i%2==0?ocher:rock).localRotation=Quaternion.Euler(17+i*7,i%2==0?-10:11,i*3);
                }
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<2;i++)
                    {
                        float z=i==0?-.139f:.22f;
                        var foot=V(side*.285f,.054f,z-.116f+side*.049f);
                        CurvedLimb(body,"Pata baja con garra "+side+"/"+i,darkRock,V(side*.149f,.315f,z),
                            V(side*.309f,.173f,z+.03f),foot,.092f,.061f,.051f);
                        Ball(body,"Placa lateral de pata",V(side*.245f,.232f,z+.018f),V(.18f,.149f,.166f),ocher)
                            .localRotation=Quaternion.Euler(-22,side*25,-side*21);
                        Ball(body,"Garra ancha",foot,V(.146f,.1f,.164f),horn);
                        for(int claw=-1;claw<=1;claw++)
                            Gem(body,"Uña afilada",foot+V(claw*.038f,-.004f,-.093f),V(.047f,.117f,.046f),
                                rock).localRotation=Quaternion.Euler(-67,0,claw*-9);
                    }
                }
                var head=G(body,"Idle:Head",V(0,.281f,-.271f));
                head.localRotation=Quaternion.Euler(-3,7,-6);
                P(head,"Cráneo acorazado bajo",m.Carved,V(0,.04f,.004f),V(.305f,.259f,.247f),rock);
                P(head,"Máscara de corteza de roca",m.Carved,V(0,.051f,-.086f),V(.26f,.156f,.119f),ocher)
                    .localRotation=Quaternion.Euler(16,0,4);
                Ball(head,"Boca bajo caparazón",V(0,-.051f,-.105f),V(.222f,.131f,.149f),darkRock);
                for(int side=-1;side<=1;side+=2)
                {
                    Sweep(head,"Cuerno arqueado "+side,horn,Knot(side*.104f,.076f,-.015f,.064f),
                        Knot(side*.209f,.097f,-.057f,.056f),Knot(side*.25f,.179f,-.106f,.039f),
                        Knot(side*.233f,.268f,-.161f,.025f),Knot(side*.190f,.292f,-.187f,.007f));
                    Sweep(head,"Mandíbula del errante "+side,rock,Knot(side*.083f,-.039f,-.10f,.045f),
                        Knot(side*.119f,-.091f,-.167f,.036f),Knot(side*.094f,-.094f,-.234f,.022f),
                        Knot(side*.041f,-.079f,-.259f,.007f));
                    Ball(head,"Órbita caoba",V(side*.087f,.031f,-.126f),V(.075f,.076f,.046f),darkRock);
                    Ball(head,"Ojo ámbar",V(side*.089f,.037f,-.15f),V(.052f,.043f,.031f),Pigment(0xE7A348));
                    Leaf(head,"Cejas de piedra",V(side*.09f,.062f,-.151f),V(.038f,.103f,.038f),ocher,V(3,0,side*66));
                    Ball(head,"Centro del ojo",V(side*.089f,.033f,-.168f),V(.018f,.027f,.016f),ink);
                }
                var tail=G(body,"Idle:Tail",V(0,.35f,.316f));
                Sweep(tail,"Cola articulada del errante",rock,Knot(0,0,0,.092f),Knot(.095f,-.04f,.114f,.068f),
                    Knot(.166f,-.019f,.211f,.052f),Knot(.224f,.049f,.252f,.032f),Knot(.276f,.099f,.233f,.009f));
            }

            void PhoenixChick()
            {
                Color vermilion=Pigment(0xC7432D), red=Pigment(0xE96529), orange=Pigment(0xFA9930), gold=Pigment(0xFFE082);
                var body=G(model,"Idle:Body",V(0,.11f,0));
                Ball(body,"Cuerpo de fénix",V(0,.46f,.026f),V(.286f,.346f,.299f),red)
                    .localRotation=Quaternion.Euler(-13,0,4);
                Ball(body,"Pecho dorado",V(0,.437f,-.087f),V(.227f,.282f,.171f),orange);
                var head=G(body,"Idle:Head",V(.027f,.673f,-.093f));
                head.localRotation=Quaternion.Euler(-7,14,-6);
                Ball(head,"Cabeza del polluelo",Vector3.zero,V(.265f,.244f,.236f),red);
                Ball(head,"Mejillas cálidas",V(0,-.046f,-.097f),V(.219f,.131f,.115f),orange);
                Gem(head,"Pico dorado",V(0,-.044f,-.145f),V(.109f,.103f,.138f),gold).localRotation=Quaternion.Euler(-69,0,0);
                MiniatureEyes(head,.071f,.037f,-.099f,.076f,Pigment(0x77412E),Pigment(0xFEE1A7),5);
                for(int i=-1;i<=1;i++)
                    Plate(head,"Penacho curvo",V(i*.058f,.133f,.044f),V(.081f,i==0?.265f:.203f,.071f),
                        i==0?gold:vermilion,V(-34,12,-i*31),"feather",FeatherEdge);
                for(int side=-1;side<=1;side+=2)
                {
                    var wing=G(body,side<0?"Idle:WingL":"Idle:WingR",V(side*.098f,.537f,.014f));
                    wing.localRotation=Quaternion.Euler(-5,side*-9,side*30);
                    Ball(wing,"Base de ala",V(side*.108f,.053f,.018f),V(.27f,.161f,.152f),vermilion);
                    for(int i=0;i<6;i++)
                    {
                        var p=V(side*(.092f+i*.050f),.076f+i*.015f,.025f+i*.021f);
                        var r=V(-8,i*6*side,-side*(36+i*7));
                        Plate(wing,"Remera curvada",p,V(.102f,.305f-i*.017f,.056f),i<3?vermilion:red,r,"feather",FeatherEdge);
                        Plate(wing,"Borde incandescente",p+V(side*.033f,.028f,-.031f),V(.079f,.249f-i*.016f,.036f),
                            i%2==0?orange:gold,r,"feather",FeatherEdge);
                    }
                    CurvedLimb(body,"Pata con garras "+side,Pigment(0xA95B34),V(side*.068f,.349f,-.025f),V(side*.079f,.289f,.003f),
                        V(side*.094f,.264f,-.038f),.033f,.025f,.022f);
                    for(int claw=-1;claw<=1;claw++)
                        Gem(body,"Garra del fénix",V(side*.091f+claw*.024f,.237f,-.058f),V(.041f,.067f,.032f),
                            gold).localRotation=Quaternion.Euler(-53,claw*14,0);
                }
                var tail=G(body,"Idle:Tail",V(0,.374f,.13f));
                Sweep(tail,"Raíz de plumas caudales",red,Knot(0,0,0,.073f),Knot(.022f,-.063f,.08f,.064f),
                    Knot(.073f,-.109f,.139f,.054f),Knot(.104f,-.145f,.173f,.016f));
                for(int i=-2;i<=2;i++)
                {
                    float spread=i*.065f;
                    Plate(tail,"Cascada de pluma",V(spread,-.127f,.153f+Mathf.Abs(i)*.022f),
                        V(.107f,.361f-Mathf.Abs(i)*.039f,.074f),i==0?gold:i%2==0?vermilion:red,V(-44,20,-i*27),"feather",FeatherEdge);
                }
            }
        }
    }
}

