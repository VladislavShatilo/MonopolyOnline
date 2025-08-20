using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
public interface ICompanyStatsUI<TData>
{
    void SetData(TData data);
}
public class CompanyUIManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private RectTransform companyInfoWindow;
    [SerializeField] private RectTransform fieldCompanyInfoWindow;
    [SerializeField] private RectTransform diceCompanyInfoWindow;

    [Header("Stats Panels")]
    [SerializeField] private UICompanyStats statsCompanyPanel;
    [SerializeField] private UIFieldCompanyStats statsFieldCompanyPanel;
    [SerializeField] private UIDiceStats statsDiceCompanyPanel;

    [Header("Settings")]
    [SerializeField] private float cellWidth = 70;
    [SerializeField] private float offset = 80;

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
            HandleClick(Input.mousePosition);
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            HandleClick(Input.GetTouch(0).position);
#endif
    }
    private void OnEnable()
    {
        EventBus.Subscribe<ShowCompanyWindowEvent>(OnShowCompanyWindow);
        EventBus.Subscribe<ShowFieldCompanyWindowEvent>(OnShowFieldCompanyWindow);
        EventBus.Subscribe<ShowDiceCompanyWindowEvent>(OnShowDiceCompanyWindow);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ShowCompanyWindowEvent>(OnShowCompanyWindow);
        EventBus.Unsubscribe<ShowFieldCompanyWindowEvent>(OnShowFieldCompanyWindow);
        EventBus.Unsubscribe<ShowDiceCompanyWindowEvent>(OnShowDiceCompanyWindow);
    }
    private void HandleClick(Vector2 screenPosition)
    {
        if (!IsPointerOverUI(screenPosition))
        {
            HideAllWindows();
            return;
        }

        if (!IsInsideAnyWindow(screenPosition))
        {
            HideAllWindows();
        }
    }

    private bool IsInsideAnyWindow(Vector2 screenPosition)
    {
        return IsPointerInsideWindow(companyInfoWindow, screenPosition)
            || IsPointerInsideWindow(fieldCompanyInfoWindow, screenPosition)
            || IsPointerInsideWindow(diceCompanyInfoWindow, screenPosition);
    }

    private bool IsPointerInsideWindow(RectTransform window, Vector2 screenPosition)
    {
        if (!window.gameObject.activeSelf) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(window, screenPosition);
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#else
        return false;
#endif
    }

    private void ConfigureWindowPosition(RectTransform window, RectTransform companyCell, StatsWindowPosition position)
    {
        Vector2 newPos = companyCell.anchoredPosition;
        Vector2 pivot = Vector2.zero;

        switch (position)
        {
            case StatsWindowPosition.Up: newPos.y -= offset; pivot = new Vector2(0.5f, 1f); break;
            case StatsWindowPosition.Down: newPos.y += offset; pivot = new Vector2(0.5f, 0f); break;
            case StatsWindowPosition.LeftUp: newPos.x += offset; newPos.y += cellWidth / 2; pivot = new Vector2(0f, 1f); break;
            case StatsWindowPosition.LeftDown: newPos.x += offset; newPos.y -= cellWidth / 2; pivot = new Vector2(0f, 0f); break;
            case StatsWindowPosition.RightUp: newPos.x -= offset; newPos.y += cellWidth / 2; pivot = new Vector2(1f, 1f); break;
            case StatsWindowPosition.RightDown: newPos.x -= offset; newPos.y -= cellWidth / 2; pivot = new Vector2(1f, 0f); break;
        }

        window.pivot = pivot;
        window.anchoredPosition = newPos;
        window.gameObject.SetActive(true);
    }

    public void HideAllWindows()
    {
        companyInfoWindow.gameObject.SetActive(false);
        fieldCompanyInfoWindow.gameObject.SetActive(false);
        diceCompanyInfoWindow.gameObject.SetActive(false);
    }

    private void ShowWindow<TPanel, TData>(RectTransform window, TPanel panel, RectTransform companyCell, StatsWindowPosition position, TData data)
        where TPanel : MonoBehaviour, ICompanyStatsUI<TData>
    {
        // Сначала закрываем все окна
        HideAllWindows();

        // Настраиваем позицию и данные
        ConfigureWindowPosition(window, companyCell, position);
        panel.SetData(data);

        // Активируем текущее окно
        window.gameObject.SetActive(true);
    }
    private void OnShowCompanyWindow(ShowCompanyWindowEvent e)
    {
        ShowWindow(companyInfoWindow, statsCompanyPanel, e.Cell, e.Position, e.Data);
    }

    private void OnShowFieldCompanyWindow(ShowFieldCompanyWindowEvent e)
    {
        ShowWindow(fieldCompanyInfoWindow, statsFieldCompanyPanel, e.Cell, e.Position, e.Data);
    }

    private void OnShowDiceCompanyWindow(ShowDiceCompanyWindowEvent e)
    {
        ShowWindow(diceCompanyInfoWindow, statsDiceCompanyPanel, e.Cell, e.Position, e.Data);
    }




}
public class ShowCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public CompanyData Data { get; }

    public ShowCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, CompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}

public class ShowFieldCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public FieldCompanyData Data { get; }

    public ShowFieldCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, FieldCompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}

public class ShowDiceCompanyWindowEvent
{
    public RectTransform Cell { get; }
    public StatsWindowPosition Position { get; }
    public DiceCompanyData Data { get; }

    public ShowDiceCompanyWindowEvent(RectTransform cell, StatsWindowPosition pos, DiceCompanyData data)
    {
        Cell = cell;
        Position = pos;
        Data = data;
    }
}