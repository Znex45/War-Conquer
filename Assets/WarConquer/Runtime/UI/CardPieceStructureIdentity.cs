using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Each building is rebuilt around its actual name/function. All geometry still
            // belongs to the shared factory, and every mesh remains an assembly fragment.
            void EnhanceStructureIdentity()
            {
                var old=new System.Collections.Generic.List<GameObject>();
                foreach(Transform child in model)old.Add(child.gameObject);
                foreach(var child in old)BoardMeshFactory.Release(child);
                var sculpture=model;model=G(sculpture,"Identidad: "+id,Vector3.zero);
                switch(id)
                {
                    case "semillero-micelial":SeedNursery();break;
                    case "jardin-micelial":FungalGarden();break;
                    case "red-micelial":MycelialNetwork();break;
                    case "micelium-root":MycelialRoot();break;
                    case "nido-de-esporas":SporeNest();break;
                    case "torre-de-esporas":SporeTower();break;
                    case "corazon-del-hongo":FungalHeart();break;
                    case "santuario-del-bosque":ForestShrine();break;
                    case "santuario-del-sueno":DreamShrine();break;
                    case "arbol-madre":MotherTree();break;
                    case "bosque-eterno":EternalGrove();break;
                    case "frogpit":FrogPond();break;
                    case "firefly-nest":FireflyLanternNest();break;
                    case "sandy-anthill":AntFortress();break;
                    case "berry-bush":BerryHedge();break;
                    case "obelisco-solar":SunObelisk();break;
                    case "obelisco-de-ruptura":RiftObelisk();break;
                    case "obelisco-de-arena":SandObelisk();break;
                    case "campamento-nomada":NomadCaravan();break;
                    case "santuario-de-arena":SandShrine();break;
                    case "templo-de-ceniza":AshTemple();break;
                    case "trono-solar":SunThrone();break;
                    case "arena-profunda":SinkingSand();break;
                    case "dunas-movedizas":RollingDunes();break;
                    case "oasis-de-cristal":CrystalOasis();break;
                    case "ciudad-sepultada":BuriedCity();break;
                    case "valiant-seal":ValorSeal();break;
                    case "courage-seal":CourageSeal();break;
                    case "emergency-seal":EmergencySeal();break;
                    case "med-camp":HealingCamp();break;
                    case "resting-camp":RestCamp();break;
                    case "vigilant-tower":ForestWatchtower();break;
                    case "mana-pool":ArcanePool();break;
                    case "void-beacon":VoidLighthouse();break;
                    case "portal":VoidGate();break;
                    case "insolator":IsolationCage();break;
                    default:Structure();break;
                }
                FinishStructureSculpture();
                model=sculpture;
            }
            bool SculptIllustratedStructure()
            {
                switch (id)
                {
                    case "valiant-seal": IllustratedSeal(0); return true;
                    case "courage-seal": IllustratedSeal(1); return true;
                    case "emergency-seal": IllustratedSeal(2); return true;
                    case "med-camp": IllustratedMedicalCamp(); return true;
                    case "resting-camp": IllustratedRestCamp(); return true;
                    case "vigilant-tower": IllustratedVigilantTower(); return true;
                    case "mana-pool": IllustratedManaPool(); return true;
                    case "berry-bush": IllustratedBerryBush(); return true;
                    case "sandy-anthill": IllustratedAnthill(); return true;
                    case "frogpit": IllustratedFrogpit(); return true;
                    case "micelium-root": IllustratedRoot(); return true;
                    case "firefly-nest": IllustratedFireflyNest(); return true;
                    default: return false;
                }
            }

            void IllustratedSeal(int emblem)
            {
                Color gold=Pigment(0xF6B544), highlight=Pigment(0xFFD572), bronze=Pigment(0x9D582B);
                bool valor=emblem==0, courage=emblem==1;
                var medallion=G(model,"Medallón suspendido",V(0,valor?.43f:.35f,0));
                medallion.localRotation=Quaternion.Euler(-24,0,0);
                float width=valor?.67f:courage?.49f:.46f;
                if(emblem==2)
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        var keep=G(model,side<0?"Torre de origen":"Torre de destino",V(side*.203f,.035f,side*.08f));
                        keep.localRotation=Quaternion.Euler(0,side*13,0);
                        var wall=Pigment(side<0?0x8F4843:0x364D93);
                        Box(keep,"Cuerpo almenado",V(0,.155f,0),V(.213f,.302f,.221f),wall);
                        Box(keep,"Cornisa almenada",V(0,.278f,0),V(.248f,.066f,.242f),wall);
                        for(int j=-1;j<=1;j++)Box(keep,"Almena",V(j*.080f,.325f,-.067f),V(.051f,.084f,.067f),wall);
                        Leaf(keep,"Heráldica de torre",V(0,.185f,-.12f),V(.109f,.16f,.035f),Pigment(side<0?0xE77827:0x5DC3FB),V(0,0,180));
                    }
                    medallion.localPosition=V(0,.60f,-.03f);
                    Gem(medallion,"Insignia de aviso",Vector3.zero,V(.49f,.48f,.077f),bronze);
                    Gem(medallion,"Campo naranja",V(0,0,-.032f),V(.422f,.411f,.06f),gold);
                    Box(medallion,"Marca vertical",V(0,.02f,-.065f),V(.053f,.193f,.027f),highlight);
                    Ball(medallion,"Punto de aviso",V(0,-.105f,-.067f),Vector3.one*.05f,highlight);
                    return;
                }
                for(int layer=0;layer<3;layer++)
                {
                    float w=width-layer*.056f;
                    var face=P(medallion,"Aro de medallón",m.Cylinder,V(0,0,.043f-layer*.038f),V(w,.033f,w),layer==0?bronze:layer==1?highlight:Pigment(valor?0x41638B:0x783B24));
                    face.localRotation=Quaternion.Euler(90,0,0);
                }
                if(courage)
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        Leaf(medallion,"Cinta bermellón",V(side*.13f,-.228f,-.007f),V(.166f,.241f,.048f),Pigment(0xBB412D),V(0,0,side*40+180));
                        Gem(medallion,"Punta de coraje",V(side*.21f,.20f,.013f),V(.103f,.20f,.056f),gold).localRotation=Quaternion.Euler(0,0,-side*28);
                    }
                    for(int i=0;i<8;i++)
                    {
                        float a=i*Mathf.PI*.25f;
                        Gem(medallion,"Rayo de brújula",V(Mathf.Sin(a)*.063f,Mathf.Cos(a)*.063f,-.071f),V(.054f,i%2==0?.159f:.10f,.025f),i%2==0?highlight:gold)
                            .localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg);
                    }
                    return;
                }
                for(int i=0;i<12;i++)
                {
                    float a=i*Mathf.PI/6f;
                    Gem(medallion,"Remache dorado",V(Mathf.Sin(a)*.285f,Mathf.Cos(a)*.285f,-.052f),V(.036f,.051f,.033f),gold);
                }
                Color twig=Pigment(0x855533), foliage=Pigment(0x7BA451);
                TaperedCurve(medallion,"Árbol en el sello",twig,.061f,V(0,-.22f,-.065f),V(.018f,-.035f,-.074f),V(0,.164f,-.065f));
                for(int side=-1;side<=1;side+=2)
                    for(int i=0;i<3;i++)
                    {
                        var tip=V(side*(.101f+i*.032f),.125f-i*.09f,-.068f);
                        Bone(medallion,"Rama del blasón",V(0,.05f-i*.08f,-.071f),tip,.031f,twig);
                        Leaf(medallion,"Copa del blasón",tip+V(side*.025f,.036f,-.008f),V(.099f,.13f,.04f),i%2==0?foliage:gold,V(0,0,-side*38));
                    }
            }

            void CanvasTent(Vector3 center, float scale, Color fabric, Color shade, Color timber)
            {
                var tent=G(model,"Tienda de lona",center);
                tent.localScale=Vector3.one*scale;
                Box(tent,"Sombra del interior",V(0,.108f,-.014f),V(.22f,.20f,.20f),shade);
                for(int side=-1;side<=1;side+=2)
                {
                    Box(tent,"Paño inclinado",V(side*.086f,.145f,0),V(.198f,.047f,.251f),fabric).localRotation=Quaternion.Euler(0,0,-side*54);
                    Bone(tent,"Costura larga",V(side*.144f,.069f,-.131f),V(0,.258f,-.131f),.014f,timber);
                    Bone(tent,"Viento de tienda",V(side*.137f,.075f,-.10f),V(side*.189f,.012f,-.173f),.012f,timber);
                }
                Bone(tent,"Cumbrera",V(0,.226f,-.16f),V(0,.226f,.15f),.024f,timber);
            }

            void IllustratedMedicalCamp()
            {
                Color canvas=Pigment(0xDDD0B0), shade=Pigment(0x655358), wood=Pigment(0x88674B);
                for(int i=0;i<3;i++)CanvasTent(V((i-1)*.244f,.025f,.15f+(i==1?.12f:0)),i==1?1.13f:.91f,canvas,shade,wood);
                for(int i=0;i<2;i++)
                {
                    var bed=G(model,"Camilla del hospital",V(-.206f+i*.294f,.064f,-.21f+i*.035f));
                    bed.localRotation=Quaternion.Euler(0,i==1?-26:16,0);
                    Box(bed,"Camilla de madera",Vector3.zero,V(.167f,.059f,.269f),wood);
                    Box(bed,"Colchoneta",V(0,.036f,0),V(.151f,.046f,.236f),Pigment(i==0?0x9FAD8F:0xA49D8B));
                    Box(bed,"Almohada",V(0,.060f,.076f),V(.137f,.052f,.076f),canvas);
                }
                Box(model,"Mesa de suministros",V(.267f,.099f,-.174f),V(.174f,.141f,.196f),wood);
                for(int i=0;i<3;i++)Stem(model,"Jarra medicinal",V(.213f+i*.052f,.16f,-.194f+i*.022f),.047f,.08f,Pigment(i==1?0xB38F61:0x718373));
            }

            void IllustratedRestCamp()
            {
                Color timber=Pigment(0x6D4938), roof=Pigment(0x395F60), night=Pigment(0x2A3A42);
                for(int i=0;i<3;i++)
                {
                    CanvasTent(V((i-1)*.265f,.012f,i==1?.251f:.071f),1,roof,night,timber);
                    Gem(model,"Farol ámbar",V((i-1)*.265f,.18f,i==1?.12f:-.06f),V(.071f,.092f,.064f),Pigment(0xFFC572));
                }
                for(int i=0;i<6;i++)Ball(model,"Piedra de fogata",V(0,0,-.17f)+Rad(i,6,.114f,.045f),V(.094f,.084f,.09f),Pigment(0x695A56));
                for(int i=0;i<3;i++)Leaf(model,"Llama central",V((i-1)*.035f,.134f,-.17f),V(.10f,.233f-i*.018f,.079f),Pigment(i==1?0xFFD266:0xE88237),V(0,0,(i-1)*22));
            }

            void IllustratedVigilantTower()
            {
                Watchtower();
                URemove("Cubierta de hojas");
                Color darkWood=Pigment(0x684331), roof=Pigment(0x99512E), blue=Pigment(0x416387), gold=Pigment(0xDD9D40);
                FaePaint(darkWood,"Pilar de madera","Mirador","Larguero de escalera");
                for(int side=-1;side<=1;side+=2)
                {
                    Box(model,"Faldón del tejado a dos aguas",V(side*.147f,1.055f,.015f),V(.341f,.068f,.50f),roof).localRotation=Quaternion.Euler(0,0,-side*37);
                    Bone(model,"Viga inclinada",V(side*.245f,.981f,-.242f),V(0,1.158f,-.242f),.062f,darkWood);
                    Leaf(model,"Pendón azul",V(side*.208f,.729f,-.225f),V(.147f,.312f,.055f),blue,V(0,0,180));
                    Gem(model,"Insignia del pendón",V(side*.207f,.739f,-.258f),V(.058f,.09f,.027f),gold);
                    Bone(model,"Brazo del farol",V(side*.203f,.88f,-.04f),V(side*.34f,.88f,-.115f),.036f,darkWood);
                    Gem(model,"Farol encendido",V(side*.34f,.792f,-.115f),V(.112f,.163f,.112f),Pigment(0xFFD272));
                }
                Bone(model,"Telescopio de latón",V(.02f,.964f,-.055f),V(.218f,1.028f,-.254f),.082f,gold);
                Ball(model,"Lente del telescopio",V(.222f,1.028f,-.257f),V(.075f,.069f,.032f),Pigment(0x6FC6DF));
            }

            void IllustratedManaPool()
            {
                Color rock=Pigment(0x3D654F), moss=Pigment(0x60846A), root=Pigment(0x52604A);
                StructureDisc(.36f,.067f,rock);
                Stem(model,"Agua turquesa",V(0,.068f,0),.605f,.028f,Pigment(0x268C91));
                P(model,"Corriente espiral",m.Vortex,V(0,.104f,0),V(.56f,.2f,.56f),Pigment(0x65DDC1));
                P(model,"Luz del centro",m.Vortex,V(0,.108f,0),V(.34f,.2f,.34f),Pigment(0xAEF2B4)).localRotation=Quaternion.Euler(0,35,0);
                for(int i=0;i<8;i++)
                {
                    var p=Rad(i,8,.317f,.103f);
                    Ball(model,"Piedra irregular de orilla",p,V(.212f,.178f-i%3*.018f,.146f),i%2==0?rock:moss).localRotation=Quaternion.Euler(0,-i*45,0);
                    if(i%2==0)TaperedCurve(model,"Raíz sobre la roca",root,.064f,p+V(0,-.05f,.07f),p+V(-.047f,.07f,0),p+V(.07f,.038f,-.069f));
                }
                for(int side=-1;side<=1;side+=2)
                    TaperedCurve(model,"Raíz elevada",root,.105f,V(side*.269f,.07f,.129f),V(side*.263f,.214f,.240f),V(side*.176f,.388f,.266f),V(side*.235f,.454f,.206f));
            }

            void IllustratedBerryBush()
            {
                Color wood=Pigment(0x724637), deep=Pigment(0x408740), light=Pigment(0x74B754);
                for(int j=0;j<7;j++)
                {
                    var twig=Rad(j,7,.219f,.296f+(j%3)*.064f);
                    TaperedCurve(model,"Rama de frambueso",wood,.068f,V(0,.026f,0),twig*.57f+V(0,.067f,0),twig);
                    Ball(model,"Masa de hojas",twig,V(.255f,.257f,.239f),j%2==0?deep:light);
                    Leaf(model,"Hoja dentada",twig+Rad(j,7,.088f,.085f),V(.183f,.239f,.065f),light,V(50,j*51,24));
                    var fruit=twig+Rad(j,7,.109f,.012f)+V(0,.079f,-.044f);
                    for(int k=0;k<5;k++)Ball(model,"Drupa de frambuesa",fruit+Rad(k,5,.035f,(k%2)*.019f),Vector3.one*.075f,Pigment(k%2==0?0xC64494:0xEC6FB6));
                }
                // The tiny silhouettes at the base reference the rabbits pictured in the art.
                for(int side=-1;side<=1;side+=2)
                {
                    var rabbit=G(model,"Conejo del claro",V(side*.266f,.024f,-.242f));
                    Ball(rabbit,"Conejo",V(0,.048f,.023f),V(.122f,.10f,.15f),Pigment(0xD8D1A3));
                    Ball(rabbit,"Cabeza de conejo",V(0,.075f,-.036f),V(.088f,.089f,.079f),ivory);
                    for(int ear=-1;ear<=1;ear+=2)Leaf(rabbit,"Oreja",V(ear*.021f,.134f,-.025f),V(.041f,.109f,.036f),ivory,V(0,0,-ear*12));
                }
            }

            void IllustratedAnthill()
            {
                Color sand=Pigment(0xCD8B48), honey=Pigment(0xE9AE5E), hollow=Pigment(0x633A2E), amber=Pigment(0xFCBF4C);
                for(int i=0;i<5;i++)
                {
                    float w=.72f-i*.113f;
                    P(model,"Ladera de arena facetada",m.Carved,V(i*.021f-.044f,.08f+i*.112f,i*.027f-.034f),V(w,.24f,w*.87f),i%2==0?sand:honey);
                }
                for(int i=0;i<3;i++)
                {
                    var mouth=V((i-1)*.147f,.16f+i*.112f,-.239f+i*.062f);
                    Ball(model,"Cavidad del hormiguero",mouth,V(.163f,.181f,.077f),hollow).localRotation=Quaternion.Euler(0,-12,15);
                    Ball(model,"Interior ámbar",mouth+V(.016f,-.031f,-.039f),V(.079f,.077f,.028f),amber);
                }
                for(int i=0;i<4;i++)
                {
                    var ant=G(model,"Hormiga de la colonia",Rad(i,4,.332f,.067f)+V(0,0,-.024f));
                    ant.localRotation=Quaternion.Euler(0,i*90+27,0);
                    for(int j=0;j<3;j++)Ball(ant,"Segmento de hormiga",V(0,0,j*.031f-.045f),V(.058f-j*.01f,.043f,.053f),hollow);
                    for(int s=-1;s<=1;s+=2)for(int j=0;j<3;j++)Bone(ant,"Pata diminuta",V(0,0,j*.021f-.035f),V(s*.040f,-.024f,j*.022f-.021f),.012f,hollow);
                }
            }

            void IllustratedFrogpit()
            {
                Color moss=Pigment(0x59714A), stoneColor=Pigment(0x52604B), yellow=Pigment(0xC8DF59);
                StructureDisc(.335f,.07f,moss);
                for(int side=-1;side<=1;side+=2)
                {
                    float offset=side*.134f;
                    Ball(model,"Bóveda cubierta de musgo",V(offset,.19f,.009f),V(.31f,.33f,.381f),moss);
                    Ball(model,"Hueco de arco",V(offset,.145f,-.174f),V(.181f,.209f,.078f),stoneColor);
                    Ball(model,"Luz de la entrada",V(offset,.137f,-.202f),V(.115f,.157f,.041f),yellow);
                    TaperedCurve(model,"Raíz alrededor de la puerta",stoneColor,.066f,V(offset-.092f,.048f,-.211f),V(offset-.087f,.185f,-.229f),V(offset,.248f,-.221f),V(offset+.088f,.169f,-.224f),V(offset+.104f,.044f,-.211f));
                }
                for(int i=0;i<2;i++)
                {
                    var p=V(i==0?-.053f:.192f,i==0?.395f:.316f,i==0?.024f:.106f);
                    float w=i==0?.64f:.33f;
                    P(model,"Sombrero musgoso del refugio",m.MushroomCap,p,V(w,w,w*.84f),stoneColor).localRotation=Quaternion.Euler(0,0,i==0?13:-12);
                    P(model,"Laminillas verdosas",m.MushroomCap,p+V(0,-.027f,0),V(w*.91f,w*.65f,w*.74f),Pigment(0x81965F));
                }
                Stem(model,"Charca junto al refugio",V(.04f,.076f,-.284f),.221f,.012f,Pigment(0x647943));
                var frog = G(model,"Sapo de la charca",V(.172f,.089f,-.267f));
                Color frogGreen = Pigment(0x85A758);
                Ball(frog,"Cuerpo del sapo",V(0,.03f,.018f),V(.117f,.093f,.116f),frogGreen);
                Ball(frog,"Cabeza del sapo",V(0,.067f,-.032f),V(.107f,.080f,.073f),frogGreen);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(frog,"Ojo elevado",V(side*.031f,.106f,-.036f),V(.033f,.04f,.035f),Pigment(0xCBCA66));
                    Ball(frog,"Pupila",V(side*.031f,.107f,-.052f),V(.015f,.018f,.012f),ink);
                }
            }

            void IllustratedRoot()
            {
                Color bark=Pigment(0x8B7856), shadow=Pigment(0x54624B), green=Pigment(0xA1A362), sap=Pigment(0xC5B163);
                Ball(model,"Tronco nudoso",V(0,.259f,.047f),V(.255f,.435f,.283f),bark);
                for(int i=0;i<6;i++)
                {
                    var tip=Rad(i,6,.30f,.034f);
                    var joint=tip*.63f+V(0,.069f,.04f);
                    TaperedCurve(model,"Raíz aérea retorcida",i%2==0?bark:shadow,.095f,V(0,.261f,.04f),joint,joint+V(.04f,-.04f,-.026f),tip);
                    if(i%2==0)
                    {
                        var top=Rad(i,6,.226f,.496f+i*.028f);
                        TaperedCurve(model,"Rama micelial",bark,.084f,V(0,.354f,.052f),top*.57f+V(0,.18f,0),top);
                        P(model,"Sombrero de rama",m.MushroomCap,top,V(.275f,.21f,.235f),i==0?shadow:green).localRotation=Quaternion.Euler(10,i*50,i*8-15);
                        Bone(model,"Conducto de savia",V(0,.35f,-.063f),top*.76f+V(0,.075f,-.03f),.021f,sap);
                    }
                }
                Ball(model,"Nudo frontal",V(0,.241f,-.093f),V(.117f,.155f,.043f),shadow);
                Mushroom(model,V(.194f,.034f,-.226f),.118f,.224f,Pigment(0x936B4E));
            }

            void IllustratedFireflyNest()
            {
                Color amber=Pigment(0xFFD66F), warm=Pigment(0xC39A42), wood=Pigment(0x747043), moss=Pigment(0x6D7D47);
                var center=V(0,.29f,.018f);
                Ball(model,"Nido ovalado",center,V(.622f,.52f,.508f),moss);
                Ball(model,"Cámara ámbar del nido",center+V(0,.015f,-.224f),V(.353f,.312f,.093f),warm);
                Ball(model,"Panales iluminados",center+V(0,.015f,-.27f),V(.274f,.243f,.051f),amber);
                for(int i=0;i<9;i++)
                {
                    float a=i*Mathf.PI*2/9f, b=a+.63f;
                    Vector3 Edge(float angle,float radius,float depth)=>center+V(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius*.84f,depth);
                    TaperedCurve(model,"Vara trenzada alrededor del hueco",i%2==0?wood:warm,.048f,Edge(a,.302f,-.13f),Edge(a+.21f,.196f,-.244f),Edge(b,.190f,-.237f),Edge(b+.2f,.287f,-.14f));
                }
                for(int i=0;i<5;i++)
                {
                    var p=Rad(i,5,.29f,.16f+i*.095f);
                    Ball(model,"Lucero del nido",p,Vector3.one*.049f,amber);
                    if(i%2==0)Leaf(model,"Hoja del entretejido",p+V(0,-.09f,.025f),V(.125f,.17f,.042f),moss,V(55,i*65,35));
                }
            }

            void StructureDisc(float radius,float height,Color color)
            {P(model,"Disco cincelado",m.Cylinder,Vector3.zero,new Vector3(radius*2,height,radius*2),color);}
            void StructureSteps(Color color,int count=3,float width=.7f)
            {for(int j=0;j<count;j++)Box(model,"Escalón esculpido",new Vector3(0,.035f+j*.065f,0),new Vector3(width-j*.075f,.065f,width-.05f-j*.075f),j%2==0?color:Color.Lerp(color,ivory,.25f));}
            void StructureFlower(Vector3 pos,float size,Color petalColor)
            {
                for(int j=0;j<5;j++)Leaf(model,"Pétalo de flor",pos+Rad(j,5,size*.2f,0),new Vector3(size*.35f,size*.65f,size*.13f),petalColor,new Vector3(55,j*72,0));
                Ball(model,"Polen nacarado",pos+Vector3.up*.02f,Vector3.one*size*.32f,glow);
            }
            void StructureRoot(Vector3 from,Vector3 to,float width,Color color)
            {
                var mid=Vector3.Lerp(from,to,.52f)+new Vector3(.025f,.05f,.015f);
                Bone(model,"Raíz orgánica",from,mid,width,color);Bone(model,"Raíz orgánica",mid,to,width*.7f,color);
            }
            void SeedNursery()
            {
                Box(model,"Bancal de cultivo",new Vector3(0,.055f,0),new Vector3(.65f,.1f,.62f),skin);
                for(int z=-1;z<=1;z+=2)Box(model,"Borde del semillero",new Vector3(0,.12f,z*.27f),new Vector3(.65f,.09f,.055f),trim);
                for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z+=2)
                {
                    var p=new Vector3(x*.18f,.11f,z*.14f);
                    P(model,"Maceta de espora",m.Cylinder,p,new Vector3(.15f,.07f,.15f),dark);
                    Mushroom(model,p+Vector3.up*.06f,.11f,.18f,x==0?leaf:cloth);
                }
                Bone(model,"Soporte de semillero",new Vector3(-.28f,.13f,.23f),new Vector3(-.28f,.46f,.23f),.035f,skin);
                Leaf(model,"Estandarte de brote",new Vector3(-.28f,.43f,.22f),new Vector3(.17f,.2f,.035f),glow,new Vector3(0,0,15));
            }
            void FungalGarden()
            {
                StructureDisc(.33f,.055f,dark);
                for(int j=0;j<5;j++)
                {
                    var p=Rad(j,5,.23f,.055f);Mushroom(model,p,.2f+j%2*.13f,.28f,j%2==0?new Color(.67f,.3f,.46f):cloth);
                    Leaf(model,"Hoja del jardín",p+Vector3.up*.07f,new Vector3(.14f,.29f,.04f),leaf,new Vector3(20,j*72,35));
                }
                Bone(model,"Tallo de flor central",new Vector3(0,.06f,0),new Vector3(0,.53f,0),.055f,leaf);
                StructureFlower(new Vector3(0,.53f,0),.37f,new Color(.72f,.38f,.62f));
                Ring(model,"Liana del jardín",Vector3.up*.07f,.3f,.028f,skin,10);
            }
            void MycelialNetwork()
            {
                StructureDisc(.33f,.045f,dark);
                for(int j=0;j<6;j++)
                {
                    var p=Rad(j,6,.27f,.07f);Bone(model,"Conector luminoso",Vector3.up*.07f,p,.045f,glow);
                    Bone(model,"Enlace de red",p,Rad(j+1,6,.27f,.07f),.028f,ivory);
                    P(model,"Nodo hexagonal",m.Cylinder,p,new Vector3(.15f,.07f,.15f),cloth);Gem(model,"Nodo de micelio",p+Vector3.up*.09f,new Vector3(.09f,.14f,.09f),glow);
                }
                Gem(model,"Nexo de la red",new Vector3(0,.18f,0),new Vector3(.17f,.25f,.17f),ivory);
            }
            void MycelialRoot()
            {
                Ball(model,"Bulbo de raíz",new Vector3(0,.21f,0),new Vector3(.42f,.41f,.38f),ivory);
                for(int j=0;j<5;j++)
                {
                    var p=Rad(j,5,.34f,.035f);StructureRoot(new Vector3(0,.28f,0),p,.085f,ivory);
                    Mushroom(model,p,.07f,.16f,cloth);
                }
                Bone(model,"Vástago de micelio",new Vector3(0,.35f,0),new Vector3(.07f,.67f,0),.09f,skin);
                Mushroom(model,new Vector3(.07f,.59f,0),.08f,.44f,cloth);
                Gem(model,"Resina de raíz",new Vector3(0,.29f,-.18f),new Vector3(.11f,.17f,.04f),glow);
            }
            void SporeNest()
            {
                StructureDisc(.31f,.07f,dark);
                for(int j=0;j<10;j++)Bone(model,"Cestería del nido",Rad(j,10,.3f,.08f),Rad(j+3,10,.28f,.23f),.045f,skin);
                for(int j=0;j<5;j++)
                {
                    var p=Rad(j,5,.15f,.18f);Ball(model,"Saco de espora",p,new Vector3(.14f,.26f,.14f),j%2==0?glow:ivory);
                    Gem(model,"Espina defensiva",Rad(j,5,.29f,.3f),new Vector3(.065f,.25f,.065f),trim);
                }
                Ball(model,"Huevo central",new Vector3(0,.31f,0),new Vector3(.19f,.31f,.19f),glow);
            }
            void SporeTower()
            {
                for(int j=0;j<4;j++)StructureRoot(new Vector3(0,.3f,0),Rad(j,4,.31f,0),.1f,skin);
                Stem(model,"Torre hueca",Vector3.zero,.25f,1.06f,ivory);
                for(int j=0;j<3;j++)
                {
                    Mushroom(model,new Vector3(j%2==0?-.12f:.12f,.19f+j*.23f,0),.07f,.36f,cloth);
                    Ball(model,"Válvula de espora",new Vector3(0,.3f+j*.23f,-.135f),new Vector3(.1f,.15f,.06f),dark);
                }
                P(model,"Corona esporífera",m.MushroomCap,new Vector3(0,1.02f,0),new Vector3(.63f,.65f,.63f),leaf);
                for(int j=0;j<5;j++)Gem(model,"Espora preparada",Rad(j,5,.17f,1.19f),new Vector3(.065f,.1f,.065f),glow);
            }
            void FungalHeart()
            {
                for(int j=0;j<5;j++)StructureRoot(new Vector3(0,.37f,0),Rad(j,5,.33f,.035f),.085f,skin);
                Color vital=new Color(.71f,.26f,.43f);
                Ball(model,"Ventrículo izquierdo",new Vector3(-.09f,.51f,-.035f),new Vector3(.32f,.46f,.32f),vital);
                Ball(model,"Ventrículo derecho",new Vector3(.1f,.55f,-.035f),new Vector3(.3f,.42f,.31f),new Color(.53f,.2f,.38f));
                Gem(model,"Punta del corazón",new Vector3(0,.29f,-.04f),new Vector3(.22f,.35f,.24f),vital);
                for(int j=0;j<4;j++)
                {
                    var from=new Vector3((j%2==0?-1:1)*.12f,.67f-j*.08f,-.19f);
                    Bone(model,"Vena luminosa",from,new Vector3(0,.35f,-.17f),.025f,glow);
                }
                Mushroom(model,new Vector3(-.16f,.7f,.04f),.1f,.3f,cloth);Mushroom(model,new Vector3(.17f,.72f,.04f),.06f,.25f,leaf);
                for(int s=-1;s<=1;s+=2)Bone(model,"Arteria micelial",new Vector3(s*.08f,.7f,0),new Vector3(s*.19f,.88f,.02f),.065f,ivory);
            }
            void ForestShrine()
            {
                for(int s=-1;s<=1;s+=2)
                {
                    Bone(model,"Tronco del santuario",new Vector3(s*.24f,0,.07f),new Vector3(s*.2f,.65f,.05f),.13f,skin);
                    Bone(model,"Arco vivo",new Vector3(s*.2f,.62f,.05f),new Vector3(0,.83f,.05f),.11f,skin);
                    Ball(model,"Copa protectora",new Vector3(s*.2f,.76f,.06f),new Vector3(.36f,.34f,.4f),leaf);
                    StructureRoot(new Vector3(s*.24f,.15f,.06f),new Vector3(s*.34f,.02f,-.22f),.08f,skin);
                }
                P(model,"Altar del bosque",m.Cylinder,new Vector3(0,0,-.07f),new Vector3(.3f,.22f,.3f),stone);
                Leaf(model,"Semilla sagrada",new Vector3(0,.4f,-.07f),new Vector3(.19f,.35f,.14f),glow,new Vector3(0,0,0));
                StructureFlower(new Vector3(0,.84f,.01f),.22f,ivory);
            }
            void DreamShrine()
            {
                StructureDisc(.33f,.07f,cloth);
                var purple=new Color(.5f,.31f,.72f);var moon=new Color(.81f,.83f,1);
                for(int s=-1;s<=1;s+=2)
                {
                    Mushroom(model,new Vector3(s*.26f,.06f,.11f),.35f,.24f,purple);
                    float anchor=(s<0?169:55)*Mathf.Deg2Rad;
                    Bone(model,"Cordón del sueño",new Vector3(s*.26f,.45f,.1f),new Vector3(Mathf.Cos(anchor)*.23f,.76f+Mathf.Sin(anchor)*.23f,.1f),.025f,ivory);
                }
                for(int j=0;j<8;j++)
                {
                    float a=(j*19+55)*Mathf.Deg2Rad,b=((j+1)*19+55)*Mathf.Deg2Rad;
                    var p=new Vector3(Mathf.Cos(a)*.23f,.76f+Mathf.Sin(a)*.23f,.1f);var q=new Vector3(Mathf.Cos(b)*.23f,.76f+Mathf.Sin(b)*.23f,.1f);
                    Bone(model,"Luna creciente",p,q,.09f,moon);
                }
                StructureFlower(new Vector3(0,.17f,-.13f),.38f,purple);
                Bone(model,"Hilo de estrella",new Vector3(.13f,.95f,.1f),new Vector3(.08f,.83f,.08f),.012f,ivory);
                Gem(model,"Estrella del sueño",new Vector3(.08f,.77f,.08f),new Vector3(.09f,.13f,.035f),glow);
            }
            void MotherTree()
            {
                for(int j=0;j<7;j++)StructureRoot(new Vector3(0,.38f,0),Rad(j,7,.35f,.015f),.1f,skin);
                Bone(model,"Tronco de Árbol Madre",new Vector3(0,.1f,0),new Vector3(.03f,.89f,.03f),.26f,skin);
                for(int j=0;j<5;j++)
                {
                    var p=Rad(j,5,.24f,.87f+j%2*.14f);Bone(model,"Brazo maternal",new Vector3(0,.55f,0),p,.11f,skin);
                    Ball(model,"Copa maternal",p,new Vector3(.4f,.39f,.38f),j%2==0?leaf:cloth);
                }
                Ball(model,"Hueco de semilla",new Vector3(0,.4f,-.132f),new Vector3(.15f,.24f,.025f),dark);
                Gem(model,"Semilla madre",new Vector3(0,.41f,-.154f),new Vector3(.08f,.15f,.035f),glow);
                for(int s=-1;s<=1;s+=2)Mushroom(model,new Vector3(s*.26f,.03f,-.14f),.12f,.21f,trim);
            }
            void EternalGrove()
            {
                StructureDisc(.35f,.045f,new Color(.35f,.38f,.36f));
                for(int j=0;j<3;j++)
                {
                    var p=Rad(j,3,.21f,.025f);float h=j==1?.94f:.69f;
                    Bone(model,"Tronco del bosque eterno",p,p+Vector3.up*h,.12f,ivory);
                    for(int s=-1;s<=1;s+=2)Bone(model,"Rama petrificada",p+Vector3.up*(h*.52f),p+new Vector3(s*.12f,h*.8f,.04f),.05f,ivory);
                    P(model,"Copa de bosque eterno",m.Cone,p+Vector3.up*(h*.54f),new Vector3(.33f,h*.51f,.34f),j==1?leaf:cloth);
                    StructureRoot(p+Vector3.up*.16f,p+new Vector3(.1f,0,-.13f),.05f,ivory);
                }
                Ring(model,"Ciclo eterno",Vector3.up*.065f,.32f,.025f,glow,12);
            }
            void FrogPond()
            {
                StructureDisc(.33f,.07f,dark);P(model,"Agua del foso",m.Cylinder,Vector3.up*.073f,new Vector3(.55f,.015f,.55f),new Color(.16f,.57f,.49f));
                for(int j=0;j<7;j++)Ball(model,"Roca del foso",Rad(j,7,.29f,.1f),new Vector3(.15f,.16f,.14f),stone);
                for(int j=0;j<3;j++)P(model,"Nenúfar",m.Cylinder,Rad(j,3,.13f,.1f),new Vector3(.15f,.015f,.13f),leaf);
                StructureFlower(new Vector3(-.13f,.13f,.02f),.16f,new Color(.73f,.43f,.69f));
                for(int j=0;j<3;j++){var p=Rad(j,3,.26f,.08f);Bone(model,"Junco del pantano",p,p+Vector3.up*.31f,.022f,leaf);Ball(model,"Espiga de junco",p+Vector3.up*.31f,new Vector3(.045f,.12f,.045f),trim);}
                Ball(model,"Puesta de rana",new Vector3(.12f,.13f,-.1f),new Vector3(.13f,.08f,.1f),glow);
            }
            void FireflyLanternNest()
            {
                StructureDisc(.31f,.06f,dark);Ring(model,"Cuenco del nido",Vector3.up*.1f,.22f,.065f,skin,10);
                for(int j=0;j<4;j++)
                {
                    var p=Rad(j,4,.24f,.03f);var top=Rad(j,4,.19f,.69f);
                    Bone(model,"Junco arqueado",p,top,.045f,leaf);Bone(model,"Tejido del farol",top,new Vector3(0,.82f,0),.035f,skin);
                    Ball(model,"Farol de luciérnaga",top-new Vector3(0,.17f,0),new Vector3(.13f,.22f,.12f),new Color(.77f,.98f,.37f));
                    Bone(model,"Hilo de farol",top,top-new Vector3(0,.1f,0),.018f,ivory);
                }
                Mushroom(model,new Vector3(0,.07f,0),.18f,.25f,cloth);
            }
            void AntFortress()
            {
                for(int j=0;j<3;j++)P(model,"Cono del hormiguero",m.Cone,new Vector3((j-1)*.14f,.03f,j%2*.12f),new Vector3(.38f,.57f+j*.11f,.39f),stone);
                for(int s=-1;s<=1;s+=2)
                {
                    Ball(model,"Boca del túnel",new Vector3(s*.13f,.14f,-.175f),new Vector3(.13f,.17f,.03f),dark);
                    for(int j=0;j<3;j++)Box(model,"Ladrillo de entrada",new Vector3(s*.13f+(j-1)*.06f,.25f,-.18f),new Vector3(.06f,.055f,.07f),trim);
                }
                Bone(model,"Ruta de hormigas",new Vector3(0,.04f,-.31f),new Vector3(.11f,.045f,-.2f),.04f,skin);
                for(int j=0;j<3;j++)Ball(model,"Hormiga de entrada",new Vector3(j*.036f-.025f,.07f,-.28f+j*.025f),new Vector3(.045f,.035f,.07f),dark);
            }
            void BerryHedge()
            {
                Shrub();
                for(int s=-1;s<=1;s+=2)Box(model,"Poste de huerto",new Vector3(s*.29f,.17f,-.23f),new Vector3(.055f,.33f,.055f),skin);
                Bone(model,"Valla de bayas",new Vector3(-.29f,.21f,-.23f),new Vector3(.29f,.21f,-.23f),.04f,trim);
                for(int j=0;j<4;j++)Leaf(model,"Hoja de baya",Rad(j,4,.29f,.33f),new Vector3(.18f,.29f,.07f),leaf,new Vector3(30,j*90,30));
                Ball(model,"Cesta de cosecha",new Vector3(.2f,.09f,-.28f),new Vector3(.19f,.16f,.17f),trim);
            }
            void SunObelisk()
            {
                DesertBuilding();
                Ring(model,"Disco solar de bronce",new Vector3(0,1.1f,0),.18f,.035f,trim,12,Quaternion.Euler(90,0,0));
                for(int j=0;j<8;j++){float a=j*Mathf.PI/4;Gem(model,"Rayo del obelisco",new Vector3(Mathf.Cos(a)*.24f,1.1f+Mathf.Sin(a)*.24f,0),new Vector3(.06f,.13f,.035f),trim).localRotation=Quaternion.Euler(0,0,j*45-90);}
                Gem(model,"Ojo del sol",new Vector3(0,1.1f,-.045f),new Vector3(.1f,.14f,.05f),glow);
            }
            void RiftObelisk()
            {
                StructureSteps(stone);
                for(int s=-1;s<=1;s+=2)
                {
                    var p=new Vector3(s*.14f,.62f,0);var shard=Gem(model,"Mitad del obelisco roto",p,new Vector3(.2f,.88f,.26f),stone);shard.localRotation=Quaternion.Euler(0,0,-s*7);
                    Gem(model,"Fisura de ruptura",new Vector3(s*.035f,.62f,-.115f),new Vector3(.035f,.64f,.03f),new Color(1,.33f,.13f));
                    Ball(model,"Fragmento caído",new Vector3(s*.28f,.24f,-.13f),new Vector3(.17f,.15f,.16f),trim);
                }
                Gem(model,"Núcleo de fractura",new Vector3(0,.58f,0),new Vector3(.12f,.25f,.13f),glow);
            }
            void SandObelisk()
            {
                for(int j=0;j<3;j++)Ball(model,"Montículo de arena",Rad(j,3,.19f,.055f),new Vector3(.3f,.11f,.3f),stone);
                P(model,"Obelisco emergente",m.Bell,new Vector3(0,.045f,0),new Vector3(.24f,.66f,.24f),stone);
                P(model,"Punta de obelisco de arena",m.Cone,new Vector3(0,.705f,0),new Vector3(.18f,.22f,.18f),trim);
                for(int j=0;j<3;j++)Gem(model,"Glifo de arena",new Vector3(0,.24f+j*.13f,-.11f+j*.01f),new Vector3(.07f,.075f,.025f),ivory);
            }
            void NomadCaravan()
            {
                Camp();
                for(int s=-1;s<=1;s+=2)Ring(model,"Rueda de caravana",new Vector3(s*.29f,.18f,.1f),.13f,.025f,trim,8,Quaternion.Euler(0,0,90));
                Bone(model,"Eje de caravana",new Vector3(-.3f,.18f,.1f),new Vector3(.3f,.18f,.1f),.04f,skin);
                for(int j=0;j<3;j++)Ball(model,"Fardo de viaje",new Vector3((j-1)*.12f,.11f,-.27f),new Vector3(.13f,.17f,.14f),j%2==0?ivory:cloth);
                Stem(model,"Mástil nómada",new Vector3(-.28f,.05f,.19f),.025f,.68f,skin);
                Leaf(model,"Banderín de ruta",new Vector3(-.2f,.62f,.19f),new Vector3(.15f,.24f,.035f),trim,new Vector3(0,0,70));
            }
            void SandShrine()
            {
                StructureSteps(stone);
                for(int s=-1;s<=1;s+=2){Stem(model,"Columna del santuario",new Vector3(s*.24f,.2f,.06f),.12f,.48f,ivory);Stem(model,"Capitel tallado",new Vector3(s*.24f,.65f,.06f),.18f,.08f,trim);}
                Box(model,"Dintel del santuario",new Vector3(0,.77f,.06f),new Vector3(.65f,.1f,.33f),stone);
                P(model,"Cúpula de arena",m.MushroomCap,new Vector3(0,.85f,.06f),new Vector3(.64f,.6f,.4f),trim);
                P(model,"Cuenco de ofrenda",m.Cylinder,new Vector3(0,.2f,-.12f),new Vector3(.24f,.18f,.24f),stone);
                Gem(model,"Reliquia del desierto",new Vector3(0,.52f,-.12f),new Vector3(.14f,.29f,.14f),glow);
                for(int j=0;j<3;j++)Box(model,"Escalera de acceso",new Vector3(0,.035f+j*.05f,-.29f+j*.035f),new Vector3(.25f,.055f,.13f),ivory);
            }
            void AshTemple()
            {
                Color basalt=new Color(.24f,.22f,.26f),ember=new Color(1,.35f,.09f);
                StructureSteps(basalt);
                Box(model,"Santuario de basalto",new Vector3(0,.53f,.07f),new Vector3(.46f,.64f,.39f),basalt);
                Box(model,"Boca del templo",new Vector3(0,.43f,-.137f),new Vector3(.19f,.36f,.025f),ink);
                for(int s=-1;s<=1;s+=2)
                {
                    Gem(model,"Columna quebrada",new Vector3(s*.26f,.58f,-.13f),new Vector3(.17f,.76f,.17f),basalt);
                    Gem(model,"Canal de ascua",new Vector3(s*.26f,.59f,-.22f),new Vector3(.035f,.48f,.025f),ember);
                    P(model,"Boca de brasero",m.Cylinder,new Vector3(s*.26f,.9f,-.13f),new Vector3(.18f,.065f,.18f),trim);
                    Gem(model,"Llama del templo",new Vector3(s*.26f,1.05f,-.13f),new Vector3(.12f,.27f,.12f),ember);
                }
                Box(model,"Cornisa de escoria",new Vector3(0,.855f,.07f),new Vector3(.55f,.04f,.45f),basalt);
                P(model,"Pirámide de escoria",m.Cone,new Vector3(0,.87f,.07f),new Vector3(.59f,.37f,.49f),basalt);
                Gem(model,"Ojo del templo",new Vector3(0,.92f,-.17f),new Vector3(.11f,.13f,.03f),ember);
                for(int j=0;j<3;j++)Bone(model,"Grieta de ceniza",new Vector3((j-1)*.11f,.22f,-.27f),new Vector3((j-1)*.13f,.22f,-.12f),.025f,ember);
            }
            void SunThrone()
            {
                StructureSteps(stone);Box(model,"Trono de oro",new Vector3(0,.42f,0),new Vector3(.37f,.17f,.32f),trim);
                Box(model,"Respaldo del emperador",new Vector3(0,.74f,.16f),new Vector3(.39f,.71f,.085f),stone);
                for(int s=-1;s<=1;s+=2){Stem(model,"Brazal real",new Vector3(s*.24f,.2f,-.06f),.085f,.37f,trim);Ball(model,"Cabeza de león del trono",new Vector3(s*.24f,.6f,-.1f),new Vector3(.16f,.18f,.18f),trim);Gem(model,"Garra tallada",new Vector3(s*.24f,.24f,-.12f),new Vector3(.13f,.14f,.16f),ivory);}
                Ring(model,"Gran disco solar",new Vector3(0,1.07f,.13f),.24f,.04f,trim,12,Quaternion.Euler(90,0,0));
                for(int j=0;j<6;j++){float a=j*Mathf.PI/3;Gem(model,"Rayo del trono",new Vector3(Mathf.Cos(a)*.29f,1.07f+Mathf.Sin(a)*.29f,.13f),new Vector3(.06f,.12f,.035f),trim).localRotation=Quaternion.Euler(0,0,j*60-90);}
                Gem(model,"Gema del trono",new Vector3(0,.79f,.1f),new Vector3(.13f,.2f,.035f),glow);
                Leaf(model,"Alfombra solar",new Vector3(0,.22f,-.17f),new Vector3(.3f,.43f,.025f),cloth,new Vector3(90,0,0));
            }
            void SinkingSand()
            {
                StructureDisc(.34f,.025f,stone);P(model,"Fosa de arena oscura",m.Cylinder,new Vector3(0,.027f,0),new Vector3(.39f,.012f,.39f),new Color(.32f,.2f,.1f));
                for(int j=0;j<12;j++)
                {
                    float radius=.13f+j*.014f;var p=Rad(j,12,radius,.05f+j*.002f);
                    var dune=Ball(model,"Labio de arena hundida",p,new Vector3(.2f,.07f,.13f),stone);dune.localRotation=Quaternion.Euler(0,-j*30,0);
                }
                var remnant=Box(model,"Pilar tragado por arena",new Vector3(.08f,.095f,.02f),new Vector3(.11f,.15f,.1f),trim);remnant.localRotation=Quaternion.Euler(0,25,-27);
                Bone(model,"Lanza semihundida",new Vector3(-.13f,.035f,-.07f),new Vector3(-.07f,.19f,-.04f),.025f,dark);
            }
            void RollingDunes()
            {
                StructureDisc(.34f,.025f,stone);
                for(int j=0;j<3;j++)
                {
                    float x=(j-1)*.19f;var dune=P(model,"Cresta de duna",m.MushroomCap,new Vector3(x,.06f,0),new Vector3(.25f,.36f,.59f),j%2==0?stone:Color.Lerp(stone,ivory,.25f));dune.localRotation=Quaternion.Euler(0,-16+j*7,-12);
                    Bone(model,"Línea de cresta",new Vector3(x-.015f,.16f,-.21f),new Vector3(x+.01f,.18f,.17f),.018f,ivory);
                }
                for(int j=0;j<4;j++)Leaf(model,"Viento sobre arena",new Vector3(-.27f+j*.17f,.07f,-.25f),new Vector3(.05f,.12f,.018f),trim,new Vector3(90,0,-35));
            }
            void CrystalOasis()
            {
                StructureDisc(.34f,.04f,stone);P(model,"Agua de oasis",m.Cylinder,Vector3.up*.045f,new Vector3(.48f,.015f,.48f),new Color(.16f,.7f,.76f));
                for(int j=0;j<5;j++)Ball(model,"Roca del oasis",Rad(j,5,.29f,.065f),new Vector3(.14f,.1f,.13f),ivory);
                for(int j=0;j<3;j++)
                {
                    Ball(model,"Roca de cristal",new Vector3(.23f-j*.055f,.075f,.12f+j*.065f),new Vector3(.13f,.1f,.13f),ivory);
                    Gem(model,"Cristal del oasis",new Vector3(.23f-j*.055f,.16f+j%2*.035f,.12f+j*.065f),new Vector3(.1f,.2f+j%2*.04f,.1f),glow);
                }
                Bone(model,"Palmera enana del oasis",new Vector3(-.24f,.035f,.08f),new Vector3(-.21f,.25f,.1f),.035f,skin);
                for(int j=0;j<4;j++)
                {
                    var apex=new Vector3(-.21f,.25f,.1f);var frond=apex+Rad(j,4,.065f,0);
                    Bone(model,"Pecíolo de palmera",apex,frond,.014f,leaf);
                    Leaf(model,"Fronda de oasis",frond,new Vector3(.06f,.2f,.025f),leaf,new Vector3(65,j*90,0));
                }
            }
            void BuriedCity()
            {
                StructureSteps(stone,2);
                for(int j=0;j<3;j++)
                {
                    var p=new Vector3((j-1)*.22f,.1f,j%2*.15f);float h=j==1?.68f:.37f;
                    Box(model,"Torre de ciudad sepultada",p+Vector3.up*(h*.5f),new Vector3(.18f,h,.2f),stone);
                    Box(model,"Ventana antigua",p+new Vector3(0,h*.63f,-.108f),new Vector3(.065f,.11f,.02f),dark);
                    Gem(model,"Cúspide rota",p+Vector3.up*(h+.04f),new Vector3(.19f,.17f,.2f),trim);
                }
                for(int s=-1;s<=1;s+=2)Bone(model,"Columna de ruina",new Vector3(s*.28f,.1f,-.22f),new Vector3(s*.28f,s<0?.47f:.34f,-.22f),.09f,ivory);
                var lintel=Box(model,"Dintel derrumbado",new Vector3(.04f,.22f,-.24f),new Vector3(.5f,.09f,.14f),trim);lintel.localRotation=Quaternion.Euler(0,10,-19);
                for(int j=0;j<3;j++)Ball(model,"Arena de la ciudad",new Vector3((j-1)*.2f,.15f,-.13f),new Vector3(.3f,.13f,.32f),stone);
            }
            void SealFloor(Color color)
            {
                StructureDisc(.34f,.055f,dark);P(model,"Sello de piedra",m.Cylinder,Vector3.up*.055f,new Vector3(.61f,.035f,.61f),color);
                Ring(model,"Aro de sello",Vector3.up*.1f,.29f,.025f,trim,12);
                for(int j=0;j<4;j++)Gem(model,"Runas del sello",Rad(j,4,.25f,.12f),new Vector3(.04f,.055f,.04f),glow);
            }
            void ValorSeal()
            {
                SealFloor(cloth);
                for(int s=-1;s<=1;s+=2)
                {
                    Bone(model,"Espadas del valor",new Vector3(s*.13f,.105f,.13f),new Vector3(-s*.12f,.105f,-.12f),.035f,ivory);
                    Gem(model,"Punta de espada",new Vector3(-s*.15f,.105f,-.15f),new Vector3(.06f,.08f,.025f),ivory).localRotation=Quaternion.Euler(90,0,-s*45);
                    Bone(model,"Guarda de espada",new Vector3(s*.07f,.14f,.14f),new Vector3(s*.15f,.14f,.06f),.035f,trim);
                }
                Gem(model,"Rubí de valentía",new Vector3(0,.16f,0),new Vector3(.12f,.08f,.12f),new Color(.8f,.28f,.28f));
            }
            void CourageSeal()
            {
                SealFloor(new Color(.22f,.43f,.29f));
                P(model,"Relieve del coraje",m.Cylinder,new Vector3(0,.09f,0),new Vector3(.45f,.035f,.42f),cloth);
                for(int s=-1;s<=1;s+=2)for(int j=0;j<3;j++)Leaf(model,"Ala del coraje",new Vector3(s*(.07f+j*.055f),.13f,.015f+j*.035f),new Vector3(.09f,.21f,.025f),ivory,new Vector3(90,0,s*(35+j*12)));
                Gem(model,"Corazón del coraje",new Vector3(0,.15f,0),new Vector3(.12f,.08f,.16f),trim);
            }
            void EmergencySeal()
            {
                SealFloor(new Color(.25f,.31f,.45f));
                Gem(model,"Escudo de emergencia",new Vector3(0,.123f,0),new Vector3(.29f,.065f,.34f),ivory);
                Bone(model,"Cruz de emergencia",new Vector3(-.09f,.16f,0),new Vector3(.09f,.16f,0),.035f,new Color(.69f,.26f,.27f));
                Bone(model,"Cruz de emergencia",new Vector3(0,.16f,-.1f),new Vector3(0,.16f,.1f),.035f,new Color(.69f,.26f,.27f));
            }
            void HealingCamp()
            {
                Camp();
                for(int s=-1;s<=1;s+=2){Bone(model,"Pata de camilla",new Vector3(s*.14f,0,-.29f),new Vector3(s*.14f,.13f,-.29f),.03f,trim);}
                Box(model,"Camilla médica",new Vector3(0,.15f,-.28f),new Vector3(.38f,.05f,.14f),ivory);
                for(int j=0;j<3;j++){Stem(model,"Frasco curativo",new Vector3(.25f,.07f,j*.08f-.02f),.055f,.1f,glow);Stem(model,"Tapón de frasco",new Vector3(.25f,.17f,j*.08f-.02f),.04f,.03f,trim);}
                Box(model,"Cruz de enfermería",new Vector3(0,.48f,-.172f),new Vector3(.16f,.045f,.025f),ivory);Box(model,"Cruz de enfermería",new Vector3(0,.48f,-.172f),new Vector3(.045f,.16f,.025f),ivory);
            }
            void RestCamp()
            {
                Camp();
                for(int j=0;j<2;j++){Box(model,"Saco de descanso",new Vector3((j==0?-.12f:.12f),.045f,-.24f),new Vector3(.15f,.09f,.26f),j==0?cloth:leaf);Ball(model,"Almohada",new Vector3(j==0?-.12f:.12f,.1f,-.16f),new Vector3(.14f,.08f,.08f),ivory);}
                for(int j=0;j<2;j++)Bone(model,"Leña de fogata",Rad(j,4,.095f,.05f)+new Vector3(.25f,0,.23f),Rad(j+2,4,.095f,.05f)+new Vector3(.25f,0,.23f),.04f,skin);
                Gem(model,"Brasa de descanso",new Vector3(.25f,.11f,.23f),new Vector3(.09f,.15f,.09f),new Color(.95f,.53f,.18f));
            }
            void ForestWatchtower()
            {
                Watchtower();
                for(int s=-1;s<=1;s+=2){Stem(model,"Poste de vigilancia",new Vector3(s*.26f,.8f,-.22f),.03f,.25f,skin);Leaf(model,"Estandarte de guardia",new Vector3(s*.24f,.93f,-.24f),new Vector3(.14f,.23f,.025f),trim,new Vector3(0,0,s*8));}
                Bone(model,"Telescopio de vigía",new Vector3(.04f,.94f,-.2f),new Vector3(.2f,1.04f,-.29f),.055f,trim);
                for(int j=0;j<3;j++)Leaf(model,"Teja de hoja",new Vector3((j-1)*.12f,1.1f,-.16f),new Vector3(.18f,.24f,.05f),leaf,new Vector3(45,0,0));
            }
            void ArcanePool()
            {
                StructureDisc(.34f,.07f,stone);P(model,"Lente de maná",m.Cylinder,Vector3.up*.075f,new Vector3(.5f,.025f,.5f),new Color(.3f,.62f,.87f));
                Ring(model,"Aro de canalización",Vector3.up*.11f,.27f,.04f,trim,12);
                for(int j=0;j<4;j++){var p=Rad(j,4,.27f,.08f);Gem(model,"Cristal canalizador",p+Vector3.up*.14f,new Vector3(.095f,.27f,.095f),glow);Bone(model,"Conducción de maná",Vector3.up*.11f,p,.018f,ivory);}
                P(model,"Espiral del manantial",m.Vortex,Vector3.up*.115f,Vector3.one*.28f,ivory);
            }
            void VoidLighthouse()
            {
                for(int j=0;j<3;j++){var p=Rad(j,3,.24f,.03f);Gem(model,"Contrafuerte del faro",p+Vector3.up*.38f,new Vector3(.16f,.76f,.17f),stone);Bone(model,"Arista del faro",p,p+Vector3.up*.6f,.022f,glow);}
                Stem(model,"Fuste del faro",Vector3.zero,.23f,.57f,stone);
                Gem(model,"Prisma de señal",new Vector3(0,.88f,0),new Vector3(.31f,.63f,.31f),glow);
                Ring(model,"Óptica inferior",new Vector3(0,.61f,0),.27f,.04f,trim,10);
                Ring(model,"Óptica superior",new Vector3(0,1.16f,0),.18f,.035f,trim,10);
                for(int j=0;j<3;j++)Bone(model,"Soporte de óptica",Rad(j,3,.13f,.98f),Rad(j,3,.18f,1.16f),.025f,trim);
                for(int s=-1;s<=1;s+=2)Gem(model,"Aleta direccional",new Vector3(s*.22f,.87f,0),new Vector3(.12f,.28f,.16f),trim);
            }
            void VoidGate()
            {
                for(int s=-1;s<=1;s+=2){Gem(model,"Anclaje de umbral",new Vector3(s*.28f,.32f,0),new Vector3(.2f,.64f,.23f),stone);Gem(model,"Glifo de umbral",new Vector3(s*.28f,.38f,-.12f),new Vector3(.05f,.25f,.025f),glow);}
                Ring(model,"Dovelas del portal",new Vector3(0,.61f,0),.38f,.11f,trim,12,Quaternion.Euler(90,0,0));
                Ring(model,"Orla interior",new Vector3(0,.61f,-.035f),.29f,.03f,glow,12,Quaternion.Euler(90,0,0));
                for(int j=0;j<3;j++)
                {
                    float a=(90+j*120)*Mathf.Deg2Rad;
                    Bone(model,"Puente de runa",new Vector3(Mathf.Cos(a)*.29f,.61f+Mathf.Sin(a)*.29f,-.025f),new Vector3(Mathf.Cos(a)*.38f,.61f+Mathf.Sin(a)*.38f,-.025f),.025f,trim);
                }
                var veil=P(model,"Plano del umbral",m.Cylinder,new Vector3(0,.61f,.025f),new Vector3(.55f,.018f,.55f),new Color(.075f,.045f,.16f));veil.localRotation=Quaternion.Euler(90,0,0);
                var vortex=P(model,"Espiral del portal",m.Vortex,new Vector3(0,.61f,-.012f),Vector3.one*.4f,glow);vortex.localRotation=Quaternion.Euler(-90,0,0);
                Gem(model,"Clave del arco",new Vector3(0,1.06f,0),new Vector3(.17f,.24f,.16f),glow);
                for(int j=0;j<3;j++)Box(model,"Peldaño del portal",new Vector3(0,.035f+j*.04f,-.28f+j*.07f),new Vector3(.44f,.05f,.13f),stone);
            }
            void IsolationCage()
            {
                StructureDisc(.33f,.09f,stone);
                for(int j=0;j<4;j++)
                {
                    var p=Rad(j,4,.26f,.06f);Gem(model,"Torre de aislamiento",p+Vector3.up*.42f,new Vector3(.15f,.84f,.15f),trim);
                    Bone(model,"Barra de contención",p+Vector3.up*.14f,Rad(j+1,4,.26f,.2f),.025f,glow);
                    Bone(model,"Barra de contención",p+Vector3.up*.68f,Rad(j+1,4,.26f,.74f),.025f,glow);
                    Bone(model,"Costilla de jaula",p+Vector3.up*.74f,new Vector3(0,1.02f,0),.055f,stone);
                }
                Gem(model,"Cristal confinado",new Vector3(0,.52f,0),new Vector3(.26f,.48f,.26f),glow);
                Ring(model,"Anillo de aislamiento",new Vector3(0,.49f,0),.2f,.03f,ivory,10);
                Gem(model,"Cerradura de la jaula",new Vector3(0,1.02f,0),new Vector3(.13f,.18f,.13f),trim);
            }
        }
    }
}
