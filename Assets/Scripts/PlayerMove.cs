using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Transform rootCellsObject;
    [SerializeField] private BoardConfig boardConfig;

    public int currentCellIndex = 0;
    public float moveDuration = 0.3f;
    public static Action<int> ShowBuyMenuAction;
    private List<Transform> boardCells;
    public int id;

    private void Start()
    {
        boardCells = new List<Transform>();
        foreach(Transform child in rootCellsObject)
        {
            boardCells.Add(child);
        }
    }

    private void OnEnable()
    {
        RandomNumbers.playerMoveAction += Move;
    }

    private void OnDisable()
    {
        RandomNumbers.playerMoveAction -= Move;
    }
    private void Move(int movesNumber)
    {
        StartCoroutine(MoveSteps(movesNumber));
        Storage.Instance.moves += movesNumber;
        Storage.Instance.Save();
      

    }

    private IEnumerator MoveSteps(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            currentCellIndex = (currentCellIndex + 1) % boardCells.Count;
            Vector3 targetPos = boardCells[currentCellIndex].position;
            yield return MoveToPosition(targetPos);
        }
        CellHandle(boardCells[currentCellIndex].gameObject, currentCellIndex);
    }
    public void CellHandle(GameObject cellGO, int currentCellID)
    {
        Debug.Log(currentCellID);
        switch (boardConfig.cells[currentCellID].cellType)
        {
            case CellType.Company:
                {
                    MessageLog.Instance.AddMessage("Вы попали в сектор " + boardConfig.cells[currentCellID].companyData.name + " и у вас забрали 1,000k");
                    ShowBuyMenuAction?.Invoke(currentCellID);
                    
                    break;
                }
            case CellType.Question:
                {
                    MessageLog.Instance.AddMessage("Вы попали в сектор говно и у вас забрали 1,000k");

                    Bank.Instance.RemoveMoney(GameManager.Instance.GetPlayerById(id),1000);
                    break;
                }
            case CellType.Spend:
                {
                    MessageLog.Instance.AddMessage("Вы попали в сектор говно-говно и у вас забрали 2,000k");
                    Bank.Instance.RemoveMoney(GameManager.Instance.GetPlayerById(id), 2000);
                    break;
                }
        }
    }
    private IEnumerator MoveToPosition(Vector3 target)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}