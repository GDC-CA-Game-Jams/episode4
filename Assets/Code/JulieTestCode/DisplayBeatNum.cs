using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayBeatNum : MonoBehaviour
{
    public TMP_Text beatText;

    int numBeats;


    public void IncreaseBeatNum()
    {
        numBeats += 1;
        beatText.text = numBeats.ToString();
    }
}
