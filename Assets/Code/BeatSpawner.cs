using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BeatSpawner : MonoBehaviour
{
    [SerializeField] private GameSettingsSO m_Settings = null;
    
    public Orientation orientation = Orientation.Horizontal;

    public float beatTempo;

    public float beatFrequency;

    public GameObject notePrefab;

    public Transform spawnPoint;

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
        if(clock > beatFrequency)
        {
            clock = 0f;
            var noteObject = Instantiate(notePrefab);
            var noteObjectScript = noteObject.GetComponent<NoteObject>();

            noteObject.transform.SetParent(gameObject.transform, false);
            noteObject.transform.position = spawnPoint.position;

            if(orientation == Orientation.Horizontal)
            {
                noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
                noteObjectScript.rb.velocity = new Vector2(beatTempo * -1, 0f);
            }
            else if(orientation == Orientation.Vertical)
            {
                noteObjectScript.rb = noteObject.GetComponent<Rigidbody2D>();
                noteObjectScript.rb.velocity = new Vector2(0f, beatTempo * -1);
            }
        }
    }
}

public enum Orientation
{
    Horizontal,
    Vertical,
}