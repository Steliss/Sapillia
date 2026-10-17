using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CDataBaseSO<T> : ScriptableObject, ISerializationCallbackReceiver where T : CDataSO
{
    [SerializeField] private List<T> _datas = new List<T>();

    [NonSerialized] private Dictionary<string, T> _lookup;
    [NonSerialized] private bool _isDirty = true;

    public IReadOnlyList<T> Datas => _datas;
    public int Count => _datas.Count;

    public T this[int index] => _datas[index];

    /// <summary>
    /// 검색 후 데이터 검색 성공 여부를 반환
    /// </summary>
    /// <param name="id">id로 검색</param>
    /// <param name="data">검색한 데이터</param>
    /// <returns></returns>
    public bool TryGet(string id, out T data)
    {
        // 리턴 하기 전에 확인
        EnsureLookup();

        return _lookup.TryGetValue(id, out data);
    }

    // 딕셔너리를 지우고 새로 갱신
    private void EnsureLookup()
    {
        // 이미 최신
        if (!_isDirty && _lookup != null)
        {
            return;
        }

        _lookup ??= new Dictionary<string, T>();

        _lookup.Clear();

        for (int i = 0; i < Count; i++)
        {
            T data = _datas[i];

            if (data == null || string.IsNullOrWhiteSpace(data.Id))
            {
                continue;
            }

            _lookup.TryAdd(data.Id, data);
        }

        _isDirty = false;
    }

    // 데이터 목록의 변화 감지
    protected virtual void OnValidate()
    {
        HashSet<string> ids = new HashSet<string>();

        for (int i = 0; i < Count; i++)
        {
            T data = _datas[i];

            if (data == null)
            {
                continue;
            }

            string id = data.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                CLog.Warn($"{name} / {i}번째 Data.Id가 비어있음");
                continue;
            }

            if (!ids.Add(id))
            {
                CLog.Warn($"{name} / 중복 {id} 발견됨");
            }
        }

        _isDirty = true;

    }


    public void OnAfterDeserialize()
    {
        _isDirty = true;
    }

    public void OnBeforeSerialize()
    {
        // Do Nothing
    }
}
