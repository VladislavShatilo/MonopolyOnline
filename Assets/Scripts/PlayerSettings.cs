using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    public GameObject PlayerPrefab;
    public Vector3 StartPosition;
}