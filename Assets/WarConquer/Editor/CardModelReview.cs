#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WarConquer.Editor
{
    public static class CardModelReview
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("War & Conquer/Modelos/Revisar esculturas de cartas")]
        public static void Gallery()
        {
            var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Card model review");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
            var m=new BoardMeshFactory();const string output="Library/CardModelQA";Directory.CreateDirectory(output);
            try
            {
                var camera=new GameObject("Gallery camera").AddComponent<Camera>();camera.transform.SetParent(root.transform);camera.scene=scene;
                camera.orthographic=true;camera.orthographicSize=3.8f;camera.cullingMask=1<<BoardMeshFactory.Layer;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.046f,.06f);
                camera.transform.position=new Vector3(0,12,-6);camera.transform.LookAt(new Vector3(0,.35f,1.9f));
                var light=new GameObject("Key").AddComponent<Light>();light.transform.SetParent(root.transform);light.type=LightType.Directional;light.intensity=1.4f;light.color=new Color(1,.94f,.84f);light.transform.rotation=Quaternion.Euler(42,-28,0);light.cullingMask=camera.cullingMask;
                var fill=new GameObject("Fill").AddComponent<Light>();fill.transform.SetParent(root.transform);fill.type=LightType.Directional;fill.intensity=.6f;fill.color=new Color(.62f,.75f,1);fill.transform.rotation=Quaternion.Euler(20,150,0);fill.cullingMask=camera.cullingMask;
                var cards=CardCatalog.Load().All.ToArray();int reviewed=0;
                var identities=new Dictionary<string,string>();var costs=new List<string>();
                foreach(var c in cards.Where(c=>c.category==Category.Spell))
                {var empty=m.Group(root.transform,"Excluded "+c.id,Vector3.zero);m.PieceModel(empty,c,Color.yellow);Check(empty.childCount==0,"Spell received a model: "+c.id);BoardMeshFactory.Release(empty.gameObject);}
                foreach(var faction in new[]{"ZUKGROK","SAHRIA","FAUNAR","XIL'THAR","GENERIC"})
                {
                    var entries=cards.Where(c=>c.leader==faction&&c.category!=Category.Spell).OrderBy(c=>c.IsStructure).ToArray();
                    for(int page=0;page*18<entries.Length;page++)
                    {
                        var gallery=m.Group(root.transform,"Gallery "+faction,Vector3.zero);
                        for(int i=0;i<18&&page*18+i<entries.Length;i++)
                        {
                            var c=entries[page*18+i];var slot=m.Group(gallery,c.id,new Vector3((i%6-2.5f)*1.55f,0,i/6*1.8f));
                            m.PieceModel(slot,c,GrayboxUI.PlayerColor(page%2));reviewed++;
                            Check(slot.Find("Escultura de carta").GetComponentsInChildren<MeshFilter>().Length>=3,"Empty sculpture "+c.id);
                            Check(slot.GetComponentInChildren<Animator>()==null,"Leader animation copied to card "+c.id);
                            Check(slot.Find("Escultura de carta")!=null,"Missing sculpture group "+c.id);
                            if(!c.IsStructure)
                            {
                                var idle=slot.gameObject.AddComponent<CardUnitPresentation>();idle.Initialize(m,c,page*18+i,false);
                                Check(idle.Sculpture!=null&&!idle.IsSpawning,"Invalid idle rig "+c.id);
                            }
                            identities[c.id]=GeometryIdentity(slot);costs.Add(c.id+","+slot.GetComponentsInChildren<MeshRenderer>().Length);
                            foreach(var f in slot.GetComponentsInChildren<MeshFilter>())
                            {
                                Check(f.sharedMesh!=null&&f.sharedMesh.vertexCount>0,"Missing geometry "+c.id);
                                Check(f.GetComponent<Renderer>().sharedMaterial.shader.isSupported,"Unsupported shader "+c.id);
                                foreach(var v in f.sharedMesh.vertices){var p=slot.InverseTransformPoint(f.transform.TransformPoint(v));Check(Mathf.Abs(p.x)<.67f&&Mathf.Abs(p.z)<.75f&&p.y>=-.025f&&p.y<1.9f,"Unseated/oversized sculpture "+c.id+" "+p);}
                            }
                            var label=m.Label(slot,"Gallery label",new Vector3(0,-.12f,-.55f),.022f,new Color(.83f,.87f,.93f));
                            label.text=c.id.Replace("-"," ");if(label.text.Length>20){int cut=label.text.LastIndexOf(' ',Mathf.Min(19,label.text.Length-1));if(cut>0)label.text=label.text.Substring(0,cut)+"\n"+label.text.Substring(cut+1);}
                            label.transform.rotation=camera.transform.rotation;
                        }
                        Capture(camera,output+"/"+faction+"-"+page+".png");BoardMeshFactory.Release(gallery.gameObject);
                    }
                }
                foreach(var pair in new[]{
                    new[]{"nomada-de-arena","guardian-del-obelisco"},new[]{"guardian-del-obelisco","emperador-de-las-dunas"},
                    new[]{"charming-mongoose","ferret-eluding-hunter"},new[]{"ferret-eluding-hunter","bober-reinforcer"},
                    new[]{"bober-reinforcer","panther-stealthy-assassin"},new[]{"battle-ant-token","escarabajo-de-cobre"},
                    new[]{"semillero-micelial","jardin-micelial"},new[]{"red-micelial","micelium-root"},
                    new[]{"arbol-madre","bosque-eterno"},new[]{"santuario-de-arena","templo-de-ceniza"},
                    new[]{"arena-profunda","dunas-movedizas"}})
                    Check(identities[pair[0]]!=identities[pair[1]],"Identical silhouettes: "+string.Join(" / ",pair));
                File.WriteAllLines(output+"/renderer-counts.csv",new[]{"card,renderers"}.Concat(costs));
                Portraits(m,root.transform,camera,cards,output);
                File.WriteAllText(output+"/review.txt","Validated "+reviewed+" unique card entries: materials, geometry, footprint, seating, no leader rigs, excluded spells.");
                Debug.Log("CARD_MODEL_GALLERY_PASSED count="+reviewed);
            }
            finally{BoardMeshFactory.Release(root);m.Dispose();EditorSceneManager.ClosePreviewScene(scene);}
        }
        static string GeometryIdentity(Transform slot)
        {
            return string.Join("|",slot.GetComponentsInChildren<MeshFilter>().Select(f=>
            {
                var matrix=slot.worldToLocalMatrix*f.transform.localToWorldMatrix;
                var numbers=new string[12];for(int row=0;row<3;row++)for(int col=0;col<4;col++)numbers[row*4+col]=matrix[row,col].ToString("F4",CultureInfo.InvariantCulture);
                return f.sharedMesh.name+":"+string.Join(",",numbers);
            }).OrderBy(s=>s,StringComparer.Ordinal));
        }
        static void Portraits(BoardMeshFactory m,Transform parent,Camera camera,CardData[] cards,string output)
        {
            Directory.CreateDirectory(output+"/Portraits");
            var position=camera.transform.position;var rotation=camera.transform.rotation;float size=camera.orthographicSize;
            camera.orthographicSize=1.04f;camera.transform.position=new Vector3(2.1f,2.05f,-3.8f);camera.transform.LookAt(new Vector3(0,.68f,0));
            foreach(var c in cards.Where(c=>c.category!=Category.Spell))
            {
                var slot=m.Group(parent,c.id,Vector3.zero);m.PieceModel(slot,c,GrayboxUI.PlayerColor(0));
                Capture(camera,output+"/Portraits/"+c.id+".png",300,360);BoardMeshFactory.Release(slot.gameObject);
            }
            camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;
        }
        static void Capture(Camera camera,string path,int width=1800,int height=1050)
        {
            var target=new RenderTexture(width,height,24);target.Create();camera.targetTexture=target;camera.aspect=(float)width/height;camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
        }
        public static void Batch(){try{Gallery();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}

        const string Pending="CardPresentation.Pending";
        static bool active;static int step;static double due;
        static GameManager game;static Board3DScene world;static Piece old,unit,flying,crystal,building;
        static string snapshot;static Transform[] structureParts;static Vector3[] finalParts;static Vector3 initialRoot,initialJoint;
        static Quaternion initialHead;static Transform head;
        public static void BeginBatch(){SessionState.SetBool(Pending,true);EditorSceneManager.OpenScene("Assets/WarConquer/Scenes/WarConquer_Graybox.unity");EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Register(){EditorApplication.update+=Tick;}
        static void Sync()=>world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool(Pending,false)){SessionState.SetBool(Pending,false);active=true;step=0;due=EditorApplication.timeSinceStartup+2;}
            if(!active||EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(step==0)
                {
                    game=new GameManager(CardCatalog.Load());game.NewGame(CardCatalog.Leaders,42,CardCatalog.LoadRules(),4);
                    var slots=game.State.tiles.Where(t=>t.unit==null&&t.structure==null&&!t.blocked&&t.baseOwner<0).Take(9).ToArray();
                    old=game.Place("charming-mongoose",0,slots[0].id);world=new GameObject("Presentation QA").AddComponent<Board3DScene>();Sync();
                    Check(!world.UnitVisual(old.id).GetComponent<CardUnitPresentation>().IsSpawning,"Old unit replayed arrival");
                    unit=game.Place("nomada-de-arena",0,slots[1].id);flying=game.Place("dark-fly",3,slots[2].id);crystal=game.Place("crystal-minion",3,slots[3].id);building=game.Place("arbol-madre",0,slots[4].id);
                    var king=game.Place("rey-micelial",0,slots[5].id);var titan=game.Place("titan-de-ceniza",2,slots[6].id);Sync();
                    foreach(var p in new[]{king,titan})Check(world.UnitVisual(p.id).GetComponent<CardUnitPresentation>().IsSpawning,"Latent unit not arriving "+p.cardId);
                    foreach(var p in new[]{unit,flying,crystal})Check(world.UnitVisual(p.id).GetComponent<CardUnitPresentation>().IsSpawning,"New unit not arriving "+p.cardId);
                    Check(world.IsAnimating,"Arrival not exposed to board input gate");
                    var root=world.UnitVisual(unit.id);initialRoot=root.localPosition;head=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Idle:Head");initialHead=head.localRotation;initialJoint=head.localPosition;
                    structureParts=world.UnitVisual(building.id).GetComponentsInChildren<MeshRenderer>().Where(r=>r.transform.localPosition.y>5).Select(r=>r.transform).ToArray();
                    Check(structureParts.Length>30,"Detailed assembly not staged");finalParts=structureParts.Select(t=>t.localPosition-Vector3.up*6).ToArray();
                    snapshot=GamePersistence.Serialize(game.State);step=1;due=EditorApplication.timeSinceStartup+.28;return;
                }
                if(step==1)
                {
                    var r=world.UnitVisual(unit.id);var presentation=r.GetComponent<CardUnitPresentation>();
                    Check(presentation.IsSpawning&&presentation.Sculpture.localScale.x>.02f&&presentation.Sculpture.localScale.x<1,"No intermediate arrival frame");
                    Check(r.localPosition==initialRoot,"Arrival moved logical root");
                    Check(structureParts.Any(t=>t.localPosition.y>3)&&structureParts.Any(t=>t.GetComponent<Renderer>().enabled),"Assembly not staggered");
                    for(int i=0;i<5;i++)Sync();step=2;due=EditorApplication.timeSinceStartup+1.6;return;
                }
                if(step==2)
                {
                    Check(!world.IsAnimating,"Idle locks input or arrival restarted");
                    Check(snapshot==GamePersistence.Serialize(game.State),"Visuals mutated game data/random state");
                    var r=world.UnitVisual(unit.id);Check(r.localPosition==initialRoot,"Idle moved root");Check(r.Find("Aparición de unidad")==null,"Arrival VFX leaked");
                    Check(Quaternion.Angle(head.localRotation,initialHead)>.05f,"Head idle not animated");Check(head.localPosition==initialJoint,"Joint pose drift");
                    for(int i=0;i<structureParts.Length;i++)Check(Vector3.Distance(structureParts[i].localPosition,finalParts[i])<.001f,"Assembly failed to settle");
                    var target=game.State.tiles.First(t=>t.unit==null&&!t.blocked&&t.baseOwner<0);MovementManager.Relocate(game,unit,target.id);Sync();
                    step=3;due=EditorApplication.timeSinceStartup+.16;return;
                }
                if(step==3)
                {
                    Check(world.UnitVisual(unit.id).GetComponent<BoardPieceMotion>().IsMoving,"Movement lost");Check(Quaternion.Angle(head.localRotation,initialHead)<.001f,"Idle competing with movement");
                    game.Restore(GamePersistence.Deserialize(snapshot));Sync();
                    Check(!world.IsAnimating,"Restore retained animation");
                    Check(world.UnitVisual(unit.id).GetComponent<CardUnitPresentation>()!=null&&!world.UnitVisual(unit.id).GetComponent<CardUnitPresentation>().IsSpawning,"Restore replayed spawn");
                    Check(world.UnitVisual(building.id).GetComponent<CardUnitPresentation>()==null,"Structure received unit idle");
                    Debug.Log("CARD_PRESENTATION_PASSED: ground/flying/crystal arrivals, idle, fixed colliders/root, stable refresh, unchanged state/RNG, nested assembly, movement suspension, restore cancellation.");
                    active=false;EditorApplication.Exit(0);
                }
            }
            catch(Exception e){Debug.LogException(e);active=false;EditorApplication.Exit(1);}
        }
    }
}
#endif
