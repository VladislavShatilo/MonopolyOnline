#if EXTENJECT_INCLUDE_ADDRESSABLE_BINDINGS
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

[ZenjectAllowDuringValidation]
[NoReflectionBaking]
public class AddressableInject<T> : AsyncInject<T> where T : UnityEngine.Object
{
    private AsyncOperationHandle<T> _handle;
    public AsyncOperationHandle AssetReferenceHandle => _handle;
    
    public AddressableInject(InjectContext context, Func<CancellationToken, Task<AsyncOperationHandle<T>>> asyncMethod) 
        : base(context)
    {
#pragma warning disable CS4014
        StartAsync(async (_, _, ct) =>
        {
            _handle = await asyncMethod(ct);
            return _handle.Result;
        }, _cancellationTokenSource.Token);
#pragma warning restore CS4014
    }
}
#endif