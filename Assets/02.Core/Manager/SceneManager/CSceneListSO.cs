using System.Collections.Generic;
using UnityEngine;


// 씬 리스트 SO
[CreateAssetMenu(fileName = "SceneList", menuName = "SO/Scene/Scene List")]
public class CSceneListSO : ScriptableObject
{
    [SerializeField] private List<CSceneInfo> _scenes = new List<CSceneInfo>();

    public CSceneInfo GetSceneName(EScene sceneID)
    {
        for (int i = 0; i < _scenes.Count; i++)
        {
            CSceneInfo scene = _scenes[i];

            if (scene == null)
            {
                continue;
            }

            if (scene.SceneID == sceneID)
            {
                return scene;
            }
        }

        return null;
    }

}
