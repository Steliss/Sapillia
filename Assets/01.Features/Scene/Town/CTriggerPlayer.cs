using UnityEngine;

/// <summary>
/// 플레이어 레이어 감지 추상 클래스
/// </summary>
public abstract class CTriggerPlayer : MonoBehaviour
{
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



    // 자식 클래스에서 구현할 동작
    protected abstract void OnPlayerEnter();

    protected abstract void OnPlayerExit();


    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        if (col == null)
        {
            return;
        }

        Gizmos.matrix = col.transform.localToWorldMatrix;

        Vector3 center = col.offset;
        Vector3 size = new Vector3(col.size.x, col.size.y, 0f);

        // 반투명 내부
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(center, size);

        // 테두리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}