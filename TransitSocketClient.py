import socket
import json
import sys
#import TransitArrayMaker

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

def CreateSocket():
    HOST = "127.0.0.1"  # Unity Server
    PORT = 5000
    #client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect((HOST, PORT))

def SendData(data_list):

    json_data_list = [{"x": item[0], "y": item[1], "type": item[2]} for item in data_list]


    
    json_data = json.dumps(json_data_list)
    client_socket.send(json_data.encode())
