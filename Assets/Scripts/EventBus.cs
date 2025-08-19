using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Событийная шина для глобальной коммуникации между модулями.
/// Не зависит от UI или Photon напрямую.
/// </summary>
public static class EventBus
{
    // Внутренний словарь: ключ - тип события, значение - список обработчиков
    private static readonly Dictionary<Type, List<Delegate>> eventHandlers = new();

    /// <summary>
    /// Подписка на событие TEvent.
    /// </summary>
    public static void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type))
            eventHandlers[type] = new List<Delegate>();

        eventHandlers[type].Add(handler);
    }

    /// <summary>
    /// Отписка от события TEvent.
    /// </summary>
    public static void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (eventHandlers.ContainsKey(type))
        {
            eventHandlers[type].Remove(handler);
            if (eventHandlers[type].Count == 0)
                eventHandlers.Remove(type);
        }
    }

    /// <summary>
    /// Публикация события TEvent.
    /// </summary>
    public static void Publish<TEvent>(TEvent eventData)
    {
        var type = typeof(TEvent);
        if (!eventHandlers.ContainsKey(type)) return;

        // Создаем копию, чтобы избежать ошибок при модификации во время перебора
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
}
