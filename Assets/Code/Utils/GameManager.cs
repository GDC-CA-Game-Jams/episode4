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
        Debug.Log("GameManager Constructor!");
        ServiceLocator.Instance.Get<EventManager>().OnDeath += OnDeath;
        ServiceLocator.Instance.Get<EventManager>().OnSongComplete += OnSongComplete;
    }

    public void Init()
    {
        
    }

    public void Reset()
    {
        Debug.Log("GameManager Resetting!");
        beatCount = -1;
        beatsElapsed = 0;
        Unpause();
    }

    private void OnDeath()
    {
        Pause("GameOver");
    }
    
    private void OnSongComplete()
    {
        Pause("SongComplete");
    }
    
    public void TogglePause(string pauseScene)
    {

        if (isPaused)
        {
            Unpause(pauseScene);
        }
        else
        {
            Pause(pauseScene);
        }
        
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
        ServiceLocator.Instance.Get<EventManager>().OnPause?.Invoke();
    }
    
    public void Unpause(string pauseScene)
    {
        Unpause();
        SceneManager.UnloadSceneAsync(pauseScene);
    }

    public void Unpause()
    {
        if (!isPaused)
        {
            return;
        }
        Time.timeScale = 1;
        isPaused = false;
        ServiceLocator.Instance.Get<EventManager>().OnUnpause?.Invoke();
    }
}
