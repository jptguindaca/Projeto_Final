using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;

    private float timeLeft;
    private int frames;
    private float fps;

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            fps = frames / updateInterval;
            if (fpsText != null)
                fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";

            frames = 0;
            timeLeft = updateInterval;
        }
    }
}
