using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/DynamiteThrow", fileName = "Enemy_Attack_DynamiteThrow_")]
public class CEnemyAttackStateSO_DynamiteThrow : CEnemyAttackStateSO
{
    #region 인스펙터

    [Header("===== 공격 =====")]
    [Header("다이너마이트 프리팹")]
    [SerializeField] private GameObject _projectile;
    [Header("선/후 딜레이, 조준 추적 스피드")]
    [SerializeField] private float _beforeDelay = 1f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private float _trackingSpeed = 5f;
    [Header("투사체 속성")]
    [SerializeField] private int _multiFire = 1;
    [SerializeField] private float _dynamiteLifeTime = 1.5f;
    [SerializeField] private Vector2 _randomPosRange = new Vector2(1f, 0.5f);

    [Space(10)]
    [Header("===== 전조 =====")]
    [Header("프리팹")]
    [SerializeField] private GameObject _warningLine;
    [Header("길이")]
    [SerializeField] private float _lineLength = 3f;
    [Header("간격")]
    [SerializeField] private float _interbal = 0.5f;
    #endregion



    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            // 선딜 + 조준
            Vector2 aimPos = await TrackingAim(ct);
            // 던지기
            for (int i = 0; i < _multiFire; i++)
            {
                Throw(aimPos);
            }

            // 후딜
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), cancellationToken: ct);



            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }

    public override void StateUpdate()
    {
        // 좌우 반전
        bool isLeft = _mashine.Target.position.x < _mashine.transform.position.x;
        _mashine.SpriteFlipX(isLeft);
    }

    private async UniTask<Vector2> TrackingAim(CancellationToken ct)
    {
        _mashine.Ani.Play(_mashine.HashAttReady);

        Vector2 myPos = _mashine.transform.position;
        Vector2 targetPos = _mashine.Target.position;

        // 라인 생성
        GameObject[] lines = new GameObject[2];
        float[] sides = { -1f, 1f };
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
            (_warningLine, _beforeDelay, myPos, Quaternion.identity);

            Vector2 scale = lines[i].transform.localScale;
            scale.y = _lineLength;
            lines[i].transform.localScale = scale;
        }

        // 플레이어 위치부터 조준 시작
        Vector2 aimPos = targetPos;

        // 조준 트래킹
        for (float time = 0f; time < _beforeDelay; time += Time.deltaTime)
        {
            myPos = _mashine.transform.position;
            targetPos = _mashine.Target.position;
            Vector2 dir = targetPos - myPos;


            // 에임 따라가기
            aimPos = Vector2.MoveTowards(aimPos, targetPos, Time.deltaTime * _trackingSpeed);

            Vector2 aimDir = (aimPos - myPos).normalized;
            Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            Vector2 linePos = myPos + aimDir * _lineLength * 0.5f;
            Quaternion qua = Quaternion.Euler(0, 0, DirToAngle(aimDir));
            float gap = Mathf.Lerp(_interbal, 0f, time / _beforeDelay);

            // 선 따라가기
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.SetPositionAndRotation(linePos + (aimDir90 * gap * sides[i]), qua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        return aimPos;
    }

    private void Throw(Vector2 toPos)
    {
        _mashine.Ani.Play(_mashine.HashAttack);

        GameObject dynamite = CreateAttackObject
        (_projectile, _dynamiteLifeTime + 1f, _mashine.transform.position, Quaternion.identity);

        // 목표 근처 랜덤범위 적용
        float ranX = UnityEngine.Random.Range(-_randomPosRange.x, _randomPosRange.x);
        float ranY = UnityEngine.Random.Range(-_randomPosRange.y, _randomPosRange.y);
        toPos.x += ranX;
        toPos.y += ranY;

        // 데미지 설정
        CEnemyProjectile_Dynamite pro = dynamite.GetComponent<CEnemyProjectile_Dynamite>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, toPos, _dynamiteLifeTime);
        }
    }
}