using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISpendCell : MonoBehaviour
{
    [SerializeField] private Image spendImage;

    public void SetupSpend(Sprite sprite)
    {
        spendImage.sprite = sprite;
    }
    public void RotateIcon()
    {
        spendImage.rectTransform.eulerAngles = new Vector3(0,0,270);
    }
}
