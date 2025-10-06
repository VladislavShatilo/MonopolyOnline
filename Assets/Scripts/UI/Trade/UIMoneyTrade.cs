using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIMoneyTrade : MonoBehaviour
{
    [SerializeField] private Button openMoneyButton;
    [SerializeField] private Image penImage;
    [SerializeField] private TMP_InputField moneyInputField;
    [SerializeField] private TextMeshProUGUI moneyTextValue;
    [SerializeField] private TextMeshProUGUI moneyText;

    private int currentMoney = 0;
    private RectTransform rectTransform;

    #region LIFE_CYCLE

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        openMoneyButton.onClick.AddListener(OpenMoneyInput);
        RefreshUI();
    }

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

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RefreshUI()
    {
        currentMoney = 0;
        moneyTextValue.text = currentMoney.ToString("N0", CultureInfo.InvariantCulture);
        moneyInputField.text = "0";
        moneyInputField.gameObject.SetActive(false);
        moneyTextValue.gameObject.SetActive(true);
        penImage.gameObject.SetActive(true);
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void HandleClick(Vector2 screenPosition)
    {
        if (!IsPointerOverUI(screenPosition))
        {
            CloseInput();
            return;
        }

        if (!IsInsideAnyWindow(screenPosition))
        {
            CloseInput();
        }
    }

    private bool IsInsideAnyWindow(Vector2 screenPosition)
    {
        return IsPointerInsideWindow(rectTransform, screenPosition);
    }

    private bool IsPointerInsideWindow(RectTransform window, Vector2 screenPosition)
    {
        if (!window.gameObject.activeSelf) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            window,
            screenPosition,
            Camera.main   // указываем камеру канваса
        );
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

    private void OpenMoneyInput()
    {
        moneyInputField.gameObject.SetActive(true);
        moneyTextValue.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(false);
        penImage.gameObject.SetActive(false);
        moneyInputField.onEndEdit.AddListener(OnMoneyChanged);
        moneyInputField.text = currentMoney.ToString();
    }

    private void OnMoneyChanged(string value)
    {
        if (int.TryParse(value, out int money))
            currentMoney = money;
        else
            currentMoney = 0;
        CloseInput();

        moneyInputField.onEndEdit.RemoveListener(OnMoneyChanged);
    }

    private void CloseInput()
    {
        moneyTextValue.text = currentMoney.ToString("N0", CultureInfo.InvariantCulture);

        penImage.gameObject.SetActive(true);
        moneyTextValue.gameObject.SetActive(true);
        moneyInputField.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(true);
    }

    #endregion PRIVATE_METHODS
}