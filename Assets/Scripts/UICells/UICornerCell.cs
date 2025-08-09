using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICornerCell : MonoBehaviour
{
    [SerializeField] private Image logoImage;
   
    public void SetLogo(Sprite sprite)
    {
        logoImage.sprite = sprite;
    }
}
