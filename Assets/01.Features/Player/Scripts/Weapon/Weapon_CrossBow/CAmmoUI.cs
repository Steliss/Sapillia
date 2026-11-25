using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerAmmoUI : MonoBehaviour
{
    #region 인스펙터

    [Header("Ammo UI")]
    [SerializeField] private GameObject _ammoPanel;
    [SerializeField] private Transform _ammoBarContainer;
    [SerializeField] private Image _ammoBarPrefab;

    #endregion

    #region 내부변수

    private readonly List<Image> _ammoBars = new();

    private int _maxAmmo;
    private int _currentAmmo;

    #endregion

    #region 프로퍼티
    public bool _isInitialized { get; private set; }


    #endregion

    #region Unity

    private void Awake()
    {
        SetVisible(false);
    }

    private void Start()
    {        
    }

    #endregion

    #region UI

    public void SetVisible(bool visible)
    {
        if (_ammoPanel != null)
        {
            _ammoPanel.SetActive(visible);
        }
    }

    // 석궁 장착 및 데이터 변경 시
    public void Initialize(int maxAmmo, int currentAmmo)
    {
        _maxAmmo = Mathf.Max(0, maxAmmo);

        SetBarCount(_maxAmmo);
        UpdateAmmo(currentAmmo);
        SetVisible(true);
        _isInitialized = true;
    }

    // 최대 탄약 수에 맞춰 막대 생성
    private void SetBarCount(int count)
    {
        if (_ammoBarContainer == null || _ammoBarPrefab == null)
        {
            return;
        }

        while (_ammoBars.Count < count)
        {
            Image bar = Instantiate(_ammoBarPrefab, _ammoBarContainer);

            bar.gameObject.SetActive(false);
            _ammoBars.Add(bar);
        }

        for (int i = 0; i < _ammoBars.Count; i++)
        {
            _ammoBars[i].gameObject.SetActive(i < count);
        }
    }

    // 현재 탄약 수 표시
    public void UpdateAmmo(int currentAmmo)
    {        
        _currentAmmo = Mathf.Clamp(currentAmmo,  0,_maxAmmo );

        for (int i = 0; i < _ammoBars.Count; i++)
        {
            if (_ammoBars[i] == null)
            {
                continue;
            }

            _ammoBars[i].enabled = i < _currentAmmo;
        }
    }

    #endregion
}