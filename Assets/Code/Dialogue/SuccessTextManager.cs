using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System.Reflection;
using UnityEngine.Events;
using Services;

public class SuccessTextManager : MonoBehaviour
{
    public Dialogue successText;

    public Transform targetLocation;
    public RectTransform textParent;

    public Animator animator;

    public TMP_Text textMesh;

    public static UnityEvent<Dialogue> onSuccessTextTrigger;

    Mesh mesh;
    Vector3[] vertices;
    Color[] colors;

    [SerializeField]
    private Camera cam;

    Gradient rainbow;

    //is the text currently animating on screen?
    private bool isVisible;

    private void Awake()
    {
        onSuccessTextTrigger = new UnityEvent<Dialogue>();
        onSuccessTextTrigger.AddListener(StartSuccessTextExternal);
    }

    private void OnEnable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnPerfect += StartSuccessText;
        em.OnExcellent += StartSuccessText;
        em.OnGood += StartSuccessText;
        em.OnPoor += StartSuccessText;
    }

    private void OnDisable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnPerfect -= StartSuccessText;
        em.OnExcellent -= StartSuccessText;
        em.OnGood -= StartSuccessText;
        em.OnPoor -= StartSuccessText;
    }

    void Start()
    {
        cam = Camera.main;
        rainbow = new Gradient();
        isVisible = false;

        //setting gradient for rainbow effect
        Color _orange = new Color(255, 165, 0);
        Color _purple = new Color32(143, 0, 254, 1);

        var colors = new GradientColorKey[6];
        colors[0] = new GradientColorKey(Color.red, 0.3f);
        colors[1] = new GradientColorKey(_orange, 0.550f);
        colors[2] = new GradientColorKey(Color.yellow, 0.551f);
        colors[3] = new GradientColorKey(Color.green, 0.552f);
        colors[4] = new GradientColorKey(Color.blue, 0.8f);
        colors[5] = new GradientColorKey(_purple, 1.0f);

        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        rainbow.SetKeys(colors, alphas);
    }

    //for activating success text directly through SuccessTextManager
    public void StartSuccessText()
    {
        animator.Play("SuccessFade", -1, 0f);
        isVisible = true;

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        string phrase = successText[UnityEngine.Random.Range(0, successText.sentences.Length)];
        textMesh.text = phrase;
        StopAllCoroutines();
        StartCoroutine(WaitToEndDialogue());
    }

    //for activating success text through a gameobject with the SuccessTextTrigger script
    public void StartSuccessTextExternal(Dialogue phrases)
    {
        animator.Play("SuccessFade", -1, 0f);
        isVisible = true;

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        string phrase = phrases[UnityEngine.Random.Range(0, phrases.sentences.Length)];
        textMesh.text = phrase;
        StopAllCoroutines();
        StartCoroutine(WaitToEndDialogue());
    }

    IEnumerator WaitToEndDialogue()
    {
        yield return new WaitForSeconds(3f);
        isVisible = false;
    }


    void Update()
    {
        //if text is currently animating and visible, create wobble/rainbow effects
        if(isVisible)
        {
            textMesh.ForceMeshUpdate();
            mesh = textMesh.mesh;
            vertices = mesh.vertices;
            colors = mesh.colors;

            for (int i = 0; i < textMesh.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo c = textMesh.textInfo.characterInfo[i];

                int index = c.vertexIndex;

                Vector3 offset = Wobble(Time.time + i);
                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;

                //animate rainbow color gradient effect on text mesh
                colors[index] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index].x * 0.001f, 1f));
                colors[index + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 1].x * 0.001f, 1f));
                colors[index + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 2].x * 0.001f, 1f));
                colors[index + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 3].x * 0.001f, 1f));
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            textMesh.canvasRenderer.SetMesh(mesh);
        }
        
        //set text spawn location to above player
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, targetLocation.position);
        textParent.position = screenPos;

    }

    Vector2 Wobble(float time)
    {
        //Sin 3.3f  Cos 1.8f
        return new Vector2(Mathf.Sin(time * 4f), Mathf.Cos(time * 4f));
    }
}
