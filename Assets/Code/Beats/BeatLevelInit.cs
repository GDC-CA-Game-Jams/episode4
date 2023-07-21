using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class BeatLevelInit : MonoBehaviour
{
    [Tooltip("Name of the beatmap file")]
    public string beatmap;
    
    // Start is called before the first frame update
    void Start()
    {
        ServiceLocator.Instance.Register(new BeatReader());
        ServiceLocator.Instance.Get<BeatReader>().Init(beatmap);
        Debug.Log(ServiceLocator.Instance.Get<BeatReader>().GetNotes());
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Unregister<BeatReader>();
    }
}
