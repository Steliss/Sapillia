using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmenuScene : MonoBehaviour
{
    private CSoundManager _sound;

    private CUniTaskOnes _soundTask = new CUniTaskOnes();

    private void OnEnable()
    {
        _soundTask.Run(async token =>
        {
            _sound = await CManagerDontDestroy.SoundAsync(token);
            _sound.PlayBGM("MainTheme");
            CLog.Warn("CmenuScene _soundTask OnEnable 작업 완료");
        });
    }

}
