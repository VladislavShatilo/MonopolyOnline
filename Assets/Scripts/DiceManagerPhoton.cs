using System.Collections;
using Photon.Pun;
using UnityEngine;

public class DiceManagerPhoton : MonoBehaviourPun
{
    [Header("Dice Prefab and Positions")]
    [SerializeField] private GameObject dice1GO;
    [SerializeField] private GameObject dice2GO;
    [SerializeField] private DiceRoll3D dice1Instance;
    [SerializeField] private DiceRoll3D dice2Instance;
   

  
    private void Start()
    {
      
        
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);

    }
    public void StartDiceRollWithResult(int first, int second, int targetPlayerId)
    {

        dice1GO.SetActive(true);
        dice2GO.SetActive(true);

        dice1Instance.RollToResult(first);
        dice2Instance.RollToResult(second);

        // Обновляем UI
        RandomNumbers.Instance.SetDiceNumbers(first, second, targetPlayerId);
    }


}
