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
        }
    }
}

