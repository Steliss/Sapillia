using UnityEngine;

// 플레이어가 바라보는 위치 조정 → 마우스 트래킹
public class CPlayerFacing : MonoBehaviour
{
    #region 인스펙터

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("정면 / 후면 스프라이트 세팅")]
    [SerializeField] private bool _frontFacesRight = true;
    [SerializeField] private bool _backFacesRight = true;

    #endregion

    #region 내부변수
    private static readonly int FacingBackID = Animator.StringToHash("bFacingBack");
    private bool _isFacingBack;

    #endregion

    public void UpdateFacing(Vector2 aimDirection)
    {
        if(_animator == null || _spriteRenderer == null)
        {
            return;
        }

        // 위쪽을 바라보면 뒷모습
        if (Mathf.Abs(aimDirection.y) > 0.01f)
        {
            _isFacingBack = aimDirection.y > 0f;

            _animator.SetBool(FacingBackID, _isFacingBack);
        }

        // 오른쪽 / 왼쪽 구분
        if (Mathf.Abs(aimDirection.x) > 0.01f)
        {
            bool facingLeft = aimDirection.x < 0f;
            bool defaultFacesRight = _isFacingBack ? _backFacesRight : _frontFacesRight;
            _spriteRenderer.flipX = defaultFacesRight ? facingLeft : !facingLeft;
        }
    }
}