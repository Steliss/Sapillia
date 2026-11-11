using TMPro;
using UnityEngine;

public class CLogUI : MonoBehaviour, IInputCommand
{
    [SerializeField] private GameObject _logUI;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _logText;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private CLocalizationManager _localization;
    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventory;

    private bool _isActive = false;
    private float _time = 0;

    private string _messages = "";
    private int _line = 0;
    private const int MaxCount = 10;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        _inputField.text = string.Empty;
        _logText.text = string.Empty;

        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Chatting, this);
            CLog.Log("CLogUI _inputTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            CLog.Log("CLogUI _localizationTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventory = _data.InventoryData.GetInventoryData();
            _inventory.OnArtifactAcquired += AddItem;
            CLog.Log("CLogUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask?.Cancel();
        _localizationTask?.Cancel();
        _dataTask?.Cancel();

        _input.Unregister(EKeyAction.Chatting, this);

        _inventory.OnArtifactAcquired -= AddItem;
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        if (_inputField.isFocused)
        {
            _time = 0f;
            return;
        }

        _time += Time.deltaTime;

        if (_time > 5f)
        {
            ActiveUI(false);
        }
    }

    public void Execute()
    {
        _time = 0f;

        if (!_isActive)
        {
            _logUI.SetActive(true);
            _isActive = true;

            _inputField.ActivateInputField();
            return;
        }

        // 채팅 전송
        string message = _inputField.text;

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        AddChat(message);

        _inputField.text = "";
    }

    public void ActiveUI(bool active)
    {
        _inputField.DeactivateInputField();
        _logUI.SetActive(false);

        _time = 0f;
        _isActive = false;
    }


    public void AddChat(string message)
    {
        AddMessage($"<color=#80FF80>{message}</color>");
    }

    private void AddItem(string nameKey)
    {
        string itemName = _localization.GetText(nameKey);

        AddMessage($"<color=#FFFF00>{itemName}</color>을 획득했습니다.");
    }

    private void AddTalk(string talkKey)
    {
        string talk = _localization.GetText(talkKey);

        AddMessage($"<color=#FFFFFF>{talk}</color>");
    }

    // 로그 추가
    private void AddMessage(string message)
    {
        if (_line > 0)
        {
            _messages += "\n";
        }

        _messages += message;
        _line++;

        // 줄 수가 넘으면 인덱스 비교 후 첫 줄 삭제
        if (_line > MaxCount)
        {
            int index = _messages.IndexOf('\n');
            _messages = _messages.Substring(index + 1);
            _line--;
        }

        _logText.text = _messages;
    }

}
