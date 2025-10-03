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

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    void Update()
    {
        // Calculate the number of child elements in the content
        int childCount = content.childCount;

        // Calculate the total number of rows required
        int columns = gridLayoutGroup.constraintCount; // Number of columns (2 in this case)
        int rows = Mathf.CeilToInt((float)childCount / columns);

        // Calculate the total height required for the content
        float totalHeight = rows * gridLayoutGroup.cellSize.y + (rows - 1) * gridLayoutGroup.spacing.y;

        // Compare the total height with the visible area height
        if (childCount <= maxVisibleElements || totalHeight <= scrollRect.viewport.rect.height)
        {
            // Disable scrolling if elements fit within the visible area
            scrollRect.vertical = false;

            // Reset scroll position to the top if the child count has changed
            if (childCount != previousChildCount)
            {
                scrollRect.verticalNormalizedPosition = 1f; // Reset to the top
            }
        }
        else
        {
            // Enable scrolling if elements exceed the visible area
            scrollRect.vertical = true;
        }

        // Update the previous child count
        previousChildCount = childCount;
    }
}