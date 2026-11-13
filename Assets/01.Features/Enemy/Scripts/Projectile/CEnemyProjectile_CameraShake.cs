using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_CameraShake : MonoBehaviour
{
    [SerializeField] private float _shakingPower = 2f;
    private void OnEnable()
    {
        CPlayerCamera.Instance?.PlayerCameraShaking(_shakingPower);
    }
}
