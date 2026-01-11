using System.Collections;
using UnityEngine;

public class FixedBlinkLight : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private float blinkInterval = 0.5f;

    private Coroutine routine;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        routine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (targetLight != null) targetLight.enabled = true;
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            targetLight.enabled = !targetLight.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
