using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public void OnClick(string name)
    {
        ServiceLocator.Instance.Get<GameManager>().Reset();
        SceneManager.LoadScene(name);
    }
}
