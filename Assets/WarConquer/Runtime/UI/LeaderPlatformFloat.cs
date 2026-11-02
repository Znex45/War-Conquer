using UnityEngine;

namespace WarConquer
{
    public sealed class LeaderPlatformFloat : MonoBehaviour
    {
        Vector3 origin;
        float phase;
        public void Initialize(int owner){origin=transform.localPosition;phase=owner*1.7f;}
        void Update(){transform.localPosition=origin+Vector3.up*(Mathf.Sin(Time.time*.8f+phase)*.07f);}
    }
}
