using UnityEngine;
using Services;
using UnityEngine.EventSystems;

public static class Bootstrapper
{
    
    /// <summary>
    /// This function is used to initialize a variety of services and setup the initial state of the game.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        ServiceLocator.Initialize();

        //Setup Services

        ServiceLocator.Instance.Register(new EventManager());
        ServiceLocator.Instance.Get<EventManager>().Init();
        ServiceLocator.Instance.Register(new GameManager());
        
        ServiceLocator.Instance.Register(new BeatReader());

        //ServiceLocator.Instance.Register(new AudioManager());
        //ServiceLocator.Instance.Register(new GameManager());
        // Get
        // ServiceLocator.Instance.Get<TaskManager>();
    }
}