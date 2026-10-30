using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_Bullet : CEnemyProjectile
{
    // 데미지 준 후 투사체 꺼지는 후처리
    protected override void HitEnd()
    {
        _attackTime = 0f;
    }

}
