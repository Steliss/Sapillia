using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDashAfterImageEffectSpawner : MonoBehaviour
{
    #region 인스펙터

    [Header("참조")]
    [SerializeField] private CPlayerMovement _movement;
    [SerializeField] private SpriteRenderer _playerSprite;

    [Header("잔상 프리팹")]
    [SerializeField] private GameObject _afterimagePrefab;

    [Header("생성 간격")]
    [Min(0.01f)]
    [SerializeField] private float _spawnInterval = 0.04f;

    #endregion

    #region 내부변수

    private float _spawnTimer;
    private bool _wasDashing;

    #endregion

    #region Unity

    private void LateUpdate()
    {
        if (_movement == null || _playerSprite == null || _afterimagePrefab == null)
        {
            return;
        }

        // 대시 중이 아니면 잔상 생성 중지
        if (_movement.MoveState != EPlayerMoveState.Dash)
        {
            _wasDashing = false;
            _spawnTimer = 0f;
            return;
        }

        // 대시 시작 순간 첫 잔상 생성
        if (!_wasDashing)
        {
            _wasDashing = true;
            _spawnTimer = _spawnInterval;

            SpawnAfterimage();
            return;
        }

        // 잔상 생성 간격
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer > 0f)
        {
            return;
        }

        SpawnAfterimage();

        _spawnTimer = _spawnInterval;
    }

    private void OnDisable()
    {
        _wasDashing = false;
        _spawnTimer = 0f;
    }

    #endregion

    #region Afterimage

    private void SpawnAfterimage()
    {
        if (CObjectPoolManager.Instance == null || !_playerSprite.enabled || _playerSprite.sprite == null)
        {
            return;
        }

        // 기존 오브젝트 풀에서 잔상 가져오기
        GameObject afterimageObject = CObjectPoolManager.Instance.GetPoolObject(_afterimagePrefab);

        if (afterimageObject == null)
        {
            return;
        }

        if (!afterimageObject.TryGetComponent(out CDashAfterimageEffect effect))
        {
            CLog.Warn("[Afterimage] 이펙트 컴포넌트 누락");
            gameObject.SetActive(false);

            return;
        }

        // 현재 플레이어 모습을 복사해서 재생
        effect.Play(_playerSprite);
    }

    #endregion
}
