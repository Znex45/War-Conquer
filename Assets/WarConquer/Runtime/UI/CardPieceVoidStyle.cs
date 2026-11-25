using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Palette and silhouettes are taken from the eight Void illustrations.
            bool SculptVoidArt()
            {
                switch (id)
                {
                    case "crystal-minion": VXCrystalHunter(); break;
                    case "dark-fly": VXFly(); break;
                    case "the-eternal-eye": VXEye(); break;
                    case "void-hole": VXMaw(); break;
                    case "sprinting-sprite": VXSprinter(); break;
                    case "void-beacon": VXBeacon(); break;
                    case "portal": VXPortal(); break;
                    case "insolator": VXCage(); break;
                    default: return false;
                }
                return true;
            }

            void VXCrystalHunter()
            {
                Color darkSmoke=Pigment(0x252035), smoke=Pigment(0x423054), violet=Pigment(0x8D4FCB);
                var body=G(model,"Idle:Body",V(0,.478f,.016f));
                Ball(body,"Núcleo de sombra",Vector3.zero,V(.48f,.456f,.438f),darkSmoke);
                for(int i=0;i<8;i++)
                {
                    float angle=i*Mathf.PI/4f;
                    var curl=Ball(body,"Nube enroscada",V(Mathf.Sin(angle)*.184f,Mathf.Cos(angle)*.181f,.042f),
                        V(.152f,.127f,.285f),i%2==0?smoke:darkSmoke);
                    curl.localRotation=Quaternion.Euler(17+5*i,29,-i*45+18);
                    if(i%2==0)
                    {
                        float side=Mathf.Sin(angle+.43f);
                        Sweep(body,"Relieve en curva "+i,smoke,Knot(side*.152f,Mathf.Cos(angle+.43f)*.189f,-.12f,.024f),
                            Knot(Mathf.Sin(angle+.14f)*.224f,Mathf.Cos(angle+.14f)*.209f,-.086f,.029f),
                            Knot(Mathf.Sin(angle-.19f)*.197f,Mathf.Cos(angle-.19f)*.166f,-.13f,.014f));
                    }
                }
                var face=G(body,"Idle:Head",V(0,.052f,-.213f));
                face.localRotation=Quaternion.Euler(22,6,-5);
                MiniatureEyes(face,.092f,.045f,-.013f,.116f,Pigment(0xBE66BE),Pigment(0xF39ED7),4);
                for(int i=-1;i<=1;i++)
                {
                    var pos=V(i*.121f,.286f-Mathf.Abs(i)*.047f,-.03f+.038f*Mathf.Abs(i));
                    var crown=Gem(body,"Cuarzo del penacho",pos,V(.139f,i==0?.327f:.233f,.151f),
                        i==0?Pigment(0xB97BDE):violet);
                    crown.localRotation=Quaternion.Euler(-22,0,-i*31);
                    Gem(crown,"Cara iluminada del cuarzo",V(-.11f,.081f,-.25f),V(.38f,.62f,.21f),Pigment(0xD99DF1));
                }
                var tail=G(body,"Idle:Tail",V(0,-.177f,.055f));
                Sweep(tail,"Vapor principal curvado",darkSmoke,Knot(0,0,0,.088f),Knot(.032f,-.062f,.037f,.071f),
                    Knot(.107f,-.114f,.067f,.057f),Knot(.126f,-.177f,.064f,.036f),Knot(.032f,-.228f,.008f,.022f),
                    Knot(-.047f,-.213f,-.028f,.009f));
                Sweep(tail,"Rizo de sombra lateral",smoke,Knot(-.074f,.012f,.006f,.046f),
                    Knot(-.147f,-.039f,.053f,.035f),Knot(-.177f,-.1f,.041f,.024f),Knot(-.121f,-.141f,.015f,.008f));
            }

            void VXFly()
            {
                Color shell=Pigment(0x211E34), joint=Pigment(0x413651), violet=Pigment(0xA058CC), pale=Pigment(0xD38AE0);
                var body=G(model,"Idle:Body",V(0,.523f,0));
                Ball(body,"Abdomen cónico",V(0,-.021f,.147f),V(.229f,.203f,.405f),shell).localRotation=Quaternion.Euler(13,0,0);
                for(int i=0;i<3;i++)
                    Ball(body,"Segmento quitinoso",V(0,.059f-i*.028f,.127f+i*.069f),
                        V(.207f-i*.039f,.123f-i*.019f,.156f),i%2==0?joint:shell).localRotation=Quaternion.Euler(18+i*5,0,0);
                Ball(body,"Tórax abisal",V(0,.029f,-.081f),V(.245f,.243f,.29f),joint);
                var head=G(body,"Idle:Head",V(0,.013f,-.235f));
                Ball(head,"Cabeza de mosca",Vector3.zero,V(.232f,.211f,.205f),shell);
                for(int side=-1;side<=1;side+=2)
                {
                    var eye=Ball(head,"Ojo compuesto",V(side*.091f,.031f,-.061f),V(.145f,.168f,.132f),violet);
                    eye.localRotation=Quaternion.Euler(13,side*21,side*9);
                    Ball(head,"Reflejo violeta",V(side*.113f,.062f,-.11f),V(.051f,.061f,.027f),pale);
                    Ball(head,"Faceta inferior",V(side*.117f,-.023f,-.091f),V(.042f,.043f,.023f),Pigment(0x6E3B96));
                    Sweep(head,"Mandíbula de mosca "+side,shell,Knot(side*.061f,-.047f,-.071f,.032f),
                        Knot(side*.079f,-.099f,-.115f,.024f),Knot(side*.059f,-.151f,-.162f,.011f));
                    for(int i=0;i<3;i++)
                    {
                        float z=-.13f+i*.111f;
                        CurvedLimb(body,"Pata de mosca oscura "+side+"/"+i,joint,V(side*.074f,-.02f,z),
                            V(side*.235f,-.074f,z+(i-1)*.066f),V(side*.291f,-.216f,z+.018f*(i-1)),.026f,.018f,.008f);
                    }
                    var wing=G(body,side<0?"Idle:WingL":"Idle:WingR",V(side*.073f,.092f,-.041f));
                    for(int pair=1;pair>=0;pair--)
                    {
                        var end=V(side*(.395f-pair*.059f),.259f-pair*.183f,.208f+pair*.130f);
                        var vane=Plate(wing,"Membrana de ala",end*.50f,V(pair==0?.198f:.167f,end.magnitude*1.08f,.041f),
                            pair==0?violet:Pigment(0x794DA9),Vector3.zero,"wing",WingEdge);
                        // The patterned front faces the elevated camera. The fixed leading
                        // edge remains aligned with the membrane's idle pivot.
                        vane.localRotation=Quaternion.LookRotation(V(-side*.13f,-.36f,.94f),end);
                        Bone(vane,"Vena central",V(0,-.438f,-.55f),V(.02f,.398f,-.59f),.052f,shell);
                        for(int rib=0;rib<2;rib++)
                            for(int wingSide=-1;wingSide<=1;wingSide+=2)
                            {
                                float y=-.16f+rib*.216f;
                                Bone(vane,"Nervio de ala",V(.01f,y,-.55f),V(wingSide*(.29f+rib*.042f),y+.21f,-.54f),
                                    .035f,rib==0?shell:pale);
                            }
                    }
                }
            }

            void VXEye()
            {
                Color rock=Pigment(0x3A344D), rim=Pigment(0x594877), crystal=Pigment(0xA581CD), iridescent=Pigment(0xD3ACEA);
                var ocular=G(model,"Idle:Head",V(0,.563f,.01f));
                ocular.localRotation=Quaternion.Euler(14,-11,7);
                Ball(ocular,"Núcleo mineral",Vector3.zero,V(.55f,.539f,.301f),rock);
                for(int i=0;i<10;i++)
                {
                    float a=2*Mathf.PI*i/10;
                    Vector3 p=V(Mathf.Sin(a)*.224f,Mathf.Cos(a)*.261f,-.016f+.02f*(i%3));
                    var rockShard=P(ocular,"Placa mineral superpuesta",m.Carved,p,V(.203f,.27f-i%3*.027f,.204f),
                        i%3==0?rock:rim);
                    rockShard.localRotation=Quaternion.Euler(-8+7*i,19*(i%2),-a*Mathf.Rad2Deg-i%3*15);
                }
                Ball(ocular,"Cuenca del ojo",V(0,0,-.151f),V(.433f,.317f,.171f),Pigment(0x302137));
                Ball(ocular,"Globo violeta",V(0,.008f,-.204f),V(.361f,.248f,.166f),crystal);
                Ball(ocular,"Iris amatista",V(0,.008f,-.273f),V(.185f,.218f,.093f),Pigment(0x9B67C5));
                Plate(ocular,"Pupila vertical",V(0,.006f,-.323f),V(.071f,.206f,.032f),ink,Vector3.zero,"leaf",LeafEdge);
                Ball(ocular,"Reflejo del iris",V(-.043f,.063f,-.316f),V(.051f,.045f,.024f),iridescent);
                for(int side=-1;side<=1;side+=2)
                {
                    Sweep(ocular,"Bisel pétreo superior "+side,rim,Knot(side*.229f,.008f,-.107f,.051f),
                        Knot(side*.148f,.119f,-.182f,.049f),Knot(side*.071f,.151f,-.184f,.044f),Knot(0,.155f,-.166f,.033f));
                    Sweep(ocular,"Bisel pétreo inferior "+side,rock,Knot(side*.229f,.008f,-.108f,.047f),
                        Knot(side*.165f,-.084f,-.199f,.047f),Knot(side*.069f,-.132f,-.194f,.038f),Knot(0,-.141f,-.168f,.027f));
                    Gem(ocular,"Amatista lateral",V(side*.192f,.238f,-.064f),V(.113f,.195f,.148f),crystal)
                        .localRotation=Quaternion.Euler(10,0,-side*36);
                    Gem(ocular,"Piedra oscura suspendida",V(side*.356f,-.103f,.023f),V(.083f,.138f,.101f),rim)
                        .localRotation=Quaternion.Euler(15,0,-side*31);
                }
                Gem(ocular,"Amatista central",V(0,.291f,-.115f),V(.141f,.264f,.138f),crystal);
                Gem(ocular,"Punta mineral inferior",V(0,-.308f,-.035f),V(.136f,.26f,.156f),rock);
            }

            void VXMaw()
            {
                Color dark=Pigment(0x211933), rock=Pigment(0x584C65), violet=Pigment(0x6F3A9B);
                var body=G(model,"Idle:Body",Vector3.zero);
                Stem(body,"Sombra del cráter",V(0,.012f,0),.63f,.027f,dark);
                Mesh funnel=m.UnitMesh(id+"/cuenco",AbyssBowl);
                P(body,"Cuenco profundo",funnel,Vector3.zero,Vector3.one,Pigment(0x302044));
                for(int i=0;i<9;i++)
                {
                    float a=i*Mathf.PI*2/9;
                    var shard=P(body,"Loseta fracturada",m.Carved,V(Mathf.Cos(a)*.304f,.057f+i%3*.021f,Mathf.Sin(a)*.304f),
                        V(.190f,.115f+i%3*.041f,.193f),i%2==0?rock:Pigment(0x4A4259));
                    shard.localRotation=Quaternion.Euler(13-i%3*11,-i*40,13+i%3*14);
                    var edge=V(Mathf.Cos(a)*.268f,.104f+i%3*.014f,Mathf.Sin(a)*.268f);
                    var outer=V(Mathf.Cos(a+.25f)*.339f,.082f,Mathf.Sin(a+.25f)*.339f);
                    Sweep(body,"Fisura radial "+i,Pigment(0x915CC3),
                        Knot(edge.x,edge.y,edge.z,.012f),Knot((edge.x+outer.x)*.5f,.119f,(edge.z+outer.z)*.5f,.016f),
                        Knot(outer.x,outer.y,outer.z,.007f));
                }
                for(int i=0;i<4;i++)
                {
                    const int samples=13;
                    var curve=new Vector4[samples];
                    for(int j=0;j<samples;j++)
                    {
                        float t=j/(float)(samples-1);
                        float angle=i*Mathf.PI*.5f+t*5.75f;
                        float r=Mathf.Lerp(.315f,.026f,t*t);
                        float y=.063f+.108f*Mathf.Sin(t*Mathf.PI*.92f)-t*.057f+(i%2)*.019f;
                        float radius=(.023f+(i%2)*.007f)*Mathf.Sin(Mathf.PI*t)*(.98f-t*.30f)+.004f;
                        curve[j]=Knot(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r,radius);
                    }
                    Sweep(body,"Brazo de remolino "+i,i%2==0?violet:Pigment(0xB27AE2),curve);
                }
            }

            static Mesh AbyssBowl()
            {
                const int sides=16, rings=4;
                float[] radii={.018f,.108f,.216f,.309f};
                float[] depth={.045f,.051f,.085f,.129f};
                var vertices=new Vector3[sides*rings+1];
                var indices=new List<int>(sides*18);
                vertices[sides*rings]=V(0,.045f,0);
                for(int ring=0;ring<rings;ring++)
                    for(int i=0;i<sides;i++)
                    {
                        float angle=2*Mathf.PI*i/sides;
                        int current=ring*sides+i, next=ring*sides+(i+1)%sides;
                        vertices[current]=V(Mathf.Cos(angle)*radii[ring],depth[ring],Mathf.Sin(angle)*radii[ring]);
                        if(ring==0) indices.AddRange(new[]{sides*rings,next,current});
                        if(ring<rings-1) indices.AddRange(new[]{current,next,current+sides,next,next+sides,current+sides});
                    }
                return Geometry("Cuenco del Vacío",vertices,indices);
            }

            void VXSprinter()
            {
                Color shadow=Pigment(0x30233F), purple=Pigment(0x634091), violet=Pigment(0x9659CE), gold=Pigment(0xE9B965);
                var body=G(model,"Idle:Body",V(0,.421f,0));
                body.localRotation=Quaternion.Euler(-9,-18,-7);
                Ball(body,"Torso de sombra en carrera",V(0,.064f,-.031f),V(.236f,.306f,.268f),shadow)
                    .localRotation=Quaternion.Euler(-52,0,-23);
                Ball(body,"Pelvis de sombra",V(0,-.013f,.078f),V(.198f,.194f,.196f),shadow);
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 knee=side<0?V(-.114f,-.154f,-.053f):V(.111f,-.116f,.199f);
                    Vector3 foot=side<0?V(-.091f,-.249f,-.185f):V(.162f,-.278f,.3f);
                    CurvedLimb(body,"Pierna en zancada "+side,shadow,V(side*.067f,-.021f,.049f),
                        knee,foot,.083f,.057f,.036f);
                    Plate(body,"Punta del pie",foot+V(0,.007f,.006f),V(.073f,.109f,.059f),purple,
                        V(side<0?-53:62,0,side*11),"leaf",LeafEdge);
                    var arm=G(body,side<0?"Idle:ArmL":"Idle:ArmR",V(side*.092f,.106f,-.1f));
                    Vector3 elbow=side<0?V(-.141f,-.027f,-.052f):V(.099f,-.035f,.136f);
                    Vector3 hand=side<0?V(-.076f,-.008f,-.174f):V(.138f,-.119f,.170f);
                    CurvedLimb(arm,"Brazo en zancada "+side,shadow,Vector3.zero,elbow,hand,.062f,.048f,.039f);
                    Ball(arm,"Puño del corredor",hand,V(.083f,.084f,.089f),purple);
                    float sweep = side<0?1:.81f;
                    Sweep(body,"Estela externa "+side,purple,Knot(side*.096f,.132f,.009f,.053f),
                        Knot(side*.15f,.167f,.119f,.052f),Knot(side*.206f,.148f,.241f*sweep,.033f),
                        Knot(side*.263f,.176f,.315f*sweep,.023f),Knot(side*.247f,.199f,.396f*sweep,.013f),
                        Knot(side*.287f,.173f,.459f*sweep,.004f));
                    Sweep(body,"Estela luminosa "+side,side<0?gold:violet,Knot(side*.091f,.179f,-.047f,.019f),
                        Knot(side*.137f,.2f,.126f,.023f),Knot(side*.19f,.177f,.227f*sweep,.019f),
                        Knot(side*.244f,.211f,.363f*sweep,.011f),Knot(side*.28f,.182f,.477f*sweep,.004f));
                }
                var head=G(body,"Idle:Head",V(0,.197f,-.159f));
                head.localRotation=Quaternion.Euler(12,-5,8);
                P(head,"Cráneo angular",m.Carved,Vector3.zero,V(.234f,.218f,.237f),shadow)
                    .localRotation=Quaternion.Euler(-21,0,-7);
                Plate(head,"Arista de frente",V(0,.066f,-.04f),V(.199f,.097f,.126f),purple,V(-17,0,-5),"shield",ShieldEdge);
                for(int side=-1;side<=1;side+=2)
                    Plate(head,"Ojo inclinado de luz",V(side*.063f,.013f,-.101f),V(.037f,.085f,.038f),
                        Pigment(0xDBA8F1),V(0,side*16,-side*67),"leaf",LeafEdge);
                Sweep(body,"Hilo bajo de energía",gold,Knot(-.13f,-.13f,-.141f,.014f),
                    Knot(-.158f,-.163f,.091f,.028f),Knot(-.131f,-.082f,.276f,.017f),Knot(-.041f,-.038f,.4f,.006f));
            }

            void VXRootBed()
            {
                P(model,"Roca violeta tallada",m.Carved,V(0,.083f,0),V(.742f,.185f,.701f),Pigment(0x4D405F));
                for(int i=0;i<5;i++)Box(model,"Losa facetada",Rad(i,5,.269f,.107f),V(.181f,.097f,.207f),Pigment(i%2==0?0x625175:0x40364E)).localRotation=Quaternion.Euler(0,i*72,0);
            }

            void VXBeacon()
            {
                VXRootBed();
                Color rock=Pigment(0x42374F), pale=Pigment(0x74648C), violet=Pigment(0xA044D1);
                Stem(model,"Cuenco escalonado",V(0,.13f,0),.465f,.113f,rock);
                Stem(model,"Borde tallado",V(0,.221f,0),.447f,.047f,pale);
                for(int i=0;i<7;i++)
                {
                    var foot=Rad(i,7,.244f,.181f);
                    var tip=Rad(i,7,.294f,.49f+(i%3)*.156f);
                    var shard=Gem(model,"Prisma irregular exterior",(foot+tip)*.5f,V(.142f,(tip-foot).magnitude+.088f,.136f),i%2==0?rock:pale);
                    shard.localRotation=Quaternion.FromToRotation(Vector3.up,tip-foot);
                }
                Leaf(model,"Llama violeta del faro",V(.006f,.55f,0),V(.302f,.614f,.239f),violet,V(0,0,-15));
                Leaf(model,"Centro púrpura brillante",V(.013f,.439f,-.093f),V(.177f,.369f,.086f),Pigment(0xE68AF2),V(15,0,17));
                Leaf(model,"Punta de llama",V(-.09f,.756f,.032f),V(.139f,.411f,.094f),Pigment(0x7D3AB6),V(-16,0,-18));
            }

            void VXPortal()
            {
                VXRootBed();
                Color rock=Pigment(0x514162), bevel=Pigment(0x746183), glowColor=Pigment(0xC887EF);
                Vector3 Arch(float angle,float z=0) => V(Mathf.Cos(angle)*.322f,.588f+Mathf.Sin(angle)*.443f,z);
                for(int i=0;i<14;i++)
                {
                    float a=i*Mathf.PI/7f, b=(i+1)*Mathf.PI/7f;
                    Flesh(model,"Dovela de obsidiana",Arch(a),Arch(b),.118f,i%3==0?bevel:rock);
                    Bone(model,"Filete del arco",Arch(a,-.062f),Arch(b,-.062f),.026f,glowColor);
                    if(i%3==0)Gem(model,"Glifo en dovela",Arch((a+b)*.5f,-.085f),V(.055f,.10f,.046f),bevel).localRotation=Quaternion.Euler(0,0,90+a*Mathf.Rad2Deg);
                }
                var veil=P(model,"Membrana violeta del umbral",m.Cylinder,V(0,.588f,.018f),V(.56f,.025f,.79f),Pigment(0x37214F));
                veil.localRotation=Quaternion.Euler(90,0,0);
                P(model,"Espiral del umbral",m.Vortex,V(0,.588f,-.04f),V(.474f,.65f,.65f),Pigment(0xA553D1)).localRotation=Quaternion.Euler(-90,0,0);
                P(model,"Veta clara de la espiral",m.Vortex,V(0,.588f,-.051f),V(.439f,.52f,.604f),glowColor).localRotation=Quaternion.Euler(-90,0,0)*Quaternion.Euler(0,69,0);
                Gem(model,"Clave del portal",V(0,1.054f,-.025f),V(.117f,.181f,.12f),glowColor);
                for(int i=0;i<3;i++)Box(model,"Peldaño frontal",V(0,.06f+i*.038f,-.288f+i*.056f),V(.381f,.071f,.144f),i%2==0?rock:bevel);
            }

            void VXCage()
            {
                VXRootBed();
                Color cloak=Pigment(0x333849), folds=Pigment(0x556183), edge=Pigment(0x789BD7);
                var wraith=G(model,"Aparición encapuchada",V(0,.155f,.018f));
                var torso=G(wraith,"Manto suspendido",V(0,.35f,.014f));
                Leaf(torso,"Caída del manto",V(0,-.046f,0),V(.38f,.678f,.276f),cloak,V(9,0,-21));
                Ball(torso,"Capucha",V(0,.224f,-.052f),V(.254f,.291f,.248f),folds);
                Ball(torso,"Vacío bajo la capucha",V(0,.232f,-.156f),V(.145f,.171f,.073f),ink);
                for(int side=-1;side<=1;side+=2)
                {
                    TaperedCurve(torso,"Brazo espectral",folds,.104f,V(side*.104f,.103f,0),V(side*.216f,.069f,-.013f),V(side*.284f,.118f,-.11f));
                    TaperedCurve(torso,"Borde luminoso",edge,.031f,V(side*.077f,.294f,-.144f),V(side*.082f,.181f,-.156f),V(side*.13f,.071f,-.113f));
                    Leaf(torso,"Pico del manto",V(side*.157f,-.168f,.005f),V(.135f,.365f,.125f),cloak,V(-24,0,side*34));
                    TaperedCurve(wraith,"Raíz retorcida",Pigment(0x584362),.095f,V(side*.286f,-.036f,-.038f),V(side*.329f,.265f,.083f),V(side*.287f,.565f,.105f),V(side*.17f,.759f,.10f));
                }
                var orbit=P(wraith,"Corriente de aislamiento",m.Vortex,V(0,.329f,-.168f),V(.622f,.45f,.467f),Pigment(0x658BC6));
                orbit.localRotation=Quaternion.Euler(-70,0,28);
            }
        }
    }
}
