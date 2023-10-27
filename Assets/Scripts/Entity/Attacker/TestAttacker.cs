using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAttacker : MonoBehaviour
{
    [SerializeField]
    private UnitStat stat = new();

    void Start()
    {
        
    }

    void Update()
    {
        stat.OnUpdate();
    }
}
