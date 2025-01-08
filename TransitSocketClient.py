import socket
import json
import sys
import time

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

def CreateSocket():
    HOST = "127.0.0.1"  # Unity Server
    PORT = 5000
    retry_delay = 2
    #client_socket.connect((HOST, PORT))

    while True:
        try:
            client_socket.connect((HOST, PORT))
            print("Connected")
            return
        except socket.error as e:
            print(f"Connection failed: {e}. Retrying in {retry_delay} seconds...")
            time.sleep(retry_delay)

def SendData(data_list):

    json_data_list = [{"x": item[0], "y": item[1], "type": item[2]} for item in data_list]

    json_data = json.dumps(json_data_list)
    client_socket.send(json_data.encode())
