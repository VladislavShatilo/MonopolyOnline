using Photon.Pun;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using Zenject;

public class PlayerView : MonoBehaviourPun, IPunInstantiateMagicCallback
{
     private PlayerSkin playerSkin;
     private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        playerSkin = GetComponent<PlayerSkin>();
    }
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        var playerRoot = GameObject.FindGameObjectWithTag("PlayerRoot");
        if (playerRoot != null)
        {
            transform.SetParent(playerRoot.transform, false);
        }

        object[] data = info.photonView.InstantiationData;
        if (data != null && data.Length == 5)
        {
            int playerId = (int)data[0];
            float r = (float)data[1];
            float g = (float)data[2];
            float b = (float)data[3];
            Vector3 position = (Vector3)data[4];

            rectTransform.anchoredPosition = position;
            Color color = new Color(r, g, b, 1);
            playerSkin.SetColorDirect(color);
        }
       
    }
}