using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WarConquer
{
    public sealed class Board3DScene : MonoBehaviour
    {
        sealed class TileVisual
        {
            public Transform root,decor,baseModel;
            public MeshRenderer ground,ring;
            public TextMesh number,baseLabel;
            public string decorationKey;
            public string baseLeader;
            public int baseOwner=-2;
        }
        sealed class PieceVisual
        {
            public Transform root;
            public TextMesh label;
            public Hex3DTarget target;
            public string shape;
        }
        readonly Dictionary<int,TileVisual> tiles=new Dictionary<int,TileVisual>();
        readonly Dictionary<int,PieceVisual> pieces=new Dictionary<int,PieceVisual>();
        readonly List<TextMesh> labels=new List<TextMesh>();
        readonly List<GameObject> routes=new List<GameObject>();
        BoardMeshFactory mesh;
        Camera previousMain;
        int previousMask;
        GameState state;
        string routeKey;
        public Camera BoardCamera { get; private set; }
        public RenderTexture Texture { get; private set; }
        public float Zoom=1,Yaw=0,Pitch=53;
        public Vector2 Pan;
        public bool ShowLabels;
        public int TileCount=>tiles.Count;
        public int PieceCount=>pieces.Count;
        public static Vector3 Position(HexTile tile)=>new Vector3(tile.x,0,tile.y);
        public void Initialize()
        {
            if(mesh!=null)return;
            mesh=new BoardMeshFactory();
            previousMain=Camera.main;if(previousMain!=null){previousMask=previousMain.cullingMask;previousMain.cullingMask&=~(1<<BoardMeshFactory.Layer);}
            var cameraRoot=mesh.Group(transform,"Cámara del tablero 3D",Vector3.zero);
            BoardCamera=cameraRoot.gameObject.AddComponent<Camera>();BoardCamera.orthographic=true;BoardCamera.clearFlags=CameraClearFlags.SolidColor;
            BoardCamera.backgroundColor=new Color(.055f,.078f,.1f);BoardCamera.nearClipPlane=.1f;BoardCamera.farClipPlane=100;BoardCamera.cullingMask=1<<BoardMeshFactory.Layer;
            BoardCamera.allowHDR=false;BoardCamera.allowMSAA=true;BoardCamera.depth=-2;
            var cameraData=cameraRoot.gameObject.AddComponent<UniversalAdditionalCameraData>();cameraData.renderPostProcessing=false;cameraData.renderShadows=true;
            BoardCamera.enabled=false;
            var key=mesh.Group(transform,"Luz principal",Vector3.zero);key.rotation=Quaternion.Euler(48,-38,0);
            var light=key.gameObject.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.93f,.81f);light.intensity=1.15f;light.shadows=LightShadows.Soft;light.shadowBias=.04f;light.shadowNormalBias=.2f;light.cullingMask=1<<BoardMeshFactory.Layer;
            var fill=mesh.Group(transform,"Luz de relleno",Vector3.zero);fill.rotation=Quaternion.Euler(32,145,0);
            var fillLight=fill.gameObject.AddComponent<Light>();fillLight.type=LightType.Directional;fillLight.color=new Color(.64f,.77f,1);fillLight.intensity=.45f;fillLight.cullingMask=1<<BoardMeshFactory.Layer;
            mesh.Part(transform,"Suelo bajo el tablero",mesh.Cube,new Vector3(0,-.2f,0),new Vector3(28,.22f,28),new Color(.09f,.13f,.16f));
        }
        public void ResizeTexture(int width,int height)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            width=Mathf.Clamp(width,480,2000);height=Mathf.Clamp(height,320,1500);
            if(Texture!=null&&Texture.width==width&&Texture.height==height)return;
            if(Texture!=null){BoardCamera.targetTexture=null;Texture.Release();BoardMeshFactory.Release(Texture);}
            Texture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){name="WarConquer tablero 3D",antiAliasing=2,filterMode=FilterMode.Bilinear};
            Texture.Create();BoardCamera.targetTexture=Texture;BoardCamera.aspect=(float)width/height;BoardCamera.enabled=true;UpdateCamera();
        }
        TileVisual CreateTile(HexTile t)
        {
            var root=mesh.Group(transform,"HEX "+(t.id+1),Position(t));var tile=new TileVisual{root=root};
            var ground=mesh.Part(root,"Casilla hexagonal 3D",mesh.Hex,Vector3.zero,Vector3.one,Color.gray,true);
            ground.AddComponent<Hex3DTarget>().tileId=t.id;tile.ground=ground.GetComponent<MeshRenderer>();
            var ring=mesh.Part(root,"Borde de selección",mesh.Ring,new Vector3(0,BoardMeshFactory.Surface+.008f,0),Vector3.one,Color.green);tile.ring=ring.GetComponent<MeshRenderer>();tile.ring.shadowCastingMode=ShadowCastingMode.Off;
            tile.number=mesh.Label(root,"Número de casilla",new Vector3(-.22f,BoardMeshFactory.Surface+.16f,-.39f),.032f,new Color(.83f,.86f,.88f));labels.Add(tile.number);
            tiles.Add(t.id,tile);return tile;
        }
        public void Sync(GameManager game,HashSet<int> valid,HashSet<int> selected,int focus)
        {
            Initialize();state=game.State;
            foreach(var t in state.tiles)
            {
                if(!tiles.TryGetValue(t.id,out var v))v=CreateTile(t);
                Color color=t.biome==Biome.Neutral?GrayboxUI.TerritoryColor(t.territory):GrayboxUI.BiomeColor(t.biome);
                mesh.Paint(v.ground,color);
                bool central=ConquestManager.IsCenter(t);v.ring.gameObject.SetActive(selected.Contains(t.id)||valid.Contains(t.id)||t.id==focus||central);
                mesh.Paint(v.ring,selected.Contains(t.id)?Color.white:valid.Contains(t.id)?GrayboxUI.Green:t.id==focus?new Color(1,.62f,.2f):new Color(.94f,.81f,.46f));
                string number=(t.id+1).ToString()+(t.owner>=0?" · J"+(t.owner+1):"")+(t.specialEffect!=null?" !":"");
                if(v.number.text!=number)v.number.text=number;
                v.number.gameObject.SetActive(ShowLabels||t.id==focus||valid.Contains(t.id)||selected.Contains(t.id));
                string decoration=t.biome+"/"+t.blocked+"/"+t.baseOwner;
                if(decoration!=v.decorationKey)
                {
                    if(v.decor!=null)BoardMeshFactory.Release(v.decor.gameObject);
                    v.decor=mesh.Group(v.root,"Bioma "+Names.Biomes[(int)t.biome],Vector3.zero);mesh.BiomeModel(v.decor,t);
                    if(t.blocked){var mountain=mesh.Part(v.decor,"Relieve no transitable",mesh.Cone,new Vector3(0,BoardMeshFactory.Surface,0),new Vector3(1.1f,1.2f,1.1f),new Color(.37f,.42f,.47f),true);mountain.AddComponent<Hex3DTarget>().tileId=t.id;}
                    v.decorationKey=decoration;
                }
                string baseLeader=t.baseOwner>=0?state.players[t.baseOwner].leader:"";
                if(v.baseOwner!=t.baseOwner||v.baseLeader!=baseLeader)
                {
                    if(v.baseModel!=null){labels.Remove(v.baseLabel);BoardMeshFactory.Release(v.baseModel.gameObject);}
                    v.baseOwner=t.baseOwner;v.baseLeader=baseLeader;
                    if(t.baseOwner>=0)
                    {
                        v.baseModel=mesh.Group(v.root,"BASE J"+(t.baseOwner+1),Vector3.up*BoardMeshFactory.Surface);mesh.Castle(v.baseModel,GrayboxUI.PlayerColor(t.baseOwner));
                        var pick=v.baseModel.gameObject.AddComponent<BoxCollider>();pick.center=new Vector3(0,.58f,0);pick.size=new Vector3(.9f,1.2f,.85f);v.baseModel.gameObject.AddComponent<Hex3DTarget>().tileId=t.id;
                        v.baseLabel=mesh.Label(v.baseModel,"Líder y Conquista",new Vector3(0,1.5f,0),.064f,GrayboxUI.PlayerColor(t.baseOwner));labels.Add(v.baseLabel);
                    }
                }
                if(t.baseOwner>=0)
                {
                    var player=state.players[t.baseOwner];v.baseLabel.text="J"+(player.id+1)+(player.isAI?" · IA":"")+(ShowLabels?"\n"+player.conquestPoints+" / 10 PC":"");
                }
            }
            var alive=new HashSet<int>();
            foreach(var p in BoardManager.Pieces(state))
            {
                alive.Add(p.id);var card=game.Data(p);string shape=p.cardId+"/"+p.owner+"/"+p.evolved+"/"+state.players[p.owner].leader;
                if(pieces.TryGetValue(p.id,out var v)&&v.shape!=shape){labels.Remove(v.label);BoardMeshFactory.Release(v.root.gameObject);pieces.Remove(p.id);v=null;}
                if(v==null)
                {
                    var root=mesh.Group(transform,(card.IsStructure?"ESTRUCTURA ":"UNIDAD ")+card.name+" · J"+(p.owner+1)+" · "+p.id,Vector3.zero);
                    v=new PieceVisual{root=root,shape=shape,target=root.gameObject.AddComponent<Hex3DTarget>()};mesh.PieceModel(root,card,GrayboxUI.PlayerColor(p.owner));
                    var collider=root.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.6f,0);collider.size=new Vector3(.94f,card.Has("Passable")?.25f:1.6f,.94f);
                    if(card.Has("Passable"))collider.center=new Vector3(0,.12f,0);
                    v.label=mesh.Label(root,"Vida y estados",new Vector3(0,card.movementType=="Flying"?2.05f:card.Has("Passable")?.45f:1.65f,0),.043f,Color.white);labels.Add(v.label);pieces.Add(p.id,v);
                }
                v.target.tileId=p.tileId;v.root.localPosition=Position(state.tiles[p.tileId])+Vector3.up*BoardMeshFactory.Surface;
                string shortName=string.Concat(card.name.Split(' ').Where(w=>w.Length>2).Take(2).Select(w=>w[0]));
                v.label.text=shortName+" · "+p.health+" ♥"+(p.poison>0?" · V"+p.poison:"")+(game.IsSleeping(p)?" · Zz":"");
                v.label.color=p.poison>0?GrayboxUI.Green:Color.white;
            }
            foreach(int id in pieces.Keys.Where(id=>!alive.Contains(id)).ToList()){labels.Remove(pieces[id].label);BoardMeshFactory.Release(pieces[id].root.gameObject);pieces.Remove(id);}
            string nextRoutes=string.Join("|",state.fastRoutes.Select(r=>r.owner+":"+r.a+":"+r.b));
            if(routeKey!=nextRoutes)
            {
                foreach(var line in routes)BoardMeshFactory.Release(line);routes.Clear();
                foreach(var route in state.fastRoutes)
                {
                    var line=mesh.Group(transform,"Vía rápida · J"+(route.owner+1),Vector3.zero).gameObject;var renderer=line.AddComponent<LineRenderer>();
                    renderer.sharedMaterial=mesh.Lit;renderer.positionCount=2;renderer.startWidth=renderer.endWidth=.035f;
                    renderer.SetPositions(new[]{Position(state.tiles[route.a])+Vector3.up*.39f,Position(state.tiles[route.b])+Vector3.up*.39f});
                    renderer.startColor=renderer.endColor=GrayboxUI.PlayerColor(route.owner);renderer.shadowCastingMode=ShadowCastingMode.Off;mesh.Paint(renderer,GrayboxUI.PlayerColor(route.owner));routes.Add(line);
                }
                routeKey=nextRoutes;
            }
            UpdateCamera();Physics.SyncTransforms();
        }
        public void UpdateCamera()
        {
            if(BoardCamera==null||state==null)return;
            Zoom=Mathf.Clamp(Zoom,1,2.5f);Pitch=Mathf.Clamp(Pitch,36,78);Pan=Vector2.ClampMagnitude(Pan,500);
            var rotation=Quaternion.Euler(Pitch,Yaw,0);var inverse=Quaternion.Inverse(rotation);
            float maxX=0,maxY=0;
            foreach(var tile in state.tiles)foreach(float height in new[]{0f,1.9f})
            {
                var local=inverse*(Position(tile)+Vector3.up*height);maxX=Mathf.Max(maxX,Mathf.Abs(local.x)+.9f);maxY=Mathf.Max(maxY,Mathf.Abs(local.y)+.9f);
            }
            BoardCamera.orthographicSize=Mathf.Max(maxY,maxX/Mathf.Max(.5f,BoardCamera.aspect))*1.03f/Zoom;
            Vector3 center=new Vector3(-Pan.x*.016f,.35f,Pan.y*.016f);BoardCamera.transform.SetPositionAndRotation(center-rotation*Vector3.forward*28,rotation);
            foreach(var label in labels)if(label!=null)label.transform.rotation=rotation;
        }
        public int Pick(Vector2 viewport)
        {
            if(BoardCamera==null||viewport.x<0||viewport.x>1||viewport.y<0||viewport.y>1)return -1;
            var ray=BoardCamera.ViewportPointToRay(viewport);
            if(Physics.Raycast(ray,out var hit,100,1<<BoardMeshFactory.Layer))
            {var target=hit.collider.GetComponentInParent<Hex3DTarget>();if(target!=null)return target.tileId;}
            return -1;
        }
        public Vector2 ViewportOf(int tile,float height=BoardMeshFactory.Surface)
        {var point=BoardCamera.WorldToViewportPoint(Position(state.tiles[tile])+Vector3.up*height);return new Vector2(point.x,point.y);}
        void OnDestroy()
        {
            if(previousMain!=null)previousMain.cullingMask=previousMask;
            if(Texture!=null){if(BoardCamera!=null)BoardCamera.targetTexture=null;Texture.Release();BoardMeshFactory.Release(Texture);}
            mesh?.Dispose();mesh=null;
        }
    }
}
