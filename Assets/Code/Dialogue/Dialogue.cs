using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [TextArea(3, 10)]
    public string[] sentences;
    public string this[int index]
    {
        get
        {
            return sentences[index];
        }
        set
        {
            sentences[index] = value;
        }
    }
}
