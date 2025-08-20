using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIQuestionCell : UICellBase
{
    [SerializeField] private TextMeshProUGUI questionText;
    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        var question = cellData.questionData;

        questionText.text = "?";

    }

    public void RotateQuestionText()
    {
        questionText.rectTransform.eulerAngles = new Vector3(0,0,180);
    }


}
