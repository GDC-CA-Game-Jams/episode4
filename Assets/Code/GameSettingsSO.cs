using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "GameSettings", order = 1)]
public class GameSettingsSO : ScriptableObject
{
    public float beatTempo;
    public float beatFrequency;
    public float beatMultiplier;
}
