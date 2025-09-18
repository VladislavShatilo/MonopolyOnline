using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] string region;
    private ISceneLoader sceneLoader;

    [Inject]
    public void Construct(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader;
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToRegion(region);

    }
    public override void OnConnectedToMaster()
    {
        if (sceneLoader != null)
        {
            //SceneFadeManager.instance.LoadAutorizationScene();

        }
        else
        {
            SceneManager.LoadScene("AutorizationScene");

        }
    }

}
