using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{

    public void OnClick(string name)
    {
        SceneManager.LoadScene(name);
        ServiceLocator.Instance.Get<GameManager>().Reset();
    }
    
    public void OnClickDelay(string name)
    {
        StartCoroutine(delayStartGame(name));
    }

    private IEnumerator delayStartGame(string name)
    {
        yield return new WaitForSecondsRealtime(1.5f);

        SceneManager.LoadScene(name);
        ServiceLocator.Instance.Get<GameManager>().Reset();
    }
}
