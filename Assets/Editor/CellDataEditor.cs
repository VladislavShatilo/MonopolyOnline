using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CellData))]
public class CellDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;

        height += EditorGUIUtility.singleLineHeight * 3; // index, cellName, cellType
        height += EditorGUIUtility.standardVerticalSpacing * 3;

        SerializedProperty cellTypeProp = property.FindPropertyRelative("cellType");
        CellType cellType = (CellType)cellTypeProp.enumValueIndex;

        switch (cellType)
        {
            case CellType.Company:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("companyData"), true);
                break;
            case CellType.FieldCompany:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("fieldCompanyData"), true);
                break;
            case CellType.DiceCompany:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("diceCompanyData"), true);
                break;
            case CellType.Spend:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("spendData"), true);
                break;
            case CellType.Question:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("questionData"), true);
                break;
            case CellType.Corner:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("cornerData"), true);
                break;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

        EditorGUI.PropertyField(rect, property.FindPropertyRelative("index"));
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, property.FindPropertyRelative("cellName"));
        rect.y += lineHeight + spacing;

        SerializedProperty cellTypeProp = property.FindPropertyRelative("cellType");
        EditorGUI.PropertyField(rect, cellTypeProp);
        rect.y += lineHeight + spacing;

        CellType cellType = (CellType)cellTypeProp.enumValueIndex;

        switch (cellType)
        {
            case CellType.Company:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("companyData"), true);
                break;
            case CellType.FieldCompany:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("fieldCompanyData"), true);
                break;
            case CellType.DiceCompany:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("diceCompanyData"), true);
                break;
            case CellType.Spend:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("spendData"), true);
                break;
            case CellType.Question:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("questionData"), true);
                break;
            case CellType.Corner:
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("cornerData"), true);
                break;
        }

        EditorGUI.EndProperty();
    }
}