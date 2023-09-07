using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class BeatReader : IService
{
    // --- Private Variable Declarations 
    private Dictionary<string, List<int>> notes = new();
    private readonly string[] allowableKeys = { "up", "down", "left", "right", 
                                "hup", "hdown", "hleft", "hright", "win",
                                "os", "om", "ol", "ox"};

    /// <summary>
    /// Initialize and read-in a beatmap from a file, provided in parameter
    /// </summary>
    /// <param name="file"></param>
    public void Init(string file)
    {
        // Preload map keys for notes dictionary structure
        LoadMapKeys();

        // File location validated as existant in BeatSpawner Awake() function
        // Currently no validation that file is a valid beatmap - only that file is valid
        // Rules / Syntax / StyleGuide for beatmaps available in Assets/Resources
        TextAsset rawText = Resources.Load<TextAsset>(file); 
        string text = rawText.text.Trim();
        string[] lines = text.Split("\n");

        foreach (string line in lines)
        {
            // Skip reading current line if comment syntax is detected
            if (line.Substring(0, 1).Equals("#"))
            {
                continue;
            }

            // Split & read in line input
            string[] split = line.Split(" ");
            string type = split[0];
            int beat = -1;
            int.TryParse(split[1], out beat);

            // Check to add a key if not already present - This should NEVER execute
            // Check kept here to handle novel / malformed note tags, just in case
            if (!notes.ContainsKey(type))
            {
                notes.Add(type, new List<int>());
            }

            // Adds new notes entry of note style, provided the beat int isn't malformed
            if (beat >= 0)
            {
                notes[type].Add(beat);
            }
        }
    }

    /// <summary>
    /// Function creates and fills in a randomly generated notes dictionary / level map
    /// </summary>
    public void InitRandomMap()
    {
        // Preset variables for qualities of the beatmap generation
        int levelLengthInBeats = 250;   // How many beats in the level
        float beatFrequency = .8f;      // Percentage of beats a note will spawn
        float beatCountOne = .7f;       // Probability 1 note will spawn
        float beatCountTwo = .9f;       // Probability 2 notes will spawn
        float beatCountThree = .97f;    // Probability 3 notes will spawn (remainder below 1f -> 4 note blast frequency)
        float longNoteChance = 0.13f;   // Probability that any given note will be a long note

        // Storage variable for whether a note track is in a long note spawning state
        bool[] inLongNote = { false, false, false, false };

        // Set up notes dictionary for storing notes
        LoadMapKeys();

        // Branching system to decide what notes to add; runs the length of level sequentially
        // This isn't the most efficient use of randoms, could definitely be using more caching & fewer calls
        // But given that this whole function will only run once in the lifetime of the level, it's alright
        for (int currentBeat = 0; currentBeat < levelLengthInBeats; currentBeat++)
        {
            // Decides whether to spawn a note at all
            if (UnityEngine.Random.Range(0f, 1f) < beatFrequency)
            {
                // Determine how many notes to spawn
                float beatCountProbability = UnityEngine.Random.Range(0f, 1f);  
                int numOfArrowSpawns = 0;

                // Boolean array to store "selected" beat directions
                bool[] bArrowSpawn = new bool[] { false, false, false, false };
                List<int> availableIndices = new List<int> { 0, 1, 2, 3 };

                if (beatCountProbability < beatCountOne) { numOfArrowSpawns = 1; }
                else if (beatCountProbability < beatCountTwo) { numOfArrowSpawns = 2; }
                else if (beatCountProbability < beatCountThree) { numOfArrowSpawns = 3; }
                else { numOfArrowSpawns = 4; }

                // Determine which directions / tracks to spawn notes in
                while (numOfArrowSpawns > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
                    int chosenIndex = availableIndices[randomIndex];

                    bArrowSpawn[chosenIndex] = true;
                    availableIndices.RemoveAt(randomIndex);
                    numOfArrowSpawns--;
                }

                // Add short / long notes into the note dict / level map
                for (int i = 0; i < bArrowSpawn.Length; i++)
                {
                    if (bArrowSpawn[i])
                    {
                        AddSelectedRandomNote(i, currentBeat);
                    }
                }
            }
        }

        // Close out any active long notes & add win call
        for (int i = 0; i < 4; i++)
        {
            if (inLongNote[i])
            {
                notes[allowableKeys[4 + i]].Add(levelLengthInBeats);
                inLongNote[i] = false;
            }
        }
        notes[allowableKeys[8]].Add(levelLengthInBeats+12);


        // Local function to add a short / long note into the notes dict / beatmap
        void AddSelectedRandomNote(int currSelect, int currBeat)
        {
            // Ensuring we're not already in a long note in this direction
            if (!inLongNote[currSelect])
            {
                // Decide whether to add a long or short note
                if (UnityEngine.Random.Range(0,1f) < longNoteChance) 
                {
                    notes[allowableKeys[4 + currSelect]].Add(currBeat);
                    inLongNote[currSelect] = true;
                }
                else
                {
                    notes[allowableKeys[currSelect]].Add(currBeat);
                }
            }
            // If already in a long note in this beat track/direction, call will end long note "hold"
            else
            {
                notes[allowableKeys[4 + currSelect]].Add(currBeat);
                inLongNote[currSelect] = false;
            }
        }
    }

    /// <summary>
    /// Preloads allowable keys as definined in private data member "allowableKeys[]"
    /// </summary>
    private void LoadMapKeys()
    {
        notes = new();
        foreach (string key in allowableKeys)
        {
            if (!notes.ContainsKey(key))
            {
                notes.Add(key, new List<int>());
            }
        }
    }

/// <summary>
/// Returns note dictionary structure for use by other parts of program.
/// Primarily to be used by BeatSpawner class/objects.
/// </summary>
/// <returns></returns>
    public Dictionary<string, List<int>> GetNotes()
    {
        return notes;
    }
}
