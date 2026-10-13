using UnityEngine;
using FMODUnity;

public class BGMSound : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter _bgmEmitter;

    private void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        _bgmEmitter.Play();
    }

    public void StopBGM()
    {
        _bgmEmitter.Stop();
    }
}