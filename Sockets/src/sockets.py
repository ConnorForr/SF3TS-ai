import socket # socket import for socket server
import json # json import for sending over sockets
import gymnasium # gymnasium import for our environment
import random

# local host IP
localhost = "127.0.0.1"

# Port number
port_number = 10305

# All possible inputs to give BizHawk
# (Missing some but those will never be used)
inputs = ["P1 Forward Kick","P1 Roundhouse Kick","P1 Short Kick",
                "P2 Forward Kick","P2 Short Kick","1 Player Start",
                "2 Players Start","Coin 1","Coin 2","P1 Down",
                "P1 Fierce Punch","P1 Jab Punch","P1 Left","P1 Right",
                "P1 Strong Punch","P1 Up","P2 Down","P2 Fierce Punch",
                "P2 Jab Punch","P2 Left","P2 Right","P2 Roundhouse Kick",
                "P2 Strong Punch","P2 Up"]

# input dictionary we are sending to BizHawk (True = pressed; False = unpressed)
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
    """
    Function that checks if we need to reload the ROM to the savestate we want. We check when 
    health = 255 since that's when a player has lost.
    """

    if(p1_health == 255 or p2_health == 255):
        return True 
    else:
        return False 


# function for when we want to send
def send_data():
    """
    Function for when we want to send data over the socket server. This will be sent once we generate new actions with 
    the AI (receive state from BizHawk -> use to find next action -> send to BizHawk).
    """
    for key, _ in new_inputs["Game Inputs"].items():
        new_inputs["Game Inputs"][key] = bool(random.getrandbits(1))

    send_inputs = json.dumps(new_inputs)
    main_socket.sendall(send_inputs.encode())


def recv_data():
    """
    Function that will receive data. Just takes the returning inputs from BizHawk to be used for the AI.
    """
    data = main_socket.recv(2048).strip()
    print(f"1: {data}")
    data_str = data.decode("utf-8")
    player_data = json.loads(data_str)
    print(f"P1 Health: {player_data['P1 Health']}, P2 Health: {player_data['P2 Health']}")

    if new_inputs["Control Inputs"]["Reset"]:
        new_inputs["Control Inputs"]["Reset"] = False
    else:
        new_inputs["Control Inputs"]["Reset"] = reset_check(player_data['P1 Health'], player_data['P2 Health'])


# Main loop for the socket server.
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as main_socket:

    # connects to the external tool's socket server.
    main_socket.connect((localhost, port_number))
    
    # infinite send receive loop
    while True:
        
        send_data()        
        
        recv_data()
    
        
