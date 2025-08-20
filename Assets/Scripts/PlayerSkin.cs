
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Image playerSkinImage;


    public void SetColorDirect(Color c)
    {
        playerSkinImage.color = c;
    }
}
