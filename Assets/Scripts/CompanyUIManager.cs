using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;

public class CompanyUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform companyInfoWindow;
    [SerializeField] private RectTransform fieldCompanyInfoWindow;
    [SerializeField] private RectTransform diceCompanyInfoWindow;

    [SerializeField] private UICompanyStats statsCompanyPanel;
    [SerializeField] private UIFieldCompanyStats statsFieldCompanyPanel;
    [SerializeField] private UIDiceStats statsDiceCompanyPanel;

    [SerializeField] private float cellWidth = 70;
    [SerializeField] private float offset = 80;

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleClick(Input.GetTouch(0).position);
        }
#endif
    }

    private void HandleClick(Vector2 screenPosition)
    {
        // Если курсор не над UI — сразу закрываем
        if (!IsPointerOverUI(screenPosition))
        {
            HideAllWindows();
            return;
        }

        // Если клик вне всех окон — закрываем
        if (!IsPointerInsideWindow(companyInfoWindow, screenPosition) &&
            !IsPointerInsideWindow(fieldCompanyInfoWindow, screenPosition) &&
            !IsPointerInsideWindow(diceCompanyInfoWindow, screenPosition))
        {
            HideAllWindows();
        }
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
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
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
            case StatsWindowPosition.Up:
                newPos.y -= offset;
                pivot = new Vector2(0.5f, 1f);
                break;
            case StatsWindowPosition.Down:
                newPos.y += offset;
                pivot = new Vector2(0.5f, 0f);
                break;
            case StatsWindowPosition.LeftUp:
                newPos.x += offset;
                newPos.y += cellWidth / 2;
                pivot = new Vector2(0f, 1f);
                break;
            case StatsWindowPosition.LeftDown:
                newPos.x += offset;
                newPos.y -= cellWidth / 2;
                pivot = new Vector2(0f, 0f);
                break;
            case StatsWindowPosition.RightUp:
                newPos.x -= offset;
                newPos.y += cellWidth / 2;
                pivot = new Vector2(1f, 1f);
                break;
            case StatsWindowPosition.RightDown:
                newPos.x -= offset;
                newPos.y -= cellWidth / 2;
                pivot = new Vector2(1f, 0f);
                break;
        }

        window.pivot = pivot;
        window.anchoredPosition = newPos;
        window.gameObject.SetActive(true);
    }

    public void ShowCompanyWindow(RectTransform companyCell, StatsWindowPosition position, CompanyData data)
    {
        ConfigureWindowPosition(companyInfoWindow, companyCell, position);

        statsCompanyPanel.SetCompanyName(data.name);
        statsCompanyPanel.SetGroupName(data.group.ToString());
        statsCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsCompanyPanel.SetRentPrices(data.rent);
        statsCompanyPanel.SetCellPrice(data.price.ToString("N0", CultureInfo.InvariantCulture));
        statsCompanyPanel.SetPledgePrice(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture));
        statsCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture));
        statsCompanyPanel.SetBranchPrice(data.branchPrice.ToString("N0", CultureInfo.InvariantCulture));
    }

    public void ShowFieldCompanyWindow(RectTransform companyCell, StatsWindowPosition position, FieldCompanyData data)
    {    
        ConfigureWindowPosition(fieldCompanyInfoWindow, companyCell, position);

        statsFieldCompanyPanel.SetCompanyName(data.name);
        statsFieldCompanyPanel.SetGroupName(data.group.ToString());
        statsFieldCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsFieldCompanyPanel.SetFieldPrices(data.rentField);
        statsFieldCompanyPanel.SetCellPrice(data.price.ToString("N0", CultureInfo.InvariantCulture));
        statsFieldCompanyPanel.SetPledgePrice(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture));
        statsFieldCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture));
    }

    public void ShowDiceCompanyWindow(RectTransform companyCell, StatsWindowPosition position, DiceCompanyData data)
    {
        ConfigureWindowPosition(diceCompanyInfoWindow, companyCell, position);

        statsDiceCompanyPanel.SetCompanyName(data.name);
        statsDiceCompanyPanel.SetGroupName(data.group.ToString());
        statsDiceCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsDiceCompanyPanel.SetDiceFieldMultiTexts(data.rentMultiplier);
        statsDiceCompanyPanel.SetCellPrice(data.price.ToString("N0", CultureInfo.InvariantCulture));
        statsDiceCompanyPanel.SetPledgePrice(data.pledgePrice.ToString("N0", CultureInfo.InvariantCulture));
        statsDiceCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString("N0", CultureInfo.InvariantCulture));
    }

    public void HideAllWindows()
    {
        companyInfoWindow.gameObject.SetActive(false);
        fieldCompanyInfoWindow.gameObject.SetActive(false);
        diceCompanyInfoWindow.gameObject.SetActive(false);
    }
}
