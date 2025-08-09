using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomNumbers : MonoBehaviour
{
    [SerializeField] private Button randomButton;
    [SerializeField] private TextMeshProUGUI firstRandomText;
    [SerializeField] private TextMeshProUGUI secondRandomText;
    public static Action<int> playerMoveAction;
    private int firstRandomNumber;
    private int secondRandomNumber;

    // Start is called before the first frame update
    void Start()
    {
        randomButton.onClick.AddListener(() =>StartCoroutine( GenerateNumbers()));
    }
    private IEnumerator GenerateNumbers()
    {
        firstRandomNumber = UnityEngine.Random.Range(1, 7);
        secondRandomNumber = UnityEngine.Random.Range(1, 7);
        firstRandomText.text = firstRandomNumber.ToString();
        secondRandomText.text = secondRandomNumber.ToString();
        yield return new WaitForSeconds(0.3f);
        playerMoveAction?.Invoke(firstRandomNumber + secondRandomNumber);
    }
   
   
}
