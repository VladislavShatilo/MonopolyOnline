using System.Collections;
using Photon.Pun;
using UnityEngine;

public class DiceManagerPhoton : MonoBehaviourPun
{
    [Header("Dice Prefab and Positions")]
   // [SerializeField] private GameObject dicePrefab;
    [SerializeField] private GameObject dice1GO;
    [SerializeField] private GameObject dice2GO;
    [SerializeField] private DiceRoll3D dice1Instance;
    [SerializeField] private DiceRoll3D dice2Instance;
    //[SerializeField] private Vector3 dice1Position = new Vector3(-1f, 0, 0);
    //[SerializeField] private Vector3 dice2Position = new Vector3(1f, 0, 0);

  
    private void Start()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        //// Создаём кубики один раз, если их нет
        //if (dice1GO == null)
        //{
        //    dice1GO = PhotonNetwork.Instantiate(dicePrefab.name, dice1Position, Quaternion.identity);
        //    dice1Instance = dice1GO.GetComponent<DiceRoll3D>();
        //}

        //if (dice2GO == null)
        //{
        //    dice2GO = Instantiate(dicePrefab, dice2Position, Quaternion.identity);
        //    dice2Instance = dice2GO.GetComponent<DiceRoll3D>();
        
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);

    }
    public void StartDiceRollWithResult(int first, int second, int targetPlayerId)
    {

        dice1GO.SetActive(true);
        dice2GO.SetActive(true);
        // Запускаем вращение с заранее известным результатом
        dice1Instance.RollToResult(first);
        dice2Instance.RollToResult(second);

        // Обновляем UI
        RandomNumbers.Instance.SetDiceNumbers(first, second, targetPlayerId);
    }


}
