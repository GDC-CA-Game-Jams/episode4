using Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BeatSpawner : MonoBehaviour
{
    [Header("Gameplay Settings")]
    [SerializeField] private GameSettingsSO m_Settings = null;
    
    public Orientation orientation = Orientation.Horizontal;
    public ReadMode readMode = ReadMode.Read; //Read = playing from file, Random = Random Spawn mode as default / test mode
    public string levelFileName; //contains file name & only name, not path

    [Header("Possibly Removable Variables")] //these don't have any need to be visible in the inspector as of now
    public float beatTempo; //gets auto overridden by settings beatTempo, CONSIDER MAKING PRIVATE
    [Tooltip("Number of 4/4 measures it takes for a note to make it from spawn to 'perfect zone' of beat button")]
    public int traversalBeatTime = 4; //default value
    [Tooltip("Controls whether beat guidelines will spawn ")]
    [SerializeField] private bool throwBeatGuidelines = true;
    //public float beatFrequency; //gets auto overridden by settings beatFrequency, CONSIDER MAKING PRIVATE

    [Header("Scene Object References")]
    [SerializeField] private GameObject[] arrows;
    [SerializeField] private GameObject guideline;
    [Tooltip("Order for slotting is counterclockwise from down: Down-0, Right-1, Up-2, Left-3")]
    public Transform[] SpawnPoints = new Transform[4];
    [Tooltip("Used to grab x position for beat buttons; any of the 4 can be slotted in, just needs X")]
    public Transform beatButtonPosition;
    [Tooltip("Used to grab the AudioController for useful purposes")]
    public AudioController AudioControlRef;
    
    //public Transform SpawnPointUp, SpawnPointDown, SpawnPointLeft, SpawnPointRight; //contains spawnpoints for arrows

    //private variables
    private float clock = 0f;
    private Dictionary<string, List<int>> levelNoteMap;
    private GameManager gm;
    private float beatVelocity = 0f;
    //stores whether each note is "in" the middle of a long/hold note
    private bool[] isSpawningLongNote = new bool[] { false, false, false, false };
    private bool[] justStartedLongNote = new bool[] { false, false, false, false };
    private GameObject[] longNoteExtendoBar = new GameObject[4];
    
    [Header("Obstacle Configuration")]
    private SortedDictionary<int, int[]> obstacleBeats = new SortedDictionary<int, int[]>();
    [SerializeField] private Sprite[] obstacleSprites;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private float obstacleCoverScale = 1;
    [SerializeField] private float[] obstacleSpriteScales;
    [SerializeField] private int delayOffset = 15;
    [SerializeField] private Vector2[] obtacleSpritePoints;

    private void Awake()
    {
        //read new value from scriptableobject for "prestige mode" speed
        //beatFrequency = m_Settings.beatFrequency;
        beatTempo = m_Settings.beatTempo;

        if (readMode == ReadMode.Read)
        {
            //if (System.IO.File.Exists("Assets/Resources/" + levelFileName + ".txt")) //checking if file is valid
            try
            {
                ServiceLocator.Instance.Get<BeatReader>().Init(levelFileName);
                levelNoteMap = ServiceLocator.Instance.Get<BeatReader>().GetNotes();
            }
            catch //if file either not selected in inspector or not valid, swaps to random mode
            {
                readMode = ReadMode.Random;
                Debug.Log("Random Mode Engaged - Invalid File Location");
            }
        }

        gm = ServiceLocator.Instance.Get<GameManager>();

        //Segment to set the speed at what speed notes should cross the screen
        //make this read from audiocontroller's BPM field
        float beatVelocityAdjust = 60f / AudioControlRef.GetBPM() ; //right now this is a magic number FIXME


        if (orientation == Orientation.Horizontal)
        {
            beatTempo = ((SpawnPoints[0].position.x - beatButtonPosition.position.x) / traversalBeatTime) * beatVelocityAdjust;
        }
        else
        {
            beatTempo = ((SpawnPoints[0].position.y - beatButtonPosition.position.y) / traversalBeatTime) * beatVelocityAdjust;
        }

        beatVelocity = SpawnPoints[0].position.x;

        LoadObstacles();
    }

    /// <summary>
    /// Follows text file instructions for note generation. Currently highly syntax sensitive.
    /// ANY CHANGES TO NOTEMAP TEXT FILE SYNTAX REQUIRES THIS BE CHANGED
    /// </summary>
    private void ReadSpawn()
    {
        //going to add beatspawner enum to beatreader's dict later to make this matching easy
        //for now this is just going to be a wee bit ugly and full of magic values for beat directions

        if (levelNoteMap["hup"].Count != 0 && levelNoteMap["hup"].Contains(gm.beatCount))
        {
            SpawnLongNote(NoteStyle.Up);
        }
        if (levelNoteMap["hdown"].Count != 0 && levelNoteMap["hdown"].Contains(gm.beatCount))
        {
            SpawnLongNote(NoteStyle.Down);
        }
        if (levelNoteMap["hleft"].Count != 0 && levelNoteMap["hleft"].Contains(gm.beatCount))
        {
            SpawnLongNote(NoteStyle.Left);
        }
        if (levelNoteMap["hright"].Count != 0 && levelNoteMap["hright"].Contains(gm.beatCount))
        {
            SpawnLongNote(NoteStyle.Right);
        }
        if ((levelNoteMap["up"].Count != 0 && levelNoteMap["up"].Contains(gm.beatCount)) && !isSpawningLongNote[0])
        {
            SpawnNote(NoteStyle.Up);
        }
        if (levelNoteMap["down"].Count != 0 && levelNoteMap["down"].Contains(gm.beatCount))
        {
            SpawnNote(NoteStyle.Down);
        }
        if (levelNoteMap["left"].Count != 0 && levelNoteMap["left"].Contains(gm.beatCount))
        {
            SpawnNote(NoteStyle.Left);
        }
        if (levelNoteMap["right"].Count != 0 && levelNoteMap["right"].Contains(gm.beatCount))
        {
            SpawnNote(NoteStyle.Right);
        }
        if (obstacleBeats.Count != 0 && obstacleBeats.ContainsKey(gm.beatCount))
        {
            RunObstacle(obstacleBeats[gm.beatCount][0], obstacleBeats[gm.beatCount][1]);
        }

        if (levelNoteMap["win"].Count != 0 && levelNoteMap["win"].Contains(gm.beatCount))
        {
            ServiceLocator.Instance.Get<EventManager>().OnClearNotes?.Invoke();
            ServiceLocator.Instance.Get<EventManager>().OnSongComplete?.Invoke();
        }

        SpawnHoldBodies();
    }

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

    private void SpawnHoldBodies()
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
                    bodyObject.GetComponent<Transform>().localScale = new Vector3((80 * beatTempo) / 119f, 1f, 1f);
                    
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
                    justStartedLongNote[i] = false;
                }
            }
        }
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
            StartCoroutine(DelayedDespawn());
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

    private IEnumerator DelayedDespawn()
    {
        int startBeat = gm.beatCount;
        yield return new WaitUntil(() => gm.beatCount >= startBeat + delayOffset);
        obstacle.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Explode");
        yield return new WaitForSeconds(0.3f);
        obstacle.SetActive(false);
    }
    
    /// <summary>
    /// spawns random arrows in all directions.
    /// </summary>
    private void RandomSpawn()
    {
        //figures out how many arrows to spawn in decreasing ~60% increments of quantity
        int numOfArrowSpawns = UnityEngine.Random.Range(0, 100);
        if (numOfArrowSpawns < 70)      { numOfArrowSpawns = 1; }
        else if (numOfArrowSpawns < 91) { numOfArrowSpawns = 2; }
        else if (numOfArrowSpawns < 98) { numOfArrowSpawns = 3; }
        else                            { numOfArrowSpawns = 4; }

        //this is an inefficient process, but RandomSpawn is quick & dirty & only really used for input testing
        //sets true booleans for selected arrow spawn directions up to the count required
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

        //spawns an arrow in the selected directions
        if (bArrowSpawn[0]) { SpawnNote(NoteStyle.Down); }
        if (bArrowSpawn[1]) { SpawnNote(NoteStyle.Right); }
        if (bArrowSpawn[2]) { SpawnNote(NoteStyle.Up); }
        if (bArrowSpawn[3]) { SpawnNote(NoteStyle.Left); }
    }

/// <summary>
/// Spawns a note in the indicated spawn lane and sends it out.
/// </summary>
/// <param name="noteStyle"></param>
    private void SpawnNote(NoteStyle noteStyle)
    {
        var noteObject = Instantiate(arrows[(int)noteStyle]);
        var noteObjectScript = noteObject.GetComponent<NoteObject>();

        int noteDirection = (int)noteStyle;

        noteObject.transform.SetParent(gameObject.transform, false);
        noteObject.transform.position = SpawnPoints[noteDirection].position;
        //noteObject.transform.Rotate(new Vector3(0, 0, 90 * noteDirection));

        if (orientation == Orientation.Horizontal)
        {
            noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
            noteObjectScript.rb.velocity = new Vector2(beatTempo * -1, 0f);
        }
        else if (orientation == Orientation.Vertical)
        {
            noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
            noteObjectScript.rb.velocity = new Vector2(0f, beatTempo * -1);
        }
    }

    private void SpawnLongNote(NoteStyle noteStyle)
    {
        //check to see if we're in a long note already (determines whether we're starting or ending a long note)
        //case: starting a long notes
            //set appropriate "isSpawningLongNote" flag bool to true (we're now in a long note)
            //instantiate a long note & send it on its way
        //case: ending a long note
            //set "isSpawningLongNote" to false (we're closing it out)
            //instantiate a long note ending
        //Need separate function to perpetuate any active long notes bodies

        int noteDirection = (int)noteStyle;

        GameObject noteObject;
        NoteObject noteObjectScript;
        noteObject = Instantiate(arrows[noteDirection + 4]);

        if (!isSpawningLongNote[noteDirection])
        {
            isSpawningLongNote[noteDirection] = true;
            justStartedLongNote[noteDirection] = true;
        }
        else
        {
            noteObject.GetComponent<Transform>().localScale = new Vector3(-1f, 1f, 1f);
            isSpawningLongNote[noteDirection] = false;
        }

        noteObjectScript = noteObject.GetComponent<NoteObject>();
        noteObjectScript.SetFlagIsLongNote(true);
        
        noteObject.transform.SetParent(gameObject.transform, false);
        noteObject.transform.position = SpawnPoints[noteDirection].position;

        if (orientation == Orientation.Horizontal)
        {
            noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
            noteObjectScript.rb.velocity = new Vector2(beatTempo * -1, 0f);
        }
        else if (orientation == Orientation.Vertical)
        {
            noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
            noteObjectScript.rb.velocity = new Vector2(0f, beatTempo * -1);
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
}


public enum Orientation
{
    Horizontal,
    Vertical,
}

public enum ReadMode
{
    Random,
    Read,
}

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