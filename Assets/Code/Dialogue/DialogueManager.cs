using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TMP_Text dialog;

    public Animator animator;

    private bool hasStarted;

    public static UnityEvent<Dialogue> onDialogueTrigger;

    //[SerializeField]
    private Queue<string> sentences;

    private void Awake()
    {
        onDialogueTrigger = new UnityEvent<Dialogue>();
        onDialogueTrigger.AddListener(StartDialogue);
    }

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();


    }

    // Update is called once per frame
    public void StartDialogue(Dialogue dialog)
    {
        if (!hasStarted)
        {
            Debug.Log("Starting dialog");

            animator.SetBool("IsOpen", true);

            sentences.Clear();

            foreach (string sentence in dialog.sentences)
            {
                sentences.Enqueue(sentence);
            }
            hasStarted = true;
            DisplayNextSentence();
        }

        else
        {
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0 )
        {
            EndDialogue();
            return;
        }

        string sentence =  sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialog.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialog.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void EndDialogue()
    {
        hasStarted = false;
        animator.SetBool("IsOpen", false);
    }
}
