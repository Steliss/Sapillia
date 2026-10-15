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

    public void Nav()
    {
        //StartCoroutine(BakeNav());

        Bake();
    }

    private void Bake()
    {
        // 기존 네브메쉬 삭제
        if (_surface.navMeshData != null)
        {
            _surface.RemoveData();
            _surface.navMeshData = null;
        }

        _surface.BuildNavMesh();
    }


    
    private IEnumerator BakeNav()
    {
        // 기존 네브메쉬 삭제.
        if (_surface.navMeshData != null)
        {
            _surface.RemoveData();
            _surface.navMeshData = null;
        }

        _surface.navMeshData = new NavMeshData();
        _surface.AddData();

        Bounds bounds = new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 10f));

        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        List<NavMeshBuildMarkup> marks = new List<NavMeshBuildMarkup>();

        NavMeshBuildMarkup markup = new NavMeshBuildMarkup
        {
            root = null,
            overrideArea = true,
            area = _surface.defaultArea,
            ignoreFromBuild = false
        };

        marks.Add(markup);

        NavMeshBuilder.CollectSources
            (
            bounds, 
            _surface.layerMask, 
            _surface.useGeometry,
            _surface.defaultArea,
            marks,
            sources
            );

        Debug.Log($"지형 갯수 {sources.Count}");

        AsyncOperation asyncOp =
        NavMeshBuilder.UpdateNavMeshDataAsync
            (
            _surface.navMeshData,
            _surface.GetBuildSettings(),
            sources,
            bounds
            );

        while (!asyncOp.isDone)
        {
            yield return null;
        }
        

    }

}
