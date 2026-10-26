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
            public Transform root,surface,decor,baseModel,leaderPlatform;
            public MeshRenderer ground,ring;
            public TextMesh number,baseLabel;
            public string decorationKey;
            public Biome lastBiome;public bool lastBlocked;public int lastDecorationBase;
            public Color groundColor;
            public string baseLeader;
            public int baseOwner=-2;
            public BoardTerrainStatusVisual terrainStatus;
        }
        sealed class PieceVisual
        {
            public Transform root;
            public TextMesh label;
            public Hex3DTarget target;
            public string shape,shortName;
            public BoardStatusVisual status;
            public BoardPieceMotion motion;
            public CardUnitPresentation presentation;
            public BoardStructureAssembly assembly;
            public int tileId=-1;
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
        BoardDiceVisual dice;
        BoardLeaderAbilityVfx leaderVfx;
        BoardBiomeVisuals biomes;
        LeaderSkybox leaderSkybox;
        BoardTurnNeon turnNeon;
        BoardLandingWave landingWaves;
        BoardCombatPresentation combat;
        Transform hoverRing;
        readonly HashSet<int> actionTargets=new HashSet<int>();
        readonly RaycastHit[] pickHits=new RaycastHit[128];
        GameManager observed;
        readonly Dictionary<int,List<int>> movementPaths=new Dictionary<int,List<int>>();
        void RecordMovement(int id,int from,int to)
        {
            if(!movementPaths.TryGetValue(id,out var path))movementPaths[id]=path=new List<int>();
            path.Add(to);
        }
        public Transform LeaderVisual(int owner){foreach(var tile in tiles.Values)if(tile.baseOwner==owner)return tile.leaderPlatform;return null;}
        public Transform UnitVisual(int id)=>pieces.TryGetValue(id,out var p)?p.root:null;
        public bool IsAnimating
        {
            get
            {
                if((landingWaves!=null&&landingWaves.IsActive)||(dice!=null&&dice.IsAnimating)||(combat!=null&&combat.IsAnimating))return true;
                foreach(var piece in pieces.Values)if((piece.motion!=null&&piece.motion.IsMoving)||(piece.assembly!=null&&piece.assembly.IsBuilding)||(piece.presentation!=null&&piece.presentation.IsSpawning))return true;
                return false;
            }
        }
        public Camera BoardCamera { get; private set; }
        public RenderTexture Texture { get; private set; }
        public const float DefaultZoom=1.6f;
        public float Zoom=DefaultZoom,Yaw=0,Pitch=53;
        public Vector2 Pan;
        public float PanHeight;
        public bool ShowLabels;
        public BoardEffectDisplay EffectDisplay=BoardEffectDisplay.Compact;
        public int FocusedPiece=-1;
        public int TileCount=>tiles.Count;
        public int PieceCount=>pieces.Count;
        public static Vector3 Position(HexTile tile)=>new Vector3(tile.x,0,tile.y);
        public void Initialize()
        {
            if(mesh!=null)return;
            mesh=new BoardMeshFactory();
            hoverRing=mesh.Part(transform,"Casilla bajo el cursor",mesh.Ring,Vector3.zero,Vector3.one*1.01f,Color.white).transform;
            hoverRing.gameObject.SetActive(false);hoverRing.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            combat=gameObject.AddComponent<BoardCombatPresentation>();combat.Initialize(mesh,this);
            landingWaves=gameObject.AddComponent<BoardLandingWave>();
            biomes=gameObject.AddComponent<BoardBiomeVisuals>();biomes.Initialize(mesh);
            leaderSkybox=gameObject.AddComponent<LeaderSkybox>();
            turnNeon=gameObject.AddComponent<BoardTurnNeon>();turnNeon.Initialize();
            dice=gameObject.AddComponent<BoardDiceVisual>();dice.Initialize(mesh,this);
            leaderVfx=gameObject.AddComponent<BoardLeaderAbilityVfx>();leaderVfx.Initialize(mesh,this);
            previousMain=Camera.main;if(previousMain!=null){previousMask=previousMain.cullingMask;previousMain.cullingMask&=~(1<<BoardMeshFactory.Layer);}
            var cameraRoot=mesh.Group(transform,"Cámara del tablero 3D",Vector3.zero);
            BoardCamera=cameraRoot.gameObject.AddComponent<Camera>();BoardCamera.orthographic=true;BoardCamera.clearFlags=CameraClearFlags.SolidColor;
            BoardCamera.backgroundColor=new Color(.025f,.035f,.05f);BoardCamera.nearClipPlane=.1f;BoardCamera.farClipPlane=100;BoardCamera.cullingMask=1<<BoardMeshFactory.Layer;
            BoardCamera.allowHDR=false;BoardCamera.allowMSAA=true;BoardCamera.depth=-2;
            var cameraData=cameraRoot.gameObject.AddComponent<UniversalAdditionalCameraData>();cameraData.renderPostProcessing=false;cameraData.renderShadows=true;
            BoardCamera.enabled=false;
            var key=mesh.Group(transform,"Luz principal",Vector3.zero);key.rotation=Quaternion.Euler(48,-38,0);
            var light=key.gameObject.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.93f,.81f);light.intensity=1.15f;light.shadows=LightShadows.Soft;light.shadowBias=.04f;light.shadowNormalBias=.2f;light.cullingMask=1<<BoardMeshFactory.Layer;
            var fill=mesh.Group(transform,"Luz de relleno",Vector3.zero);fill.rotation=Quaternion.Euler(32,145,0);
            var fillLight=fill.gameObject.AddComponent<Light>();fillLight.type=LightType.Directional;fillLight.color=new Color(.64f,.77f,1);fillLight.intensity=.45f;fillLight.cullingMask=1<<BoardMeshFactory.Layer;
            // Leave the space between floating tiles open to the leader skybox.
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
            var root=mesh.Group(transform,"HEX "+(t.id+1),Position(t));var surface=mesh.Group(root,"Superficie · onda de impacto",Vector3.zero);var tile=new TileVisual{root=root,surface=surface};landingWaves.Register(t,surface);
            var ground=mesh.Part(surface,"Casilla hexagonal 3D",mesh.Hex,Vector3.zero,Vector3.one,Color.gray,true);
            ground.AddComponent<Hex3DTarget>().tileId=t.id;tile.ground=ground.GetComponent<MeshRenderer>();
            var ring=mesh.Part(surface,"Borde de selección",mesh.Ring,new Vector3(0,BoardMeshFactory.Surface+.045f,0),Vector3.one,Color.green);tile.ring=ring.GetComponent<MeshRenderer>();tile.ring.shadowCastingMode=ShadowCastingMode.Off;
            tile.number=mesh.Label(surface,"Número de casilla",new Vector3(-.22f,BoardMeshFactory.Surface+.16f,-.39f),.032f,new Color(.83f,.86f,.88f));labels.Add(tile.number);
            tiles.Add(t.id,tile);return tile;
        }
        public void Sync(GameManager game,HashSet<int> valid,HashSet<int> selected,int focus)
        {
            Initialize();
            combat.Observe(game);actionTargets.Clear();actionTargets.UnionWith(valid);Hover(-1);
            if(observed!=game){if(observed!=null)observed.PieceRelocated-=RecordMovement;observed=game;observed.PieceRelocated+=RecordMovement;movementPaths.Clear();}
            bool rebuilding=state!=game.State;
            if(rebuilding)
            {
                foreach(var tile in tiles.Values)BoardMeshFactory.Release(tile.root.gameObject);
                foreach(var piece in pieces.Values)BoardMeshFactory.Release(piece.root.gameObject);
                foreach(var route in routes)BoardMeshFactory.Release(route);
                movementPaths.Clear();landingWaves.Clear();biomes.Clear();turnNeon.Clear();tiles.Clear();pieces.Clear();labels.Clear();routes.Clear();routeKey=null;
            }
            state=game.State;dice.Sync(game);leaderSkybox.Sync(game,BoardCamera);
            foreach(var t in state.tiles)
            {
                if(!tiles.TryGetValue(t.id,out var v))v=CreateTile(t);
                Color color=t.biome==Biome.Neutral?GrayboxUI.TerritoryColor(t.territory):GrayboxUI.BiomeColor(t.biome);
                // Ground color is interpolated by the existing slab visual during changes.
                bool central=ConquestManager.IsCenter(t);v.ring.gameObject.SetActive(selected.Contains(t.id)||valid.Contains(t.id)||t.id==focus||central);
                mesh.Paint(v.ring,selected.Contains(t.id)?Color.white:valid.Contains(t.id)?GrayboxUI.Green:t.id==focus?new Color(1,.62f,.2f):new Color(.94f,.81f,.46f));
                v.number.characterSize=central?.06f:.032f;
                if(t.specialEffect!=null&&v.terrainStatus==null)
                {
                    var effectRoot=mesh.Group(v.surface,"Estado del terreno",Vector3.up*(BoardMeshFactory.Surface+.018f));
                    v.terrainStatus=effectRoot.gameObject.AddComponent<BoardTerrainStatusVisual>();v.terrainStatus.Initialize(mesh,BoardCamera);
                }
                if(v.terrainStatus!=null)
                {
                    v.terrainStatus.Sync(t.specialEffect);
                    v.terrainStatus.gameObject.SetActive(t.specialEffect!=null&&EffectDisplay!=BoardEffectDisplay.Hidden
                        &&(EffectDisplay==BoardEffectDisplay.Compact||selected.Contains(t.id)||t.id==focus));
                }
                string number=central?"+1 PC":(t.id+1).ToString()+(t.owner>=0?" · J"+(t.owner+1):"")+(t.specialEffect!=null?" !":"");
                if(v.number.text!=number)v.number.text=number;
                v.number.gameObject.SetActive(ShowLabels||!t.IsOccupied&&(central||t.id==focus||valid.Contains(t.id)||selected.Contains(t.id)));
                if(v.decorationKey==null||v.lastBiome!=t.biome||v.lastBlocked!=t.blocked||v.lastDecorationBase!=t.baseOwner)
                {
                    if(v.decor!=null){v.decor.gameObject.SetActive(false);BoardMeshFactory.Release(v.decor.gameObject);}
                    v.decor=mesh.Group(v.surface,"Bioma "+Names.Biomes[(int)t.biome],Vector3.zero);
                    biomes.Sync(t,v.decor,v.ground,v.decorationKey!=null,v.groundColor);
                    if(v.decorationKey==null)mesh.Paint(v.ground,color);
                    v.groundColor=color;
                    if(t.blocked){var mountain=mesh.Part(v.decor,"Relieve no transitable",mesh.Cone,new Vector3(0,BoardMeshFactory.Surface,0),new Vector3(1.1f,1.2f,1.1f),new Color(.37f,.42f,.47f),true);mountain.AddComponent<Hex3DTarget>().tileId=t.id;}
                    v.decorationKey="initialized";v.lastBiome=t.biome;v.lastBlocked=t.blocked;v.lastDecorationBase=t.baseOwner;
                }
                string baseLeader=t.baseOwner>=0?state.players[t.baseOwner].leader:"";
                if(v.baseOwner!=t.baseOwner||v.baseLeader!=baseLeader)
                {
                    if(v.baseModel!=null){labels.Remove(v.baseLabel);BoardMeshFactory.Release(v.baseModel.gameObject);}
                    v.baseOwner=t.baseOwner;v.baseLeader=baseLeader;
                    if(t.baseOwner>=0)
                    {
                        v.baseModel=mesh.Group(v.root,"BASE J"+(t.baseOwner+1),Vector3.up*BoardMeshFactory.Surface);
                        var faction=GrayboxUI.PlayerColor(t.baseOwner);
                        mesh.Part(v.baseModel,"Emblema de base",mesh.Ring,Vector3.up*.025f,Vector3.one*.8f,faction);
                        var pick=v.baseModel.gameObject.AddComponent<BoxCollider>();pick.center=new Vector3(0,.12f,0);pick.size=new Vector3(1.2f,.24f,1.2f);v.baseModel.gameObject.AddComponent<Hex3DTarget>().tileId=t.id;
                        Vector3 outward=Position(t).normalized;
                        if(outward.sqrMagnitude<.01f)outward=Quaternion.Euler(0,t.baseOwner*90f,0)*Vector3.forward;
                        Vector3 location=Position(t)+outward*4f;
                        // Keep the entire platform clear of every playable hex, even on irregular maps.
                        while(state.tiles.Any(hex=>Vector3.Distance(location,Position(hex))<3.2f))location+=outward*.4f;
                        v.leaderPlatform=mesh.Group(v.baseModel,"Plataforma flotante",location-Position(t)+Vector3.up*.9f);
                        mesh.Part(v.leaderPlatform,"Isla de obsidiana",mesh.Hex,new Vector3(0,-.65f,0),new Vector3(2.7f,1.8f,2.7f),new Color(.13f,.17f,.24f));
                        mesh.Part(v.leaderPlatform,"Borde de facción",mesh.Ring,Vector3.up*.015f,Vector3.one*2.62f,faction);
                        var crystal=mesh.Part(v.leaderPlatform,"Cristal suspendido",mesh.Cone,new Vector3(0,-.5f,0),new Vector3(1.6f,1.3f,1.6f),faction);
                        crystal.transform.localRotation=Quaternion.Euler(180,0,0);
                        var presentation=mesh.Group(v.leaderPlatform,"Observador",Vector3.up*.03f);
                        presentation.localScale=Vector3.one*2.6f;
                        // Imported leaders face local -Z after their prefab alignment.
                        presentation.rotation=Quaternion.LookRotation(outward,Vector3.up);
                        if(!LeaderModels.Spawn(presentation,baseLeader))mesh.Castle(presentation,faction);
                        var platformPick=v.leaderPlatform.gameObject.AddComponent<BoxCollider>();platformPick.center=new Vector3(0,2.2f,0);platformPick.size=new Vector3(5.2f,6.4f,5.2f);v.leaderPlatform.gameObject.AddComponent<Hex3DTarget>().tileId=t.id;
                        v.leaderPlatform.gameObject.AddComponent<LeaderPlatformFloat>().Initialize(t.baseOwner);
                        v.baseLabel=mesh.Label(v.leaderPlatform,"Líder y Conquista",new Vector3(0,6.5f,0),.09f,faction);labels.Add(v.baseLabel);
                    }
                }
                turnNeon.Register(t,v.root);
                if(t.baseOwner>=0)
                {
                    var player=state.players[t.baseOwner]; v.baseLabel.text = "J" + (player.id + 1) + " · " + player.leader + (player.isAI ? " · IA" : "") + (ShowLabels ? "\n" + player.conquestPoints + " / 10 PC" : "");
                }
            }
            var alive=new HashSet<int>();
            foreach(var p in BoardManager.Pieces(state))
            {
                alive.Add(p.id);var card=game.Data(p);string shape=p.cardId+"/"+p.owner+"/"+p.evolved+"/"+state.players[p.owner].leader;
                bool newPiece=!pieces.ContainsKey(p.id);
                if(pieces.TryGetValue(p.id,out var v)&&v.shape!=shape){labels.Remove(v.label);BoardMeshFactory.Release(v.root.gameObject);pieces.Remove(p.id);v=null;}
                if(v==null)
                {
                    var root=mesh.Group(transform,(card.IsStructure?"ESTRUCTURA ":"UNIDAD ")+card.name+" · J"+(p.owner+1)+" · "+p.id,Vector3.zero);
                    v=new PieceVisual{root=root,shape=shape,shortName=string.Concat(card.name.Split(' ').Where(w=>w.Length>2).Take(2).Select(w=>w[0])),target=root.gameObject.AddComponent<Hex3DTarget>()};mesh.PieceModel(root,card,GrayboxUI.PlayerColor(p.owner));root.localScale=Vector3.one*1.25f;
                    if(card.IsStructure&&newPiece&&!rebuilding){v.assembly=root.gameObject.AddComponent<BoardStructureAssembly>();v.assembly.Begin();}
                    if(!card.IsStructure)
                    {
                        v.motion=root.gameObject.AddComponent<BoardPieceMotion>();v.motion.Landed=landingWaves.Impact;
                        v.presentation=root.gameObject.AddComponent<CardUnitPresentation>();v.presentation.Initialize(mesh,card,p.id,newPiece&&!rebuilding);
                    }
                    v.status=root.gameObject.AddComponent<BoardStatusVisual>();v.status.Initialize(mesh,BoardCamera);
                    var collider=root.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.6f,0);collider.size=new Vector3(.94f,card.Has("Passable")?.25f:1.6f,.94f);
                    if(card.Has("Passable"))collider.center=new Vector3(0,.12f,0);
                    v.label=mesh.Label(root,"Vida y estados",new Vector3(0,card.movementType=="Flying"?2.05f:card.Has("Passable")?.45f:1.65f,0),.043f,Color.white);labels.Add(v.label);pieces.Add(p.id,v);
                }
                v.target.tileId=p.tileId;
                var destination=Position(state.tiles[p.tileId])+Vector3.up*BoardMeshFactory.Surface;
                if(v.tileId<0||v.motion==null)v.root.localPosition=destination;
                else if(v.tileId!=p.tileId)
                {
                    if(movementPaths.TryGetValue(p.id,out var path)&&path.Count>0&&path[path.Count-1]==p.tileId)
                        foreach(int stop in path)v.motion.Enqueue(Position(state.tiles[stop])+Vector3.up*BoardMeshFactory.Surface,stop);
                    else v.motion.Enqueue(destination,p.tileId);
                }
                v.tileId=p.tileId;movementPaths.Remove(p.id);
                string shortName=v.shortName;
                bool pieceFocused=FocusedPiece>=0?p.id==FocusedPiece:p.tileId==focus||selected.Contains(p.tileId);
                var occupiedTile=state.tiles[p.tileId];
                bool primaryOccupant=occupiedTile.unit==null||occupiedTile.structure==null
                    ||(FocusedPiece==occupiedTile.structure.id?p==occupiedTile.structure:p==occupiedTile.unit);
                v.label.text=shortName+" · "+p.health+" ♥";
                v.label.gameObject.SetActive(primaryOccupant&&ShowLabels);
                v.label.color=p.poison>0?new Color(.8f,.56f,1):Color.white;v.status.Sync(game,p);v.status.SetDisplay(primaryOccupant?EffectDisplay:BoardEffectDisplay.Hidden,pieceFocused);
            }
            combat.Sync(game);
            foreach(int id in pieces.Keys.Where(id=>!alive.Contains(id)).ToList())
            {labels.Remove(pieces[id].label);if(!combat.Retire(id,pieces[id].root))BoardMeshFactory.Release(pieces[id].root.gameObject);pieces.Remove(id);}
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
            movementPaths.Clear();
            turnNeon.Sync(game,this);leaderVfx.Sync(game);UpdateCamera();Physics.SyncTransforms();
        }
        public void UpdateCamera()
        {
            if(BoardCamera==null||state==null)return;
            // Only reject invalid numeric input; camera travel has no gameplay bounds.
            if(float.IsNaN(Zoom)||float.IsInfinity(Zoom)||Zoom<=0)Zoom=DefaultZoom;
            Pitch=Mathf.Repeat(Pitch,360f);Yaw=Mathf.Repeat(Yaw,360f);
            var rotation=Quaternion.Euler(Pitch,Yaw,0);var inverse=Quaternion.Inverse(rotation);
            float maxX=0,maxY=0;
            foreach(var tile in state.tiles)for(int level=0;level<2;level++)
            {
                float height=level==0?0:1.9f;var local=inverse*(Position(tile)+Vector3.up*height);maxX=Mathf.Max(maxX,Mathf.Abs(local.x)+.9f);maxY=Mathf.Max(maxY,Mathf.Abs(local.y)+.9f);
            }
            foreach(var tile in tiles.Values)
            {
            if(tile.leaderPlatform==null)continue;
            for(int level=0;level<2;level++)
            {
                float height=level==0?-2f:6.9f;var local=inverse*(tile.leaderPlatform.position-transform.position+Vector3.up*height);
                maxX=Mathf.Max(maxX,Mathf.Abs(local.x)+2.5f);maxY=Mathf.Max(maxY,Mathf.Abs(local.y)+2.5f);
            }
            }
            BoardCamera.orthographicSize=Mathf.Max(maxY,maxX/Mathf.Max(.5f,BoardCamera.aspect))*1.03f/Zoom;
            float radius=0;foreach(var tile in state.tiles)radius=Mathf.Max(radius,Position(tile).magnitude);radius+=16f;
            float distance=Mathf.Max(28f,radius);
            BoardCamera.nearClipPlane=.01f;BoardCamera.farClipPlane=distance+radius+Pan.magnitude*.016f+Mathf.Abs(PanHeight)+100f;
            Vector3 center=new Vector3(-Pan.x*.016f,.35f+PanHeight,Pan.y*.016f);BoardCamera.transform.SetPositionAndRotation(center-rotation*Vector3.forward*distance,rotation);
            foreach(var label in labels)if(label!=null)label.transform.rotation=rotation;
        }
        public bool GroundPoint(Vector2 viewport,out Vector3 point)
        {
            point=Vector3.zero;
            if(BoardCamera==null)return false;
            var ray=BoardCamera.ViewportPointToRay(viewport);
            var plane=new Plane(Vector3.up,new Vector3(0,BoardMeshFactory.Surface,0));
            if(!plane.Raycast(ray,out float distance))return false;
            point=ray.GetPoint(distance);return true;
        }
        void ShiftCamera(Vector3 offset)
        {
            Pan+=new Vector2(-offset.x,offset.z)/.016f;
            PanHeight+=offset.y;
            UpdateCamera();
        }
        public void DragCamera(Vector2 from,Vector2 to)
        {
            // Keep the grabbed ground point under the cursor at any yaw, pitch or zoom.
            if(Mathf.Abs(BoardCamera.transform.forward.y)>.1f&&GroundPoint(from,out var start)&&GroundPoint(to,out var end))ShiftCamera(start-end);
            else
            {
                var delta=from-to;
                ShiftCamera((BoardCamera.transform.right*delta.x*BoardCamera.aspect+BoardCamera.transform.up*delta.y)*2f*BoardCamera.orthographicSize);
            }
        }
        public void ZoomAt(float steps,Vector2 cursor)
        {
            bool anchored=Mathf.Abs(BoardCamera.transform.forward.y)>.1f&&GroundPoint(cursor,out _);
            GroundPoint(cursor,out var before);
            float next=Zoom*Mathf.Exp(steps*.12f);
            if(next>0&&!float.IsInfinity(next)&&!float.IsNaN(next))Zoom=next;
            UpdateCamera();
            if(anchored&&GroundPoint(cursor,out var after))ShiftCamera(before-after);
        }
        public int Pick(Vector2 viewport)
        {
            if(BoardCamera==null||viewport.x<0||viewport.x>1||viewport.y<0||viewport.y>1)return -1;
            int ground=-1;
            if(state!=null&&GroundPoint(viewport,out var point))
            {
                float nearest=float.MaxValue;
                foreach(var tile in state.tiles)
                {
                    var delta=point-Position(tile);float x=Mathf.Abs(delta.x),z=Mathf.Abs(delta.z);
                    // Resolve the full hex, including bevels and tiny visual seams,
                    // independently of tall models or non-interactive decoration.
                    if(x>.805f||z>.6972f||.8660254f*x+.5f*z>.6972f)continue;
                    float distance=delta.x*delta.x+delta.z*delta.z;
                    if(distance<nearest){nearest=distance;ground=tile.id;}
                }
            }
            if(actionTargets.Contains(ground))return ground;
            var ray=BoardCamera.ViewportPointToRay(viewport);
            int count=Physics.RaycastNonAlloc(ray,pickHits,BoardCamera.farClipPlane,1<<BoardMeshFactory.Layer,QueryTriggerInteraction.Ignore);
            int hitId=-1;float closest=float.MaxValue;
            for(int i=0;i<count;i++)
            {
                var hit=pickHits[i];var target=hit.collider.GetComponentInParent<Hex3DTarget>();
                if(target==null||target.tileId<0||state==null||target.tileId>=state.tiles.Count)continue;
                if(hit.distance<closest){closest=hit.distance;hitId=target.tileId;}
            }
            return hitId>=0?hitId:ground;
        }
        public void Hover(int tile)
        {
            if(hoverRing==null)return;
            bool visible=state!=null&&tile>=0&&tile<state.tiles.Count;
            hoverRing.gameObject.SetActive(visible);
            if(!visible)return;
            hoverRing.localPosition=Position(state.tiles[tile])+Vector3.up*(BoardMeshFactory.Surface+.064f);
            mesh.Paint(hoverRing.GetComponent<Renderer>(),actionTargets.Contains(tile)?new Color(.65f,1,.8f):Color.white);
        }
        public Vector2 ViewportOf(int tile,float height=BoardMeshFactory.Surface)
        {var point=BoardCamera.WorldToViewportPoint(Position(state.tiles[tile])+Vector3.up*height);return new Vector2(point.x,point.y);}
        void OnDestroy()
        {
            if(observed!=null)observed.PieceRelocated-=RecordMovement;
            if(previousMain!=null)previousMain.cullingMask=previousMask;
            if(Texture!=null){if(BoardCamera!=null)BoardCamera.targetTexture=null;Texture.Release();BoardMeshFactory.Release(Texture);}
            mesh?.Dispose();mesh=null;
        }
    }
}
