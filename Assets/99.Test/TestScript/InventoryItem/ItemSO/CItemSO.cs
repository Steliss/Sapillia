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
    #region 인스펙터

    [SerializeField] private int _itemID;
    [SerializeField] private string _nameKey;
    [SerializeField] private string _descriptionKey;
    [SerializeField] private Sprite _icon;

    #endregion


    #region 프로퍼티

    public int ItemID => _itemID;
    public string NameKey => _nameKey;
    public string DescriptionKey => _descriptionKey;
    public Sprite Icon => _icon;

    public virtual EItemType Type { get; }

    #endregion





}
