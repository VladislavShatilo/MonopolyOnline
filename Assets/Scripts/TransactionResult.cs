using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransactionResult : MonoBehaviour
{
    public bool Success { get; }
    public string Message { get; }

    public TransactionResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
}
