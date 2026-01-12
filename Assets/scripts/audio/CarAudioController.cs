using UnityEngine;

public class StaticCarEngine : MonoBehaviour
{
    [SerializeField] AudioSource engineSource;

    void Start()
    {
        if (!engineSource.isPlaying)
            engineSource.Play();
    }
}