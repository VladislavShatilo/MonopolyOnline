using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectDataCopier : MonoBehaviour
{
    [MenuItem("Tools/Copy SO Data")]
    
    public static void CopyData()
    {
        // Указываем пути к ScriptableObject-ам
        var source = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/Scripts/BoardConfig.asset");
        var target = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/Scripts/BoardConfig1.asset");

        if (source == null || target == null)
        {
            Debug.LogError("Не удалось загрузить объекты!");
            return;
        }


        for(int i = 0; i < source.cells.Count; i++)
        {
            target.cells[i].index = source.cells[i].index;
            target.cells[i].cellName = source.cells[i].cellName;
            target.cells[i].cellType = source.cells[i].cellType;
            target.cells[i].companyData = source.cells[i].companyData;
            target.cells[i].cornerData = source.cells[i].cornerData;
            target.cells[i].questionData = source.cells[i].questionData;

        }

        Debug.Log("Данные перенесены!");
    }
}
