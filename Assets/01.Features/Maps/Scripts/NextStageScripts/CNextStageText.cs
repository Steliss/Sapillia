using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CNextStageText : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private GameObject _interractObject;
    [SerializeField] private TextMeshPro _text;

    #endregion

    #region 내부변수

    private bool _isActive = false;

    #endregion


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return;
        }

        OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return;
        }

        OnPlayerExit();
    }

    protected void OnPlayerEnter()
    {
        _isActive = true;
        _interractObject.SetActive(true);
    }

    protected void OnPlayerExit()
    {
        _isActive = false;
        _interractObject.SetActive(false);
    }


}
