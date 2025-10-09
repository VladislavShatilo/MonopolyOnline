using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class EventBusTests
{
    private EventBus eventBus;

    [SetUp]
    public void Setup()
    {
        eventBus = new EventBus();
    }

    private class TestEvent
    {
        public int Value;
        public TestEvent(int Value)
        {
            this.Value = Value;
        }
    }

    [Test]
    public void Subscribe_ShouldInvokeHandler_WhenEventIsPublished()
    {
        int received = 0;
        eventBus.Subscribe<TestEvent>(e => received = e.Value);

        eventBus.Publish(new TestEvent (42));

        Assert.AreEqual(42, received);
    }

    [Test]
    public void Unsubscribe_ShouldNotInvokeHandler_AfterUnsubscribed()
    {
        int received = 0;
        Action<TestEvent> handler = e => received = e.Value;
        eventBus.Subscribe(handler);
        eventBus.Unsubscribe(handler);

        eventBus.Publish(new TestEvent (42));

        Assert.AreEqual(0, received);
    }

    [Test]
    public void Publish_ShouldNotThrow_WhenNoSubscribers()
    {
        Assert.DoesNotThrow(() => eventBus.Publish(new TestEvent (10)));
    }

    [Test]
    public void Publish_ShouldInvokeMultipleHandlers()
    {
        var results = new List<int>();
        eventBus.Subscribe<TestEvent>(e => results.Add(e.Value));
        eventBus.Subscribe<TestEvent>(e => results.Add(e.Value * 2));

        eventBus.Publish(new TestEvent (5));

        Assert.AreEqual(2, results.Count);
        Assert.Contains(5, results);
        Assert.Contains(10, results);
    }

    [Test]
    public void Publish_ShouldHandleExceptionInHandler()
    {
        bool handlerCalled = false;

        // Подписчики: первый кидает исключение
        eventBus.Subscribe<TestEvent>(e => throw new Exception("Test exception"));
        // Второй должен отработать
        eventBus.Subscribe<TestEvent>(e => handlerCalled = true);

        // Ожидаем ошибку в логе Unity, чтобы тест не падал
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("EventBus: ошибка в обработчике события TestEvent"));

        // Должен обработать исключение в первом обработчике и вызвать второй
        eventBus.Publish(new TestEvent (1));

        Assert.IsTrue(handlerCalled);
    }
}
