using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerRoot : MonoBehaviour
{
    public static Transform PlayerRoot;

    [SerializeField] private Transform playerRoot;

    private void Awake()
    {
        PlayerRoot = playerRoot;
    }
}
