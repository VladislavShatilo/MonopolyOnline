using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell: MonoBehaviour
{
    [SerializeField] private Transform[] anchors; // якорные точки

    private Dictionary<Transform, Transform> occupied = new();
    // token -> anchor

    public Vector3 GetAnchorPosition(Transform token)
    {
        if (occupied.TryGetValue(token, out Transform anchor))
            return anchor.position;

        // если токен новый, ищем свободный слот
        foreach (var a in anchors)
        {
            if (!occupied.ContainsValue(a))
            {
                occupied[token] = a;
                return a.position;
            }
        }

        // если все зан€то Ч fallback: в центр
        return transform.position;
    }

    public void ReleaseAnchor(Transform token)
    {
        if (occupied.ContainsKey(token))
            occupied.Remove(token);
    }
}
