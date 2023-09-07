using Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BeatSpawner : MonoBehaviour
{
    // --- Serialized Variable Declarations --- //
    [Header("Game Mode Settings")]
    [SerializeField] private GameSettingsSO m_Settings = null;
    [Tooltip("Play mode - SHOULD STAY 'Horizontal' - UI not oriented for vertical play")]
    [SerializeField] private Orientation orientation = Orientation.Horizontal;
    [Tooltip("Read from file or create a random level")]
    [SerializeField] private ReadMode readMode = ReadMode.Read;
    [Tooltip("File name (and only file name) - no path or extension")]
    [SerializeField] private string levelFileName;

    [Header("Gameplay Settings")]
    [Tooltip("Number of 4/4 measures it takes for a note to make it from spawn to 'perfect zone' of beat button")]
    [SerializeField] private int traversalBeatTime = 4; // Default value
    [Tooltip("Controls whether beat guidelines will spawn")]
    [SerializeField] private bool throwBeatGuidelines = true;

    [Header("Scene Object References")]
    [Tooltip("Must be loaded in order ('Down, Right, Up, Left') by style ('Std, HoldStart, HoldBody, HoldEnd')")]
    [SerializeField] private GameObject[] arrows;
    [Tooltip("Contains reference to helper 'guideline' bar")]
    [SerializeField] private GameObject guideline;
    [Tooltip("Order for slotting is counterclockwise from down: Down-0, Right-1, Up-2, Left-3")]
    [SerializeField] private Transform[] SpawnPoints = new Transform[4];
    [Tooltip("Used to grab x position for beat buttons; any of the 4 can be slotted in, just needs X")]
    [SerializeField] private Transform beatButtonPosition;
    [Tooltip("Used to grab the AudioController for useful purposes")]
    [SerializeField] private AudioController AudioControlRef;
    //[Tooltip("Used to grab the AudioManager to play correct obstacle sfx")]
    //public AudioManager audioManager;

    [Header("Obstacle Configuration")]
    private SortedDictionary<int, int[]> obstacleBeats = new SortedDictionary<int, int[]>();
    [SerializeField] private Sprite[] obstacleSprites;
    [SerializeField] private AudioSource[] obstacleSFX;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private float obstacleCoverScale = 1;
    [SerializeField] private float[] obstacleSpriteScales;
    [SerializeField] private int delayOffset = 15;
    [SerializeField] private Vector2[] obtacleSpritePoints;

    // --- Private Variable Declarations - to be used internally --- //
    private Dictionary<string, List<int>> levelNoteMap; // Level map provided by beat reader
    private GameManager gm;                             // Game Manager service link
    private float beatTempo = 0f;                       // Note speed for crossing the screen

    // Storage used for long note logic
    private bool[] isSpawningLongNote = new bool[] { false, false, false, false };  
    private bool[] justStartedLongNote = new bool[] { false, false, false, false };

    // Allowable note styles
    private readonly string[] mapKeys = { "up", "down", "left", "right", 
                                        "hup", "hdown", "hleft", "hright", "win" };

    private void Awake()
    {     
        gm = ServiceLocator.Instance.Get<GameManager>();

        // Set gameplay mode
        if (readMode == ReadMode.Read)
        {
            try
            {
                ServiceLocator.Instance.Get<BeatReader>().Init(levelFileName);
                levelNoteMap = ServiceLocator.Instance.Get<BeatReader>().GetNotes();
            }
            catch   // If file either not selected in inspector or not valid, swaps to random mode
            {
                readMode = ReadMode.Random;
                Debug.Log("ERROR: Invalid file location for beatmap - defaulting to randomspawn mode");
            }
        }
        else
        {
            try
            {
                ServiceLocator.Instance.Get<BeatReader>().InitRandomMap();
                levelNoteMap = ServiceLocator.Instance.Get<BeatReader>().GetNotes();
                readMode = ReadMode.Read;
            }
            catch
            {
                Debug.Log("ERROR: Random map generation failed - defaulting to randomspawn mode");
                readMode = ReadMode.Random;
            }
        }

        // Set the speed at what speed notes should cross the screen
        float beatVelocityAdjust = 60f / AudioControlRef.GetBPM();

        // Set beat traversal speed based on game orientation (horizontal / vertical)
        if (orientation == Orientation.Horizontal)
        {
            beatTempo = ((SpawnPoints[0].position.x - beatButtonPosition.position.x) / traversalBeatTime) * beatVelocityAdjust;
        }
        else
        {
            beatTempo = ((SpawnPoints[0].position.y - beatButtonPosition.position.y) / traversalBeatTime) * beatVelocityAdjust;
        }

        LoadObstacles();
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes += OnClearNotes;
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes -= OnClearNotes;
    }

    /// <summary>
    /// Called under 1-step interval in SerializedField in audio controller -
    /// Initiates beat counting and note spawns
    /// </summary>
    public void OnBeat()
    {
        ++gm.beatCount;
        ++gm.beatsElapsed;
        if (throwBeatGuidelines)
        {
            SpawnGuideLine();
        }
        if (readMode == ReadMode.Read)
        {
            ReadSpawn();
        }
        else
        {
            RandomSpawn();
        }
    }

    /// <summary>
    /// Follows text file instructions for note generation - highly syntax sensitive -
    /// ANY CHANGES TO NOTEMAP TEXT FILE SYNTAX WILL REQUIRE CHANGES HERE
    /// </summary>
    private void ReadSpawn()
    {
        // Run through & spawn the various styles of note in the various directions
        for (int i = 0; i < 4; i++)
        {
            if (levelNoteMap[mapKeys[4 + i]].Count != 0 && levelNoteMap[mapKeys[4 + i]].Contains(gm.beatCount))
            {
                SpawnLongNote((NoteStyle)i);
            }
            else if (levelNoteMap[mapKeys[i]].Count != 0 && levelNoteMap[mapKeys[i]].Contains(gm.beatCount))
            {
                SpawnNote((NoteStyle)i);
            }
        }

        // Check for & run obstacles
        if (obstacleBeats.Count != 0 && obstacleBeats.ContainsKey(gm.beatCount))
        {
            RunObstacle(obstacleBeats[gm.beatCount][0], obstacleBeats[gm.beatCount][1]);
        }

        // Check for win condition
        if (levelNoteMap["win"].Count != 0 && levelNoteMap["win"].Contains(gm.beatCount))
        {
            ServiceLocator.Instance.Get<EventManager>().OnClearNotes?.Invoke();
            ServiceLocator.Instance.Get<EventManager>().OnSongComplete?.Invoke();
        }
    }

    /// <summary>
    /// Backup random spawn mode, early iteration, only singular arrows
    /// </summary>
    private void RandomSpawn()
    {
        // Figures out how many arrows to spawn in decreasing ~60% increments of quantity
        int numOfArrowSpawns = UnityEngine.Random.Range(0, 100);
        if (numOfArrowSpawns < 70) { numOfArrowSpawns = 1; }
        else if (numOfArrowSpawns < 91) { numOfArrowSpawns = 2; }
        else if (numOfArrowSpawns < 98) { numOfArrowSpawns = 3; }
        else { numOfArrowSpawns = 4; }

        // This is an inefficient process, but RandomSpawn is quick & dirty & only really used for input testing
        // Sets true booleans for selected arrow spawn directions up to the count required
        bool[] bArrowSpawn = new bool[] { false, false, false, false }; //up,down,left,right
        while (numOfArrowSpawns > 0)
        {
            int randGrab = UnityEngine.Random.Range(0, 4);
            if (!bArrowSpawn[randGrab])
            {
                bArrowSpawn[randGrab] = true;
                numOfArrowSpawns--;
            }
        }

        // Spawns an arrow in the selected directions
        if (bArrowSpawn[0]) { SpawnNote(NoteStyle.Down); }
        if (bArrowSpawn[1]) { SpawnNote(NoteStyle.Right); }
        if (bArrowSpawn[2]) { SpawnNote(NoteStyle.Up); }
        if (bArrowSpawn[3]) { SpawnNote(NoteStyle.Left); }
    }

    private void LoadObstacles()
    {
        obstacle.GetComponent<ObstacleBehaviour>().beatTempo = beatTempo;
        LoadObstacle("os", ObstacleType.Small);
        LoadObstacle("om", ObstacleType.Medium);
        LoadObstacle("ol", ObstacleType.Large);
        LoadObstacle("ox", ObstacleType.XL);
    }

    private void LoadObstacle(string key, ObstacleType t)
    {
        if (!levelNoteMap.ContainsKey(key))
        {
            return;
        }
        for (int i = 0; i < levelNoteMap[key].Count - 1; i += 2)
        {
            obstacleBeats.TryAdd(levelNoteMap[key][i], new []{(int)t, levelNoteMap[key][i + 1]});
            obstacleBeats.TryAdd(levelNoteMap[key][i + 1], new []{0, 0});
        }
    }
    
    /// <summary>
    /// Spawns or despawns an obstacle
    /// </summary>
    private void RunObstacle(int t, int end)
    {
        if (obstacle.activeSelf)
        {
            StartCoroutine(DelayedDespawn(t));
            return;
        }
        
        obstacle.GetComponent<ObstacleBehaviour>().startBeat = gm.beatCount;
        Transform sprite = obstacle.transform.GetChild(0);
        sprite.GetComponent<SpriteRenderer>().sprite = obstacleSprites[t];
        sprite.localScale = Vector3.one * obstacleSpriteScales[t];
        sprite.localPosition = obtacleSpritePoints[t];
        Transform cover = obstacle.transform.GetChild(1);
        cover.localScale = new Vector3((end - gm.beatCount) * obstacleCoverScale, cover.localScale.y);
        obstacle.transform.position = new Vector2(SpawnPoints[4].position.x + (cover.localScale.x / 2f),
            SpawnPoints[4].position.y);
        obstacle.SetActive(true);
    }

    private IEnumerator DelayedDespawn(int s)
    {
        int startBeat = gm.beatCount;
        yield return new WaitUntil(() => gm.beatCount >= startBeat + delayOffset);
        obstacle.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Explode");
        // explode sfx
        obstacleSFX[s].Play();
        Debug.Log("playing sfx #:" + s);

        yield return new WaitForSeconds(0.4f);
        obstacle.SetActive(false);
    }

/// <summary>
/// Spawns a short note in the indicated spawn lane and sends it out.
/// </summary>
/// <param name="noteStyle"></param>
    private void SpawnNote(NoteStyle noteStyle)
    {
        // Instantiate a new arrow segment of correct variety
        int noteDirection = (int)noteStyle;
        var noteObject = Instantiate(arrows[noteDirection]);
        var noteObjectScript = noteObject.GetComponent<NoteObject>();

        // Setting note segment's position
        noteObject.transform.SetParent(gameObject.transform, false);
        noteObject.transform.position = SpawnPoints[noteDirection].position;

        // Setting note segment's velocitty
        noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
        if (orientation == Orientation.Horizontal)
        {
            noteObjectScript.rb.velocity = new Vector2(beatTempo * -1, 0f);
        }
        else if (orientation == Orientation.Vertical)
        {
            noteObjectScript.rb.velocity = new Vector2(0f, beatTempo * -1);
        }
    }

    /// <summary>
    /// Spawns a long note start/end and sends it out.
    /// </summary>
    /// <param name="noteStyle"></param>
    private void SpawnLongNote(NoteStyle noteStyle)
    {
        // Declare variables for use in body of function
        int noteDirection = (int)noteStyle;
        GameObject noteObject;
        NoteObject noteObjectScript;       

        // Decide whether we're spawning a start or end to a long note
        if (!isSpawningLongNote[noteDirection])
        {
            noteObject = Instantiate(arrows[noteDirection + 4]);
            isSpawningLongNote[noteDirection] = true;
            justStartedLongNote[noteDirection] = true;
        }
        else
        {
            noteObject = Instantiate(arrows[noteDirection + 12]);
            isSpawningLongNote[noteDirection] = false;
        }

        // Setting a flag to be used by the input controls system
        noteObjectScript = noteObject.GetComponent<NoteObject>();
        noteObjectScript.SetFlagIsLongNote(true);
        
        // Setting note segment's position
        noteObject.transform.SetParent(gameObject.transform, false);
        noteObject.transform.position = SpawnPoints[noteDirection].position;

        // Setting note segment's velocity
        noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
        if (orientation == Orientation.Horizontal)
        {
            noteObjectScript.rb.velocity = new Vector2(beatTempo * -1, 0f);
        }
        else if (orientation == Orientation.Vertical)
        {
            noteObjectScript.rb.velocity = new Vector2(0f, beatTempo * -1);
        }
    }

    /// <summary>
    /// Called under 4-step interval SerializedField in audio controller
    /// Because it needs to do its work between beats.
    /// </summary>
    public void SpawnHoldBodies()
    {
        for (int i = 0; i < 4; i++)
        {
            if (isSpawningLongNote[i])
            {
                if (!justStartedLongNote[i])
                {
                    var bodyObject = Instantiate(arrows[8 + i]);

                    //scaling of hold bodies relative to screen velocity
                    //note that 120 is actually the proper scaling, but 119f gives a tiny bite of overlap
                    //the overlap is used to cover any gaps from tiny bits of screen lag (which would sometimes appear)
                    bodyObject.GetComponent<Transform>().localScale = new Vector3((80 * beatTempo) / 119f / 3f, 1f, 1f);

                    bodyObject.transform.SetParent(gameObject.transform, false);
                    bodyObject.transform.position = SpawnPoints[i].position;

                    if (orientation == Orientation.Horizontal)
                    {
                        bodyObject.GetComponent<Rigidbody2D>().velocity = new Vector2(beatTempo * -1, 0f);
                    }
                    else if (orientation == Orientation.Vertical)
                    {
                        bodyObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, beatTempo * -1);
                    }
                }
                else
                {
                    justStartedLongNote[i] = false; //functions as a 1 tick delay to avoid sprite clipping
                }
            }
        }
    }

    /// <summary>
    /// Temporary function to do beat measure guidelines
    /// </summary>
    public void SpawnGuideLine()
    {
        var guidelineObject = Instantiate(guideline) ;
        guidelineObject.transform.SetParent(gameObject.transform, false);
        guidelineObject.transform.position = SpawnPoints[1].position;
        if (gm.beatCount % 4 == 0) //drawing quarter note lines
        {
            guidelineObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        else if (gm.beatCount % 2 == 0) //drawing 8th note lines
        {
            guidelineObject.GetComponentInChildren<SpriteRenderer>().color = Color.grey;
        }
        else //draw 16th note lines
        {
            guidelineObject.GetComponentInChildren<SpriteRenderer>().color = Color.black;
        }
        guidelineObject.GetComponent<Rigidbody2D>().velocity = new Vector2(beatTempo * -1, 0f);
    }

    private void OnClearNotes()
    {
        for (int i = 0; i < isSpawningLongNote.Length; ++i)
        {
            isSpawningLongNote[i] = false;
            justStartedLongNote[i] = false;
        }
    }
}

// --- Enum Declarations ---
/// <summary>
/// Gameplay orientation - game currently set up for horizontal play mode
/// </summary>
public enum Orientation
{
    Horizontal,
    Vertical,
}
/// <summary>
/// Dictates whether game reads from a beatmap file or spawns randomly
/// </summary>
public enum ReadMode
{
    Random,
    Read,
}
/// <summary>
/// 4 cardinal directions, counterclockwise from down position
/// </summary>
public enum NoteStyle
{
    Down, 
    Right, 
    Up, 
    Left,
}
public enum ObstacleType
{
    Small,
    Medium,
    Large,
    XL
}