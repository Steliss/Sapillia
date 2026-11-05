using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerDashDust : MonoBehaviour
{
    #region 인스펙터
    [Header("먼지 데이터")]
    [SerializeField] private CDashDustDataSO _dustData;

    [Header("먼지 프리팹")]
    [SerializeField] private GameObject _dustPrefab;

    [Header("생성 위치")]
    [SerializeField] private Transform _spawnPoint;
    #endregion

    public void PlayDust(Vector2 dashDirection)
    {
        if (_dustData == null || _dustPrefab == null || _spawnPoint == null)
        {
            CLog.Warn("[DashDustSpawner] 참조 누락");
            return;
        }

        if (dashDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        EDashDustDirection direction = GetDirection8(dashDirection);

        Sprite[] sprites = _dustData.GetSprites(direction);

        if (sprites == null || sprites.Length == 0)
        {
            CLog.Warn($"[DashDustSpawner] {direction} 스프라이트 없음");
            return;
        }

        GameObject dustObject = null;

        if (CObjectPoolManager.Instance != null)
        {
            dustObject = CObjectPoolManager.Instance.GetPoolObject(_dustPrefab);
        }
        else
        {
            dustObject = Instantiate(_dustPrefab);
        }

        if (dustObject == null)
        {
            return;
        }

        if (!dustObject.TryGetComponent(out CDashEffect dustEffect))
        {
            CLog.Warn("[DashDustSpawner] CDashDustEffect 없음");
            return;
        }

        dustEffect.Play(sprites, _spawnPoint.position);
    }

    private EDashDustDirection GetDirection8(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
        {
            angle += 360f;
        }

        if (angle >= 22.5f && angle < 67.5f)
        {
            return EDashDustDirection.RightUp;
        }
        if (angle >= 67.5f && angle < 112.5f)
        {
            return EDashDustDirection.Up;
        }
        if (angle >= 112.5f && angle < 157.5f)
        {
            return EDashDustDirection.LeftUp;
        }
        if (angle >= 157.5f && angle < 202.5f)
        {
            return EDashDustDirection.Left;
        }
        if (angle >= 202.5f && angle < 247.5f)
        {
            return EDashDustDirection.LeftDown;
        }
        if (angle >= 247.5f && angle < 292.5f)
        {
            return EDashDustDirection.Down;
        }
        if (angle >= 292.5f && angle < 337.5f)
        {
            return EDashDustDirection.RightDown;
        }

        return EDashDustDirection.Right;
    }
}
