using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Transform rootCellsObject;
    [SerializeField] private CellsManager cellsManager;

    public int currentCellIndex = 0;
    public float moveDuration = 0.3f;
    private List<Transform> boardCells;

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
        Debug.Log(currentCellIndex);
        cellsManager.CellHandle(boardCells[currentCellIndex].gameObject, currentCellIndex);
        Debug.Log(currentCellIndex);
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