using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;

public class TransitSimpleVisualizer : MonoBehaviour
{
    public GameObject stationPrefab;
    public GameObject sharedMemoryReaderGameObject;
        
    private SharedMemoryReader smr;
    private List<GameObject> stationList = new List<GameObject>();

    void Start()
    {
        smr = sharedMemoryReaderGameObject.GetComponent<SharedMemoryReader>();
        smr.OnDataReceived += UpdateStations;
        //Debug.Log(smr);
    }

    private void OnDisable()
    {
        smr.OnDataReceived -= UpdateStations;
    }



    void UpdateStations(SharedMemoryReader.DataStruct[] newData) 
    {
        foreach (GameObject station in stationList)
        {
            Destroy(station);
        }

        stationList.Clear();
        for(int i=0; i<newData.Length; i++)
        {
            GameObject newStation = Instantiate(stationPrefab);     // circle sprite with text mesh attatched
            newStation.GetComponentInChildren<TextMeshPro>().text = newData[i].type.ToString();      // shows station type
            newStation.transform.position = new Vector2((newData[i].x * 16) - 8, (newData[i].y * -10) + 5);
            stationList.Add(newStation);
        }
    }
}
