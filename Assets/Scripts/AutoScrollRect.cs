using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class AutoScrollRect : MonoBehaviour
{
    [SerializeField] private int maxVisibleElements = 8;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int previousChildCount = 0;

    #region LIFE_CYCLE

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            throw new ArgumentNullException(nameof(scrollRect));
        }

        content = scrollRect.content;
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }
    }

    private void Update()
    {
        int childCount = content.childCount;
        if (gridLayoutGroup == null) return;
        int columns = gridLayoutGroup.constraintCount;
        int rows = Mathf.CeilToInt((float)childCount / columns);

        float totalHeight = rows * gridLayoutGroup.cellSize.y + (rows - 1) * gridLayoutGroup.spacing.y;

        if (childCount <= maxVisibleElements || totalHeight <= scrollRect.viewport.rect.height)
        {
            scrollRect.vertical = false;

            if (childCount != previousChildCount)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
        else
        {
            scrollRect.vertical = true;
        }

        previousChildCount = childCount;
    }

    #endregion LIFE_CYCLE
}