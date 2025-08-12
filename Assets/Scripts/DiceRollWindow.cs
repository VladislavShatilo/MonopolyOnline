using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollWindow : MonoBehaviour
{
    public static DiceRollWindow Instance { get; private set; }

    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;
    private int localPlayerId;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.TurnChanged += OnTurnChanged;
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.TurnChanged -= OnTurnChanged;
    }

    private void Start()
    {
        localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        HideWindow();
    }

    private void OnTurnChanged(int currentPlayerId)
    {
        Debug.Log($"DiceRollWindow: OnTurnChanged called with currentPlayerId={currentPlayerId}, localPlayerId={localPlayerId}");

        if (currentPlayerId == localPlayerId)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            ShowWindow();
        }
        else
        {
            if (hideCoroutine == null)
                hideCoroutine = StartCoroutine(DelayedHide());
        }
    }

    private IEnumerator DelayedHide()
    {
        // Чтобы избежать резких переключений, небольшая задержка перед скрытием окна
        yield return new WaitForSeconds(0.1f);
        HideWindow();
        hideCoroutine = null;
    }

    public void ShowWindow()
    {

        windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
    }
    public void HideWindow()
    {
        windowRectTransform.DOAnchorPos(new Vector2(0, 160), animationDuration);
    }

}
