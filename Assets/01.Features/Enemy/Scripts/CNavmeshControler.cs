using Cysharp.Threading.Tasks;
using NavMeshPlus.Components;
using UnityEngine;

public class CNavmeshControler : MonoBehaviour
{
    private NavMeshSurface _surface;
    private CUniTaskOnes _uni = new CUniTaskOnes();

    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
        _uni.Run(async ct =>
        {
            CDungeonGenerator dungeon = GetComponentInParent<CDungeonGenerator>();
            
            if (dungeon == null)
            {
                CLog.Warn("제너레이터 찾지 못함. 네브메시 베이크 예약 취소");
                return;
            }

            
            // 찾았으면 준비 완료될때까지 대기
            while(!dungeon.IsReady)
            {
                await UniTask.Yield(ct);
            }

            await UniTask.Yield(ct);
            // 네브메시 굽기
            Bake();
            CLog.Log("네브메시 베이크 완료!");

        });
    }

    public void Bake()
    {
        // 기존 네브메쉬 삭제
        if (_surface.navMeshData != null)
        {
            _surface.RemoveData();
            _surface.navMeshData = null;
        }

        _surface.BuildNavMesh();
    }
}
