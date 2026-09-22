using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    // Scores are observed once per state update. Loading a save establishes a silent baseline.
    public sealed class ConquestAudioFeedback:MonoBehaviour
    {
        GameState observed;
        readonly int[] scores=new int[4];
        readonly Queue<int> pending=new Queue<int>();
        AudioSource source;
        AudioClip trumpet;
        double nextSound;
        public int PendingPoints=>pending.Count;
        public int PlayedPoints { get; private set; }
        public AudioClip Trumpet=>trumpet;
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.volume=.6f;
            source.ignoreListenerPause=true;source.priority=64;
            trumpet=Resources.Load<AudioClip>("WarConquer/Audio/conquest-trumpet");
            // The graybox can be opened in a scene without a main camera or listener.
            if(!FindObjectsByType<AudioListener>().Any(l=>l.isActiveAndEnabled))gameObject.AddComponent<AudioListener>();
        }
        public void Sync(GameState state)
        {
            if(state==null)return;
            if(observed!=state)
            {
                observed=state;pending.Clear();source.Stop();nextSound=0;
                for(int i=0;i<scores.Length;i++)scores[i]=i<state.players.Count?state.players[i].conquestPoints:0;
                return;
            }
            for(int i=0;i<state.players.Count&&i<scores.Length;i++)
            {
                int current=state.players[i].conquestPoints;
                for(int point=scores[i];point<current;point++)pending.Enqueue(i);
                scores[i]=current;
            }
        }
        void Update()
        {
            if(pending.Count==0||trumpet==null||AudioSettings.dspTime<nextSound)return;
            pending.Dequeue();source.PlayOneShot(trumpet);PlayedPoints++;
            nextSound=AudioSettings.dspTime+trumpet.length+.08;
        }
        void OnDisable(){pending.Clear();if(source!=null)source.Stop();}
    }
}
