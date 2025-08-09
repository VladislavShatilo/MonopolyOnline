using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIQuestionCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;

    public void SetupQuestion()
    {
        questionText.text = "?";
    }
    public void RotateQuestionText()
    {
        questionText.rectTransform.eulerAngles = new Vector3(0,0,180);
    }


}
