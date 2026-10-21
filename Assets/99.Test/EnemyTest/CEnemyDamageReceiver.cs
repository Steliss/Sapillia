using UnityEngine;

public class CEnemyDamageReceiver : MonoBehaviour, IDamageInfoReceiver
{
    #region 내부변수
    private float _maxHP;
    private float _currentHP;
    // Enemy 컨트롤러나 전체적인 데이터를 담고 있는 거 참조
    private CDataEnemySO _enemyData;

    #endregion

    #region 유니티 생명주기
    private void Awake()
    {
        if(_enemyData == null)
        {
            return;
        }

        _maxHP = _enemyData.Hp;
        _currentHP = _maxHP;
        
    }

    #endregion

    #region 방어관련 데미지 계산
    // 방향성 없는 데미지
    public void TakeDamage(int damage)
    {
        TakeDamage(new SDamageInfo(damage, Vector2.zero));
    }

    public void TakeDamage(SDamageInfo damageInfo)
    {
        int finalDamage = damageInfo.Damage;

        _currentHP = Mathf.Max(0, _currentHP - Mathf.Max(0, finalDamage));

        if(_currentHP <= 0)
        {
            // 사망처리
        }
    }

    #endregion
}
