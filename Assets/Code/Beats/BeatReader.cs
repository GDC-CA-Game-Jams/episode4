using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class BeatReader : IService
{

    private Dictionary<string, List<int>> notes = new ();

    public void Init(string file)
    {
        TextAsset rawText = Resources.Load<TextAsset>(file);
        string text = rawText.text.Trim();
        string[] lines = text.Split("\n");

        foreach (string line in lines)
        {
            string[] split = line.Split(" ");
            string type = split[0];
            int beat = -1;
            int.TryParse(split[1], out beat);

            if (!notes.ContainsKey(type))
            {
                notes.Add(type, new List<int>());
            }

            if (beat >= 0)
            {
                notes[type].Add(beat);
            }
        }
    }


    public Dictionary<string, List<int>> GetNotes()
    {
        return notes;
    }
}
