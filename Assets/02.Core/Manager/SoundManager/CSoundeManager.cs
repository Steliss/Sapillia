using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;


public enum EBGM
{
    MainTheme,
    TownTheme,
    PlaceDungeonMole
}




public class CSoundManager : MonoBehaviour
{
    [SerializeField] private CSoundCatalog _catalog;


    // 현재 재생 중인 BGM 구분
    private EventInstance _playBgm;
    private EventReference _currentBGM;

    /// <summary>
    ///  이펙트 사운드, 필요시 Vector3값 없으면 zero
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    public void PlayEffect(string name, Vector3 position = default)
    {
        if (!TryFindSound(_catalog.Effect, name, out EventReference sound))
        {
            CLog.Error("Effect Sound가 재생목록에 없음");
            return;
        }

        RuntimeManager.PlayOneShot(sound, position);
    }


    /// <summary>
    ///  환경 사운드, 필요시 Vector3값 없으면 zero
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    public void PlayEnviroment(string name, Vector3 position = default)
    {
        if (!TryFindSound(_catalog.Enviroment, name, out EventReference sound))
        {
            CLog.Error("Enviroment Sound가 재생목록에 없음");
            return;
        }

        RuntimeManager.PlayOneShot(sound, position);
    }


    /// <summary>
    ///  BGM 사운드
    /// </summary>
    /// <param name="name"></param>
    public void PlayBGM(EBGM bgm)
    {
        if (!TryFindSound(_catalog.BGM, bgm.ToString(), out EventReference sound))
        {
            CLog.Error("BGM Sound가 재생목록에 없음");
            return;
        }

        // 같은 BGM이 재생 중이면 유지
        if (_playBgm.isValid() && _currentBGM.Guid.Equals(sound.Guid))
        {
            _playBgm.getPlaybackState(out PLAYBACK_STATE state);

            if (state != PLAYBACK_STATE.STOPPED)
            {
                return;
            }
        }

        StopBGM();

        _currentBGM = sound;
        _playBgm = RuntimeManager.CreateInstance(sound);
        _playBgm.start();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    /// <param name="allowFadeout"></param>
    public void StopBGM()
    {
        if (!_playBgm.isValid())
        {
            return;
        }

        _playBgm.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _playBgm.release();
        _playBgm.clearHandle();

        _currentBGM = default;
    }


    // 목록에서 사운드 검색
    private bool TryFindSound(List<CSound> list, string name, out EventReference sound)
    {
        sound = default;

        foreach (CSound entry in list)
        {
            if (entry == null || entry.Name != name)
            {
                continue;
            }

            if (entry.Sound.IsNull)
            {
                Debug.LogWarning($"연결되지 않은 사운드: {name}", this);
                return false;
            }

            sound = entry.Sound;
            return true;
        }

        Debug.LogWarning($"사운드 목록에서 이름을 찾을 수 없슴: {name}", this);
        return false;
    }



    /// <summary>
    /// 현재 설정의 볼륨을 FMOD에 적용
    /// </summary>
    /// <param name="setting"></param>
    public void ApplyVolume(CSettings setting)
    {
        SetBusVolume("bus:/", setting.MasterSound);
        SetBusVolume("bus:/Master Bus/BGMGlobal", setting.BGMSound);
        SetBusVolume("bus:/Master Bus/SFX", setting.EffectSound);
        SetBusVolume("bus:/Master Bus/AMB", setting.EnvironmentSound);
    }

    private void SetBusVolume(string path, float volume)
    {
        FMOD.RESULT result = RuntimeManager.StudioSystem.getBus(path, out FMOD.Studio.Bus bus);

        if (result != FMOD.RESULT.OK)
        {
            CLog.Error($"Bus 조회 실패: {path}, {result}");
            return;
        }

        result = bus.setVolume(Mathf.Clamp01(volume));

        if (result != FMOD.RESULT.OK)
        {
            CLog.Error($"Bus 볼륨 적용 실패: {path}, {result}");
        }
    }



    // 필요시 onDestroy로 재생중인 BGM 정리 
}