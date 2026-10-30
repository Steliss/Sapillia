using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRoomData : MonoBehaviour
{
	#region 인스펙터
	[Header("방 크기")]
	[SerializeField] private Vector2Int _roomSize = new Vector2Int (1,1);

	[Header("방 종류")]
	[SerializeField] private ERoomType _roomType = ERoomType.Combat;

	[Header("문 위치")]
	[SerializeField] private bool _up1;
    [SerializeField] private bool _up2;
    [SerializeField] private bool _down1;
    [SerializeField] private bool _down2;
    [SerializeField] private bool _left1;
	[SerializeField] private bool _left2;
	[SerializeField] private bool _right1;
	[SerializeField] private bool _right2;

	#endregion


	#region 프로퍼티

	public Vector2Int RoomSize => _roomSize;

	public bool Up1 => _up1;
	public bool Up2 => _up2;
	public bool Down1 => _down1;
	public bool Down2 => _down2;
	public bool Left1 => _left1;
	public bool Left2 => _left2;
	public bool Right1 => _right1;
	public bool Right2 => _right2;

	public ERoomType RoomType => _roomType;
	#endregion





}
