using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System.Data;
using Services;

public class SuccessTextManager : MonoBehaviour
{
    public Dialogue successText;

    public Transform targetLocation;

    public Animator animator;

    public TMP_Text textMesh;
    Mesh mesh;
    Vector3[] vertices;
    Color[] colors;

    [SerializeField]
    private Camera cam;

    Gradient rainbow;

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
        //textMesh = GetComponent<TMP_Text>();
        cam = Camera.main;
        rainbow = new Gradient();
    }

    //for activating success text directly through SuccessTextManager
    public void StartSuccessText()
    {
        Debug.Log("Text starting!");
        animator.Play("SuccessFade", -1, 0f);
        Random.InitState(System.DateTime.Now.Millisecond);
        string phrase = successText[Random.Range(0, successText.sentences.Length)];
        textMesh.text = phrase;
    }

    //for activating success text through a gameobject with the SuccessTextTrigger script
    public void StartSuccessTextExternal(Dialogue phrases)
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        string phrase = phrases[Random.Range(0, phrases.sentences.Length)];
        textMesh.text = phrase;
    }

    
    void Update()
    {
        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        vertices = mesh.vertices;
        colors = mesh.colors;

        for (int i = 0; i < textMesh.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo c = textMesh.textInfo.characterInfo[i];

            int index = c.vertexIndex;

            //add rainbow color gradient effect to each character in text mesh
            colors[index] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index].x * 0.001f, 1f));
            colors[index + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 1].x * 0.001f, 1f));
            colors[index + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 2].x * 0.001f, 1f));
            colors[index + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index + 3].x * 0.001f, 1f));
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            //add wobble effect to each vertice in the text mesh
            Vector3 offset = Wobble(Time.time + i);

            vertices[i] = vertices[i] + offset;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        textMesh.canvasRenderer.SetMesh(mesh);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, targetLocation.position);
        textMesh.rectTransform.position = screenPos;

    }

    Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * 3.3f), Mathf.Cos(time * 1.8f));
    }
}
