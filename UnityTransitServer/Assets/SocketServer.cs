

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketServer : MonoBehaviour
{

    private Thread serverThread;
    private TcpListener server;
    private CancellationTokenSource cancellationTokenSource;
    private TcpClient client;
    private NetworkStream stream;
    private DataStruct[] dataArray;

    public event Action<DataStruct[]> OnDataReceived;

    private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();


    [Serializable]
    public struct DataStruct
    {
        public float x;
        public float y;
        public string type;
    }
    
    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        serverThread = new Thread(() => SetupServer(cancellationTokenSource.Token));
        serverThread.IsBackground = true; // Ensure the thread exits when the application stops
        serverThread.Start();
    }

    private void Update()
    {
        // Execute all queued actions on the main thread
        while (MainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    private void SetupServer(CancellationToken cancellationToken)
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, 5000);
            server.Start();
            Debug.Log("Server started. Waiting for connection...");

            byte[] buffer = new byte[2048];
            //string message;

            while (!cancellationToken.IsCancellationRequested)
            {
                //ScheduleOnMainThread(() => Debug.Log("Waiting for connection..."));

                client = server.AcceptTcpClient();
                ScheduleOnMainThread(() => Debug.Log("Connected!"));
                int bytesRead;
                stream = client.GetStream();

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    ScheduleOnMainThread(() => Debug.Log("Received: " + message));

                    dataArray = JsonHelper.FromJson<DataStruct>(message);
                    ScheduleOnMainThread(() =>
                    {
                        foreach (var dataPoint in dataArray)
                        {
                            Debug.Log($"x: {dataPoint.x}, y: {dataPoint.y}, type: {dataPoint.type}");
                        }

                        // Trigger the event on the main thread
                        OnDataReceived?.Invoke(dataArray);
                    });
                }
            }
        }
        catch (SocketException e)
        {
            ScheduleOnMainThread(() => Debug.Log("SocketException: " + e));
        }
        finally
        {
            server.Stop();
        }
    }

    private void ScheduleOnMainThread(Action action)
    {
        MainThreadActions.Enqueue(action);
    }

    private void OnDestroy()
    {
        // Wait for the thread to finish
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(); // Blocks until the thread terminates
        }

        cancellationTokenSource.Dispose(); // Clean up the token source
        
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }

}


