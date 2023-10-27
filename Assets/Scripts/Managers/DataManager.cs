using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    [SerializeField]
    private GameObject testAttackerPrefab = null;
    void Start()
    {
        //NetworkEventManager.Instance.AddEvent("onPrefabData", (strList) =>
        //{
            
        //});

        Debug.Log(JsonUtility.ToJson(testAttackerPrefab));
    }

    void Update()
    {
        
    }
}
