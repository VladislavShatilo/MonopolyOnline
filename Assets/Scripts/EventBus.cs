using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> eventHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type))
            eventHandlers[type] = new List<Delegate>();

        eventHandlers[type].Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (eventHandlers.ContainsKey(type))
        {
            eventHandlers[type].Remove(handler);
            if (eventHandlers[type].Count == 0)
                eventHandlers.Remove(type);
        }
    }

    public void Publish<TEvent>(TEvent eventData)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type)) return;

        var handlersCopy = new List<Delegate>(eventHandlers[type]);
        foreach (var handler in handlersCopy)
        {
            try
            {
                ((Action<TEvent>)handler)?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventBus: ошибка в обработчике события {type.Name}: {ex}");
            }
        }
    }

    public void ClearAll()
    {
        eventHandlers.Clear();
    }
}
public class EndAuctionWithWinnerEvent
{
    public int WinnerId;
    public int CompanyId;
    public int FinalPrice;
    public EndAuctionWithWinnerEvent(int winnerId, int finalPrice, int companyId)
    {
        WinnerId = winnerId;
        FinalPrice = finalPrice;
        CompanyId = companyId;
    }
}