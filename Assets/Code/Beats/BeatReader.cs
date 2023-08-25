using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class BeatReader : IService
{

    private Dictionary<string, List<int>> notes = new ();

    public void Init(string file)
    {
        notes = new();
        //preloading handled note keywords; required as beatreader isn't doing nullkey checking now
        //"h+direction" connotes a hold note, comes in pairs
        string[] possKeys = { "hup", "hdown", "hleft", "hright", "up", "down", "left", "right", "win" };
        foreach (string key in possKeys)
        {
            if (!notes.ContainsKey(key))
            {
                notes.Add(key, new List<int>());
            }
        }

        TextAsset rawText = Resources.Load<TextAsset>(file); //load the BeatMap
        string text = rawText.text.Trim();
        string[] lines = text.Split("\n");

        foreach (string line in lines)
        {
            if (line.Substring(0, 1).Equals("#"))
            {
                continue;
            }
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
