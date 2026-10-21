
public interface IDamageable
{
    void TakeDamage(float damage, UnityEngine.Vector2 pos);

}



public interface IEnemyState
{
    void StateEnter();
    void StateUpdate();
    void StateExit();
}