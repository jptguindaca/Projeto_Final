using UnityEngine;

public class FlashlightItem : MonoBehaviour, IUsable
{
    public GameObject lightObject;
    
    public void Use(GameObject user)
    {
        if (lightObject != null)
            lightObject.SetActive(!lightObject.activeSelf);
    }
}
