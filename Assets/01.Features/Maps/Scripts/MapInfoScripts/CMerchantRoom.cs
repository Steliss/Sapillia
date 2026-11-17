using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMerchantRoom : MonoBehaviour
{
    #region 인스펙터

    [Header("방랑상인 생성 위치")]
    [SerializeField] private Transform _merchantSpawnPoint;

    #endregion

    #region 프로퍼티

    public Transform MerchantSpawnPoint => _merchantSpawnPoint;

    #endregion
}
