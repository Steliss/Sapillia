using UnityEngine;


public enum EPlayerMoveState
{
    Idle,
    Walk,
    Run,
    Dash
}

public class CPlayerMovement : MonoBehaviour//, IInputCommand<Vector2>, IInputCommand<float>
{
    #region 인스펙터
    [Header("InputCommandHub")]
    [SerializeField] private CInputCommandHub _inputHub;

    [Header("PlayerDataBase")]
    
    
    #endregion

    #region 내부변수
    private float _movementSpeed;
    private float _runSpeed;
    #endregion


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
