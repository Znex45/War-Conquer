using UnityEngine;

public class LogoPulse : MonoBehaviour
{
    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 1.5f) * 0.02f;

        transform.localScale = baseScale * pulse;
    }
}