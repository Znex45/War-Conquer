using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffectAsset damageAsset;
    [SerializeField] private VisualEffectAsset leaderAsset;

    public void PlayVFX(Vector3 position, string type)
    {
        GameObject vfxObject = new GameObject("VFX_Instance");

        vfxObject.transform.position = position;

        VisualEffect effect = vfxObject.AddComponent<VisualEffect>();

        switch (type)
        {
            case "damage":
                effect.visualEffectAsset = damageAsset;
                break;
            case "Leader":
                effect.visualEffectAsset = leaderAsset;
                break;

        }

        effect.Play();

        Destroy(vfxObject, 3f);
    }
}