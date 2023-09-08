using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BeatReader : IService
{
    // --- Private Variable Declarations 
    private Dictionary<string, List<int>> notes = new();
    // PLEASE DO NOT CHANGE THIS STRING, IT'S USED AS A REFERENCE FOR A BUNCH OF FUNCTIONS RELATING TO BEATS
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
    public void InitRandomMap(int difficulty)
    {
        // Preset variables for qualities of the beatmap generation
        int levelLengthInBeats = 240;           // How many beats in the level must be a minimum of 12x obstacle length
        int obsLength = 14;                     // Length of Obstacles, defaulted to 14 like in game
        int obsIndexOffset = 9;                 // Used to store offest to get to obstacle codes in allowableKeys

        // Difficulty variables
        // It's set up as a slidable index of probability demands
        float[] beatFrequency  = { 0.60f, 0.70f, 0.80f, 0.90f, 1.00f };       // Percentage of beats a note will spawn
        float[] beatCountOne   = { 0.90f, 0.80f, 0.70f, 0.60f, 0.50f };       // Probability 1 note will spawn
        float[] beatCountTwo   = { 0.96f, 0.92f, 0.90f, 0.85f, 0.80f };       // Probability 2 notes will spawn
        float[] beatCountThree = { 0.99f, 0.98f, 0.97f, 0.96f, 0.95f };       // Probability 3 notes will spawn (rem < 1f -> 4 note blast)
        float[] longNoteChance = { 0.03f, 0.08f, 0.13f, 0.18f, 0.23f };       // Probability that any given note will be a long note

        // Storage variable for whether a note track is in a long note spawning state
        bool[] inLongNote = { false, false, false, false };
        int[] longNoteCount = { 0, 0, 0, 0 };                               // THE MOST BRILLIANT FIX IN HISTORY - BRETTS!
        int difficultyAdjust = difficulty - 1;                              // Have an easier start

        // Storage & precomputes for obstacle insertion logic
        // SERIOUSLY, PLEASE MAKE SURE THE LEVELLENGTHINBEATS IS WELL ABOVE 9x obstaclelength - PREFERABLY ~250
        // I'M DOING THIS QUICK & DIRTY AND AM NOT DOING ANY VALIDATION LOGIC -BRETT
        int[] beatThreshholds = { 0, 0, 0, 0, -obsLength };                 // Stores threshholds for obstacle starts
        beatThreshholds[0] = levelLengthInBeats / 4;                        // Small Obstacle start beat
        beatThreshholds[1] = beatThreshholds[0] * 2;                        // Medium Obstacle start beat
        beatThreshholds[2] = beatThreshholds[0] * 3;                        // Large Obstacle start beat
        beatThreshholds[3] = levelLengthInBeats - obsLength - 1;            // Final (Large) Obstacle start beat
        int currentObstacleLoading = 0;                                     // Which obstacle are we trying to load?

        // Set up notes dictionary for storing notes
        LoadMapKeys();

        // Branching system to decide what notes to add; runs the length of level sequentially
        // This isn't the most efficient use of randoms, could definitely be using more caching & fewer calls
        // But given that this whole function will only run once in the lifetime of the level, it's alright
        for (int currentBeat = 0; currentBeat < levelLengthInBeats; currentBeat++)
        {
            // Decides whether and which notes to spawn in current beat
            if (UnityEngine.Random.Range(0f, 1f) < beatFrequency[difficultyAdjust])
            {
                // Determine how many notes to spawn
                float beatCountProbability = UnityEngine.Random.Range(0f, 1f);  
                int numOfArrowSpawns = 0;

                // Boolean array to store "selected" beat directions
                bool[] bArrowSpawn = new bool[] { false, false, false, false };
                List<int> availableIndices = new List<int> { 0, 1, 2, 3 };

                if (beatCountProbability < beatCountOne[difficultyAdjust]) { numOfArrowSpawns = 1; }
                else if (beatCountProbability < beatCountTwo[difficultyAdjust]) { numOfArrowSpawns = 2; }
                else if (beatCountProbability < beatCountThree[difficultyAdjust]) { numOfArrowSpawns = 3; }
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

            // Logic to insert Obstacles into the sequence
            if (currentBeat == beatThreshholds[currentObstacleLoading])
            {
                notes[allowableKeys[obsIndexOffset + currentObstacleLoading]].Add(currentBeat);
                // Adjust difficulty based off spot in level
                if (currentObstacleLoading == 3) // Make final obstacle harder
                {
                    difficultyAdjust = difficulty + 1;
                }
            }
            else if (currentBeat == beatThreshholds[currentObstacleLoading] + obsLength)
            {
                notes[allowableKeys[obsIndexOffset + currentObstacleLoading]].Add(currentBeat);
                currentObstacleLoading++;
                difficultyAdjust = difficulty;      // Reset difficulty after first obstacle
            }

            // Required to sanitize the edges of an obstacle from long note debris.
            if (currentBeat == beatThreshholds[currentObstacleLoading] + 2 || currentBeat == beatThreshholds[currentObstacleLoading] + obsLength)
            {
                KillLongNotes(currentBeat);
            }


        }

        // Close out any active long notes & add win call
        KillLongNotes(levelLengthInBeats);
        notes[allowableKeys[8]].Add(levelLengthInBeats+12);

        // Local func - nukes long notes & sets one far back iff apppropriate
        void KillLongNotes(int currBeat) 
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    if (notes[allowableKeys[4 + i]].Contains(currBeat - j))
                    {
                        notes[allowableKeys[4 + i]].Remove(currBeat - j);
                        longNoteCount[i]--;
                    }
                }
                if (longNoteCount[i] < 0)
                {
                    longNoteCount[i] = 0;
                }
                else if (longNoteCount[i] % 2 != 0) 
                {
                    notes[allowableKeys[4 + i]].Add(currBeat - 4);
                    longNoteCount[i]++;
                }
                inLongNote[i] = false;
            }
        }

        // Local function to add a short / long note into the notes dict / beatmap
        void AddSelectedRandomNote(int currSelect, int currBeat)
        {
            // Ensuring we're not already in a long note in this direction
            if (!inLongNote[currSelect])
            {
                // Decide whether to add a long or short note
                if (UnityEngine.Random.Range(0,1f) < longNoteChance[difficultyAdjust]) 
                {
                    notes[allowableKeys[4 + currSelect]].Add(currBeat);
                    inLongNote[currSelect] = true;
                    longNoteCount[currSelect]++;
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
                longNoteCount[currSelect]++;
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