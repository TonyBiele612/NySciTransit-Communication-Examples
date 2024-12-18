This is to demonstrate the transfer information from Python OpenCV code to Unity for visualization.
The information consists of an array of structs in the form of (float position x, float position y, int type),
with a max size of 50. The positions are normalized between 0 snd 1 with 0,0 being the upper left corner.
There are two different methods being explored here:
1) Socket communication-
   
   Python scripts: TransitArrayMaker, TransitSocketClient.
     
   Unity Script: SocketServer
   
2) Shared Memory-
 
   Python scripts: TransitArrayMakerSharedMemory, SharedMemoryWriter.
   
   Unity Scripts: SharedMemoryReader, TransitSimpleVisualizer   
   * This is the preferred method

The Unity project is UnityTransitServer. Scripts are in Assets.   
In both circumstances, the entire array is sent when the array is updated in Python. It does not necessarily conform 
to the order of objects in the previously sent array.
