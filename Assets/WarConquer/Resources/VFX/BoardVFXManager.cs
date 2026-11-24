using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace WarConquer
{
    public sealed class BoardVFXManager : MonoBehaviour
    {
        [Header("Effects")]
        [SerializeField] private VisualEffectAsset damageEffect;
        [SerializeField] private VisualEffectAsset drawEffect;

        private readonly List<GameObject> activeEffects = new();
        private const float BurstLifetime = 8f;

        private void Awake()
        {
            if (damageEffect == null)
                damageEffect = Resources.Load<VisualEffectAsset>("VFX/Damage");
            if (drawEffect == null)
                drawEffect = Resources.Load<VisualEffectAsset>("VFX/Draw");
        }

        public void PlayDamage(Vector3 position) => PlayBurst(damageEffect, "Damage", position);
        public void PlayDraw(Vector3 position) => PlayBurst(drawEffect, "Draw", position);

        private void PlayBurst(VisualEffectAsset asset, string effectName, Vector3 position)
        {
            if (asset == null)
            {
                Debug.LogWarning($"BoardVFXManager: no se encontró VFX/{effectName}.");
                return;
            }

            GameObject instance = new GameObject($"VFX · {effectName}");
            instance.SetActive(false);
            instance.layer = BoardMeshFactory.Layer;
            instance.transform.SetParent(transform, true);
            instance.transform.position = position;
            instance.transform.localScale = Vector3.one * (effectName == "Damage" ? 1.55f : 1.2f);

            VisualEffect effect = instance.AddComponent<VisualEffect>();
            effect.visualEffectAsset = asset;
            instance.SetActive(true);
            effect.Reinit();
            effect.Play();
            SetLayerRecursively(instance, BoardMeshFactory.Layer);

            activeEffects.Add(instance);
            StartCoroutine(RemoveBurst(instance));
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private IEnumerator RemoveBurst(GameObject instance)
        {
            yield return new WaitForSeconds(BurstLifetime);
            activeEffects.Remove(instance);
            BoardMeshFactory.Release(instance);
        }

        public void ClearEffects()
        {
            StopAllCoroutines();
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                {
                    effect.SetActive(false);
                    BoardMeshFactory.Release(effect);
                }
            }

            activeEffects.Clear();
        }

        private void OnDisable() => ClearEffects();
    }
}
