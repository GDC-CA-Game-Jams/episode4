using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dan.Main;
using Services;
using UnityEngine.Events;


public class ScoreKeeper : MonoBehaviour
{
    /**
     * Use SPACEBAR to increase points and multiplier (multiplier changes at a combo of 4,8,8,10,10) 
     * Use BACKSPACE to miss notes (3 will currently bring up scoreboard)
     */

    //keep track of values
    public float curPoints;
    private float pointValue = 100f;
    private static GameManager gameManager;
    private static ScoreManager scoreManager;
    private string publicLeaderboardKey = "9dcab203467529a028c995351789458c9e9aeea545f860b239e8daf1ddbf9277";
    public string input;

    [Header("References")]
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI multiText;
    public GameObject scoreboard;
    public GameObject glamMeter;
    public GameObject Submit;

    [SerializeField]
     private List<TextMeshProUGUI> playerNames;
    [SerializeField]
    private List<TextMeshProUGUI> playerScores;
    [SerializeField]
    private TMP_InputField inputName;

    public TextMeshProUGUI curName;
    public TextMeshProUGUI pointsDisplay;

    [SerializeField] AudioManager m_MyAudioManager;

    [Header("Variables")]
    public float pointMultiplier = 1.0f;
    private int multiCount = 1;
    public int multiTracker;
    public int tempMissCount = 0;
    public int[] multiThresholds;
    public bool tempName;

    public float glamIncrease = 10;

    private string[] testNames = { "Luke", "Percy", "Vader", "Frodo", "Sam",
                                   "Pipen", "Gandalf", "Sauron", "Harley",
                                   "Sally", "Jack", "Ivy", "Ahsoka"};
    System.Random random = new System.Random();
    private int randomName;
    private string newName;
    public UnityEvent<string, int> submitScoreEvent;

    private bool gameOver = false;

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnMiss += OnMiss;
        ServiceLocator.Instance.Get<EventManager>().OnMissObstacle += OnMissObstacle;
        ServiceLocator.Instance.Get<EventManager>().OnPerfect += OnHitPerfect;
        ServiceLocator.Instance.Get<EventManager>().OnExcellent += OnHitExcellent;
        ServiceLocator.Instance.Get<EventManager>().OnGood += OnHitGood;
        ServiceLocator.Instance.Get<EventManager>().OnPoor += OnHitPoor;
        ServiceLocator.Instance.Get<EventManager>().OnDeath += OnDeath;
        ServiceLocator.Instance.Get<EventManager>().OnSongComplete += OnDeath;
    }
    
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnMiss -= OnMiss;
        ServiceLocator.Instance.Get<EventManager>().OnMissObstacle -= OnMissObstacle;
        ServiceLocator.Instance.Get<EventManager>().OnPerfect -= OnHitPerfect;
        ServiceLocator.Instance.Get<EventManager>().OnExcellent -= OnHitExcellent;
        ServiceLocator.Instance.Get<EventManager>().OnGood -= OnHitGood;
        ServiceLocator.Instance.Get<EventManager>().OnPoor -= OnHitPoor;
        ServiceLocator.Instance.Get<EventManager>().OnDeath -= OnDeath;
        ServiceLocator.Instance.Get<EventManager>().OnSongComplete -= OnDeath;
    }

    private void Start()
    {
        
        //this bool is what brings up the score board
        gameOver = false;
        // sets points and multiplier values
        pointText.text = "Score: 0";
        multiText.text = "Multiplier: x1";
        scoreboard.SetActive(false);


        /// *** this will be changed when player input for name is implemented. *** ///
        //Uses the random functionality to choose a random name from a list
        //its just a fun extra thing i added so the names aren't all the same for the moment
        System.Random random = new System.Random();
        randomName = random.Next(testNames.Length);
        newName = testNames[randomName];
        input = newName;
        /***
        tempName = true;
        playerNameField.SetActive(false);
        */
        Debug.Log(input);
        //playerNameRef = playerName.GetComponent<PlayerName>().input;
        GetLeaderboard();
    }
    public void NewEntry(string name, int points)
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, newName, (int)curPoints, (msg) =>
        {
            LeaderboardCreator.ResetPlayer(() =>
            {
                
                GetLeaderboard();
            });

            
        });
    }
        public void SubmitScore()
    {
        int curScore = (int)curPoints;
        Debug.Log(curScore);
        submitScoreEvent.Invoke(newName, curScore);
        Submit.SetActive(false);
        
    }

    public void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, ((msg) =>
        {
            int loopLength = (msg.Length < playerNames.Count) ? msg.Length : playerNames.Count;
            for (int i = 0; i < loopLength; i++)
            {
                playerNames[i].text = msg[i].Username;
                playerScores[i].text = msg[i].Score.ToString();
            }
        }));
    }
    public void Update()
    {
        //takes the multiplier count and changes the glam bar based on current multiplier
        //could probably be changed to a switch statement but works for now
            if (multiCount == 2)
            {
                glamMeter.GetComponent<Image>().color = Color.yellow;
            }
            else if (multiCount == 3)
            {
                glamMeter.GetComponent<Image>().color = Color.red;
            }
            else if (multiCount == 4)
            {
                glamMeter.GetComponent<Image>().color = Color.blue;
            }
            else if (multiCount == 5)
            {
                glamMeter.GetComponent<Image>().color = Color.cyan;
            }
            else if (multiCount == 6)
            {
                glamMeter.GetComponent<Image>().color = Color.magenta;
            }
            else 
            {
                glamMeter.GetComponent<Image>().color = Color.green;
            }

        /// *** Thes are test inputs taht will be changed when button presses are available *** ///
        //Test inputs to see how score reacts.
        /**
        if (Input.GetKeyUp(KeyCode.Space))
        {
            
            OnHitPerfect();
        }
        else if (Input.GetKeyUp(KeyCode.Backspace))
        {
            OnMiss();

        }
        */
    }
    public void PlayerNameInput(string s)
    {
        Debug.Log("this is working --- 1");
        if (s.Length >= 3 && s.Length <= 8)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                input = s;
                //playerNameField.SetActive(false);
                Debug.Log("this is working");
                tempName = false;
                //GameOver();
            }
        }
        else
        {
            return;
        }



        Debug.Log(input);
    }

    public void OnDeath()
    {
        
        if (!gameOver)
        {
            //SubmitScore();
            pointsDisplay.text = curPoints.ToString();
            curName.text = newName;            
            scoreboard.SetActive(true);
            scoreboard.GetComponent<ScoreManager>().NewEntry(input, (int)curPoints);
            //scoreboard.GetComponent<ScoreManager>().GetHighScores();
            Debug.Log(curPoints + " --- " + input);
            gameOver = true;
        }
    }
    public void GameOver()
    {
        if (tempName)
        {
            //playerNameField.SetActive(true);
        }
    }
    
    public void MultiplierIncrease()
    {
        //keeps track of the multiplier and counts up currently set to increase points by 20% per multiplier
        if (multiCount - 1 < multiThresholds.Length)
        {
            multiTracker++;
            if (multiThresholds[multiCount - 1] <= multiTracker)
            {
                multiTracker = 0;
                pointMultiplier += .2f;
                multiCount++;
            }

        }
    }


    public void OnMiss()
    {
        if (gameOver)
        {
            return;
        }

        multiTracker = 0;
        pointMultiplier = 1.0f;
        tempMissCount++;
        multiCount = 1;

        multiText.text = "Multiplier: x" + 1;
        
        m_MyAudioManager.PlaySFX("BeatMiss");

        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(-glamIncrease * 1.8f);

    }
    
    public void OnMissObstacle()
    {
        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(-glamIncrease * 4f);
    }
    public void OnHitPerfect()
    {
        MultiplierIncrease();

        curPoints += pointValue * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
        
        m_MyAudioManager.PlaySFX("BeatPerfect");

        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(glamIncrease);
    }
    /// <summary>
    /// All Of the below are for use later and will probably get changed. they are functions that were meant to
    /// add points when pressing based on timing for the notes, but i can't implement them yet.
    /// </summary>

    public void OnHitExcellent()
    {
        MultiplierIncrease();

        curPoints += pointValue - 20f * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;

        m_MyAudioManager.PlaySFX("BeatGood");
        
        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(glamIncrease * 0.8f);
    }

    public void OnHitGood()
    {
        MultiplierIncrease();

        curPoints += pointValue - 30f * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
        
        m_MyAudioManager.PlaySFX("BeatPoor");

        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(glamIncrease * 0.5f);
    }

    public void OnHitPoor()
    {
        MultiplierIncrease();

        curPoints += pointValue - 50f * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
        
        m_MyAudioManager.PlaySFX("BeatPoor");

        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(glamIncrease * 0.3f);
    }




}
