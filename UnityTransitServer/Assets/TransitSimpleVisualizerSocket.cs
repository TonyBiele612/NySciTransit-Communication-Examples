using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TransitSimpleVisualizerSocket : MonoBehaviour
{
    public GameObject stationPrefab;
    public GameObject socketServerGameObject;

    private List<GameObject> stationList = new List<GameObject>();
    private SocketServer ss;

    void Start()
    {
        ss = socketServerGameObject.GetComponent<SocketServer>();
        ss.OnDataReceived += UpdateStations;
    }

    private void OnDisable()
    {
        ss.OnDataReceived -= UpdateStations;
    }



    void UpdateStations(SocketServer.DataStruct[] newData)
    {
        foreach (GameObject station in stationList)
        {
            Destroy(station);
        }

        stationList.Clear();
        for (int i = 0; i < newData.Length; i++)
        {
            GameObject newStation = Instantiate(stationPrefab);     // circle sprite with text mesh attatched
            newStation.GetComponentInChildren<TextMeshPro>().text = newData[i].type.ToString();      // shows station type
            newStation.transform.position = new Vector2((newData[i].x * 16) - 8, (newData[i].y * -10) + 5);
            stationList.Add(newStation);
        }
    }
}

