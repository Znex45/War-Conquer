using UnityEngine;

namespace WarConquer
{
    public sealed class MatchMusic : MonoBehaviour
    {
        const string MuteKey="WarConquer.MatchMusicMuted";
        AudioSource source;
        AudioClip[] tracks;
        public bool Muted { get; private set; }
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();
            source.playOnAwake=false;source.loop=true;source.spatialBlend=0;
            source.volume=.16f;source.priority=192;source.ignoreListenerPause=true;
            tracks=new[]{
                Resources.Load<AudioClip>("WarConquer/Audio/mystical-celtic-forest-05"),
                Resources.Load<AudioClip>("WarConquer/Audio/mystical-celtic-forest-06")
            };
            Muted=PlayerPrefs.GetInt(MuteKey,0)!=0;source.mute=Muted;
        }
        public void StartMatch()
        {
            var available=System.Array.FindAll(tracks,clip=>clip!=null);
            if(available.Length==0)return;
            source.Stop();source.clip=available[Random.Range(0,available.Length)];source.mute=Muted;source.Play();
        }
        public void StopMatch(){if(source!=null){source.Stop();source.clip=null;}}
        public void ToggleMute()
        {
            Muted=!Muted;if(source!=null)source.mute=Muted;
            PlayerPrefs.SetInt(MuteKey,Muted?1:0);PlayerPrefs.Save();
        }
    }
}
