using Cysharp.Threading.Tasks;
using System;
using System.Threading;
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
    public struct SEnemyFireBulletInfo
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
    public struct SEnemySquareAttackInfo
    {
        [Header("이펙트 프리팹")]
        public GameObject Effect;
        [Header("선딜, 후딜")]
        public float BeforeDelay;
        public float AfterDelay;
        [Header("공격 범위")]
        public Vector2 Range;
        [Header("투사체 정보")]
        public SEnemyFireBulletInfo[] BulletInfos;

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
    /*
    private async UniTask SquareAttackBefore(CancellationToken ct, Vector2 pos, SEnemySquareAttackInfo info)
    {
        Vector2 range = info.Range * 0.5f;

        // 라인 생성
        GameObject[] xLines = new GameObject[4];
        GameObject[] yLines = new GameObject[4];
        float[] sides = { -1f, 1f };

        for (int i = 0; i < xLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.x += range.x * sides[i % 2];
            xLines[i] = CreateAttackObject
                (
                    _warningLine,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 0)
                );
            Vector2 scale = xLines[i].transform.localScale;
            scale.y = _xLineLength;
            xLines[i].transform.localScale = scale;
        }

        for (int i = 0; i < yLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.y += range.y * sides[i % 2];
            yLines[i] = CreateAttackObject
                (
                    _warningLine,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 90)
                );
            Vector2 scale = yLines[i].transform.localScale;
            scale.y = _yLineLength;
            yLines[i].transform.localScale = scale;
        }

        // 라인 이동
        float moveTime = info.BeforeDelay;
        for (float time = 0f; time < moveTime; time += Time.deltaTime)
        {
            float xGap = Mathf.Lerp(_yLineLength * 0.5f, range.x, time / moveTime);
            float yGap = Mathf.Lerp(_xLineLength * 0.5f, range.y, time / moveTime);

            for (int i = 0; i < 2; i++)
            {
                xLines[i].transform.position = pos + (Vector2.right * xGap * sides[i]);
                yLines[i].transform.position = pos + (Vector2.up * yGap * sides[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _uni.Run(async linect =>
        {
            // 페이드 아웃
            SpriteRenderer[] sps = new SpriteRenderer[8];
            for (int i = 0; i < 4; i++)
            {
                sps[i] = xLines[i].GetComponent<SpriteRenderer>();
                sps[i + 4] = yLines[i].GetComponent<SpriteRenderer>();
            }

            for (float time = 0f; time < info.AfterDelay; time += Time.deltaTime)
            {
                float t = 1 - (time / info.AfterDelay);
                Color alpha = sps[0].color;
                alpha.a = t;

                for (int i = 0; i < sps.Length; i++)
                {
                    sps[i].color = alpha;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, linect);
            }
        });
    }
    */

    #endregion
}
