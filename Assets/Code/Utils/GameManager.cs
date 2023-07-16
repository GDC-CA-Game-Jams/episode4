using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : IService
{

    private bool isPaused;
    
    public GameManager()
    {
      
    }

    public void Init()
    {
        
    }

    public void Reset()
    {
        
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
}
