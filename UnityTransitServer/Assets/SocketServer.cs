using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using static ServerScript;

public class ServerScript : MonoBehaviour
{
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    [System.Serializable]
    public struct DataPoint
    {
        public float x;
        public float y;
        public int type; // Match the key "type" from the JSON
    }


    public DataPoint[] dataArray = null;

    private void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, 5000);
            server.Start();

            byte[] buffer = new byte[2048];
            string message = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                message = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    message = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + message);
                    dataArray = JsonHelper.FromJson<DataPoint>(message);
                    foreach (var dataPoint in dataArray)
                    {
                        Debug.Log($"x: {dataPoint.x}, y: {dataPoint.y}, type: {dataPoint.type}");
                    }

                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

}

public static class JsonHelper    // allows JSON to deserialize an array of structs
{
    public static T[] FromJson<T>(string json)
    {
        string wrappedJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}