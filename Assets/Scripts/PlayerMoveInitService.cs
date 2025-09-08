using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerMoveInitService : IPlayerMoveInitService,IInitializable,ILateDisposable
{
    private IBoardService boardService;
    private ILocalPlayerService localPlayerService;

    [Inject]
    public void Construct(IBoardService boardService, ILocalPlayerService localPlayerService)
    {
        this.boardService = boardService;
        this.localPlayerService = localPlayerService;
    }
    void IInitializable.Initialize()
    {
        EventBus.Subscribe<EnablePlayerMoveEvent>(_ => Init());
    }
    void ILateDisposable.LateDispose()
    {
        EventBus.Unsubscribe<EnablePlayerMoveEvent>(_ => Init());
    }
    public void Init() 
    {
        EventBus.Publish(new InitializePlayerMoveEvent(boardService, localPlayerService));
    }


}
