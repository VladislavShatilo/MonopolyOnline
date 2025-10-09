using UnityEngine;

public interface IMessageLogView 
{
    void AddMessage(string formattedText);
    event System.Action<string> OnSendClicked;
}
