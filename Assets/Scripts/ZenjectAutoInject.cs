using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ZenjectAutoInject : MonoBehaviour
{
    void Awake()
    {
        ProjectContext.Instance.Container.InjectGameObject(gameObject);
    }
}
