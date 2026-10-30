using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CEnemyProjectile_Homing : CEnemyProjectile_Bullet
{
    private Transform _targetTr;
    private float _speed;
    private float _turnSpeed;

    public void HomingTargetSet(Transform targetTr, Vector2 velosity, float speed, float turnSpeed)
    {
        _targetTr = targetTr;
        _speed = speed;
        _turnSpeed = turnSpeed;
        _rb.velocity = velosity.normalized * _speed;
    }


    private void FixedUpdate()
    {
        float currentAngle = DirToAngle(_rb.velocity);
        Vector2 targetPos = _targetTr.position;
        float targetAngle = DirToAngle((targetPos - _rb.position).normalized);

        float nextAngle = Mathf.MoveTowardsAngle
            (currentAngle, targetAngle, Time.fixedDeltaTime * _turnSpeed);

        _dir = AngleToDir(nextAngle).normalized;
        _rb.velocity = _dir * _speed;
    }

    private float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    private Vector2 AngleToDir(float angle)
    {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }
}
