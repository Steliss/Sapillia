using System.Collections;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum ENPC
{
    Cape,
    Conner,
    Fat,
    Mayer,
    Mayerson
}

public class CNPCTalk : CTriggerPlayer, IInputCommand
{
    [Header("NPC")]
    [SerializeField] private ENPC npc;
    [SerializeField] private Sprite _npcSprite;

    [Header("공통 UI")]
    [SerializeField] private CNPCTalkUI _talkUI;
    [SerializeField] private CLogUI _logUI;

    [Header("대화 UI")]
    [SerializeField] private GameObject _headUI;
    [SerializeField] private TextMeshProUGUI _talkText;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("로컬라이징 키")]
    [SerializeField] private string[] _dialogueKeys;

    [Header("글자 출력 간격")]
    [SerializeField, Min(0.001f)]
    private float _characterInterval = 0.05f;

    private CLocalizationManager _localization;
    private CInputCommandHub _input;

    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _typingTask = new CUniTaskOnes();

    private bool _isNear;
    private bool _isActive;
    private bool _isTyping;

    private int _dialogueIndex;


    private void Awake()
    {
        _headUI.SetActive(false);
    }

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Interaction, this);

            CLog.Log("CNPCTalk _inputTask OnEnable 작업 완료");
        });

        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            CLog.Log("CNPCTalk _localizationTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _localizationTask.Cancel();

        _input?.Unregister(EKeyAction.Interaction, this);

        _isNear = false;
    }

    protected override void OnPlayerEnter()
    {
        _isNear = true;
    }

    protected override void OnPlayerExit()
    {
        _isNear = false;
    }

    public void Execute()
    {
        // 대화 출력
        if (!_isActive)
        {

            if (!_isNear)
            {
                return;
            }

            if (_talkUI == null)
            {
                return;
            }

            if (_talkUI.IsActive && !_talkUI.IsOwner(this))
            {
                return;
            }

            if (_talkUI == null || _logUI == null || _localization == null)
            {
                CLog.Warn($"{npc} : 참조 확인");
                return;
            }

            if (_dialogueKeys == null || _dialogueKeys.Length == 0)
            {
                CLog.Warn($"{npc} : 키 값 확인");
                return;
            }

            StartTalk();
            return;
        }

        // 출력 중이면 현재 문장 전체 표시
        if (_isTyping)
        {
            CompleteTyping();
            return;
        }

        // 대화 순회
        _dialogueIndex++;

        if (_dialogueIndex >= _dialogueKeys.Length)
        {
            EndTalk();
            return;
        }

        ShowDialogue();
    }


    // 종료만 기입 타 함수에서 true 호툴할 일 있을시 수정 필요
    public void ActiveUI(bool active)
    {
        if (!active)
        {
            EndTalk();
            return;
        }
    }

    private void StartTalk()
    {
        _isActive = true;
        _dialogueIndex = 0;

        _headUI.SetActive(true);
        _talkUI.Show(this, _npcSprite);

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        StopTyping();

        string dialogue = _localization.GetText(_dialogueKeys[_dialogueIndex]);

        _talkText.maxVisibleCharacters = 0;
        _nameText.text = _localization.GetText($"talk.{npc.ToString().ToLowerInvariant()}.name"); // Enum을 string 소문자 변환
        _talkText.text = dialogue;
        _talkText.ForceMeshUpdate();

        // 현재 문장을 로그에 기록
        _logUI.AddTalk(dialogue);

        _isTyping = true;

        _typingTask.Run(async token =>
        {
            await TypeDialogue(token);
        });
    }

    private async UniTask TypeDialogue(CancellationToken token)
    {
        int characterCount = _talkText.textInfo.characterCount;
        int visibleCount = 0;
        float elapsed = 0f;

        float interval = Mathf.Max(0.001f, _characterInterval);

        while (visibleCount < characterCount)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            token.ThrowIfCancellationRequested();

            elapsed += Time.unscaledDeltaTime;

            while (elapsed >= interval && visibleCount < characterCount)
            {
                elapsed -= interval;
                visibleCount++;

                _talkText.maxVisibleCharacters = visibleCount;
            }
        }

        _isTyping = false;
    }

    private void CompleteTyping()
    {
        StopTyping();

        _talkText.maxVisibleCharacters = int.MaxValue;
    }

    private void StopTyping()
    {
        _typingTask.Cancel();

        _isTyping = false;
    }

    public void EndTalk()
    {
        StopTyping();

        _isActive = false;
        _dialogueIndex = 0;

        _headUI.SetActive(false);
        _logUI.ActiveUI(false);

        if (_talkUI != null && _talkUI.IsOwner(this))
        {
            _talkUI.Hide();
        }
    }
}