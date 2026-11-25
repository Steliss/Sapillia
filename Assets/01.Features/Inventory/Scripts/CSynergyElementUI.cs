using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSynergyElementUI : MonoBehaviour
{
	#region 인스펙터
	[Header("바뀌는 오브젝트 연결")]
	[SerializeField] private Image _backGround;
	[SerializeField] private Image _icon;
	[SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;
	#endregion

	private EArtifactSynergy _im;

	public event Action<EArtifactSynergy> OnMouseEnter;
    public event Action OnMouseExit;

    public void SetSynergyIcon(Sprite sprite, EArtifactSynergy synergy)
	{
		_icon.sprite = sprite;
		_im = synergy;

        SetButton(_button);
	}

	public void SetSynergyLevel(string text)
	{
        gameObject.SetActive(true);
        _text.SetText(text);
	}

	public void SetActive(bool onOff)
	{
		gameObject.SetActive(onOff);
	}

	public EArtifactSynergy GetSynergy()
	{
		return _im;
	}


    // 마우스 이동에 따른 설명 UI 오픈
    private void SetButton(Button button)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 마우스 진입: 설명창 열기
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(eventData =>
        {
            OnMouseEnter.Invoke(_im);
        });

        // 마우스 이탈: 설명창 닫기
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(eventData =>
        {
            OnMouseExit.Invoke();
        });

        // 이벤트에 대한 해제 이벤트가 추가되면 고민이 필요
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }
}
