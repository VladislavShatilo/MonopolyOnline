using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum StatsWindowPosition
{
    Up,
    LeftUp,
    LeftDown,
    Down,
    RightDown,
    RightUp

}

public class CompanyWindowPopup : MonoBehaviour
{
    [SerializeField] private Button showWindowButton;
    private int id;

    public event Action<int> OnCompanyClicked;

    public void Init(int id)
    {
        this.id = id;
        showWindowButton.onClick.AddListener(() => OnCompanyClicked?.Invoke(this.id));
    }
}
