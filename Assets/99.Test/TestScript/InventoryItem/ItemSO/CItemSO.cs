using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 공통사항
/// 
///  ItemID = Data ID갑
///  Name   = 이름 번역 키값
///  Description = 설명 번역 키값
///  Icon = 아이콘
///  
/// </summary>
public abstract class CItemSO : ScriptableObject
{
    public int ItemID;
    public string Name;
    public string Description;
    public EItemType Type;
    public Sprite Icon;
}