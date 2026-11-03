using UnityEngine;

public class CEnemyState_Death : IEnemyState
{
    private CStateMashine _mashine;
    private float _fadeTimeMax = 3f;
    private float _fadeTime;

    public CEnemyState_Death(CStateMashine mashine)
    {
        _mashine = mashine;
    }


    public void StateEnter()
    {
        _fadeTime = 0f;
        _mashine.Rigidbody.simulated = false;
        _mashine.Nav.enabled = false;
    }

    public void StateExit()
    {
    }

    public void StateUpdate()
    {
        _fadeTime += Time.deltaTime;
        float t = 1 - (_fadeTime / _fadeTimeMax);

        Color alpha = _mashine.Sprite.color;
        alpha.a = t;
        _mashine.Sprite.color = alpha;


        if (_mashine.Sprite.color.a <= 0.01f)
        {
            _mashine.gameObject.SetActive(false);
        }
    }
}
