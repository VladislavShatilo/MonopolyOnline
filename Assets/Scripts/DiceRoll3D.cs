using System.Collections;
using UnityEngine;

public class DiceRoll3D : MonoBehaviour
{
    [SerializeField] private float rollTime = 2f;
    [SerializeField] private float minSpinSpeed = 300f;
    [SerializeField] private float maxSpinSpeed = 1000f;
    [SerializeField] private float alignDuration = 0.2f; // время выравнивания

    public int Result { get; private set; }
    public bool IsRolling { get; private set; }

    private Vector3 spinAxis;
    private float spinSpeed;

    public void RollToResult(int targetValue)
    {
        StopAllCoroutines();
        StartCoroutine(RollRoutine(targetValue));
    }
    private IEnumerator RollRoutine(int targetValue)
    {
        IsRolling = true;
        float elapsed = 0f;

        spinAxis = Random.onUnitSphere;
        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);

        while (elapsed < rollTime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / rollTime);
            transform.Rotate(spinAxis, spinSpeed * t * Time.deltaTime, Space.World);
            yield return null;
        }

        // Фиксируем заранее известное число
        Result = targetValue;
        yield return StartCoroutine(AlignToFace(Result));

        IsRolling = false;
    }


    private IEnumerator AlignToFace(int value)
    {
        Quaternion targetRotation = value switch
        {
            1 => Quaternion.Euler(0, 0, 0),      // сверху
            2 => Quaternion.Euler(-90, 0, 0),    // снизу
            3 => Quaternion.Euler(0, 90, 0),    // сзади
            4 => Quaternion.Euler(0, -90, 0),     // справа
            5 => Quaternion.Euler(90, 0, 0),    // слева
            6 => Quaternion.Euler(180, 0, 0),    // спереди
            _ => Quaternion.identity
        };

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
