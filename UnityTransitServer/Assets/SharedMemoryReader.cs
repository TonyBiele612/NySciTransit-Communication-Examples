using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

public class SharedMemoryReader : MonoBehaviour
{
    const string SHARED_MEMORY_NAME = "TransitSharedMemory"; // Shared memory block name. Make sure it matches the name from the sending script
    const string MUTEX_NAME = "Global\\TransitMemoryMutex";  // Mutex name. Global so all Windows apps can read it. Make sure it matches name from the sending script
    const int STRUCT_SIZE = 12; // 2 floats (4 bytes each) + 1 int (4 bytes)
    const int HEADER_SIZE = 12; // 8 bytes for timestamp + 4 bytes for list size
    const int MAX_STRUCTS = 50; // No more than 50 stations

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataStruct    // position x, position y, station type
    {
        public float x;
        public float y;
        public int type;
    }

    public event Action<DataStruct[]> OnDataReceived;  // C# event to pass received data

    public DataStruct[] transitData;    
    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private Mutex mutex;  // Mutex locks the process so the memory can't be read while it is being written by Python camera app
    private double lastTimestamp = 0;

    void Start()
    {
        try
        {
            // Open the shared memory block with the same name
            mmf = MemoryMappedFile.OpenExisting(SHARED_MEMORY_NAME);  
            accessor = mmf.CreateViewAccessor();
            mutex = Mutex.OpenExisting(MUTEX_NAME);

            Debug.Log("Shared memory opened successfully.");
        }
        catch (FileNotFoundException)
        {
            Debug.LogError($"Shared memory block '{SHARED_MEMORY_NAME}' not found. Make sure the writer is running.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error opening shared memory or mutex: {ex.Message}");
        }
    }

    void Update()
    {
        if (accessor == null || mutex == null) return;

        try
        {
            if (mutex.WaitOne(100))   // if mutex is locked, wait 100 milliseconds
            {
                try
                {
                    // Read timestamp
                    double currentTimestamp = accessor.ReadDouble(0);   
                    if (currentTimestamp <= lastTimestamp)   // Timestap prevents reading the same data every single frame
                    {
                        // No new data
                        return;
                    }

                    lastTimestamp = currentTimestamp;

                    // Read list size
                    int listSize = accessor.ReadInt32(8);
                    if (listSize < 0 || listSize > MAX_STRUCTS)
                    {
                        throw new InvalidOperationException($"Invalid list size ({listSize}) in shared memory.");
                    }

                    // Read and validate data
                    transitData = new DataStruct[listSize];
                    for (int i = 0; i < listSize; i++)
                    {
                        int offset = HEADER_SIZE + (i * STRUCT_SIZE);
                        transitData[i].x = accessor.ReadSingle(offset);
                        transitData[i].y = accessor.ReadSingle(offset + 4);
                        transitData[i].type = accessor.ReadInt32(offset + 8);
                    }

                    OnDataReceived?.Invoke(transitData);
                    Debug.Log($"Received data array (size: {listSize}):");
                    /*
                    for (int i = 0; i < listSize; i++)
                    {
                        Debug.Log($"[{i}] X: {transitData[i].x}, Y: {transitData[i].y}, Type: {transitData[i].type}");
                    }
                    */
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError($"Data error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error during memory read: {ex.Message}");
                }
            }
        }
        finally
        {
            mutex.ReleaseMutex();   // done reading, allow memory to be written to by Python app
        }
    }

    private void OnDestroy()        // clean up memory
    {
        try
        {
            accessor?.Dispose();
            mmf?.Dispose();
            Debug.Log("Shared memory cleaned up successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cleaning up shared memory: {ex.Message}");
        }
    }
}