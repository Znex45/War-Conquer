using System;
using UnityEngine;

namespace WarConquer
{
    // Original sculptures, independent of leader prefabs, rigs and textures.
    public static partial class CardPieceModels
    {
        public static Biome Habitat(CardData c)
        {
            if(c.leader=="XIL'THAR")return Biome.Void;
            if(c.leader=="SAHRIA")return c.requiresAshLand?Biome.AshLand:c.id=="wandering-beast"?Biome.Wasteland:Biome.Desert;
            if(c.leader=="ZUKGROK")return c.biomes!=null&&Array.IndexOf(c.biomes,Biome.Swamp)>=0?Biome.Swamp:Biome.Forest;
            if(c.biomes!=null&&c.biomes.Length==1)return c.biomes[0];
            return c.leader=="FAUNAR"?Biome.Forest:Biome.Neutral;
        }
        public static Transform Build(BoardMeshFactory m,Transform parent,CardData c,Color owner)
        {
            if(c.category==Category.Spell)return null;
            return new Sculptor(m,parent,c,owner).Build();
        }
        sealed partial class Sculptor
        {
            readonly BoardMeshFactory m;readonly Transform parent;readonly CardData card;
            readonly string id;readonly Biome biome;readonly Color owner;
            Color skin,stone,dark,trim,glow,cloth,leaf;
            readonly Color ivory=new Color(.89f,.85f,.65f),ink=new Color(.025f,.035f,.045f);
            Transform model;
            public Sculptor(BoardMeshFactory factory,Transform root,CardData c,Color color)
            {
                m=factory;parent=root;card=c;id=c.id;owner=color;biome=Habitat(c);
                skin=new Color(.56f,.35f,.19f);stone=new Color(.45f,.48f,.37f);dark=new Color(.12f,.18f,.14f);
                trim=new Color(.65f,.49f,.22f);glow=new Color(.45f,.86f,.3f);cloth=new Color(.17f,.39f,.23f);leaf=new Color(.23f,.5f,.24f);
                if(biome==Biome.Swamp){skin=new Color(.57f,.65f,.39f);stone=new Color(.24f,.37f,.3f);dark=new Color(.075f,.16f,.15f);trim=new Color(.37f,.63f,.47f);glow=new Color(.53f,.94f,.72f);cloth=new Color(.22f,.4f,.36f);leaf=new Color(.29f,.56f,.38f);}
                if(biome==Biome.Desert){skin=new Color(.65f,.41f,.23f);stone=new Color(.78f,.65f,.43f);dark=new Color(.2f,.14f,.12f);trim=new Color(.85f,.61f,.22f);glow=new Color(.22f,.82f,.83f);cloth=new Color(.22f,.4f,.48f);leaf=new Color(.34f,.52f,.36f);}
                if(biome==Biome.Wasteland||biome==Biome.AshLand||biome==Biome.Volcanic){skin=new Color(.33f,.28f,.25f);stone=new Color(.27f,.25f,.27f);dark=new Color(.09f,.07f,.08f);trim=new Color(.66f,.31f,.14f);glow=new Color(1,.45f,.09f);cloth=new Color(.36f,.17f,.13f);}
                if(biome==Biome.Void){skin=new Color(.21f,.19f,.35f);stone=new Color(.17f,.14f,.26f);dark=new Color(.045f,.035f,.095f);trim=new Color(.44f,.29f,.69f);glow=new Color(.18f,.77f,1);cloth=new Color(.32f,.18f,.5f);leaf=glow;}
            }
            bool Has(string text)=>id.IndexOf(text,StringComparison.Ordinal)>=0;
            Transform G(Transform p,string n,Vector3 pos)=>m.Group(p,n,pos);
            Transform P(Transform p,string n,Mesh mesh,Vector3 pos,Vector3 scale,Color color)=>m.Part(p,n,mesh,pos,scale,color).transform;
            Transform Ball(Transform p,string n,Vector3 pos,Vector3 scale,Color color)=>P(p,n,m.SculptureBody,pos,scale,color);
            Transform Box(Transform p,string n,Vector3 pos,Vector3 scale,Color color)=>P(p,n,m.SculptureBlock,pos,scale,color);
            Transform Stem(Transform p,string n,Vector3 pos,float width,float height,Color color)=>P(p,n,m.Cylinder,pos,new Vector3(width,height,width),color);
            Transform Gem(Transform p,string n,Vector3 pos,Vector3 scale,Color color)=>P(p,n,m.Crystal,pos,scale,color);
            Transform Bone(Transform p,string n,Vector3 a,Vector3 b,float width,Color color)
            {var t=P(p,n,m.Cylinder,a,new Vector3(width,(b-a).magnitude,width),color);t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return t;}
            Vector3 Rad(int i,int count,float radius,float y)
            {float a=i*Mathf.PI*2/count;return new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);}
            void Ring(Transform p,string name,Vector3 center,float radius,float width,Color color,int count=12,Quaternion rotation=default)
            {
                if(rotation==default)rotation=Quaternion.identity;
                for(int i=0;i<count;i++)Bone(p,name,center+rotation*Rad(i,count,radius,0),center+rotation*Rad(i+1,count,radius,0),width,color);
            }
            void Eyes(Transform head,float x,float y,float z,float size=.055f,Color? eye=null)
            {
                for(int s=-1;s<=1;s+=2)
                {
                    // Recessed almond eyes, with a matching lid instead of protruding white balls.
                    Ball(head,"Órbita",new Vector3(s*x,y,z+.006f),new Vector3(size*1.9f,size*1.05f,size*.85f),Color.Lerp(skin,dark,.5f));
                    Ball(head,"Pupila",new Vector3(s*x,y,z-size*.25f),new Vector3(size*1.25f,size*.72f,size*.4f),eye??new Color(.16f,.1f,.055f));
                    Ball(head,"Reflejo ocular",new Vector3(s*x-size*.15f,y+size*.15f,z-size*.43f),Vector3.one*size*.23f,ivory);
                }
            }
            Transform Leaf(Transform p,string name,Vector3 pos,Vector3 scale,Color color,Vector3 angle)
            {var t=P(p,name,m.Petal,pos,scale,color);t.localRotation=Quaternion.Euler(angle);return t;}
            void Mushroom(Transform p,Vector3 pos,float h,float w,Color cap)
            {
                Stem(p,"Pie micelial",pos,.14f*w,h,ivory);
                P(p,"Laminillas",m.MushroomCap,pos+Vector3.up*(h-.025f),new Vector3(w,.42f*w,w),Color.Lerp(cap,ivory,.65f));
                P(p,"Sombrero esculpido",m.MushroomCap,pos+Vector3.up*h,new Vector3(w,w,w),cap);
                for(int j=0;j<4;j++)Ball(p,"Mota nacarada",pos+Rad(j,4,w*.24f,h+w*.23f),new Vector3(.075f,.025f,.065f)*w,ivory);
            }
            public Transform Build()
            {
                P(parent,"Peana de facción",m.Cylinder,Vector3.zero,new Vector3(.88f,.065f,.88f),Color.Lerp(owner,dark,.25f));
                P(parent,"Bisel de la peana",m.Cylinder,Vector3.up*.064f,new Vector3(.81f,.025f,.81f),trim);
                P(parent,"Suelo del hábitat",m.Cylinder,Vector3.up*.09f,new Vector3(.78f,.035f,.78f),stone);
                model=G(parent,"Escultura de carta",Vector3.up*.125f);
                if(card.IsStructure)EnhanceStructureIdentity();
                else{Unit();EnhanceUnitIdentity();FinishUnitSculpture();}
                return model;
            }
            void Unit()
            {
                if(id=="titan-de-ceniza"||id=="sandy-ember"){AshGolem();return;}
                if(id=="guardian-del-oasis"){Tortoise();return;}
                if(Has("larva")){Larva();return;}
                if(Has("frog")){Frog();return;}
                if(Has("alligator")){Crocodile();return;}
                if(Has("escorpion")||Has("escarabajo")||Has("battle-ant")||Has("firefly")||id=="dark-fly"){Insect();return;}
                if(id=="baby-phoenix"){Bird();return;}
                if(biome==Biome.Void){VoidCreature();return;}
                if(Has("ciervo")||Has("mongoose")||Has("ferret")||Has("bober")||Has("panther")||Has("rabbit")||Has("cammel")||Has("bestia")||Has("wandering-beast")){Quadruped();return;}
                if(card.leader=="ZUKGROK"||Has("mushroom")||id=="lushroom"){Fungal();return;}
                Humanoid();
            }
            void Larva()
            {
                for(int i=0;i<6;i++)
                {
                    var pos=new Vector3(Mathf.Sin(i*.8f)*.08f,.15f+Mathf.Sin(i*.55f)*.04f,.28f-i*.105f);
                    Ball(model,"Segmento de larva",pos,new Vector3(.32f-i*.016f,.24f,.22f),i%2==0?skin:ivory);
                    for(int s=-1;s<=1;s+=2)Ball(model,"Pata pequeña",pos+new Vector3(s*.13f,-.08f,0),new Vector3(.08f,.09f,.11f),dark);
                }
                var head=G(model,"Idle:Head",new Vector3(0,.2f,-.32f));Ball(head,"Cabeza larval",Vector3.zero,new Vector3(.3f,.27f,.27f),skin);Eyes(head,.08f,.04f,-.12f);
                for(int s=-1;s<=1;s+=2){Bone(head,"Antena",new Vector3(s*.07f,.08f,0),new Vector3(s*.12f,.24f,-.03f),.027f,trim);Ball(head,"Espora sensorial",new Vector3(s*.12f,.24f,-.03f),Vector3.one*.075f,glow);}
            }
            void Frog()
            {
                Ball(model,"Cuerpo anfibio",new Vector3(0,.22f,0),new Vector3(.47f,.35f,.42f),leaf);
                Ball(model,"Pecho crema",new Vector3(0,.22f,-.17f),new Vector3(.36f,.25f,.11f),ivory);
                for(int s=-1;s<=1;s+=2)
                {
                    Ball(model,"Muslo plegado",new Vector3(s*.24f,.17f,.09f),new Vector3(.25f,.24f,.3f),skin);
                    Bone(model,"Brazo anfibio",new Vector3(s*.16f,.28f,-.1f),new Vector3(s*.22f,.05f,-.25f),.065f,leaf);
                    for(int j=0;j<3;j++)Bone(model,"Dedos palmeados",new Vector3(s*.2f,.035f,-.24f),new Vector3(s*(.18f+j*.055f),.02f,-.34f),.03f,trim);
                }
                var head=G(model,"Idle:Head",new Vector3(0,.37f,-.12f));Ball(head,"Cabeza ancha",Vector3.zero,new Vector3(.43f,.23f,.28f),leaf);
                for(int s=-1;s<=1;s+=2){Ball(head,"Ojo elevado",new Vector3(s*.14f,.12f,-.04f),Vector3.one*.15f,trim);Ball(head,"Pupila anfibia",new Vector3(s*.14f,.13f,-.112f),new Vector3(.04f,.075f,.03f),ink);}
            }
            void Crocodile()
            {
                Ball(model,"Lomo escamado",new Vector3(0,.23f,.02f),new Vector3(.32f,.28f,.62f),skin);
                var head=G(model,"Idle:Head",new Vector3(0,.25f,-.23f));Ball(head,"Mandíbula larga",new Vector3(0,-.025f,-.13f),new Vector3(.26f,.13f,.35f),ivory);Ball(head,"Hocico",new Vector3(0,.045f,-.12f),new Vector3(.29f,.15f,.37f),leaf);Eyes(head,.09f,.12f,-.13f,.046f);
                for(int j=0;j<5;j++)for(int s=-1;s<=1;s+=2)Gem(head,"Diente",new Vector3(s*.105f,0,-.04f-j*.05f),new Vector3(.028f,.065f,.028f),ivory);
                for(int j=0;j<5;j++)Gem(model,"Cresta dorsal",new Vector3(0,.39f,-.12f+j*.1f),new Vector3(.1f,.13f,.12f),dark);
                var tail=G(model,"Idle:Tail",new Vector3(0,.21f,.28f));Bone(tail,"Cola acorazada",Vector3.zero,new Vector3(.08f,-.08f,.24f),.13f,leaf);Gem(tail,"Punta de cola",new Vector3(.08f,-.08f,.24f),new Vector3(.12f,.1f,.24f),skin);
                for(int i=0;i<4;i++){int s=i%2==0?-1:1;Bone(model,"Pata reptil",new Vector3(s*.1f,.23f,i<2?-.15f:.2f),new Vector3(s*.25f,.065f,i<2?-.22f:.25f),.1f,leaf);Ball(model,"Garra",new Vector3(s*.25f,.045f,i<2?-.23f:.25f),new Vector3(.13f,.07f,.16f),ivory);}
            }
            void Quadruped()
            {
                bool deer=Has("ciervo"),rabbit=Has("rabbit"),camel=Has("cammel"),beaver=Has("bober"),panther=Has("panther"),fungal=Has("bestia-micelial"),ferret=Has("ferret"),mongoose=Has("mongoose");
                bool heavy=Has("bestia-de")||Has("wandering");
                Color fur=panther?new Color(.12f,.15f,.21f):rabbit?new Color(.8f,.76f,.63f):fungal?new Color(.27f,.38f,.3f):skin;
                float h=deer||camel?.52f:heavy?.37f:.3f;
                Ball(model,"Torso animal",new Vector3(0,h,.04f),new Vector3(heavy?.55f:beaver?.47f:ferret?.27f:.34f,heavy?.45f:beaver?.38f:.29f,ferret?.73f:beaver?.52f:.64f),fur);
                for(int i=0;i<4;i++)
                {
                    float side=i%2==0?-1:1,z=i<2?-.17f:.22f;
                    bool rear=i>=2;float thickness=heavy?.145f:beaver?.12f:rabbit&&rear?.14f:.09f;
                    var hip=new Vector3(side*.12f,h-.015f,z);
                    var knee=new Vector3(side*.155f,h*.52f,z+(rear?.07f:-.025f));
                    var ankle=new Vector3(side*.16f,.09f,z-.035f);
                    Ball(model,rear?"Anca anatómica":"Escápula anatómica",hip,new Vector3(thickness*1.65f,Mathf.Max(.19f,h*.58f),rear?.24f:.2f),fur);
                    Flesh(model,"Muslo articulado",hip,knee,thickness,fur);
                    Ball(model,"Rodilla animal",knee,Vector3.one*thickness*.92f,fur);
                    Flesh(model,"Pata articulada",knee,ankle,thickness*.63f,fur);
                    Ball(model,"Pezuña o garra",new Vector3(side*.16f,.055f,z-.065f),new Vector3(deer||camel?.095f:.12f,.095f,rabbit&&rear?.21f:.17f),dark);
                }
                float neck=camel||deer?.32f:.1f;Flesh(model,"Cuello",new Vector3(0,h-.025f,-.17f),new Vector3(0,h+neck+.07f,-.31f),heavy?.25f:camel?.2f:.18f,fur);
                var head=G(model,"Idle:Head",new Vector3(0,h+neck+.08f,-.29f));Ball(head,"Cabeza animal",Vector3.zero,new Vector3(heavy?.32f:beaver?.32f:ferret?.22f:.24f,beaver?.3f:.25f,.28f),fur);
                Ball(head,"Hocico",new Vector3(0,-.04f,-.15f),new Vector3(panther?.2f:.18f,.13f,camel?.27f:.18f),rabbit||beaver?ivory:Color.Lerp(fur,ivory,.24f));Ball(head,"Nariz",new Vector3(0,-.015f,camel?-.285f:-.24f),new Vector3(.08f,.055f,.05f),ink);Eyes(head,.085f,.055f,-.105f,.042f,panther?glow:(Color?)null);
                for(int s=-1;s<=1;s+=2)
                {
                    if(ferret||mongoose||beaver)Ball(head,"Oreja redonda",new Vector3(s*.12f,.13f,.01f),new Vector3(.13f,.14f,.065f),fur);
                    else Leaf(head,"Oreja",new Vector3(s*.105f,.14f,.01f),new Vector3(.11f,rabbit?.43f:.18f,.09f),fur,new Vector3(-8,0,-s*15));
                    if(beaver)Box(head,"Diente incisivo",new Vector3(s*.035f,-.105f,-.205f),new Vector3(.05f,.09f,.035f),ivory);
                    if(panther)for(int j=0;j<2;j++)Bone(head,"Bigote",new Vector3(s*.06f,-.05f,-.22f),new Vector3(s*.19f,-.025f-j*.04f,-.2f),.008f,ivory);
                    if(deer){Bone(head,"Asta",new Vector3(s*.085f,.13f,.04f),new Vector3(s*.2f,.42f,.09f),.045f,ivory);Bone(head,"Rama de asta",new Vector3(s*.15f,.29f,.06f),new Vector3(s*.3f,.37f,.02f),.035f,ivory);}
                    if(heavy)Gem(head,"Colmillo",new Vector3(s*.15f,-.04f,-.18f),new Vector3(.08f,.27f,.09f),ivory);
                }
                var tail=G(model,"Idle:Tail",new Vector3(0,h,.31f));var end=new Vector3(.06f,.06f,rabbit?.1f:.27f);
                Ball(tail,"Cola",end*.5f,new Vector3(beaver?.23f:.09f,beaver?.05f:.1f,end.z*1.7f),beaver?dark:fur);
                if(beaver)for(int j=0;j<4;j++)Bone(tail,"Talla de cola",new Vector3(-.08f,.03f,j*.06f),new Vector3(.08f,.03f,j*.06f),.014f,trim);
                if(panther||mongoose)Bone(tail,"Curva de cola",new Vector3(.05f,.03f,.17f),new Vector3(.17f,.19f,.23f),mongoose?.075f:.045f,fur);
                if(ferret||mongoose)Ball(model,"Pechera de pelaje",new Vector3(0,h+.025f,-.19f),new Vector3(.25f,.24f,.12f),ivory);
                if(camel){Ball(model,"Joroba",new Vector3(0,h+.2f,.08f),new Vector3(.32f,.34f,.38f),fur);Box(model,"Alforja tejida",new Vector3(.21f,h+.08f,.03f),new Vector3(.16f,.24f,.25f),cloth);}
                if(fungal||deer){Mushroom(model,new Vector3(.05f,h+.08f,.12f),.09f,.29f,cloth);for(int j=0;j<3;j++)Leaf(model,"Musgo del lomo",new Vector3(-.1f,h+.16f,j*.12f-.05f),new Vector3(.13f,.21f,.08f),leaf,new Vector3(20,30,25));}
                if(heavy)for(int j=0;j<3;j++)Ball(model,"Placa de roca",new Vector3(0,h+.2f,j*.15f-.12f),new Vector3(.39f,.16f,.2f),stone);
                if(Has("mongoose")||Has("ferret"))Ball(head,"Máscara de pelaje",new Vector3(0,.02f,-.116f),new Vector3(.25f,.09f,.04f),dark);
                if(panther)for(int s=-1;s<=1;s+=2)Leaf(model,"Hombrera de follaje",new Vector3(s*.16f,h+.12f,-.08f),new Vector3(.17f,.24f,.07f),cloth,new Vector3(0,0,s*55));
            }
            void Insect()
            {
                bool scorpion=Has("escorpion"),ant=Has("battle-ant"),fly=Has("fly"),darkFly=id=="dark-fly";
                Color shell=scorpion||ant?new Color(.48f,.27f,.12f):darkFly?trim:Has("escarabajo")?new Color(.68f,.4f,.16f):cloth;
                float y=fly?.5f:.2f;
                Ball(model,"Abdomen segmentado",new Vector3(0,y,.12f),new Vector3(ant?.29f:.4f,.27f,ant?.32f:.43f),shell);
                Ball(model,"Tórax",new Vector3(0,y,-.12f),new Vector3(.23f,.22f,.2f),dark);
                var head=G(model,"Idle:Head",new Vector3(0,y+.01f,-.29f));Ball(head,"Cabeza de insecto",Vector3.zero,new Vector3(.25f,.2f,.19f),shell);Eyes(head,.085f,.04f,-.075f,.055f,darkFly?glow:(Color?)null);
                for(int s=-1;s<=1;s+=2)
                {
                    Bone(head,"Antena",new Vector3(s*.055f,.07f,-.02f),new Vector3(s*.16f,.2f,-.13f),.025f,trim);
                    for(int j=0;j<3;j++)
                    {var joint=new Vector3(s*.24f,y-.04f,j*.13f-.16f);Bone(model,"Fémur de insecto",new Vector3(s*.08f,y,j*.13f-.16f),joint,.04f,shell);Bone(model,"Tibia de insecto",joint,new Vector3(s*.35f,fly?.3f:.02f,j*.16f-.18f),.025f,trim);}
                }
                if(scorpion)
                {
                    var tail=G(model,"Idle:Tail",new Vector3(0,y,.28f));Vector3 last=Vector3.zero;
                    for(int j=1;j<=5;j++){var next=new Vector3(0,j*.09f,Mathf.Sin(j*.53f)*.14f);Ball(tail,"Anillo de cola",next,Vector3.one*(.15f-j*.01f),shell);Bone(tail,"Nervio de cola",last,next,.07f,trim);last=next;}
                    Gem(tail,"Aguijón",last+new Vector3(0,.02f,-.07f),new Vector3(.07f,.09f,.17f),ink);
                    for(int s=-1;s<=1;s+=2){Bone(model,"Brazo de pinza",new Vector3(s*.12f,y,-.12f),new Vector3(s*.3f,y,-.34f),.07f,shell);Ball(model,"Pinza",new Vector3(s*.3f,y,-.39f),new Vector3(.17f,.12f,.21f),trim);Box(model,"Apertura de pinza",new Vector3(s*.3f,y+.055f,-.46f),new Vector3(.025f,.025f,.11f),dark);}
                }
                else if(fly)
                {
                    Ball(model,"Órgano luminiscente",new Vector3(0,y,.3f),new Vector3(.25f,.19f,.21f),glow);
                    for(int s=-1;s<=1;s+=2){var wing=G(model,s<0?"Idle:WingL":"Idle:WingR",new Vector3(s*.1f,y+.1f,0));Leaf(wing,"Ala de nácar",new Vector3(s*.18f,0,.04f),new Vector3(.2f,.54f,.035f),darkFly?glow:ivory,new Vector3(90,0,s*65));Bone(wing,"Nervio alar",Vector3.zero,new Vector3(s*.33f,.01f,.11f),.018f,trim);}
                }
                else if(!ant){Bone(model,"Sutura del caparazón",new Vector3(0,y+.14f,-.06f),new Vector3(0,y+.14f,.31f),.025f,trim);for(int j=0;j<3;j++)Ball(model,"Engarce del caparazón",new Vector3(0,y+.15f,j*.11f),Vector3.one*.045f,glow);}
            }
            void Bird()
            {
                Color feather=new Color(.85f,.25f,.07f);
                Ball(model,"Torso de ave",new Vector3(0,.55f,0),new Vector3(.26f,.38f,.3f),feather);
                var head=G(model,"Idle:Head",new Vector3(0,.79f,-.08f));Ball(head,"Cabeza del fénix",Vector3.zero,Vector3.one*.22f,trim);Gem(head,"Pico",new Vector3(0,-.025f,-.14f),new Vector3(.07f,.08f,.18f),dark);Eyes(head,.07f,.03f,-.08f,.04f);
                for(int s=-1;s<=1;s+=2)
                {
                    var wing=G(model,s<0?"Idle:WingL":"Idle:WingR",new Vector3(s*.1f,.62f,0));
                    for(int j=0;j<5;j++)Leaf(wing,"Pluma de brasa",new Vector3(s*(.09f+j*.052f),-.03f*j,.02f+j*.027f),new Vector3(.11f,.4f-j*.025f,.045f),j%2==0?trim:feather,new Vector3(15,0,s*65));
                    Bone(model,"Pata de ave",new Vector3(s*.07f,.43f,0),new Vector3(s*.085f,.3f,-.04f),.03f,dark);
                }
                var tail=G(model,"Idle:Tail",new Vector3(0,.48f,.1f));for(int j=-1;j<=1;j++)Leaf(tail,"Pluma caudal",new Vector3(j*.06f,-.1f,.13f),new Vector3(.1f,.38f,.04f),j==0?trim:feather,new Vector3(-55,0,j*15));
            }
            void Fungal()
            {
                bool king=Has("rey-micelial"),giant=Has("coloso")||Has("ancestro")||king,tree=Has("guardian-del-bosque")||Has("ancestro"),small=id=="espora"||Has("mushroom-token");
                float height=giant?.88f:small?.4f:.62f;
                var body=G(model,"Idle:Body",Vector3.zero);
                for(int s=-1;s<=1;s+=2){Ball(body,"Raíz del pie",new Vector3(s*.13f,.07f,-.03f),new Vector3(.18f,.14f,.25f),dark);Bone(body,"Pierna de raíz",new Vector3(s*.13f,.12f,0),new Vector3(s*.12f,.32f,0),.13f,skin);}
                Ball(body,tree?"Tronco tallado":"Torso micelial",new Vector3(0,height*.6f,0),new Vector3(giant?.52f:.35f,height*.78f,.3f),tree?skin:ivory);
                for(int s=-1;s<=1;s+=2)
                {
                    var arm=G(body,s<0?"Idle:ArmL":"Idle:ArmR",new Vector3(s*(giant?.27f:.18f),height*.78f,0));Bone(arm,"Brazo de raíz",Vector3.zero,new Vector3(s*.13f,-.2f,-.05f),giant?.14f:.075f,skin);Ball(arm,"Mano",new Vector3(s*.13f,-.2f,-.05f),Vector3.one*.11f,ivory);
                    Leaf(body,"Placa de musgo",new Vector3(s*.15f,height*.7f,-.13f),new Vector3(.18f,.3f,.07f),leaf,new Vector3(0,0,s*20));
                }
                var head=G(body,"Idle:Head",new Vector3(0,height,0));
                Ball(head,"Rostro del bosque",Vector3.zero,new Vector3(tree?.32f:.29f,.28f,.25f),tree?skin:ivory);Eyes(head,.075f,.025f,-.12f,.044f,tree?glow:(Color?)null);
                if(tree)
                {
                    for(int s=-1;s<=1;s+=2){Bone(head,"Rama coronaria",new Vector3(s*.09f,.1f,0),new Vector3(s*.25f,.32f,0),.06f,skin);Leaf(head,"Follaje",new Vector3(s*.22f,.29f,.03f),new Vector3(.3f,.32f,.19f),leaf,new Vector3(0,0,s*35));}
                }
                else
                {
                    Color cap=Has("cazador")?new Color(.23f,.48f,.42f):Has("chaman")?new Color(.39f,.28f,.57f):Has("matriarca")?new Color(.65f,.32f,.47f):new Color(.29f,.5f,.36f);
                    P(head,"Capucha de laminillas",m.MushroomCap,new Vector3(0,.1f,0),new Vector3(giant?.66f:small?.41f:.52f,.72f,.49f),cap);
                    for(int j=0;j<5;j++)Ball(head,"Perla de espora",Rad(j,5,.15f,.27f),new Vector3(.055f,.035f,.055f),glow);
                }
                if(Has("recolector")){Ball(body,"Cesta de esporas",new Vector3(0,height*.5f,.24f),new Vector3(.3f,.3f,.2f),trim);Mushroom(body,new Vector3(0,height*.6f,.25f),.12f,.22f,cloth);}
                if(Has("explorador")||Has("cazador")){Bone(body,"Arco orgánico",new Vector3(.3f,.18f,-.12f),new Vector3(.38f,.72f,-.04f),.035f,trim);Bone(body,"Cuerda de arco",new Vector3(.3f,.18f,-.12f),new Vector3(.3f,.72f,-.12f),.012f,ivory);}
                if(Has("chaman")||Has("matriarca")){for(int j=0;j<3;j++)Mushroom(body,new Vector3((j-1)*.19f,.2f,.19f),.25f+j*.06f,.25f,cloth);Ball(body,"Semilla ritual",new Vector3(.31f,height*.55f,-.07f),Vector3.one*.14f,glow);}
                if(giant)for(int j=0;j<4;j++){float s=j%2==0?-1:1;Ball(body,"Corteza protectora",new Vector3(s*.23f,.3f+j*.13f,.015f),new Vector3(.22f,.28f,.36f),stone);Mushroom(body,new Vector3(s*.21f,.34f+j*.13f,.1f),.12f,.25f,cloth);}
                if(king)
                {
                    for(int j=0;j<5;j++)Gem(head,"Corona de ámbar",Rad(j,5,.21f,.37f),new Vector3(.07f,.19f,.07f),trim);
                    Leaf(body,"Manto de corteza",new Vector3(0,.5f,.19f),new Vector3(.6f,.75f,.1f),cloth,new Vector3(-10,0,0));
                }
            }
            void AshGolem()
            {
                bool titan=id=="titan-de-ceniza";
                Color rock=new Color(.24f,.21f,.23f),ember=new Color(1,.42f,.09f),gold=new Color(.71f,.48f,.23f);
                var body=G(model,"Idle:Body",Vector3.zero);
                for(int s=-1;s<=1;s+=2)
                {
                    Ball(body,"Pie de basalto",new Vector3(s*.16f,.1f,-.035f),new Vector3(.24f,.19f,.3f),rock);
                    Gem(body,"Pierna monolítica",new Vector3(s*.15f,.29f,0),new Vector3(.21f,.4f,.24f),rock);
                    var arm=G(body,s<0?"Idle:ArmL":"Idle:ArmR",new Vector3(s*.26f,.76f,0));
                    Ball(arm,"Hombrera de basalto",Vector3.zero,new Vector3(.27f,.29f,.3f),rock);
                    Gem(arm,"Grieta del hombro",new Vector3(0,.02f,-.15f),new Vector3(.035f,.19f,.025f),ember);
                    Bone(arm,"Brazo de roca",new Vector3(s*.05f,-.09f,0),new Vector3(s*.08f,-.31f,-.04f),.17f,rock);
                    Ball(arm,"Puño tallado",new Vector3(s*.08f,-.35f,-.04f),Vector3.one*.22f,gold);
                }
                Ball(body,"Coraza de roca",new Vector3(0,.65f,0),new Vector3(.53f,.57f,.34f),rock);
                for(int j=0;j<3;j++)Gem(body,"Fisura ardiente",new Vector3((j-1)*.1f,.65f,-.17f),new Vector3(.04f,.3f-j*.04f,.03f),ember);
                Ring(body,"Sello pectoral",new Vector3(0,.73f,-.19f),.12f,.027f,gold,8,Quaternion.Euler(90,0,0));
                var head=G(body,"Idle:Head",new Vector3(0,1,0));Gem(head,"Cabeza de escoria",Vector3.zero,new Vector3(.3f,.34f,.29f),rock);Eyes(head,.08f,.01f,-.12f,.04f,ember);
                for(int s=-1;s<=1;s+=2)Gem(head,"Cuerno de obsidiana",new Vector3(s*.14f,.16f,.02f),new Vector3(.1f,.31f,.12f),gold);
                if(titan){for(int j=0;j<3;j++)Gem(body,"Chimenea dorsal",new Vector3((j-1)*.14f,.94f,.14f),new Vector3(.14f,.42f,.14f),rock);body.localScale=Vector3.one*1.08f;}
                else body.localScale=Vector3.one*.78f;
            }
            void Tortoise()
            {
                Ball(model,"Cuerpo de tortuga",new Vector3(0,.2f,.02f),new Vector3(.43f,.25f,.53f),skin);
                Ball(model,"Caparazón del oasis",new Vector3(0,.36f,.04f),new Vector3(.57f,.38f,.57f),cloth);
                for(int j=0;j<6;j++)Ball(model,"Placa del caparazón",Rad(j,6,.19f,.45f),new Vector3(.19f,.13f,.2f),stone);
                Gem(model,"Cristal del manantial",new Vector3(0,.58f,.04f),new Vector3(.16f,.25f,.16f),glow);
                for(int i=0;i<4;i++)Ball(model,"Pata de tortuga",new Vector3(i%2==0?-.23f:.23f,.08f,i<2?-.18f:.23f),new Vector3(.18f,.16f,.22f),skin);
                var head=G(model,"Idle:Head",new Vector3(0,.24f,-.32f));Ball(head,"Cabeza de tortuga",Vector3.zero,new Vector3(.23f,.23f,.27f),skin);Eyes(head,.075f,.045f,-.1f,.04f);
            }
            void Humanoid()
            {
                bool mummy=Has("mummy"),priest=Has("sacerdote"),guardian=Has("guardian"),royal=Has("emperador"),avatar=Has("avatar"),ember=Has("ember");
                if(ember){stone=new Color(.19f,.16f,.16f);skin=stone;glow=new Color(1,.41f,.07f);}
                var body=G(model,"Idle:Body",new Vector3(0,avatar?.28f:0,0));
                for(int s=-1;s<=1;s+=2){Ball(body,"Bota",new Vector3(s*.12f,.07f,-.035f),new Vector3(.17f,.14f,.25f),dark);Bone(body,"Pierna",new Vector3(s*.12f,.13f,0),new Vector3(s*.11f,.43f,0),.11f,mummy?ivory:skin);}
                P(body,"Túnica modelada",m.Bell,new Vector3(0,.25f,0),new Vector3(.48f,priest||royal?.59f:.4f,.4f),mummy?ivory:cloth);
                Ball(body,"Torso",new Vector3(0,.68f,0),new Vector3(.36f,.35f,.24f),mummy?ivory:ember?stone:skin);
                Stem(body,"Cinturón",new Vector3(0,.5f,0),.38f,.05f,trim);Gem(body,"Broche",new Vector3(0,.54f,-.21f),new Vector3(.08f,.12f,.05f),glow);
                if(!mummy&&!ember){Ball(body,"Pectoral de bronce",new Vector3(0,.74f,-.1f),new Vector3(.39f,.15f,.12f),trim);for(int s=-1;s<=1;s+=2)Ball(body,"Hombrera",new Vector3(s*.23f,.78f,0),new Vector3(.2f,.15f,.23f),guardian?stone:trim);}
                var head=G(body,"Idle:Head",new Vector3(0,.99f,0));Ball(head,"Rostro",Vector3.zero,new Vector3(.25f,.3f,.24f),mummy?ivory:skin);Eyes(head,.065f,.02f,-.112f,.037f,ember?glow:(Color?)null);
                Ball(head,"Nariz",new Vector3(0,-.015f,-.13f),new Vector3(.05f,.07f,.06f),mummy?ivory:skin);
                if(mummy)
                {
                    for(int j=0;j<4;j++)Stem(head,"Venda de cabeza",new Vector3(0,-.13f+j*.075f,0),.26f,.026f,j%2==0?ivory:stone);
                    for(int j=0;j<6;j++)Stem(body,"Venda del torso",new Vector3(0,.52f+j*.045f,0),.35f,.018f,stone);
                }
                else if(ember){for(int s=-1;s<=1;s+=2)Gem(head,"Cuerno de obsidiana",new Vector3(s*.12f,.15f,.02f),new Vector3(.12f,.3f,.12f),dark);for(int j=0;j<3;j++)Gem(body,"Grieta incandescente",new Vector3((j-1)*.08f,.65f,-.13f),new Vector3(.035f,.24f,.025f),glow);}
                else
                {
                    Ball(head,"Casquete",new Vector3(0,.12f,.015f),new Vector3(.31f,.18f,.28f),priest?ivory:cloth);
                    if(guardian||royal){P(head,"Corona de guardia",m.Cone,new Vector3(0,.13f,.02f),new Vector3(.28f,.2f,.27f),trim);Gem(head,"Cristal de rango",new Vector3(0,.16f,-.13f),new Vector3(.07f,.13f,.04f),glow);}
                    else Leaf(head,"Paño de nuca",new Vector3(0,-.07f,.13f),new Vector3(.3f,.4f,.06f),cloth,new Vector3(8,0,0));
                }
                for(int s=-1;s<=1;s+=2)
                {
                    var arm=G(body,s<0?"Idle:ArmL":"Idle:ArmR",new Vector3(s*.22f,.78f,0));
                    Bone(arm,"Brazo",Vector3.zero,new Vector3(s*.08f,-.25f,-.08f),.09f,mummy?ivory:skin);Ball(arm,"Guante",new Vector3(s*.08f,-.25f,-.08f),Vector3.one*.105f,mummy?ivory:trim);
                    if(s>0&&!mummy)
                    {
                        if(priest||avatar){Ball(arm,"Orbe ritual",new Vector3(.1f,-.18f,-.12f),Vector3.one*.16f,glow);Ring(arm,"Engaste",new Vector3(.1f,-.18f,-.12f),.12f,.022f,trim,8,Quaternion.Euler(90,0,0));}
                        else{Bone(arm,"Empuñadura",new Vector3(.08f,-.29f,-.08f),new Vector3(.08f,-.14f,-.08f),.045f,dark);Gem(arm,"Hoja de khopesh",new Vector3(.12f,.06f,-.08f),new Vector3(.13f,.48f,.045f),ivory);Bone(arm,"Guarda",new Vector3(.015f,-.1f,-.08f),new Vector3(.19f,-.1f,-.08f),.04f,trim);}
                    }
                    if(s<0&&guardian){Ball(arm,"Escudo tallado",new Vector3(-.1f,-.18f,-.13f),new Vector3(.29f,.43f,.1f),stone);Gem(arm,"Insignia del escudo",new Vector3(-.1f,-.18f,-.19f),new Vector3(.1f,.16f,.025f),trim);}
                }
                if(avatar)Ring(body,"Disco solar",new Vector3(0,.96f,.14f),.32f,.04f,trim,16,Quaternion.Euler(90,0,0));
                if(Has("halconero"))for(int s=-1;s<=1;s+=2){var wing=G(body,s<0?"Idle:WingL":"Idle:WingR",new Vector3(s*.18f,.72f,.08f));for(int j=0;j<4;j++)Leaf(wing,"Pluma de halcón",new Vector3(s*(.12f+j*.06f),-.03f*j,0),new Vector3(.1f,.34f,.04f),ivory,new Vector3(0,0,s*65));}
                if(royal){Leaf(body,"Manto ceremonial",new Vector3(0,.6f,.17f),new Vector3(.5f,.75f,.05f),cloth,new Vector3(-8,0,0));for(int j=0;j<3;j++)Gem(body,"Sello de rango",new Vector3((j-1)*.095f,.73f,-.18f),new Vector3(.05f,.08f,.025f),glow);}
            }
            void VoidCreature()
            {
                if(id=="void-hole")
                {
                    P(model,"Boca del vacío",m.Cylinder,Vector3.up*.015f,new Vector3(.62f,.04f,.62f),ink);Ring(model,"Borde quebrado",Vector3.up*.05f,.3f,.07f,trim);
                    var body=G(model,"Idle:Body",Vector3.up*.08f);P(body,"Espiral abisal",m.Vortex,Vector3.zero,Vector3.one*.5f,glow);
                    for(int i=0;i<7;i++)Gem(model,"Diente de falla",Rad(i,7,.31f,.12f),new Vector3(.1f,.21f,.1f),stone);return;
                }
                if(id=="the-eternal-eye")
                {
                    var head=G(model,"Idle:Head",new Vector3(0,.67f,0));Ball(head,"Esfera ocular",Vector3.zero,new Vector3(.48f,.4f,.37f),ivory);Ball(head,"Iris abisal",new Vector3(0,0,-.18f),new Vector3(.27f,.28f,.06f),glow);Gem(head,"Pupila vertical",new Vector3(0,0,-.22f),new Vector3(.07f,.2f,.03f),ink);
                    Ring(head,"Anillo de contención",Vector3.zero,.32f,.045f,trim,14,Quaternion.Euler(90,0,0));
                    for(int i=0;i<5;i++)Leaf(head,"Tentáculo de sombra",new Vector3((i-2)*.075f,-.3f,.06f),new Vector3(.08f,.36f,.07f),cloth,new Vector3(i*7,0,(i-2)*12));return;
                }
                bool sprite=id=="sprinting-sprite";
                var core=G(model,"Idle:Body",new Vector3(0,sprite?.47f:.34f,0));Gem(core,"Núcleo cristalino",Vector3.zero,new Vector3(sprite?.23f:.4f,.48f,.27f),glow);
                for(int s=-1;s<=1;s+=2)
                {
                    if(sprite){Bone(core,"Pierna de sombra",new Vector3(s*.07f,-.15f,0),new Vector3(s*.15f,-.37f,-s*.08f),.055f,trim);Leaf(core,"Estela mineral",new Vector3(s*.11f,.05f,.16f),new Vector3(.12f,.5f,.05f),cloth,new Vector3(-55,0,s*20));}
                    else for(int j=0;j<2;j++){var joint=new Vector3(s*.26f,-.04f,j==0?-.17f:.17f);Bone(core,"Extremidad de mineral",new Vector3(s*.12f,-.04f,0),joint,.085f,trim);Gem(core,"Pie de geoda",joint+Vector3.down*.16f,new Vector3(.13f,.3f,.14f),stone);}
                }
                var head2=G(core,"Idle:Head",new Vector3(0,.24f,-.02f));Gem(head2,"Máscara de geoda",Vector3.zero,new Vector3(.23f,.23f,.2f),stone);Eyes(head2,.06f,.015f,-.09f,.03f,glow);
                if(!sprite)for(int i=0;i<3;i++)Gem(core,"Cresta de cuarzo",new Vector3((i-1)*.12f,.15f,.08f),new Vector3(.11f,.31f,.11f),trim);
            }
            void Structure()
            {
                if(biome==Biome.Void){VoidBuilding();return;}
                if(card.Has("Passable")||Has("seal")||Has("pool")||Has("arena-profunda")||Has("dunas-movedizas")||Has("oasis")){GroundRelic();return;}
                if(Has("anthill")){Anthill();return;}
                if(Has("camp")||Has("campamento")){Camp();return;}
                if(Has("vigilant")){Watchtower();return;}
                if(Has("berry")){Shrub();return;}
                if(card.leader=="ZUKGROK"){FungalBuilding();return;}
                DesertBuilding();
            }
            void GroundRelic()
            {
                bool water=Has("oasis")||Has("pool")||Has("frogpit");
                P(model,"Cuenco ritual",m.Cylinder,Vector3.zero,new Vector3(.69f,.055f,.69f),dark);
                P(model,water?"Agua del manantial":"Grabado central",m.Cylinder,Vector3.up*.056f,new Vector3(.57f,.015f,.57f),water?glow:Has("dunas")||Has("arena-profunda")?dark:cloth);
                Ring(model,"Borde cincelado",Vector3.up*.06f,.31f,.044f,trim);
                for(int j=0;j<6;j++)
                {
                    var pos=Rad(j,6,.3f,.095f);Gem(model,"Runas del borde",pos,new Vector3(.045f,.085f,.045f),glow);
                    if(water&&j%2==0)Leaf(model,"Junco",pos+Vector3.up*.08f,new Vector3(.06f,.25f,.035f),leaf,new Vector3(0,j*60,10));
                }
                if(Has("valiant"))
                {
                    for(int s=-1;s<=1;s+=2){Bone(model,"Espadas cruzadas",new Vector3(s*.15f,.1f,.15f),new Vector3(-s*.15f,.1f,-.15f),.035f,ivory);Gem(model,"Pomo de espada",new Vector3(s*.15f,.105f,.15f),new Vector3(.06f,.035f,.06f),trim);}
                }
                else if(Has("courage"))for(int j=0;j<3;j++)Leaf(model,"Pluma del valor",new Vector3((j-1)*.1f,.1f,0),new Vector3(.09f,.3f,.035f),ivory,new Vector3(90,0,25));
                else if(Has("seal")){Gem(model,"Escudo del sello",new Vector3(0,.085f,0),new Vector3(.23f,.05f,.31f),ivory);Bone(model,"Cruz protectora",new Vector3(-.09f,.12f,0),new Vector3(.09f,.12f,0),.028f,trim);Bone(model,"Cruz protectora",new Vector3(0,.12f,-.11f),new Vector3(0,.12f,.11f),.028f,trim);}
                if(Has("dunas")||Has("arena-profunda"))for(int j=0;j<3;j++)Ball(model,"Duna ondulada",new Vector3((j-1)*.17f,.07f,.03f*j),new Vector3(.28f,.12f,.39f),stone);
            }
            void Camp()
            {
                Box(model,"Tarima",new Vector3(0,.035f,0),new Vector3(.6f,.07f,.57f),dark);
                var tent=G(model,"Tienda de campaña",new Vector3(0,.08f,.08f));
                for(int s=-1;s<=1;s+=2){var side=Box(tent,"Lona de tienda",new Vector3(s*.14f,.23f,0),new Vector3(.035f,.53f,.48f),cloth);side.localRotation=Quaternion.Euler(0,0,s*32);Bone(tent,"Poste",new Vector3(s*.28f,0,-.25f),new Vector3(0,.46f,-.25f),.035f,trim);}
                Bone(tent,"Cumbrera",new Vector3(0,.46f,-.26f),new Vector3(0,.46f,.26f),.04f,dark);
                for(int j=0;j<2;j++)Ball(model,"Suministros",new Vector3(-.25f+j*.14f,.12f,-.23f),new Vector3(.14f,.2f,.14f),ivory);
                if(Has("med")){Box(model,"Señal médica",new Vector3(.25f,.35f,-.18f),new Vector3(.18f,.05f,.03f),ivory);Box(model,"Cruz de cura",new Vector3(.25f,.35f,-.18f),new Vector3(.05f,.18f,.03f),ivory);Stem(model,"Poste de señal",new Vector3(.25f,.04f,-.16f),.028f,.39f,trim);}
                else Ball(model,"Brasero",new Vector3(.23f,.08f,-.22f),new Vector3(.18f,.12f,.18f),trim);
            }
            void Watchtower()
            {
                for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)Bone(model,"Pilar de madera",new Vector3(s*.23f,0,z*.2f),new Vector3(s*.16f,.77f,z*.14f),.07f,skin);
                for(int s=-1;s<=1;s+=2)Bone(model,"Riostra",new Vector3(s*.2f,.15f,-.2f),new Vector3(-s*.17f,.68f,-.14f),.045f,trim);
                Box(model,"Mirador",new Vector3(0,.78f,0),new Vector3(.56f,.09f,.5f),skin);
                for(int j=0;j<4;j++)Box(model,"Almena",new Vector3(j%2==0?-.24f:.24f,.9f,j<2?-.19f:.19f),new Vector3(.08f,.22f,.08f),trim);
                P(model,"Cubierta de hojas",m.Cone,new Vector3(0,1.02f,0),new Vector3(.72f,.27f,.65f),cloth);
                for(int j=0;j<5;j++)Bone(model,"Peldaño",new Vector3(-.1f,.15f+j*.12f,-.24f),new Vector3(.1f,.15f+j*.12f,-.24f),.027f,ivory);
            }
            void Shrub()
            {
                for(int j=0;j<5;j++){var pos=Rad(j,5,.18f,.16f);Bone(model,"Rama del arbusto",Vector3.zero,pos+Vector3.up*.16f,.07f,skin);Ball(model,"Follaje tallado",pos+Vector3.up*.13f,new Vector3(.3f,.34f,.3f),j%2==0?leaf:cloth);for(int k=0;k<3;k++)Ball(model,"Baya",pos+Rad(k,3,.13f,.18f),Vector3.one*.075f,new Color(.61f,.19f,.3f));}
            }
            void Anthill()
            {
                for(int j=0;j<4;j++)Ball(model,"Estrato de termitero",new Vector3(-.03f*j,.12f+j*.15f,0),new Vector3(.6f-j*.1f,.3f,.56f-j*.1f),stone);
                Ball(model,"Entrada oscura",new Vector3(0,.13f,-.265f),new Vector3(.17f,.19f,.035f),dark);
                for(int j=0;j<6;j++)Ball(model,"Piedra de entrada",Rad(j,6,.25f,.02f),Vector3.one*.1f,trim);
            }
            void FungalBuilding()
            {
                bool tree=Has("arbol")||Has("bosque-eterno"),nest=Has("nido")||Has("firefly"),root=Has("root")||Has("red-"),garden=Has("jardin")||Has("semillero"),tower=Has("torre"),heart=Has("corazon"),frog=Has("frogpit");
                if(frog){GroundRelic();for(int j=0;j<4;j++)Ball(model,"Piedra del estanque",Rad(j,4,.28f,.06f),new Vector3(.18f,.13f,.18f),stone);return;}
                for(int j=0;j<5;j++)Bone(model,"Raíz extendida",Vector3.up*.08f,Rad(j,5,.34f,.025f),.075f,skin);
                if(tree)
                {
                    Bone(model,"Tronco ancestral",Vector3.zero,new Vector3(.04f,.9f,0),.24f,skin);
                    for(int j=0;j<5;j++){var end=Rad(j,5,.26f,.77f+j%2*.15f);Bone(model,"Rama",new Vector3(0,.49f,0),end,.1f,skin);Ball(model,"Copa de follaje",end,new Vector3(.36f,.36f,.34f),j%2==0?leaf:cloth);Mushroom(model,Rad(j,5,.2f,.12f),.14f,.25f,trim);}
                    return;
                }
                if(root){for(int j=0;j<6;j++){var a=Rad(j,6,.12f,.08f);var b=Rad(j+1,6,.34f,.08f);Bone(model,"Red de raíces",a,b,.07f,ivory);Mushroom(model,b,.12f+j%2*.1f,.22f,cloth);}Gem(model,"Nódulo micelial",new Vector3(0,.2f,0),Vector3.one*.26f,glow);return;}
                if(nest)
                {
                    Ring(model,"Nido tejido",Vector3.up*.15f,.25f,.085f,skin);
                    for(int j=0;j<5;j++){Bone(model,"Rama trenzada",Rad(j,5,.3f,.07f),Rad(j+2,5,.28f,.23f),.04f,trim);Ball(model,"Huevo de espora",Rad(j,5,.12f,.14f),new Vector3(.1f,.16f,.1f),glow);}
                    if(Has("firefly"))for(int j=0;j<3;j++){Bone(model,"Junco del nido",Rad(j,3,.26f,0),Rad(j,3,.25f,.47f),.025f,leaf);Ball(model,"Luciérnaga",Rad(j,3,.25f,.48f),Vector3.one*.07f,glow);}return;
                }
                if(garden){for(int j=0;j<7;j++)Mushroom(model,j==6?Vector3.zero:Rad(j,6,.24f,0),.14f+j%3*.08f,.22f+j%2*.07f,j%2==0?cloth:trim);return;}
                if(heart){Ball(model,"Núcleo orgánico",new Vector3(0,.48f,0),new Vector3(.46f,.58f,.4f),cloth);for(int j=0;j<6;j++){Bone(model,"Costilla protectora",Rad(j,6,.28f,.08f),Rad(j,6,.2f,.62f),.075f,ivory);Ball(model,"Nódulo vital",Rad(j,6,.18f,.54f),Vector3.one*.09f,glow);}return;}
                float h=tower?.88f:.48f;Stem(model,"Tallo arquitectónico",Vector3.zero,tower?.25f:.39f,h,ivory);
                Ball(model,"Entrada del santuario",new Vector3(0,.17f,-.2f),new Vector3(.17f,.28f,.03f),dark);
                Mushroom(model,new Vector3(0,h-.12f,0),.12f,tower?.58f:.75f,Has("sueno")?new Color(.43f,.3f,.61f):cloth);
                if(tower)for(int j=0;j<2;j++)Mushroom(model,new Vector3(j==0?-.2f:.2f,.22f+j*.2f,.02f),.1f,.35f,leaf);
                for(int j=0;j<4;j++)Leaf(model,"Laminilla tallada",Rad(j,4,.18f,.3f),new Vector3(.04f,.36f,.16f),trim,new Vector3(0,j*90,0));
            }
            void DesertBuilding()
            {
                bool obelisk=Has("obelisco"),throne=Has("trono"),city=Has("ciudad");
                for(int j=0;j<3;j++)Box(model,"Escalón de arenisca",new Vector3(0,.035f+j*.065f,0),new Vector3(.69f-j*.08f,.065f,.62f-j*.07f),j%2==0?stone:ivory);
                if(obelisk)
                {
                    P(model,"Obelisco biselado",m.Bell,new Vector3(0,.2f,0),new Vector3(.31f,.75f,.31f),stone);
                    P(model,"Piramidión de bronce",m.Cone,new Vector3(0,.95f,0),new Vector3(.22f,.25f,.22f),trim);
                    for(int j=0;j<4;j++)Gem(model,"Glifo incrustado",new Vector3(0,.38f+j*.13f,-.14f+j*.017f),new Vector3(.065f,.075f,.025f),Has("ruptura")?new Color(.95f,.38f,.15f):glow);
                }
                else if(throne)
                {
                    Box(model,"Asiento",new Vector3(0,.4f,0),new Vector3(.36f,.14f,.36f),trim);Box(model,"Respaldo tallado",new Vector3(0,.69f,.15f),new Vector3(.4f,.65f,.1f),stone);
                    for(int s=-1;s<=1;s+=2){Stem(model,"Brazal del trono",new Vector3(s*.23f,.2f,-.06f),.085f,.39f,trim);Ball(model,"Remate solar",new Vector3(s*.23f,.6f,-.06f),Vector3.one*.12f,ivory);}
                    Ring(model,"Sol del trono",new Vector3(0,.83f,.07f),.16f,.035f,trim,10,Quaternion.Euler(90,0,0));
                }
                else
                {
                    Box(model,"Cámara del templo",new Vector3(0,.4f,.05f),new Vector3(.43f,.4f,.36f),stone);
                    Box(model,"Puerta tallada",new Vector3(0,.34f,-.14f),new Vector3(.17f,.26f,.025f),dark);
                    for(int s=-1;s<=1;s+=2){Stem(model,"Columna acanalada",new Vector3(s*.25f,.2f,-.2f),.11f,.5f,ivory);Stem(model,"Capitel de bronce",new Vector3(s*.25f,.66f,-.2f),.17f,.06f,trim);}
                    Box(model,"Dintel",new Vector3(0,.76f,-.03f),new Vector3(.64f,.1f,.5f),trim);P(model,"Techo piramidal",m.Cone,new Vector3(0,.81f,.02f),new Vector3(.52f,.3f,.43f),stone);
                    if(city)for(int j=0;j<3;j++)Box(model,"Ruina lateral",new Vector3((j-1)*.25f,.16f,.26f),new Vector3(.16f,.28f+j*.08f,.15f),stone);
                }
                for(int s=-1;s<=1;s+=2)Gem(model,"Engarce turquesa",new Vector3(s*.26f,.2f,-.25f),Vector3.one*.075f,glow);
            }
            void VoidBuilding()
            {
                for(int j=0;j<6;j++)Gem(model,"Cimiento de obsidiana",Rad(j,6,.26f,.1f),new Vector3(.19f,.26f,.19f),stone);
                if(id=="portal")
                {
                    Ring(model,"Arco poligonal",new Vector3(0,.55f,0),.36f,.1f,trim,12,Quaternion.Euler(90,0,0));
                    Ring(model,"Borde interior",new Vector3(0,.55f,-.015f),.28f,.026f,glow,12,Quaternion.Euler(90,0,0));
                    for(int s=-1;s<=1;s+=2)Gem(model,"Anclaje del portal",new Vector3(s*.25f,.21f,0),new Vector3(.19f,.42f,.2f),stone);
                    P(model,"Remolino del umbral",m.Vortex,new Vector3(0,.55f,0),Vector3.one*.39f,cloth).localRotation=Quaternion.Euler(90,0,0);
                }
                else if(id=="insolator")
                {
                    for(int j=0;j<4;j++){Gem(model,"Aguja de aislamiento",Rad(j,4,.22f,.42f),new Vector3(.16f,.79f,.16f),trim);Bone(model,"Conducto",Rad(j,4,.22f,.58f),new Vector3(0,.6f,0),.03f,glow);}
                    Gem(model,"Reactor cautivo",new Vector3(0,.55f,0),new Vector3(.24f,.39f,.24f),glow);
                }
                else
                {
                    Stem(model,"Pedestal del faro",Vector3.zero,.26f,.45f,stone);Gem(model,"Cristal del faro",new Vector3(0,.67f,0),new Vector3(.29f,.58f,.29f),glow);
                    Ring(model,"Corona del faro",new Vector3(0,.43f,0),.27f,.035f,trim,10);
                    for(int j=0;j<3;j++)Gem(model,"Contrafuerte",Rad(j,3,.24f,.3f),new Vector3(.13f,.5f,.13f),trim);
                }
            }
        }
    }
}
