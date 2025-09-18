using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerMoveInitService : IPlayerMoveInitService,IInitializable,ILateDisposable
{
    private IBoardService boardService;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;
    [Inject]
    public void Construct(IBoardService boardService, ILocalPlayerService localPlayerService, IEventBus eventBus)
    {
        this.boardService = boardService;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<EnablePlayerMoveEvent>(_ => Init());
    }
    void ILateDisposable.LateDispose()
    {
        eventBus.Unsubscribe<EnablePlayerMoveEvent>(_ => Init());
    }
    public void Init() 
    {
        eventBus.Publish(new InitializePlayerMoveEvent(boardService, localPlayerService, eventBus));
    }


}
