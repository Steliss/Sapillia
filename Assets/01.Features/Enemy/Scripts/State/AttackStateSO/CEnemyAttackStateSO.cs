using System;
using UnityEngine;

/*
- 근접 공격
 ㄴ 일반 공격
 ㄴ 추적 공격
 ㄴ 사각형 공격 (투사체 생성, 유도? 지정위치?)
 ㄴ 점프 사각형 공격 (투사체 생성)
 ㄴ 한방향 돌진 공격

- 원거리 공격
 ㄴ 한 방향 이동
 ㄴ 유도
 ㄴ 지정위치 이동
 ㄴ 이동하며 투사체 뿌리기 (유도? 한방향?)

- 전조
 ㄴ 근접공격 4라인 (벌어짐)
 ㄴ 범위공격 8라인 (벌어짐)
 ㄴ 원거리 공격 2라인 (좁혀짐)
*/

public abstract class CEnemyAttackStateSO : ScriptableObject, IEnemyState
{
    #region 구조체
    [Serializable]
    public struct SEnemyBulletInfo
    {
        [Header("총알 프리팹")]
        [SerializeField] public GameObject Projectile;
        [Header("총알 속도")]
        [SerializeField] public float BulletSpeed;
        [Header("추적 속도 (초당 회전각도)")]
        [SerializeField] public float TrackingSpeed;
        [Header("각도 보정")]
        [SerializeField] public float AnglePlus;
        [Header("총알 수명")]
        [SerializeField] public float BulletLifeTime;
    }

    [Serializable]
    public struct SEnemyAttackInfo
    {
        [Header("이펙트 프리팹")]
        public GameObject Effect;
        [Header("선딜, 후딜")]
        public float BeforeDelay;
        public float AfterDelay;
        [Header("공격 범위")]
        public Vector2 Range;
        [Header("투사체 정보")]
        public SEnemyBulletInfo[] BulletInfos;

        [Header("===== 애니메이터 =====")]
        public string AniAttReady;
        public string AniAttack;

        [Header("해시용 변수. 입력x")]
        [HideInInspector] public int HashAttReady;
        [HideInInspector] public int HashAttack;
    }

    [Serializable]
    public struct SWarningLineInfo
    {
        [Header("===== 전조 =====")]
        [Header("프리팹")]
        [SerializeField] public GameObject Prefeb;
        [Header("속성")]
        [SerializeField] public float LineLength;
        [SerializeField] public int LineCount;
        [SerializeField] public bool IsClose;
        [SerializeField] public float Interbal;
        [SerializeField] public float StartInterbal;
    }


    #endregion

    #region 상태머신
    protected CStateMashine _mashine;
    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
    #endregion

    #region 함수
    protected GameObject CreateAttackObject(GameObject prefeb, float lifeTime, Vector2 pos, Quaternion angle)
    {
        GameObject go = null;

        if (prefeb != null && CObjectPoolManager.Instance != null)
        {
            go = CObjectPoolManager.Instance.GetPoolObject(prefeb, lifeTime);
            go.transform.rotation = angle;
            go.transform.position = pos;
            _mashine.GameObjectsListAdd(go);
        }

        return go;
    }

    protected float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    protected Vector2 AngleToDir(float angle)
    {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }
    #endregion
}
