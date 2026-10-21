using System.Collections.Generic;
using UnityEngine;

// 오브젝트 풀을 거쳐간 오브젝트는 이 클래스를 가져가게 됨
public class CObjectPoolId : MonoBehaviour
{
    public int Id;
}



public class CObjectPoolManager : MonoBehaviour
{

    // 풀 관리용 장부
    private Dictionary<int, Queue<GameObject>> _poolMap = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, Transform> _lootMap = new Dictionary<int, Transform>();

    // 수명 관리용 장부
    private Dictionary<GameObject, float> _lifeMap = new Dictionary<GameObject, float>();
    private List<GameObject> _alives = new List<GameObject>();

    private bool _isRemoving = false;
    public static CObjectPoolManager Instance { get; private set; }
    public bool IsRemoving => _isRemoving;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        LifeUpdate();
    }

    private void OnDestroy()
    {
        Instance = null;
        ClearAll();
    }

    /// <summary>
    /// 오브젝트 풀이 없으면 생성하고 풀이 있으면 풀에서 꺼내서 반환한다.
    /// 수명이 없는 오브젝트 풀. 오브젝트가 꺼지면 자동으로 반납된다.
    /// </summary>
    /// <param name="prefeb"></param>
    /// <returns></returns>
    public GameObject TryGetObjectPool(GameObject prefeb)
    {
        return TryObjectPool(prefeb);
    }

    /// <summary>
    /// 오브젝트 풀이 없으면 생성하고 풀이 있으면 풀에서 꺼내서 반환한다.
    /// 수명이 지나면 반납된다.
    /// </summary>
    /// <param name="prefeb">생성할 프리팹</param>
    /// <param name="life">수명(초)</param>
    /// <returns></returns>
    public GameObject TryGetObjectPool(GameObject prefeb, float lifeTime)
    {
        GameObject go = TryObjectPool(prefeb);
        _lifeMap[go] = lifeTime;
        return go;
    }

    
    private GameObject TryObjectPool(GameObject prefeb)
    {
        if (prefeb == null)
        {
            return null;
        }

        int id = prefeb.GetInstanceID();

        Queue<GameObject> pool = TryPoolGet(id, prefeb.name);

        // 풀에서 오브젝트 꺼내기. 부족하면 생성
        GameObject go;
        if (pool.Count == 0)
        {
            go = Instantiate(prefeb);
            var opid = go.AddComponent<CObjectPoolId>();
            opid.Id = id;
        }
        else
        {
            go = pool.Dequeue();
        }


        // 장부 등록
        if (!_alives.Contains(go))
        {
            _alives.Add(go);
        }
        go.SetActive(true);

        // 초기 세팅 보장
        go.transform.parent = null;
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.WakeUp();
        }
        go.transform.localScale = prefeb.transform.localScale;
        go.transform.rotation = prefeb.transform.rotation;
        return go;
    }



    /// <summary>
    /// 오브젝트 풀 모두 삭제
    /// </summary>
    public void ClearAll()
    {
        _isRemoving = true;

        for (int i = _alives.Count - 1; i >= 0; i--)
        {
            if (_alives[i] != null)
            {
                Destroy(_alives[i]);
            }
        }

        foreach (Transform loots in _lootMap.Values)
        {
            if (loots != null && loots.gameObject != null)
            {
                Destroy(loots.gameObject);
            }
        }

        _poolMap.Clear();
        _lootMap.Clear();
        _alives.Clear();
        _lifeMap.Clear();

        _isRemoving = false;
    }



    private void LifeUpdate()
    {
        if (_lifeMap.Count <= 0)
        {
            return;
        }

        // 리스트 역순회
        for (int i = _alives.Count - 1; i >= 0; i--)
        {
            GameObject go = _alives[i];

            // 오브젝트가 없는 경우 (꼬임 방지)
            if (go == null)
            {
                _alives.RemoveAt(i);
                continue;
            }

            if (!go.activeSelf)
            {
                ReturnPool(go);
            }

            // 수명이 있는 경우에만 감소
            if (_lifeMap.ContainsKey(go))
            {
                _lifeMap[go] -= Time.deltaTime;

                // 수명이 다 된 오브젝트 회수
                if (_lifeMap[go] <= 0f)
                {
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                    continue;
                }
            }
        }
    }

    private void RemoveLifeMap(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        if (_lifeMap.ContainsKey(go))
        {
            _lifeMap.Remove(go);
        }

        if (_alives.Contains(go))
        {
            _alives.Remove(go);
        }
    }

    public void ReturnPool(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        RemoveLifeMap(go);
        
        // 오브젝트의 ID로 풀 검색하여 회수
        if (go.TryGetComponent<CObjectPoolId>(out CObjectPoolId opid))
        {
            int id = opid.Id;

            // 회수
            if (_poolMap.ContainsKey(id))
            {
                // 리지드바디 초기화
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = 0f;
                }

                // 켜져있는 경우 비활성화
                if (go.activeSelf)
                {
                    go.SetActive(false);
                }

                if (_lootMap[id] != null)
                {
                    go.transform.parent = _lootMap[id];
                }

                _poolMap[id].Enqueue(go);
            }
            // 풀이 없는 경우 파괴
            else
            {
                Destroy(go);
            }
        }
        // ID가 없는 경우 파괴
        else
        {
            Destroy(go);
        }
    }

    private Queue<GameObject> TryPoolGet(int id, string name)
    {
        if (_poolMap.TryGetValue(id, out Queue<GameObject> pool))
        {
            return pool;
        }
        else
        {
            // 새로운 풀, 아래로 정리할 오브젝트 생성
            _poolMap.Add(id, new Queue<GameObject>());
            _lootMap.Add(id, new GameObject($"Loot_{name}").transform);
            CLog.Log($"새로운 풀 생성 : {name}");
            return _poolMap[id];
        }
    }
}
