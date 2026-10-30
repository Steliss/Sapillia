using Cysharp.Threading.Tasks;
using UnityEngine;

public class CStartScene : MonoBehaviour
{
    private CSceneManager scene;
    private CUniTaskOnes _sceneTask = new CUniTaskOnes();


    private void OnEnable()
    {
        _sceneTask.Run(async token =>
        {
            scene = await CManagerDontDestroy.SceneManagerAsync(token);
            CLog.Log("CStartScene _sceneTask OnEnable 작업 완료");
            scene.ChangeScreenAsync(EScene.Menu).Forget();
        });
    }

    private void OnDisable()
    {
        _sceneTask.Cancel();
    }


}
