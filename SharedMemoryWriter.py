import mmap
import struct
import time
from multiprocessing import shared_memory, Lock
import ctypes

# Constants
SHARED_MEMORY_NAME = "TransitSharedMemory"
MUTEX_NAME = "Global\\TransitMemoryMutex"
MAX_STRUCTS = 50
STRUCT_SIZE = struct.calcsize('ffi')  # 12 bytes per struct
HEADER_SIZE = struct.calcsize('d') + struct.calcsize('I')  # Timestamp + List size
BUFFER_SIZE = HEADER_SIZE + (STRUCT_SIZE * MAX_STRUCTS)

# Global shared memory variable
shm = None
mutex = None
kernel32 = None

def InitializeSharedMemory():
    global shm, mutex, kernel32
    try:
        shm = shared_memory.SharedMemory(name=SHARED_MEMORY_NAME, create=True, size=BUFFER_SIZE)
        print(f"Shared memory created with name: {SHARED_MEMORY_NAME}")
    except Exception as e:
        print(f"Error initializing shared memory: {e}")
        raise

    try:
        kernel32 = ctypes.windll.kernel32  # Load kernel32 on Windows
        mutex = kernel32.CreateMutexW(None, False, MUTEX_NAME)
    except AttributeError:
        raise RuntimeError("This code requires Windows and the ctypes.windll.kernel32 library.")


def WriteToSharedMemory(data):
    global shm, mutex, kernel32
    try:
        if shm is None:
            raise ValueError("Shared memory is not initialized. Call initialize_shared_memory() first.")

        if len(data) > MAX_STRUCTS:
            raise ValueError(f"List size ({len(data)}) exceeds maximum allowed ({MAX_STRUCTS})")

        kernel32.WaitForSingleObject(mutex, 0xFFFFFFFF) #infinite wait 
        buffer = shm.buf

        # Write timestamp
        timestamp = time.time()
        buffer[:struct.calcsize('d')] = struct.pack('d', timestamp)

        # Write list size
        buffer[struct.calcsize('d'):HEADER_SIZE] = struct.pack('I', len(data))

        # Write the structs
        for i, (f1, f2, i1) in enumerate(data):
            start = HEADER_SIZE + (i * STRUCT_SIZE)
            buffer[start:start + STRUCT_SIZE] = struct.pack('ffi', f1, f2, i1)

        print("Data written to shared memory successfully.")

    except ValueError as e:
        print(f"Value error: {e}")
    except Exception as e:
        print(f"Unexpected error during write: {e}")
        raise
    finally:
        kernel32.ReleaseMutex(mutex)

def CleanupSharedMemory():
    global shm, mutex
    try:
        if shm is not None:
            kernel32.CloseHandle(mutex)
            shm.close()
            shm.unlink()
            print("Shared memory cleaned up successfully.")
            shm = None
        else:
            print("Shared memory is not initialized.")
    except FileNotFoundError:
        print("Shared memory already removed.")
    except Exception as e:
        print(f"Error during cleanup: {e}")
