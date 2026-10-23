using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstMiniBossGate : MonoBehaviour
{
    #region 인스펙터

    [Header("문 스프라이트")]
    [SerializeField] private Animator _anim;

    #endregion

    #region 인스펙터

    private bool _isOpened = true;

    #endregion




    void Update()
    {
        DoorClose();
        DoorOpen();

    }


    private void DoorClose()
    {
        if (!_isOpened)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            _anim.SetTrigger("Close");
            _isOpened = false;
        }
    }


    private void DoorOpen()
    {
        if (_isOpened)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            _anim.SetTrigger("Open");
        }
    }


}
