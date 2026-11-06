using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstMiniBossGate : MonoBehaviour
{
    #region 인스펙터

    [Header("문 스프라이트")]
    [SerializeField] private Animator _anim;

    #endregion

    #region 내부변수

    private bool _isOpened = true;

    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("플레이어가 문 트리거에 닿음");

        if (_isOpened)
        {
            DoorClose();
        }
        else
        {
            DoorOpen();
        }
    }

    private void DoorClose()
    {
        if (!_isOpened)
            return;

        _anim.ResetTrigger("Open");
        _anim.SetTrigger("Close");

        _isOpened = false;

        Debug.Log("문 닫기");
    }

    private void DoorOpen()
    {
        if (_isOpened)
            return;

        _anim.ResetTrigger("Close");
        _anim.SetTrigger("Open");

        _isOpened = true;

        Debug.Log("문 열기");
    }
}