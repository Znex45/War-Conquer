using System.Collections.Generic;
using UnityEngine;

namespace WarConquer
{
    // A bounded, visual-only timeline. Game rules resolve immediately; only miniature children move.
    [DefaultExecutionOrder(200)]
    public sealed class BoardCombatPresentation : MonoBehaviour
    {
        sealed class Actor
        {
            public int id;public Transform root,pivot,sculpture;public BoardPieceMotion motion;public bool retired;
        }
        sealed class Clip
        {
            public CombatVisualKind kind;public Actor actor;public float start,duration;public Vector3 direction,from,to;
            public Transform effect,projectile;public Transform[] motes;public bool ranged;public Color color;
        }
        readonly List<CombatVisualSignal> pending=new List<CombatVisualSignal>();
        readonly List<Clip> clips=new List<Clip>();
        readonly Dictionary<int,Actor> actors=new Dictionary<int,Actor>();
        readonly HashSet<int> deaths=new HashSet<int>();
        readonly HashSet<Actor> retired=new HashSet<Actor>();
        BoardMeshFactory mesh;Board3DScene board;GameManager game;GameState state;
        float clock,sequenceEnd;
        public bool IsAnimating=>pending.Count>0||clips.Count>0;
        public int ActiveClipCount=>clips.Count;
        public int RetiredCount=>retired.Count;
        public void Initialize(BoardMeshFactory factory,Board3DScene world){mesh=factory;board=world;}
        void Receive(CombatVisualSignal signal)
        {if(isActiveAndEnabled&&signal.state==state)pending.Add(signal);}
        public void Observe(GameManager current)
        {
            if(game!=current)
            {
                if(game!=null)game.CombatVisual-=Receive;
                Clear();game=current;state=current?.State;
                if(game!=null)game.CombatVisual+=Receive;
            }
            if(state!=current?.State){Clear();state=current?.State;}
        }
        Actor ActorFor(int id,Transform root)
        {
            if(root==null)return null;
            if(actors.TryGetValue(id,out var actor)&&actor.root==root)return actor;
            var sculpture=root.Find("Escultura de carta");
            if(sculpture==null)sculpture=root.Find("Observador");
            if(sculpture==null)return null;
            var pivot=mesh.Group(root,"Reacción de combate",Vector3.zero);
            sculpture.SetParent(pivot,false);
            actor=new Actor{id=id,root=root,pivot=pivot,sculpture=sculpture,motion=root.GetComponent<BoardPieceMotion>()};actors[id]=actor;return actor;
        }
        Vector3 TilePoint(int tile)
        {return board.transform.TransformPoint(Board3DScene.Position(state.tiles[tile])+Vector3.up*(BoardMeshFactory.Surface+.95f));}
        public void Sync(GameManager current)
        {
            Observe(current);if(state==null||pending.Count==0)return;
            float start=Mathf.Max(clock,sequenceEnd),strike=start;bool attacking=false;
            foreach(var signal in pending)
            {
                if(signal.state!=state)continue;
                if(signal.tile<0||signal.tile>=state.tiles.Count)continue;
                bool leader=signal.kind==CombatVisualKind.LeaderDamage;
                int id=leader?-100-signal.owner:signal.piece;
                var actor=ActorFor(id,leader?board.LeaderVisual(signal.owner):board.UnitVisual(id));
                if(actor==null)continue;
                var clip=new Clip{kind=signal.kind,actor=actor,start=strike,duration=.4f,color=GrayboxUI.PlayerColor(signal.owner)};
                clip.to=TilePoint(signal.tile);
                int sourceTile=signal.sourceTile>=0&&signal.sourceTile<state.tiles.Count?signal.sourceTile:signal.tile;
                clip.from=TilePoint(sourceTile);clip.direction=Vector3.ProjectOnPlane(clip.to-clip.from,Vector3.up).normalized;
                if(clip.direction.sqrMagnitude<.1f)clip.direction=Vector3.forward;
                if(signal.kind==CombatVisualKind.Attack)
                {
                    if(attacking)start+=.65f;
                    attacking=true;strike=start+.3f;clip.start=start;clip.duration=.6f;
                    clip.ranged=signal.range>1;BuildAttack(clip);
                }
                else if(signal.kind==CombatVisualKind.Death)
                {
                    clip.start=strike+.12f;clip.duration=.68f;deaths.Add(id);BuildBurst(clip,new Color(.72f,.64f,.55f),10);
                }
                else
                {
                    clip.duration=.42f;BuildBurst(clip,new Color(1,.34f,.2f),7);
                    var status=actor.root.GetComponent<BoardStatusVisual>();if(status!=null)status.DelayHealthFeedback(Mathf.Max(0,clip.start-clock));
                }
                clips.Add(clip);sequenceEnd=Mathf.Max(sequenceEnd,clip.start+clip.duration);
            }
            pending.Clear();UpdateSuspensions();
        }
        void BuildAttack(Clip clip)
        {
            clip.effect=mesh.Group(transform,clip.ranged?"Proyectil de ataque":"Arco de ataque",Vector3.zero);clip.effect.gameObject.SetActive(false);
            if(clip.ranged)
            {
                clip.projectile=mesh.Part(clip.effect,"Trazo luminoso",mesh.Crystal,Vector3.zero,new Vector3(.11f,.42f,.11f),clip.color).transform;
                mesh.Part(clip.projectile,"Núcleo",mesh.Sphere,Vector3.zero,Vector3.one*.55f,new Color(1,.94f,.68f));
            }
            else
            {
                clip.projectile=mesh.Part(clip.effect,"Impacto de arma",mesh.Ring,Vector3.zero,Vector3.one*.36f,new Color(1,.86f,.47f)).transform;
            }
        }
        void BuildBurst(Clip clip,Color color,int count)
        {
            clip.effect=mesh.Group(transform,clip.kind==CombatVisualKind.Death?"Desaparición de la pieza":"Impacto de daño",Vector3.zero);clip.effect.gameObject.SetActive(false);
            clip.motes=new Transform[count];
            for(int i=0;i<count;i++)clip.motes[i]=mesh.Part(clip.effect,"Fragmento",mesh.Carved,Vector3.zero,Vector3.one*.08f,color).transform;
        }
        public bool Retire(int id,Transform root)
        {
            if(!deaths.Contains(id)||!actors.TryGetValue(id,out var actor)||actor.root!=root)return false;
            actor.retired=true;retired.Add(actor);
            foreach(var collider in root.GetComponentsInChildren<Collider>())collider.enabled=false;
            foreach(var text in root.GetComponentsInChildren<TextMesh>())text.gameObject.SetActive(false);
            var status=root.GetComponent<BoardStatusVisual>();if(status!=null){status.SetDisplay(BoardEffectDisplay.Hidden,false);status.enabled=false;}
            var health=root.Find("Cambio de vida · partículas");if(health!=null)health.gameObject.SetActive(false);
            var idle=root.GetComponent<CardUnitPresentation>();if(idle!=null)idle.enabled=false;
            var motion=root.GetComponent<BoardPieceMotion>();if(motion!=null)motion.enabled=false;
            var assembly=root.GetComponent<BoardStructureAssembly>();if(assembly!=null)assembly.CompleteImmediately();
            return true;
        }
        static void ResetActor(Actor actor)
        {if(actor.pivot!=null){actor.pivot.localPosition=Vector3.zero;actor.pivot.localRotation=Quaternion.identity;actor.pivot.localScale=Vector3.one;}}
        void UpdateSuspensions()
        {
            foreach(var actor in actors.Values)if(actor.motion!=null)actor.motion.SuspendForCombat=false;
            foreach(var clip in clips)if(clip.kind==CombatVisualKind.Attack&&clip.actor.motion!=null&&clock<clip.start+clip.duration)clip.actor.motion.SuspendForCombat=true;
        }
        void LateUpdate()=>Tick(Time.unscaledDeltaTime);
        // Exposed for deterministic frame-independent presentation checks in the editor.
        public void Tick(float dt)
        {
            clock+=Mathf.Max(0,dt);
            foreach(var actor in actors.Values)ResetActor(actor);
            for(int i=0;i<clips.Count;i++)
            {
                var clip=clips[i];float time=clock-clip.start;
                if(time<0)continue;
                if(clip.actor.root==null||clip.actor.pivot==null||time>=clip.duration)
                {
                    if(clip.effect!=null)BoardMeshFactory.Release(clip.effect.gameObject);
                    if(clip.kind==CombatVisualKind.Death)
                    {
                        deaths.Remove(clip.actor.id);
                        if(clip.actor.retired){if(clip.actor.root!=null)BoardMeshFactory.Release(clip.actor.root.gameObject);retired.Remove(clip.actor);if(actors.TryGetValue(clip.actor.id,out var current)&&current==clip.actor)actors.Remove(clip.actor.id);}
                    }
                    clips.RemoveAt(i--);continue;
                }
                float t=Mathf.Clamp01(time/clip.duration);var pivot=clip.actor.pivot;
                clip.effect.gameObject.SetActive(true);
                Vector3 direction=clip.actor.root.InverseTransformDirection(clip.direction);
                if(clip.kind==CombatVisualKind.Attack)
                {
                    float lunge=t<.35f?-Mathf.Sin(t/.35f*Mathf.PI)*.06f:Mathf.Sin((t-.35f)/.65f*Mathf.PI)*.25f;
                    pivot.localPosition+=direction*lunge;
                    float facing=Vector3.SignedAngle(Vector3.back,direction,Vector3.up);
                    pivot.localRotation*=Quaternion.Euler(-Mathf.Sin(t*Mathf.PI)*10,facing*Mathf.Sin(t*Mathf.PI),0);
                    float flight=Mathf.Clamp01((t-.22f)/.38f);
                    clip.projectile.position=clip.ranged?Vector3.Lerp(clip.from,clip.to,flight)+Vector3.up*(Mathf.Sin(flight*Mathf.PI)*.3f):clip.to;
                    clip.projectile.rotation=clip.ranged?Quaternion.FromToRotation(Vector3.up,clip.to-clip.from):Quaternion.Euler(60,t*180,0);
                    float size=clip.ranged?Mathf.Sin(flight*Mathf.PI):Mathf.Sin(t*Mathf.PI);
                    clip.projectile.localScale=clip.ranged?new Vector3(.11f,.42f,.11f)*size:Vector3.one*(.15f+.4f*t)*size;
                }
                else if(clip.kind==CombatVisualKind.Death)
                {
                    float fall=t*t;pivot.localPosition+=Vector3.down*(fall*.22f);
                    pivot.localRotation*=Quaternion.Euler(fall*65,0,fall*16);
                    pivot.localScale=Vector3.one*Mathf.Max(.015f,1-fall);
                    Burst(clip,t,clip.actor.root.position+Vector3.up*.25f,.65f);
                }
                else
                {
                    float wave=Mathf.Sin(t*5*Mathf.PI)*(1-t);
                    pivot.localPosition+=direction*(wave*.075f);
                    pivot.localRotation*=Quaternion.Euler(0,0,wave*9);
                    Burst(clip,t,clip.actor.root.position+Vector3.up*.85f,.48f);
                }
            }
            UpdateSuspensions();if(clips.Count==0)sequenceEnd=clock;
        }
        static void Burst(Clip clip,float t,Vector3 center,float radius)
        {
            for(int i=0;i<clip.motes.Length;i++)
            {
                float angle=i*Mathf.PI*2/clip.motes.Length;
                clip.motes[i].position=center+new Vector3(Mathf.Cos(angle)*(t*radius),Mathf.Sin(t*Mathf.PI)*(.18f+i%3*.07f),Mathf.Sin(angle)*(t*radius));
                clip.motes[i].localScale=Vector3.one*(.08f*(1-t));clip.motes[i].localRotation=Quaternion.Euler(t*170,i*37,t*90);
            }
        }
        public void Clear()
        {
            foreach(var clip in clips)if(clip.effect!=null)BoardMeshFactory.Release(clip.effect.gameObject);
            foreach(var actor in actors.Values)
            {
                ResetActor(actor);
                if(actor.motion!=null)actor.motion.SuspendForCombat=false;
                if(actor.retired){if(actor.root!=null)BoardMeshFactory.Release(actor.root.gameObject);}
                else if(actor.pivot!=null)
                {
                    if(actor.sculpture!=null)actor.sculpture.SetParent(actor.root,false);
                    BoardMeshFactory.Release(actor.pivot.gameObject);
                }
            }
            foreach(var actor in retired)if(actor.root!=null)BoardMeshFactory.Release(actor.root.gameObject);
            clips.Clear();pending.Clear();actors.Clear();retired.Clear();deaths.Clear();sequenceEnd=clock;
        }
        void OnDisable()=>Clear();
        void OnDestroy(){if(game!=null)game.CombatVisual-=Receive;Clear();}
    }
}
