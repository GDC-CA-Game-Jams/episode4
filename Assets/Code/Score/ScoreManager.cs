using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;


public class ScoreManager : MonoBehaviour
{
    /**
     * This script is used to manage the High scores of players
     * It creates a Highscores obj that acts as a display of information of points and name.
     * The HighscoresTable obj is an object to save a list of the Highscores objects as data to json
     * using the functions GetHighScores() or NewEntry() it is possible to display or add new info to the table
     */
    
    private Transform container;
    private Transform template;

    private List<Highscores> highscoresList;
    private List<Transform> highscoreTransformList;

    private static ScoreKeeper scoreKeeper;


    private void Awake()
    {
        //finds objects in scene to create a table template for displaying high score information
        container = transform.Find("highscoreEntryContainer");
        template = container.Find("highscoreEntryTemplate");

        template.gameObject.SetActive(false);


        //used to test creating the highscores table
        //probably don't need anymore
        /*
        highscoresList = new List<Highscores>()
        {
            new Highscores{points = 400,name="Alpo"},
            new Highscores{points = 20,name="Commy"},
            new Highscores{points = 2,name="Draco"},
            new Highscores{points = 52,name="Potta"},
            new Highscores{points = 10,name="Pipen"},
            new Highscores{points = 10,name="Pipen"},
            new Highscores{points = 10,name="Pipen"},
            new Highscores{points = 10,name="Pipen"},
            new Highscores{points = 10,name="Pipen"},
            new Highscores{points = 10,name="Pipen"},
        };

        SaveScore();
        */
    }

    /// this is used for saving a newly created table
    public void SaveScore()
    {
        //creates a Table object to save the information into json
        HighscoresTable highscoresTable = new HighscoresTable { highscoresList = highscoresList };        
        string json = JsonUtility.ToJson(highscoresTable);
        PlayerPrefs.SetString("scores", json);
        PlayerPrefs.Save();
        //debugs to confirm content saved.
        Debug.Log(PlayerPrefs.GetString("scores"));
        Debug.Log("Saved to Json");
    }

    public void GetHighScores()
    {
        //gets the Json and turns it into a string.
        string jsonString = PlayerPrefs.GetString("scores");
        HighscoresTable highscoresTable = JsonUtility.FromJson<HighscoresTable>(jsonString);
        Debug.Log(jsonString);

        //sorts the list before creating each row and displaying it.        
        highscoresTable.highscoresList.Sort((x, y) => y.points.CompareTo(x.points));

        highscoreTransformList = new List<Transform>();

        foreach (Highscores highscores in highscoresTable.highscoresList)
        {
                CreateHighScoreTransform(highscores, container, highscoreTransformList);
        }
        
    }

    public void NewEntry(float points, string name)
    {
        
        Highscores highscores = new Highscores { points = points, name = name };

        string jsonString = PlayerPrefs.GetString("scores");
        HighscoresTable highscoresTable = JsonUtility.FromJson<HighscoresTable>(jsonString);

        //adds the entry to the list then checks list for the how many entries it has,
        //sorts them then gets rid of the lowest rank
        highscoresTable.highscoresList.Add(highscores);
        if (highscoresTable.highscoresList.Count > 10)
        {
            highscoresTable.highscoresList.Sort((x, y) => y.points.CompareTo(x.points));
            highscoresTable.highscoresList.RemoveRange(10, highscoresTable.highscoresList.Count - 10);
        }
        string json = JsonUtility.ToJson(highscoresTable);
        PlayerPrefs.SetString("scores", json);
        PlayerPrefs.Save();
        //debugs to confirm content saved.
        Debug.Log(highscoresTable.highscoresList.Count + " " + PlayerPrefs.GetString("scores") );
        Debug.Log("Saved to Json");

    }

    private void CreateHighScoreTransform(Highscores highscores, Transform transformContainer, List<Transform> transformList)
    {
        //this is the distance the rows move down between the rankings
        float templateHeight = 40f;

        //instanciates the template information
        Transform entryTransform = Instantiate(template, container);
        RectTransform rectTransform = entryTransform.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);

        entryTransform.gameObject.SetActive(true);

        //displays the information from the created list.
        int rank = transformList.Count + 1;
        
        string rankString = rank.ToString();
        entryTransform.Find("rankText").GetComponent<TextMeshProUGUI>().text = rankString;

        float points = highscores.points;
        entryTransform.Find("pointsText").GetComponent< TextMeshProUGUI> ().text = points.ToString();

        string name = highscores.name;
        entryTransform.Find("nameText").GetComponent< TextMeshProUGUI> ().text = name;

        entryTransform.Find("rankBackground").gameObject.SetActive(rank % 2 == 1);
        
        transformList.Add(entryTransform);
        
    }

    private class HighscoresTable
    {
        public List<Highscores> highscoresList;
    }


    [Serializable]
    private class Highscores
    {
        public float points;
        public string name;
    }


}
