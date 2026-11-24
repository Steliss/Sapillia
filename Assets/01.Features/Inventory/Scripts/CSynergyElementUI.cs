using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSynergyElementUI : MonoBehaviour
{
	#region 인스펙터
	[Header("바뀌는 오브젝트 연결")]
	[SerializeField] private Image _backGround;
	[SerializeField] private Image _icon;
	[SerializeField] private TextMeshProUGUI _text;
    [Header("시너지 아이콘 종류")]
    [SerializeField] private Sprite _robust;
    [SerializeField] private Sprite _precision;
    [SerializeField] private Sprite _windsong;

    #endregion

    public void SetSynergyInfo(EArtifactSynergy synergy, string text)
	{
		if (_backGround == null || _icon == null || _text == null)
		{
			CLog.Warn($"{name} : 인스펙터 연결 필요");
			return;
		}

        gameObject.SetActive(true);

		switch (synergy)
		{
			case EArtifactSynergy.Robust:
				_icon.sprite = _robust;
                break;
			case EArtifactSynergy.Precision:
				_icon.sprite = _precision;
                break;
			case EArtifactSynergy.WindSong:
				_icon.sprite = _windsong;
                break;
		}

		_text.text = text;

	}
}
