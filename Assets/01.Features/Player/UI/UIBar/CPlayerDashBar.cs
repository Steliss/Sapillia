using UnityEngine;
using UnityEngine.UI;


// 연결 이미지가 많아서 만든 UIBar 상속 불가 
[System.Serializable]
public class CDashSlotUI
{
    [Header("이미지")]
    [SerializeField] private Image _emptyImage;
    [SerializeField] private Image _blueImage;
    [SerializeField] private Image _orangeImage;
    [SerializeField] private Image _purpleImage;

    // 해당 칸의 보유 단계와 회복 진행률 반영
    public void SetValue(int count, float recoveryProgress)
    {
        recoveryProgress = Mathf.Clamp01(recoveryProgress);

        _emptyImage.enabled = true;

        if (count == 0)
        {
            _blueImage.fillAmount = recoveryProgress;

            _blueImage.enabled = recoveryProgress > 0.01f;
            _orangeImage.enabled = false;
            _purpleImage.enabled = false;

        }
        else if (count == 1)
        {
            _blueImage.fillAmount = 1f;
            _orangeImage.fillAmount = recoveryProgress;

            _blueImage.enabled = true;
            _orangeImage.enabled = recoveryProgress > 0.01f;
            _purpleImage.enabled = false;

        }
        else if (count == 2)
        {
            _orangeImage.fillAmount = 1f;
            _purpleImage.fillAmount = recoveryProgress;

            _blueImage.enabled = false;
            _orangeImage.enabled = true;
            _purpleImage.enabled = recoveryProgress > 0.01f;

        }
        else if (count == 3)
        {
            _purpleImage.fillAmount = 1f;

            _blueImage.enabled = false;
            _orangeImage.enabled = false;
            _purpleImage.enabled = true;

        }
    }
}


public class CPlayerDashBar : MonoBehaviour
{
    private const int SlotCount = 5;

    [Header("1번부터 연결")]
    [SerializeField] private CDashSlotUI[] _slots = new CDashSlotUI[SlotCount];

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private int _currentDash;
    private int _maxDash;
    private float _recoveryProgress;


    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            var data = await CManagerDontDestroy.DataMangerAsync(token);

            _data = data;
            _playerData = _data.PlayerData.GetPlayerData();

            _playerData.OnDashCountChanged += HandleDashCountChanged;
            _playerData.OnDashProgressChanged += HandleDashProgressChanged;

            // UI가 켜지는 순간의 데이터 반영
            _recoveryProgress = 0;

            SetValue(_currentDash, _maxDash, _recoveryProgress);

            CLog.Log("CPlayerDashUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _playerData.OnDashCountChanged -= HandleDashCountChanged;
        _playerData.OnDashProgressChanged -= HandleDashProgressChanged;
    }

    // 대쉬 값만 전달
    private void HandleDashCountChanged(int currentDash, int maxDash)
    {
        _currentDash = currentDash;
        _maxDash = maxDash;
    }

    // 대쉬 바 업데이트. 대쉬 게이지 관련 건들게 생기면 함수 이동 필요
    private void HandleDashProgressChanged(float recoveryProgress)
    {
        _recoveryProgress = recoveryProgress;

        SetValue(_currentDash, _maxDash, _recoveryProgress);
    }


    public void SetValue(int currentDash, int maxDash, float recoveryProgress)
    {
        maxDash = _playerData.Stats.DashCount;
        currentDash = _playerData.CurrentDashCount;
        recoveryProgress = Mathf.Clamp01(recoveryProgress);

        // 회복할 슬롯 구하기 
        int recoverySlot = currentDash % SlotCount;

        // 회복할 슬롯에 값을 넣어줌
        for (int i = 0; i < SlotCount; i++)
        {
            int count = currentDash / SlotCount;

            if (i < currentDash % SlotCount)
            {
                count++;
            }

            if (currentDash < maxDash && i == recoverySlot)
            {
                _slots[i].SetValue(count, recoveryProgress);
                continue;
            }

            _slots[i].SetValue(count, 0);
        }
    }
}