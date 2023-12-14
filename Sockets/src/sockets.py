import socket
import sys
import json
import random


localhost = "127.0.0.1"
port_number = 10305

inputs = ["P1 Forward Kick","P1 Roundhouse Kick","P1 Short Kick",
                "P2 Forward Kick","P2 Short Kick","1 Player Start",
                "2 Players Start","Coin 1","Coin 2","P1 Down",
                "P1 Fierce Punch","P1 Jab Punch","P1 Left","P1 Right",
                "P1 Strong Punch","P1 Up","P2 Down","P2 Fierce Punch",
                "P2 Jab Punch","P2 Left","P2 Right","P2 Roundhouse Kick",
                "P2 Strong Punch","P2 Up"]

new_inputs = {
                "Game Inputs" : {"P1 Forward Kick" : False,"P1 Roundhouse Kick" : False,"P1 Short Kick" : False,
                "P2 Forward Kick" : False,"P2 Short Kick" : False,"1 Player Start" : False,
                "2 Players Start" : False,"Coin 1" : False,"Coin 2" : False,"P1 Down" : False,
                "P1 Fierce Punch" : False,"P1 Jab Punch" : False,"P1 Left" : False,"P1 Right" : False,
                "P1 Strong Punch" : False,"P1 Up" : False,"P2 Down" : False,"P2 Fierce Punch" : False,
                "P2 Jab Punch" : False,"P2 Left" : False,"P2 Right" : False,"P2 Roundhouse Kick" : False,
                "P2 Strong Punch" : False,"P2 Up" : False},

                "Control Inputs" : {"Reset" : False}
                }

def reset_check(p1_health, p2_health):
    if(p1_health == 255 or p2_health == 255):
        return True 
    else:
        return False 
    
def send_data():
    for key, _ in new_inputs["Game Inputs"].items():
        new_inputs["Game Inputs"][key] = bool(random.getrandbits(1))

    send_inputs = json.dumps(new_inputs)
    main_socket.sendall(send_inputs.encode())


def recv_data():
    data = main_socket.recv(2048).strip()
    print(f"1: {data}")
    data_str = data.decode("utf-8")
    player_data = json.loads(data_str)
    print(f"P1 Health: {player_data['P1 Health']}, P2 Health: {player_data['P2 Health']}")

    if new_inputs["Control Inputs"]["Reset"]:
        new_inputs["Control Inputs"]["Reset"] = False
    else:
        new_inputs["Control Inputs"]["Reset"] = reset_check(player_data['P1 Health'], player_data['P2 Health'])


with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as main_socket:
    main_socket.connect((localhost, port_number))
    
    restart = False
    while True:
        
        print("sending...")
        send_data()        
        print("sent")

        print("waiting...")
        recv_data()
        print("done waiting")
        

        

 # {"P1 Forward Kick":false,"P1 Roundhouse Kick":false,"P1 Short Kick":false,
 # "P2 Forward Kick":false,"P2 Short Kick":false,"1 Player Start":false,
 # "2 Players Start":false,"Coin 1":false,"Coin 2":false,"P1 Down":false,
 # "P1 Fierce Punch":false,"P1 Jab Punch":false,"P1 Left":false,"P1 Right":false,
 # "P1 Strong Punch":false,"P1 Up":false,"P2 Down":false,"P2 Fierce Punch":false,
 # "P2 Jab Punch":false,"P2 Left":false,"P2 Right":false,"P2 Roundhouse Kick":false,
 # "P2 Strong Punch":false,"P2 Up":false,"Service 1":false,"Service Mode":false}
