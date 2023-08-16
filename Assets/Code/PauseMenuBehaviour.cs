using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class PauseMenuBehaviour : MonoBehaviour
{

    public void Unpause()
    {
        ServiceLocator.Instance.Get<GameManager>().Unpause("Pause");
    }
    
}
