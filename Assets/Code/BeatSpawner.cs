using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BeatSpawner : MonoBehaviour
{
    [SerializeField] private GameSettingsSO m_Settings = null;

    public Orientation orientation = Orientation.Horizontal;
    public ReadMode readmode = ReadMode.Random;

    public float beatTempo; //gets auto overridden by settings beatTempo

    public float beatFrequency; //gets auto overridden by settings beatFrequency

    public GameObject notePrefab;

    public Transform SpawnPointUp, SpawnPointDown, SpawnPointLeft, SpawnPointRight;

    private float clock = 0f;

    private void Awake()
    {
        //read new value from scriptableobject for "prestige mode" speed
        beatFrequency = m_Settings.beatFrequency;
        beatTempo = m_Settings.beatTempo;
    }

    private void Update()
    {
        clock += Time.deltaTime;
        if (clock > beatFrequency)
        {
            clock = 0f;

            if (readmode == ReadMode.Random)
            {
                RandomSpawn();
            }
        }
    }
    private void RandomSpawn()
    {
        var noteObject = Instantiate(notePrefab);
        var noteObjectScript = noteObject.GetComponent<NoteObject>();

        noteObject.transform.SetParent(gameObject.transform, false);

        int direction = UnityEngine.Random.Range(0, 4);
        switch(direction)
        {
            case 0: //down
                noteObject.transform.position = SpawnPointDown.position;
                break;
            case 1: //right
                noteObject.transform.position = SpawnPointRight.position;
                noteObject.transform.Rotate(new Vector3(0, 0, 90));
                break;
            case 2: //up
                noteObject.transform.position = SpawnPointUp.position;
                noteObject.transform.Rotate(new Vector3(0, 0, 180));
                break;
            case 3: //left
                noteObject.transform.position = SpawnPointLeft.position;
                noteObject.transform.Rotate(new Vector3(0, 0, 270));
                break;
        }

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