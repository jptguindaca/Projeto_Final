using UnityEngine;

public class AmbientController : MonoBehaviour
{
    [SerializeField] AudioSource ambientSource;

    public void SetVolume(float value)
    {
        ambientSource.volume = value;
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    public void PlayAmbient()
    {
        if (!ambientSource.isPlaying)
            ambientSource.Play();
    }
}

