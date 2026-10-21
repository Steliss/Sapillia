using UnityEngine;

public class CPlayerDamageReceiver : MonoBehaviour, IDamageInfoReceiver
{
    #region 인스펙터

    [Header("참조")]
    [SerializeField] private CPlayerController _playerController;

    #endregion

    #region 내부변수
    // 현재 방패를 가진 무기인지?
    private IDamageDefense _currentDefense;

    #endregion

    #region 방어 기능 등록

    public void SetDefense(IDamageDefense defense)
    {
        _currentDefense = defense;
    }
  
    #endregion



    #region 피해 처리

    // 기존 IDamageable 호환
    public void TakeDamage(int damage)
    {
        TakeDamage(new SDamageInfo(damage, Vector2.zero));
    }

    // 새로운 피해 정보 처리
    public void TakeDamage(SDamageInfo damageInfo)
    {
        if (damageInfo.Damage <= 0)
        {
            return;
        }

        if (_playerController == null || _playerController.PlayerData == null)
        {
            return;
        }

        int finalDamage = damageInfo.Damage;

        // 현재 방어 기능이 존재하는지 확인
        if (_currentDefense != null)
        {
            finalDamage = _currentDefense.ResolveDamage(damageInfo);
        }

        finalDamage = Mathf.Max(0, finalDamage);

        // 최종 피해량만 HP에 적용
        if (finalDamage > 0)
        {
            _playerController.PlayerData.TakeDamage(finalDamage);
        }
    }

    


    #endregion
}