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
        _interractObject.SetActive(true);
    }

    protected void OnPlayerExit()
    {
        _interractObject.SetActive(false);
    }


}
