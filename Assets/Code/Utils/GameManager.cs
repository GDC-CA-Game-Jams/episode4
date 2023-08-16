using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : IService
{

    private bool isPaused;

    public int beatCount = -1;
    public int beatsElapsed = 0;
    
    public GameManager()
    {
        ServiceLocator.Instance.Get<EventManager>().OnDeath += OnDeath;
    }

    public void Init()
    {
        
    }

    public void Reset()
    {
        beatCount = -1;
        beatsElapsed = 0;
        Unpause();
    }

    private void OnDeath()
    {
        Pause("GameOver");
    }
    
    public void TogglePause(string pauseScene)
    {

        if (isPaused)
        {
            SceneManager.UnloadSceneAsync(pauseScene);
            Time.timeScale = 1;
        }
        else
        {
            SceneManager.LoadScene(pauseScene, LoadSceneMode.Additive);
            Time.timeScale = 0;
        }

        isPaused = !isPaused;
        
    }

    public void Pause(string pauseScene)
    {
        if (isPaused)
        {
            return;
        }
        SceneManager.LoadScene(pauseScene, LoadSceneMode.Additive);
        Time.timeScale = 0;
        isPaused = true;
    }
    
    public void Unpause(string pauseScene)
    {
        if (!isPaused)
        {
            return;
        }

        SceneManager.UnloadSceneAsync(pauseScene);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void Unpause()
    {
        if (!isPaused)
        {
            return;
        }
        Time.timeScale = 1;
        isPaused = false;
    }
}
