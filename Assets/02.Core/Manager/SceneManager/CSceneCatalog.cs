using System.Collections.Generic;
using UnityEngine;


public enum EScene
{
    Menu,
    Town,
    Field,
    BossRoom
}


[System.Serializable]
public class CSceneInfo
{
    [SerializeField] private EScene _sceneID;
    [SerializeField] private string _sceneName;

    [Header("페이드 설정")]
    [SerializeField] private bool _useFade = true;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private Color _fadeColor = Color.black;

    [SerializeField] private GameObject _backGroundPrefab = null;

    public EScene SceneID => _sceneID;
    public string SceneName => _sceneName;

    public bool UseFade => _useFade;
    public float FadeOutDuration => _fadeOutDuration;
    public float FadeInDuration => _fadeInDuration;
    public Color FadeColor => _fadeColor;

    public GameObject BackGroundPrefab => _backGroundPrefab;
}


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
