using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    // Serialized Variables
    [Header("Game Mode Settings")]
    [SerializeField] private GameSettingsSO m_Settings = null;
    [SerializeField] bool mainMenuButton = false;
    [SerializeField] private ReadMode readmode = ReadMode.Read;
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;

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
        if (mainMenuButton)
        {
            m_Settings.gameMode = readmode;
            m_Settings.difficulty = difficulty;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        SceneManager.LoadScene(name);
        ServiceLocator.Instance.Get<GameManager>().Reset();
    }
}
