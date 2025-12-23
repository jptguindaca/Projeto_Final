using UnityEditorInternal;
using UnityEngine;
using TMPro;

public class HUDcontroller : MonoBehaviour
{
    [SerializeField] TMP_Text interactText;
    public static HUDcontroller instance;

    private void Awake()
    {
        instance = this;
    }  

    public void EnableInteractionText(string text)
    {
        interactText.text = text + " (E)";
        interactText.gameObject.SetActive(true);   
    }

    public void DisableInteractionText()
    {
        interactText.gameObject.SetActive(false);
    }
}
