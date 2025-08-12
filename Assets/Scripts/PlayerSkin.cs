using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Image playerSkinImage;

    public void SetSkinColor(Color color)
    {
        playerSkinImage.color = color;
    }
}
