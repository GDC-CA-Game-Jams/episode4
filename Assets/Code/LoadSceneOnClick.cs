using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{

    public void OnClick(string name)
    {
        StartCoroutine(delayStartGame(name));

    }

    private IEnumerator delayStartGame(string name)
    {
        yield return new WaitForSeconds(1.5f);

        ServiceLocator.Instance.Get<GameManager>().Reset();
        SceneManager.LoadScene(name);
    }
}
