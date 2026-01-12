using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Level1";
    public string creditsSceneName = "Credits";

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Credits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit Game");

    }
}

