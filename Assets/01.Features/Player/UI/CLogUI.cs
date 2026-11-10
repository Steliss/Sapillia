using TMPro;
using UnityEngine;

public class CLogUI : MonoBehaviour, IInputCommand
{
    [SerializeField] private GameObject _logUI;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _logText;

    // 아이템 획득 연결 기본값 null 


    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private CLocalizationManager _localization;
    private CInputCommandHub _input;
    private CDataManger _data;

    private bool _isActive = false;
    private float _time = 0;

    private string _messages = "";
    private int _line = 0;
    private const int MaxCount = 100;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        _inputField.text = string.Empty;
        _logText.text = string.Empty;

        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.ESC, this);
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
            CLog.Log("CLogUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask?.Cancel();
        _localizationTask?.Cancel();
        _dataTask?.Cancel();

        _input.Unregister(EKeyAction.ESC, this);
        _input.Unregister(EKeyAction.Chatting, this);
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
        if (_line > 0)
        {
            _messages += "\n";
        }

        _messages += message;
        _line++;

        if (_line > MaxCount)
        {
            int index = _messages.IndexOf('\n');
            _messages = _messages.Substring(index + 1);
            _line--;
        }

        _logText.text = _messages;
    }


}
