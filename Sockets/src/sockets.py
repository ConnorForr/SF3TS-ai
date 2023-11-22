import socket
import sys
import json

localhost = "127.0.0.1"
port_number = 10305

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as main_socket:
    main_socket.connect((localhost, port_number))
    
    while True:
        frame_count = main_socket.recv(4)
        try:
            print(frame_count)
            frame_count = int(frame_count.decode("utf-8"))
            print(f'frame count: {frame_count}')
        except ValueError:
            print(f"broke: {frame_count}")
            exit()
        
        
        if frame_count % 2 == 0:
            inputs = json.dumps({"P1 Strong Punch": True})
            main_socket.sendall(bytes(inputs, encoding="utf-8"))
        
        data = main_socket.recv(2048).strip()
        data_str = data.decode("utf-8")
        print(data_str)



 # {"P1 Forward Kick":false,"P1 Roundhouse Kick":false,"P1 Short Kick":false,
 # "P2 Forward Kick":false,"P2 Short Kick":false,"1 Player Start":false,
 # "2 Players Start":false,"Coin 1":false,"Coin 2":false,"P1 Down":false,
 # "P1 Fierce Punch":false,"P1 Jab Punch":false,"P1 Left":false,"P1 Right":false,
 # "P1 Strong Punch":false,"P1 Up":false,"P2 Down":false,"P2 Fierce Punch":false,
 # "P2 Jab Punch":false,"P2 Left":false,"P2 Right":false,"P2 Roundhouse Kick":false,
 # "P2 Strong Punch":false,"P2 Up":false,"Service 1":false,"Service Mode":false}
