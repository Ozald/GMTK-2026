using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public PauseTransition pauseTransition;


    public void LoadTitleScreen()
    {
        StopEverything();

        if (RunStats.Instance != null)
        {
            RunStats.Instance.ResetStats();
        }

        SceneManager.LoadScene(0);
    }


    public void LoadGame()
    {
        StopEverything();

        if (RunStats.Instance != null)
        {
            RunStats.Instance.ResetStats();
            RunStats.Instance.StartTimer();
        }

        // Resumes FMOD's audio context after user interaction
        FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
        FMODUnity.RuntimeManager.CoreSystem.mixerResume();

        SceneManager.LoadScene(1);
    }


    public void LoadResults()
    {
        StopEverything();
        RunStats.Instance.CalculateFinalScore();

        if (RunStats.Instance != null)
        {
            RunStats.Instance.StopTimer();
        }

        SceneManager.LoadScene(2);
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


        Time.timeScale = 1f;
    }
}