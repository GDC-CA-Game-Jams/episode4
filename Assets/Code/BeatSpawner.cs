using Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class BeatSpawner : MonoBehaviour
{
    [Header("Gameplay Settings")]
    [SerializeField] private GameSettingsSO m_Settings = null;

    public Orientation orientation = Orientation.Horizontal;
    public ReadMode readMode = ReadMode.Read; //Read = playing from file, Random = Random Spawn mode as default / test mode
    public string levelFileName; //contains file name & only name, not path

    [Header("Possibly Removable Variables")] //these don't have any need to be visible in the inspector as of now
    public float beatTempo; //gets auto overridden by settings beatTempo, CONSIDER MAKING PRIVATE
    public float beatFrequency; //gets auto overridden by settings beatFrequency, CONSIDER MAKING PRIVATE

    [Header("Scene Object References")]
    public GameObject notePrefab; //contains a reference to the note prefab to be rotated & used in spawns
    [Tooltip("Order for slotting is counterclockwise from down: Down-0, Right-1, Up-2, Left-3")]
    public Transform[] SpawnPoints = new Transform[4]; 
    
    //public Transform SpawnPointUp, SpawnPointDown, SpawnPointLeft, SpawnPointRight; //contains spawnpoints for arrows

    //private variables
    private float clock = 0f;
    private int beatCount = -1; //tracks current beat of game
    private Dictionary<string, List<int>> levelNoteMap; //contains the
    
    [Header("Obstacle Configuration")]
    private SortedDictionary<int, int[]> obstacleBeats = new SortedDictionary<int, int[]>();
    [SerializeField] private Sprite[] obstacleSprites;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private float obstacleCoverScale = 1;
    [SerializeField] private float[] obstacleSpriteScales;

    private void Awake()
    {
        //read new value from scriptableobject for "prestige mode" speed
        beatFrequency = m_Settings.beatFrequency;
        beatTempo = m_Settings.beatTempo;

        if (readMode == ReadMode.Read)
        {
            if (System.IO.File.Exists("Assets/Resources/" + levelFileName + ".txt")) //checking if file is valid
            {
                ServiceLocator.Instance.Get<BeatReader>().Init(levelFileName);
                levelNoteMap = ServiceLocator.Instance.Get<BeatReader>().GetNotes();
            }
            else //if file either not selected in inspector or not valid, swaps to random mode
            {
                readMode = ReadMode.Random;
                Debug.Log("Random Mode Engaged - Invalid File Location");
            }
        }
        
        LoadObstacles();

    }


    private void Update()
    {
        clock += Time.deltaTime;
        if (clock > beatFrequency)
        {
            clock = 0f;
            beatCount++;

            if (readMode == ReadMode.Read)
            {
                ReadSpawn();
            }
            else
            {
                RandomSpawn();
            }
        }
    }

    /// <summary>
    /// Follows text file instructions for note generation. Currently highly syntax sensitive.
    /// ANY CHANGES TO NOTEMAP TEXT FILE SYNTAX REQUIRES THIS BE CHANGED
    /// </summary>
    private void ReadSpawn()
    {
        //going to add beatspawner enum to beatreader's dict later to make this matching easy
        //for now this is just going to be a wee bit ugly and full of magic values for beat directions

        if (levelNoteMap["up"].Count != 0 && levelNoteMap["up"][0] <= beatCount)
        {
            SpawnNote(NoteStyle.Up);
            levelNoteMap["up"].Remove(beatCount);
        }
        if (levelNoteMap["down"].Count != 0 && levelNoteMap["down"][0] <= beatCount)
        {
            SpawnNote(NoteStyle.Down);
            levelNoteMap["down"].Remove(beatCount);
        }
        if (levelNoteMap["left"].Count != 0 && levelNoteMap["left"][0] <= beatCount)
        {
            SpawnNote(NoteStyle.Left);
            levelNoteMap["left"].Remove(beatCount);
        }
        if (levelNoteMap["right"].Count != 0 && levelNoteMap["right"][0] <= beatCount)
        {
            SpawnNote(NoteStyle.Right);
            levelNoteMap["right"].Remove(beatCount);
        }
        if (obstacleBeats.Count != 0 && obstacleBeats.ContainsKey(beatCount))
        {
            RunObstacle(obstacleBeats[beatCount][0], obstacleBeats[beatCount][1]);
        }
    }

    private void LoadObstacles()
    {
        LoadObstacle("os", ObstacleType.Small);
        LoadObstacle("om", ObstacleType.Medium);
        LoadObstacle("ol", ObstacleType.Large);
        LoadObstacle("ox", ObstacleType.XL);
    }

    private void LoadObstacle(string key, ObstacleType t)
    {
        for (int i = 0; i < levelNoteMap[key].Count - 1; ++i)
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
        Debug.Log("Running obstacle, start: " + beatCount + " end: " + end);
        if (obstacle.activeSelf)
        {
            Debug.Log("Despawning obstacle!");
            obstacle.SetActive(false);
            return;
        }

        Debug.Log("Spawning Obstacle!");
        obstacle.transform.position = SpawnPoints[4].position;
        Transform sprite = obstacle.transform.GetChild(0);
        sprite.GetComponent<Image>().sprite = obstacleSprites[t];
        sprite.localScale = Vector3.one * obstacleSpriteScales[t];
        Transform cover = obstacle.transform.GetChild(1);
        cover.localScale = new Vector3((end - beatCount) * obstacleCoverScale, cover.localScale.y);
        obstacle.SetActive(true);
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
        var noteObject = Instantiate(notePrefab);
        var noteObjectScript = noteObject.GetComponent<NoteObject>();

        int noteDirection = (int)noteStyle;

        noteObject.transform.SetParent(gameObject.transform, false);
        noteObject.transform.position = SpawnPoints[noteDirection].position;
        noteObject.transform.Rotate(new Vector3(0, 0, 90 * noteDirection));

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