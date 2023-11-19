import socket
import sys
import json

localhost = "127.0.0.1"
port_number = 10305

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as main_socket:
    main_socket.connect((localhost, port_number))
