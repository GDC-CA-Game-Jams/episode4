using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessTextTrigger : MonoBehaviour
{
    public Dialogue successText;

    public void TriggerSuccessText()
    {
        SuccessTextManager.onSuccessTextTrigger.Invoke(successText);
    }
}
