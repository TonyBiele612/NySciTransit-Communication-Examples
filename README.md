This is to demonstrate the transfer information from Python OpenCV code to Unity for visualization.
The information consists of an array of structs in the form of (float position x, float position y, int type),
with a max size of 50. The positions are normalized between 0 and 1 with 0,0 being the upper left corner.
There are two different methods being explored here:
1) Socket communication-
   
   Python scripts: TransitArrayMaker, TransitSocketClient.
     
   Unity Script: SocketServer, TransitSimpleVisualizerSocket

   To Run: Python side: Run TransitArrayMaker
           Unity Side: DISABLE GameObjects SharedMemoryReader, Visualizer SMR
                       ENABLE GameObjects SocketServer, Visualizer SS
   
2) Shared Memory-
   * This is the preferred method
     
   Python scripts: TransitArrayMakerSharedMemory, SharedMemoryWriter.
   
   Unity Scripts: SharedMemoryReader, TransitSimpleVisualizer
   
  To Run: Python side: Run TransitArrayMakerSharedMemory
           Unity Side: DISABLE GameObjects SocketServer, Visualizer SS
                       ENABLE GameObjects SharedMemoryReader, Visualizer SMR
                       
To operate: Left click in the Python window to create a station at the click location.
New station will appear and be given a random Type number
The station will appear in the same location and Type in the Unity window
     

The Unity project is UnityTransitServer. Scripts are in Assets.   
In both circumstances, the entire array is sent when the array is updated in Python. It does not necessarily conform to the order of objects in the previously sent array.
