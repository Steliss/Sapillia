using UnityEngine;

public class CmenuScene : MonoBehaviour
{
    private CSoundManager _sound;
    private CDataManger _data;

    private CUniTaskOnes _soundTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private EReady _ready = EReady.None;
    private EReady RequiredReady = EReady.Sound | EReady.Data;

    private bool _isInitialize;

    private void OnEnable()
    {
        _ready = EReady.None;
        _isInitialize = false;

        _soundTask.Run(async token =>
        {
            _sound = await CManagerDontDestroy.SoundAsync(token);
            token.ThrowIfCancellationRequested();

            CLog.Warn("CmenuScene _soundTask OnEnable 작업 완료");
            SetReady(EReady.Sound);
        });

        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            token.ThrowIfCancellationRequested();

            CLog.Warn("CmenuScene _dataTask OnEnable 작업 완료");
            SetReady(EReady.Data);
        });
    }

    private void OnDisable()
    {
        _soundTask.Cancel();
        _dataTask.Cancel();
    }

    private void SetReady(EReady ready)
    {
        _ready |= ready;

        if (_isInitialize)
        {
            return;
        }

        if ((_ready & RequiredReady) != RequiredReady)
        {
            return;
        }

        _isInitialize = true;

        _sound.ApplyVolume(_data.CurrentSetting);
        _sound.PlayBGM("MainTheme");
    }
}