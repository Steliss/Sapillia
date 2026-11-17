using UnityEngine;

public class CEnemyEffect_DashDust : MonoBehaviour
{
	#region 내부 변수
	private Animator _ani;
	private string[] _aniDir = 
		{
        "Down","DownRight","Right","UpRight",
        "Up","UpLeft","Left","DownLeft"
		};
	private int[] _hashDir = new int[8];
    #endregion

    private void Awake()
    {
		for (int i = 0; i < _hashDir.Length; i++)
		{
			_hashDir[i] = Animator.StringToHash(_aniDir[i]);
		}
		_ani = GetComponent<Animator>();
    }


	public void SetDir(Vector2 dir)
	{
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
		angle = Mathf.Repeat(angle, 360f);
		int index = Mathf.RoundToInt(angle / 45f) % 8; 
		_ani.Play(_hashDir[index]);
	}

}
