using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/*

- 전조
 ㄴ 근접공격 4라인 (벌어짐)
 ㄴ 범위공격 8라인 (벌어짐)
 ㄴ 원거리 공격 2라인 (좁혀짐)
*/

public abstract class CEnemyAttackStateSO : ScriptableObject, IEnemyState
{
    #region 구조체
    



    [Serializable]
    public struct SLineInfo
    {
        [Header("===== 전조 =====")]
        [Header("프리팹")]
        public GameObject Prefeb;
        [Header("선 / 후딜")]
        public float BeforeDelay;
        public float AfterDelay;
        [Header("일반 라인 속성")]
        public float LineLength;
        public float Interbal;
        public float Distanse;
        public bool IsTracking;
        public float TrackingSpeed;
        [Header("사각 라인 속성")]
        public float xLineLength;
        public float yLineLength;
        public Vector2 SquareRange;
    }
    #endregion

    #region 상태머신
    protected CStateMashine _mashine;
    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public virtual void StateEnter() { }
    public virtual void StateExit() { }
    public virtual void StateUpdate() { }
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
    
    protected async UniTask WarningLine_Square(CancellationToken ct, Vector2 pos, SLineInfo info)
    {
        Vector2 range05 = info.SquareRange * 0.5f;

        // 좌우 반전
        bool isLeft = _mashine.Target.position.x < _mashine.transform.position.x;
        _mashine.SpriteFlipX(isLeft);

        // 라인 생성
        GameObject[] xLines = new GameObject[4];
        GameObject[] yLines = new GameObject[4];
        float[] sides = { -1f, 1f };

        for (int i = 0; i < xLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.x += range05.x * sides[i % 2];
            xLines[i] = CreateAttackObject
                (
                    info.Prefeb,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 0)
                );
            Vector2 scale = xLines[i].transform.localScale;
            scale.y = info.xLineLength;
            xLines[i].transform.localScale = scale;
        }

        for (int i = 0; i < yLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.y += range05.y * sides[i % 2];
            yLines[i] = CreateAttackObject
                (
                    info.Prefeb,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 90)
                );
            Vector2 scale = yLines[i].transform.localScale;
            scale.y = info.yLineLength;
            yLines[i].transform.localScale = scale;
        }

        // 라인 이동
        float moveTime = info.BeforeDelay;
        for (float time = 0f; time < moveTime; time += Time.deltaTime)
        {
            float xGap = Mathf.Lerp(info.yLineLength * 0.5f, range05.x, time / moveTime);
            float yGap = Mathf.Lerp(info.xLineLength * 0.5f, range05.y, time / moveTime);

            for (int i = 0; i < 2; i++)
            {
                xLines[i].transform.position = pos + (Vector2.right * xGap * sides[i]);
                yLines[i].transform.position = pos + (Vector2.up * yGap * sides[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        FadeOutLine(ct, xLines).Forget();
        FadeOutLine(ct, yLines).Forget();
    }
    
    protected async UniTask<Vector2> WarningLine_FirePos(CancellationToken ct, SLineInfo info)
    {
        Vector2 myPos = _mashine.transform.position;
        Vector2 targetPos = _mashine.Target.position;

        // 라인 생성
        GameObject[] lines = new GameObject[2];
        float[] sides = { -1f, 1f };
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
            (info.Prefeb, info.BeforeDelay + 1.5f, myPos, Quaternion.identity);

            Vector2 scale = lines[i].transform.localScale;
            scale.y = info.LineLength;
            lines[i].transform.localScale = scale;
        }

        // 플레이어 위치부터 조준 시작
        Vector2 aimPos = targetPos;

        // 조준 트래킹
        for (float time = 0f; time < info.BeforeDelay; time += Time.deltaTime)
        {
            myPos = _mashine.transform.position;
            targetPos = _mashine.Target.position;
            Vector2 dir = targetPos - myPos;


            // 에임 따라가기
            if (info.IsTracking)
            {
                aimPos = Vector2.MoveTowards(aimPos, targetPos, Time.deltaTime * info.TrackingSpeed);
            }

            Vector2 aimDir = (aimPos - myPos).normalized;
            Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            Vector2 linePos = myPos + aimDir * info.LineLength * 0.5f;
            Quaternion qua = Quaternion.Euler(0, 0, DirToAngle(aimDir));
            float gap = Mathf.Lerp(info.Interbal, 0f, time / info.BeforeDelay);

            // 좌우 반전
            bool isLeft = aimDir.x < 0f;
            _mashine.SpriteFlipX(isLeft);

            // 선 따라가기
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.SetPositionAndRotation(linePos + (aimDir90 * gap * sides[i]), qua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        FadeOutLine(ct, lines).Forget();
        return aimPos;
    }

    protected async UniTask<float> WarningLine_FireAngle(CancellationToken ct, SLineInfo info)
    {
        Vector2 myPos = _mashine.transform.position;
        Vector2 targetPos = _mashine.Target.position;

        // 라인 생성
        GameObject[] lines = new GameObject[2];
        float[] sides = { -1f, 1f };
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
            (info.Prefeb, info.BeforeDelay + 1.5f, myPos, Quaternion.identity);

            Vector2 scale = lines[i].transform.localScale;
            scale.y = info.LineLength;
            lines[i].transform.localScale = scale;
        }

        // 플레이어 위치부터 조준 시작
        float targetAngle = DirToAngle((targetPos - myPos).normalized);
        float aimAngle = targetAngle;

        // 조준 트래킹
        for (float time = 0f; time < info.BeforeDelay; time += Time.deltaTime)
        {
            myPos = _mashine.transform.position;
            targetPos = _mashine.Target.position;
            Vector2 dir = targetPos - myPos;


            // 타겟 각도 갱신
            if (info.IsTracking)
            {
                targetAngle = DirToAngle(dir.normalized);
            }

            // 에임 따라가기
            aimAngle = Mathf.MoveTowardsAngle(aimAngle, targetAngle, Time.deltaTime * info.TrackingSpeed);

            Vector2 aimDir = AngleToDir(aimAngle);
            Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            Vector2 linePos = myPos + aimDir * info.LineLength * 0.5f;
            Quaternion qua = Quaternion.Euler(0, 0, aimAngle);
            float gap = Mathf.Lerp(info.Interbal, 0f, time / info.BeforeDelay);

            // 좌우 반전
            bool isLeft = aimDir.x < 0f;
            _mashine.SpriteFlipX(isLeft);

            // 선 따라가기
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.SetPositionAndRotation(linePos + (aimDir90 * gap * sides[i]), qua);
            }
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        FadeOutLine(ct, lines).Forget();
        return aimAngle;
    }

    protected async UniTask FadeOutLine(CancellationToken ct, GameObject[] lines, float fadeTime = 1f)
    {
        // 페이드 아웃
        SpriteRenderer[] sps = new SpriteRenderer[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            sps[i] = lines[i].GetComponent<SpriteRenderer>();
        }

        for (float time = 0f; time < fadeTime; time += Time.deltaTime)
        {
            float t = 1 - (time / fadeTime);
            Color alpha = sps[0].color;
            alpha.a = t;

            for (int i = 0; i < sps.Length; i++)
            {
                sps[i].color = alpha;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    protected async UniTask<float> WarningLine_Melee(CancellationToken ct, float aimAngle, SLineInfo info)
    {
        _mashine.Nav.velocity = Vector3.zero;

        Transform my = _mashine.transform;
        Transform target = _mashine.Target;

        Vector2 aimDir = AngleToDir(aimAngle);
        Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);



        // 라인 생성
        GameObject[] lines = new GameObject[4];
        float[] sides = { -1f, 1f };

        Vector3 scale = info.Prefeb.transform.localScale;
        scale.y = info.LineLength;

        Vector2 linePos = _mashine.transform.position;
        linePos += aimDir * info.Distanse;


        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
                (
                info.Prefeb,
                info.BeforeDelay + 1.5f,
                Vector2.zero,
                Quaternion.identity
                );
            lines[i].transform.localScale = scale;
        }

        // 라인으로 추적
        for (float time = 0f; time < info.BeforeDelay; time += Time.deltaTime)
        {
            Vector2 targetDir = target.position - my.position;
            float targetAngle = DirToAngle(targetDir.normalized);


            // TrackingSpeed의 속도로 플레이어를 추적
            if (info.IsTracking)
            {
                aimAngle = Mathf.MoveTowardsAngle
                    (
                        aimAngle,
                        targetAngle,
                        Time.deltaTime * info.TrackingSpeed
                    );
            }

            aimDir = AngleToDir(aimAngle).normalized;
            aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            linePos = (Vector2)my.position + aimDir * info.Distanse;
            Quaternion aimQua = Quaternion.Euler(0f, 0f, aimAngle);

            // 좌우 반전
            bool isLeft = aimDir.x < 0;
            _mashine.SpriteFlipX(isLeft);

            // 고정된 라인 조정
            for (int i = 2; i < 4; i++)
            {
                lines[i].transform.SetPositionAndRotation
                    (linePos + (aimDir90 * info.Interbal * sides[i % 2]), aimQua);
            }

            float gap = Mathf.Lerp(0.3f, info.Interbal, time / info.BeforeDelay);

            // 움직이는 라인 조정
            for (int i = 0; i < 2; i++)
            {
                lines[i].transform.SetPositionAndRotation
                    (linePos + (aimDir90 * gap * sides[i]), aimQua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        FadeOutLine(ct, lines).Forget();

        return aimAngle;
    }


    #endregion
}
