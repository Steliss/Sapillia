using TMPro;
using UnityEngine;

public class CDamageFont : MonoBehaviour
{
	#region 인스펙터
	[SerializeField] private CanvasGroup _cg;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private GameObject _criMark;
    [SerializeField] private float _fadeTime;
    [SerializeField] private Vector2 _movePosPlus;
    [SerializeField] private Vector2 _randomStartPos;
    #endregion

    #region 내부 변수
    private Vector2 _startPos;
    private Vector2 _targetPos;
    private float _lifeTime = 0f;
    #endregion

    private void Update()
    {
        if (_lifeTime > _fadeTime)
        {
            _cg.alpha = 0f;
            gameObject.SetActive(false);
            return;
        }

        _lifeTime += Time.deltaTime;
        float t = _lifeTime / _fadeTime;
        Vector2 fontPos = Vector2.Lerp(_startPos, _targetPos, t);
        float fade = Mathf.Lerp(1f, 0f, t);

        transform.position = fontPos;
        _cg.alpha = fade;
    }

    public void SetDamageFont(float damage, Vector2 startPos, bool isCritical = false, bool isRed = false)
    {
        if (_text == null)
        {
            return;
        }

        // 시작 지점과 목표 지점 설정
        Vector2 ranPos = _randomStartPos * 0.5f;
        ranPos.x = UnityEngine.Random.Range(-ranPos.x, ranPos.x);
        ranPos.y = UnityEngine.Random.Range(-ranPos.y, ranPos.y);
        _startPos = startPos + ranPos;
        _targetPos = startPos + ranPos + _movePosPlus;

        // 초기 설정
        transform.position = _startPos;
        _cg.alpha = 1.0f;
        string red = isRed ? "<color=red>" : "";
        _text.text = $"{red}{damage:0}";
        _criMark.SetActive(isCritical);
        _lifeTime = 0f;
    }


}
