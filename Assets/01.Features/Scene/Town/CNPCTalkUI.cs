using UnityEngine;
using UnityEngine.UI;

public class CNPCTalkUI : MonoBehaviour
{
    [SerializeField] private GameObject _talkUI;
    [SerializeField] private Image _npcImage;


    private CNPCTalk _currentTalk;

    public bool IsActive => _currentTalk != null;


    private void Awake()
    {
        Hide();
    }

    public void Show(CNPCTalk npcTalk, Sprite npcSprite)
    {
        _currentTalk = npcTalk;
        _npcImage.sprite = npcSprite;

        _talkUI.SetActive(true);
    }

    public bool IsOwner(CNPCTalk npcTalk)
    {
        return _currentTalk == npcTalk;
    }

    public void CloseTalk()
    {
        if (_currentTalk != null)
        {
            _currentTalk.ActiveUI(false);
        }
    }

    public void Hide()
    {
        _currentTalk = null;
        _talkUI.SetActive(false);
    }
}