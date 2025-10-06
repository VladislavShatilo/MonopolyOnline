using System.IO;
using UnityEditor;
using UnityEngine;

public class CodeLineCounterWindow : EditorWindow
{

    private int totalLines;
    private bool calculated;

    [MenuItem("Tools/Code Line Counter")]
    public static void ShowWindow()
    {
        GetWindow<CodeLineCounterWindow>("Code Line Counter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Подсчёт строк кода", EditorStyles.boldLabel);

        if (GUILayout.Button("Посчитать строки в Assets/Scripts"))
        {
            CountLines();
        }

        if (calculated)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox($"Всего строк кода в папке Scripts: {totalLines}", MessageType.Info);
        }
    }

    private void CountLines()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        if (!Directory.Exists(scriptsPath))
        {
            EditorUtility.DisplayDialog("Ошибка", "Папка Assets/Scripts не найдена!", "OK");
            return;
        }

        string[] files = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        int lineCount = 0;

        foreach (var file in files)
        {
            lineCount += File.ReadAllLines(file).Length;
        }

        totalLines = lineCount;
        calculated = true;
        Repaint();
    }
}
