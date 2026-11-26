
using UnityEngine;

public class CPlayerSound : MonoBehaviour
{
    #region 인스펙터

    [Header("플레이어 참조")]
    [SerializeField] private CPlayerController _playerController;
    [SerializeField] private Transform _soundPoint;

    [Header("이동")]
    [SerializeField] private string _dash = "dashMove";
    [SerializeField] private string _footstep = "footstepDirt";

    [Header("검과 방패")]
    [SerializeField] private string _swordNormal = "attackswish01";
    [SerializeField] private string _swordNormal3 = "swordandshield03";
    [SerializeField] private string _swordSpecial = "attackSpin";
    [SerializeField] private string _swordDash = "attackBash";

    [Header("석궁")]
    [SerializeField] private string _crossbowNormal = "crossbow_Normal";
    [SerializeField] private string _crossbowSpecial = "attackBowMagical";
    [SerializeField] private string _shotgunNormal = "crossbow_Shotgun";
    [SerializeField] private string _reloadStart = "crossbowReload1";
    [SerializeField] private string _reloadComplete = "crossbowReload2";

    [Header("피격 및 사망")]
    [SerializeField] private string _hit = "PlayerHit";
    [SerializeField] private string _death = "diePlayer";

    #endregion

    #region 내부변수

    private CSoundManager _soundManager;
    private CPlayerGamingDataSO _playerData;
    private int _previousHP;

    #endregion

    #region Unity

    private void OnEnable()
    {
        BindSoundManager();

        if (_playerController != null)
        {
            BindPlayerData(_playerController.PlayerData);
        }
    }

    private void OnDisable()
    {
        if (_playerData != null)
        {
            _playerData.OnHPChanged -= HandleHPChanged;
        }

        _playerData = null;
    }

    #endregion

    #region SoundManager

    private void BindSoundManager()
    {
        if (_soundManager != null)
        {
            return;
        }

        if (CManagerDontDestroy.Instance == null)
        {
            return;
        }

        _soundManager = CManagerDontDestroy.Instance.Sound;
    }

    private void Play(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName))
        {
            return;
        }

        if (_soundManager == null)
        {
            BindSoundManager();
        }

        if (_soundManager == null)
        {
            return;
        }

        Vector3 position = _soundPoint != null ? _soundPoint.position : transform.position;

        _soundManager.PlayEffect(soundName, position);
    }

    #endregion

    #region 플레이어 데이터

    public void BindPlayerData(CPlayerGamingDataSO data)
    {
        if (ReferenceEquals(_playerData, data))
        {
            return;
        }

        if (_playerData != null)
        {
            _playerData.OnHPChanged -= HandleHPChanged;
        }

        _playerData = data;

        if (_playerData == null)
        {
            return;
        }

        _previousHP = _playerData.CurrentHP;

        _playerData.OnHPChanged += HandleHPChanged;
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        if (currentHP < _previousHP)
        {
            if (currentHP <= 0)
            {
                PlayDeath();
            }
            else
            {
                PlayHit();
            }
        }

        _previousHP = currentHP;
    }

    #endregion

    #region 이동

    public void PlayDash()
    {
        Play(_dash);
    }

    // Animation Event를 통해 호출
    public void PlayFootstep()
    {
        Play(_footstep);
    }

    #endregion

    #region 검과 방패

    // 검 일반공격
    public void PlaySwordNormal(int comboIndex = 1)
    {
        if (comboIndex == 3)
        {
            Play(_swordNormal3);
            return;
        }

        Play(_swordNormal);
    }
    // 검 특수공격
    public void PlaySwordSpecial()
    {
        Play(_swordSpecial);
    }
    // 검 대시공격
    public void PlaySwordDashAttack()
    {
        Play(_swordDash);
    }

    #endregion

    #region 석궁
    // 석궁 일반공격
    public void PlayCrossbowNormal()
    {
        Play(_crossbowNormal);
    }
    // 석궁 특수공격
    public void PlayCrossbowSpecial()
    {
        Play(_crossbowSpecial);
    }
    // 샷건 일반공격
    public void PlayShotgunNormal()
    {
        Play(_shotgunNormal);
    }
    // 장전 시작
    public void PlayReloadStart()
    {
        Play(_reloadStart);
    }
    // 장전 끝
    public void PlayReloadComplete()
    {
        Play(_reloadComplete);
    }

    #endregion

    #region 피격 및 사망
    // 피격
    public void PlayHit()
    {
        Play(_hit);
    }
    // 사망
    public void PlayDeath()
    {
        Play(_death);
    }

    #endregion

    public void DrinkPotion()
    {
        Play("drinkPotion");
    }
}
