using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStandTorch : MonoBehaviour
{
    #region 인스펙터

    [Header("횃불")]
    [SerializeField] private GameObject _animationTorch;
    [SerializeField] private GameObject _remains;

    [Header("파편")]
    [SerializeField] private CTorchFragment _fragmentA;
    [SerializeField] private CTorchFragment _fragmentB;

    #endregion

    #region 내부변수

    private bool _isDestroyed;

    #endregion



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DestroyTorch();
        }
    }



    public void DestroyTorch()
    {
        if( _isDestroyed)
        {
            return;
        }

        _isDestroyed = true;

        _animationTorch.SetActive(false);

        _remains.SetActive(true);


        Vector2 randomVector = new Vector2(Random.Range(-1f, 1f), Random.Range(0, 1f));

        _fragmentA.gameObject.SetActive(true);
        _fragmentA.Launch(randomVector);


        Vector2 randomVectorB = randomVector;
        randomVectorB.x = -randomVector.x;

        _fragmentB.gameObject.SetActive(true);
        _fragmentB.Launch(randomVectorB);



    }


}
