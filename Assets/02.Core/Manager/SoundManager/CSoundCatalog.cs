using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class CSound
{
    [SerializeField] private string _name;
    [SerializeField] private EventReference _sound;

    public string Name => _name;
    public EventReference Sound => _sound;
}



public class CSoundCatalog : MonoBehaviour
{

    [Header("배경 음악")]
    [SerializeField] private List<CSound> _BGM = new();

    [Header("효과음악")]
    [SerializeField] private List<CSound> _effect = new();

    [Header("환경음악")]
    [SerializeField] private List<CSound> _enviroment = new();


    public List<CSound> BGM => _BGM;
    public List<CSound> Effect => _effect;
    public List<CSound> Enviroment => _enviroment;
}
