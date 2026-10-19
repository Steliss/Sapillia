using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public class CUniTaskOnes
{
    private CancellationTokenSource _cts;

    /// <summary>
    /// 유니테스크 시작 함수. 람다식.
    /// </summary>
    /// <param name="sequence"></param>
    public void Run(Func<CancellationToken, UniTask> sequence)
    {
        if (_cts != null)
        {
            return;
        }
        _cts = new CancellationTokenSource();

        Runner(sequence, _cts.Token).Forget();
    }

    // 실제 비동기 작업
    private async UniTask Runner
        (
        Func<CancellationToken, UniTask> sequence,
        CancellationToken ct
        )
    {

        try
        {
            // 함수 실행
            if (sequence != null)
            {
                await sequence(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // 취소 시 여기로 들어옴
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            // 오류가 발생해도 끝나면 토큰 반납
            Cancel();
        }
    }

    /// <summary>
    /// 유니테스크 취소 및 정리
    /// </summary>
    public void Cancel()
    {
        if (_cts == null)
        {
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

}