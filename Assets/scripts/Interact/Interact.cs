using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Interact : MonoBehaviour
{
    Outline outline;
    public string message;

    public UnityEvent onInteraction;

    void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void interact()
    {
        onInteraction.Invoke();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        outline.enabled = true;
    }
}
