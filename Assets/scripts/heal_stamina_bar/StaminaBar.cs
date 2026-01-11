using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] float maxStamina = 100f;

    void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void Update()
    {
        if (CameraController.Instance == null) return;

        float current = CameraController.Instance.CurrentStamina;
        slider.value = current / maxStamina;
    }
}
