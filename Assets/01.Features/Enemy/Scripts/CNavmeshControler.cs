using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CNavmeshControler : MonoBehaviour
{
    private NavMeshSurface _surface;

    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
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
