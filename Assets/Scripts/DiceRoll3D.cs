using System;
using System.Collections;
using UnityEngine;

public class DiceRoll3D : MonoBehaviour
{
    [Header("Roll Settings")]
    [SerializeField] private float rollTime = 2f;
    [SerializeField] private float minSpinSpeed = 300f;
    [SerializeField] private float maxSpinSpeed = 1000f;
    [SerializeField] private float alignDuration = 0.2f;
    [SerializeField] private AnimationCurve rollCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    public int Result { get; private set; }
    public bool IsRolling { get; private set; }

    public event Action<int> OnRollEnded;
    public event Action OnRollStarted;

    private Vector3 spinAxis;
    private float spinSpeed;

    private readonly Quaternion[] faceRotations = new Quaternion[6]
    {
        Quaternion.Euler(0, 0, 0),       // 1 сверху
        Quaternion.Euler(-90, 0, 0),     // 2 снизу
        Quaternion.Euler(0, 90, 0),      // 3 сзади
        Quaternion.Euler(0, -90, 0),     // 4 справа
        Quaternion.Euler(90, 0, 0),      // 5 слева
        Quaternion.Euler(180, 0, 0)      // 6 спереди
    };

    public void RollToResult(int targetValue)
    {
        if (IsRolling) return;

        OnRollStarted?.Invoke();
        StopAllCoroutines();
        StartCoroutine(RollRoutine(targetValue));
    }

    private IEnumerator RollRoutine(int targetValue)
    {
        IsRolling = true;
        float elapsed = 0f;

        spinAxis = UnityEngine.Random.onUnitSphere;
        spinSpeed = UnityEngine.Random.Range(minSpinSpeed, maxSpinSpeed);

        while (elapsed < rollTime)
        {
            elapsed += Time.deltaTime;
            float t = rollCurve.Evaluate(elapsed / rollTime);
            transform.Rotate(spinAxis, spinSpeed * t * Time.deltaTime, Space.Self);
            yield return null;
        }

        Result = targetValue;
        yield return StartCoroutine(AlignToFace(Result));

        IsRolling = false;
        OnRollEnded?.Invoke(Result);
    }

    private IEnumerator AlignToFace(int value)
    {
        if (value < 1 || value > 6) value = 1;

        Quaternion targetRotation = faceRotations[value - 1];
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < alignDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / alignDuration);
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
