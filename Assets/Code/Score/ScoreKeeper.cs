using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Services;


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

    [Header("References")]
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI multiText;
    public GameObject scoreboard;
    public GameObject glamMeter;

    [Header("Variables")]
    public float pointMultiplier = 1.0f;
    private int multiCount = 1;
    public int multiTracker;
    public int tempMissCount = 0;
    public int[] multiThresholds;
    private string[] testNames = { "Luke", "Percy", "Vader", "Frodo", "Sam", 
                                   "Pipen", "Gandalf", "Sauron", "Harley",
                                   "Sally", "Jack", "Ivy", "Ahsoka"};
    System.Random random = new System.Random();
    private int randomName;
    private string newName;

    private bool gameOver = false;
    
    //an array of the highest scoring players
    public string[] topScores;

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
                glamMeter.GetComponent<Image>().color = Color.white;
            }

        /// *** Thes are test inputs taht will be changed when button presses are available *** ///
        //Test inputs to see how score reacts.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            
            OnHitPerfect();
        }
        else if (Input.GetKeyUp(KeyCode.Backspace))
        {
            OnMiss();

        }

        if (tempMissCount >= 3 && gameOver == false )
        {
            /// *** This will be updated to have player input their name with up to 8 chars *** ///
            ///*** This also get updated to have the player not lose on 3 misses but when the meter gets too low *** ///
            

            //if the player misses 3 beats the game ends and the scores come up including the new one
            //I also plan to add a pause here or "rewind" when we get a chance,
            //but I don't know how to use the current pause on the GameManager
            scoreboard.SetActive(true);
            scoreboard.GetComponent<ScoreManager>().NewEntry(curPoints, newName);
            scoreboard.GetComponent<ScoreManager>().GetHighScores();
            Debug.Log(curPoints + " --- " + newName);
            gameOver = true;            
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
        multiTracker = 0;
        pointMultiplier = 1.0f;
        tempMissCount++;
        multiCount = 1;

        multiText.text = "Multiplier: x" + 1;

    }
    public void OnHitPerfect()
    {
        MultiplierIncrease();

        curPoints += pointValue * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
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
    }

    public void OnHitGood()
    {
        MultiplierIncrease();

        curPoints += pointValue - 30f * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
    }

    public void OnHitPoor()
    {
        MultiplierIncrease();

        curPoints += pointValue - 50f * pointMultiplier;
        pointText.text = "Score: " + curPoints;

        multiText.text = "Multiplier: x" + pointMultiplier;
    }




}
