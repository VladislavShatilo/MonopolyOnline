using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIQuestionCell : UICellBase
{
    [SerializeField] private TextMeshProUGUI questionText;

    #region PUBLIC_METHODS

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        if (questionText == null)
            throw new ArgumentNullException(nameof(questionText));
        var question = cellData.questionData;
        questionText.text = "?";

    }

    public void RotateQuestionText()
    {
        if (questionText == null)
            throw new ArgumentNullException(nameof(questionText));
        questionText.rectTransform.eulerAngles = new Vector3(0, 0, 180);
    }

    #endregion PUBLIC_METHODS



}
