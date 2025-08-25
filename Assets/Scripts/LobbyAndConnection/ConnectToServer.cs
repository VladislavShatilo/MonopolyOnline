using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] string region;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToRegion(region);

    }
    public override void OnConnectedToMaster()
    {
        if (SceneFadeManager.instance != null)
        {
            SceneFadeManager.instance.LoadAutorizationScene();

        }
        else
        {
            SceneManager.LoadScene("AutorizationScene");

        }
    }

}
