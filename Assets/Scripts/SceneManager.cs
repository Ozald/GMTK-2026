using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public PauseTransition pauseTransition;

    public void LoadNextScene()
    {
        StopEverything();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadPreviousScene()
    {
        StopEverything();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    private void StopEverything()
    {
        // Close pause menu
        if (PauseTransition.isPauseOpen)
        {
            pauseTransition.Exit();
        }

        // Stop FMOD music
        if (AudioManager.instance != null)
        {
            AudioManager.StopCurrentBGMusic();
        }

        // Make sure the next scene isn't frozen
        Time.timeScale = 1f;
    }
}