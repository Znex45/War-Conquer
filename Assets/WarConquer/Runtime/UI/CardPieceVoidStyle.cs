using System.Linq;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Original voidborn silhouettes: living chitin, connective tissue and mineral symbiosis.
            // Constructed before presentation captures its idle joints; no game data is touched.
            readonly Color vxShell=new Color(.105f,.065f,.18f);
            readonly Color vxPlate=new Color(.285f,.13f,.415f);
            readonly Color vxRidge=new Color(.49f,.265f,.63f);
            readonly Color vxTissue=new Color(.64f,.13f,.39f);
            readonly Color vxLight=new Color(.23f,.81f,.93f);
            readonly Color vxTooth=new Color(.81f,.73f,.81f);

            void ApplyVoidStyle()
            {
                if(!new[]{"crystal-minion","dark-fly","the-eternal-eye","void-hole","sprinting-sprite","void-beacon","portal","insolator"}.Contains(id))return;
                // Preserve the sculpture root, so arrival, movement and combat continue to share it.
                foreach(var child in model.Cast<Transform>().ToArray())BoardMeshFactory.Release(child.gameObject);
                switch(id)
                {
                    case "crystal-minion":VXCrystalHunter();break;
                    case "dark-fly":VXFly();break;
                    case "the-eternal-eye":VXEye();break;
                    case "void-hole":VXMaw();break;
                    case "sprinting-sprite":VXSprinter();break;
                    case "void-beacon":VXBeacon();break;
                    case "portal":VXPortal();break;
                    case "insolator":VXCage();break;
                }
            }
            void VXCurve(Transform p,string name,Color color,float width,params Vector3[] points)
            {
                for(int j=1;j<points.Length;j++)Flesh(p,name,points[j-1],points[j],width*(1-(j-1)*.14f),color);
                for(int j=1;j<points.Length-1;j++)Ball(p,name+" · articulación",points[j],Vector3.one*width*.75f,color);
            }
            void VXClaw(Transform p,string name,Vector3 a,Vector3 bend,Vector3 tip,float width)
            {
                VXCurve(p,name,vxPlate,width,a,bend);
                Flesh(p,name+" · filo",bend,tip,width*.58f,vxTooth);
                Ball(p,name+" · bisagra",bend,Vector3.one*width*.86f,vxRidge);
            }
            void VXFace(Transform head,bool elongated=false)
            {
                Ball(head,"Cráneo de quitina",Vector3.zero,new Vector3(.245f,.22f,elongated?.29f:.23f),vxShell);
                Ball(head,"Placa frontal del depredador",new Vector3(0,.06f,-.03f),new Vector3(.255f,.13f,.2f),vxPlate);
                for(int side=-1;side<=1;side+=2)
                {
                    var eye=Leaf(head,"Ojo oblicuo del vacío",new Vector3(side*.068f,.015f,-.105f),new Vector3(.045f,.088f,.022f),vxLight,new Vector3(0,0,-side*62));
                    VXClaw(head,"Mandíbula prensil",new Vector3(side*.085f,-.055f,-.045f),new Vector3(side*.11f,-.1f,-.14f),new Vector3(side*.045f,-.1f,-.19f),.047f);
                }
            }
            void VXCrystalHunter()
            {
                var body=G(model,"Idle:Body",new Vector3(0,.36f,0));
                Ball(body,"Tórax blindado del esbirro",Vector3.zero,new Vector3(.4f,.4f,.34f),vxShell);
                Gem(body,"Cristal simbiótico del pecho",new Vector3(0,.015f,-.14f),new Vector3(.23f,.34f,.15f),vxLight);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(body,"Costado de quitina",new Vector3(side*.15f,.02f,.025f),new Vector3(.17f,.36f,.32f),vxPlate).localRotation=Quaternion.Euler(0,0,-side*12);
                    for(int j=0;j<2;j++)
                    {
                        var knee=new Vector3(side*.27f,-.105f,j==0?-.18f:.19f);
                        var foot=new Vector3(side*.31f,-.32f,j==0?-.24f:.27f);
                        VXCurve(body,"Pata segmentada del esbirro",vxPlate,.09f,new Vector3(side*.14f,-.055f,j==0?-.08f:.12f),knee,foot);
                        Ball(body,"Apoyo de garra",foot,new Vector3(.095f,.085f,.14f),vxShell);
                    }
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",new Vector3(side*.18f,.09f,-.025f));
                    VXClaw(arm,"Guadaña del esbirro",Vector3.zero,new Vector3(side*.16f,-.07f,-.05f),new Vector3(side*.1f,-.2f,-.19f),.105f);
                    Gem(body,"Espina escapular",new Vector3(side*.14f,.19f,.06f),new Vector3(.105f,.3f,.13f),vxRidge).localRotation=Quaternion.Euler(12,0,-side*25);
                }
                VXFace(G(body,"Idle:Head",new Vector3(0,.245f,-.045f)));
            }
            void VXFly()
            {
                var body=G(model,"Idle:Body",new Vector3(0,.58f,0));
                Ball(body,"Abdomen de mosca abisal",new Vector3(0,0,.14f),new Vector3(.32f,.25f,.4f),vxPlate);
                Ball(body,"Tórax articulado de mosca",new Vector3(0,.02f,-.11f),new Vector3(.25f,.23f,.27f),vxShell);
                Ball(body,"Saco bioluminiscente",new Vector3(0,-.015f,.29f),new Vector3(.21f,.15f,.16f),vxTissue);
                for(int side=-1;side<=1;side+=2)
                {
                    for(int j=0;j<3;j++)VXCurve(body,"Pata colgante de mosca",vxRidge,.034f,new Vector3(side*.09f,-.025f,-.12f+j*.11f),new Vector3(side*.24f,-.09f,-.18f+j*.15f),new Vector3(side*.3f,-.24f,-.2f+j*.17f));
                    var wing=G(body,side<0?"Idle:WingL":"Idle:WingR",new Vector3(side*.09f,.1f,-.06f));
                    Leaf(wing,"Membrana alar abisal",new Vector3(side*.15f,0,.07f),new Vector3(.19f,.57f,.027f),vxTissue,new Vector3(90,0,side*64));
                    Bone(wing,"Nervadura de ala",Vector3.zero,new Vector3(side*.33f,.01f,.18f),.022f,vxRidge);
                    Ball(body,"Élitro orgánico",new Vector3(side*.105f,.115f,.09f),new Vector3(.14f,.13f,.29f),vxShell);
                }
                var head=G(body,"Idle:Head",new Vector3(0,.02f,-.26f));VXFace(head,true);head.localScale=Vector3.one*.8f;
            }
            void VXEye()
            {
                var head=G(model,"Idle:Head",new Vector3(0,.67f,0));
                Ball(head,"Esclera del ojo eterno",Vector3.zero,new Vector3(.5f,.4f,.38f),new Color(.77f,.62f,.76f));
                Ball(head,"Iris del ojo eterno",new Vector3(0,0,-.182f),new Vector3(.27f,.29f,.065f),vxLight);
                Gem(head,"Pupila hendida",new Vector3(0,0,-.217f),new Vector3(.055f,.23f,.025f),ink);
                for(int side=-1;side<=1;side+=2)
                {
                    VXCurve(head,"Párpado de quitina",vxPlate,.075f,new Vector3(side*.23f,0,-.095f),new Vector3(side*.15f,.16f,-.12f),new Vector3(0,.19f,-.12f));
                    VXCurve(head,"Mandíbula del ojo",vxShell,.067f,new Vector3(side*.23f,0,-.095f),new Vector3(side*.16f,-.16f,-.12f),new Vector3(0,-.18f,-.115f));
                    VXClaw(head,"Cuerno orbital",new Vector3(side*.16f,.13f,.01f),new Vector3(side*.29f,.28f,.03f),new Vector3(side*.21f,.37f,-.055f),.075f);
                }
                for(int j=-1;j<=1;j++)VXCurve(head,"Zarcillo sensorial",vxTissue,.054f,new Vector3(j*.12f,-.12f,.06f),new Vector3(j*.16f,-.31f,.1f),new Vector3(j*.1f,-.43f,.17f));
            }
            void VXMaw()
            {
                var body=G(model,"Idle:Body",Vector3.zero);
                P(body,"Garganta del vacío",m.Cylinder,Vector3.zero,new Vector3(.58f,.055f,.58f),ink);
                P(body,"Remolino del abismo",m.Vortex,Vector3.up*.074f,Vector3.one*.42f,vxLight);
                for(int j=0;j<8;j++)
                {
                    var p=Rad(j,8,.275f,.095f);
                    var plate=Ball(body,"Labio acorazado del agujero",p,new Vector3(.23f,.14f,.17f),j%2==0?vxPlate:vxShell);plate.localRotation=Quaternion.Euler(0,-j*45,0);
                    VXClaw(body,"Diente del horizonte",Rad(j,8,.28f,.1f),Rad(j,8,.27f,.26f),Rad(j,8,.19f,.31f),.052f);
                }
            }
            void VXSprinter()
            {
                var body=G(model,"Idle:Body",new Vector3(0,.47f,0));
                Ball(body,"Tórax esbelto del corredor",Vector3.zero,new Vector3(.255f,.37f,.24f),vxShell);
                Ball(body,"Quilla pectoral",new Vector3(0,-.01f,-.095f),new Vector3(.15f,.27f,.09f),vxTissue);
                for(int side=-1;side<=1;side+=2)
                {
                    var knee=new Vector3(side*.13f,-.22f,side<0?.13f:-.11f);var foot=new Vector3(side*.16f,-.42f,side<0?.24f:-.2f);
                    VXCurve(body,"Pata digitígrada del corredor",vxPlate,.074f,new Vector3(side*.075f,-.11f,0),knee,foot);
                    Ball(body,"Garra de apoyo del corredor",foot,new Vector3(.09f,.1f,.17f),vxShell);
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",new Vector3(side*.1f,.095f,-.015f));
                    VXClaw(arm,"Guadaña del corredor",Vector3.zero,new Vector3(side*.15f,-.04f,-side*.06f),new Vector3(side*.12f,-.17f,-side*.15f),.078f);
                    Leaf(body,"Aleta escapular del corredor",new Vector3(side*.09f,.15f,.065f),new Vector3(.12f,.3f,.08f),vxPlate,new Vector3(-30,0,side*27));
                }
                VXFace(G(body,"Idle:Head",new Vector3(0,.23f,-.07f)),true);
                var tail=G(body,"Idle:Tail",new Vector3(0,-.055f,.09f));
                VXCurve(tail,"Cola de equilibrio",vxPlate,.065f,Vector3.zero,new Vector3(.045f,.035f,.18f),new Vector3(.1f,.14f,.3f));
            }
            void VXRootBed()
            {
                Ball(model,"Raíz basal del organismo",new Vector3(0,.065f,0),new Vector3(.66f,.15f,.62f),vxShell);
                for(int j=0;j<3;j++)VXCurve(model,"Raíz de quitina",vxPlate,.09f,Rad(j,3,.3f,.035f),Rad(j,3,.17f,.14f),new Vector3(0,.2f,0));
            }
            void VXBeacon()
            {
                VXRootBed();
                Ball(model,"Columna vertebral del faro",new Vector3(0,.52f,0),new Vector3(.26f,.88f,.25f),vxShell);
                Ball(model,"Órgano de señal del faro",new Vector3(0,.98f,0),new Vector3(.32f,.4f,.3f),vxLight);
                for(int j=0;j<3;j++)
                {
                    VXCurve(model,"Contrafuerte orgánico del faro",vxPlate,.105f,Rad(j,3,.29f,.06f),Rad(j,3,.2f,.48f),Rad(j,3,.12f,.86f));
                    var p=Rad(j,3,.13f,1.05f);
                    VXClaw(model,"Corona de quitina del faro",p,Rad(j,3,.22f,1.26f),Rad(j,3,.1f,1.32f),.08f);
                }
                for(int j=0;j<3;j++)Ball(model,"Vesícula de energía",new Vector3(0,.3f+j*.19f,-.115f),new Vector3(.095f,.13f,.075f),vxTissue);
            }
            void VXPortal()
            {
                VXRootBed();
                Vector3 Point(float angle){float a=angle*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(a)*.335f,.59f+Mathf.Sin(a)*.47f,0);}
                for(int j=0;j<12;j++)VXCurve(model,"Arco de carapacho",j%2==0?vxPlate:vxShell,.1f,Point(j*30),Point(j*30+15),Point(j*30+30));
                var veil=P(model,"Membrana del umbral",m.Cylinder,new Vector3(0,.59f,.025f),new Vector3(.51f,.025f,.76f),new Color(.075f,.025f,.105f));veil.localRotation=Quaternion.Euler(90,0,0);
                var vortex=P(model,"Espiral del umbral vivo",m.Vortex,new Vector3(0,.59f,-.015f),new Vector3(.38f,.44f,.38f),vxLight);vortex.localRotation=Quaternion.Euler(-90,0,0);
                for(int side=-1;side<=1;side+=2)
                {
                    VXClaw(model,"Diente del arco",new Vector3(side*.31f,.7f,-.025f),new Vector3(side*.24f,.72f,-.07f),new Vector3(side*.19f,.6f,-.075f),.062f);
                    VXClaw(model,"Espolón del umbral",new Vector3(side*.23f,.92f,0),new Vector3(side*.31f,1.12f,.015f),new Vector3(side*.2f,1.2f,-.025f),.08f);
                }
            }
            void VXCage()
            {
                VXRootBed();
                Ball(model,"Saco confinado de aislamiento",new Vector3(0,.51f,0),new Vector3(.29f,.46f,.27f),vxLight);
                for(int j=0;j<4;j++)
                {
                    VXCurve(model,"Costilla de jaula viva",vxPlate,.075f,Rad(j,4,.27f,.08f),Rad(j,4,.29f,.52f),Rad(j,4,.2f,.93f),new Vector3(0,1.09f,0));
                    VXCurve(model,"Tendón de contención",vxTissue,.042f,Rad(j,4,.22f,.11f),Rad(j,4,.1f,.34f),new Vector3(0,.41f,0));
                }
                Ball(model,"Nudo de cierre de la jaula",new Vector3(0,1.08f,0),new Vector3(.17f,.19f,.17f),vxShell);
                Gem(model,"Sello vivo del aislamiento",new Vector3(0,.53f,-.133f),new Vector3(.1f,.18f,.055f),vxTissue);
            }
        }
    }
}
