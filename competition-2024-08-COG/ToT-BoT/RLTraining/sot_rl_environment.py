import numpy as np

import gymnasium as gym
from gymnasium import spaces
import json
import socket
import subprocess
import random
import pickle
import os 
import signal
import time
import psutil
import struct


class TalesOfTribute(gym.Env):
    metadata = {"render_modes": ["text"]}

    def __init__(self, render_mode=None, size=5):

        # Sockets are connections between computers (or the same computer) that allow for communication in between programs
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # In case your local firewall blocks some ports, replace 12345 with the port you want to bind to
        # if you change the port here, it will also need to be changed in rl-bridge.py
        server_address = ('localhost', 12345)
        print('starting up on %s port %s' % server_address)
        self.sock.bind(server_address)
        
        self.process = None
        self.connection = None

        # In a next we would need to encode the observation/state and the action space.
        # The problem: each action space differs in size depending on the current cards on the board.
        # This is why we just encode a weight vector to evaluate actions.
        # This makes the output size consistent which should be much
        # easier for the agent to learn.

        # Define the observation space for floating-point numbers
        # The shape argument specifies the shape of the array, which in your case is 14075
        # TODO put the minimal and maximal value of the state observation here
        low_bound = -100  # Replace with the actual lower bound for your numbers, if known
        high_bound = 100  # Replace with the actual upper bound for your numbers, if known
        self.observation_space = spaces.Box(low=low_bound, high=high_bound, shape=(14079,), dtype=np.float32)        
        self.last_observation = None

        # Out of the selection, we return a weighting vector
        # TODO put the minimal and maximal heuristic value here
        self.action_space = spaces.Box(low=-10, high=150, shape=(105,))


        assert render_mode is None or render_mode in self.metadata["render_modes"]
        self.render_mode = render_mode

    # I aimed to provide you some little helper functions
    # for sending and receiving information between the agent and the environment
    def receive_data_from_agent(self):
        # First, receive the length of the incoming pickle data (4 bytes for 'I' format)
        length_bytes = self.connection.recv(4)
        message_length = struct.unpack('!I', length_bytes)[0]

        # Now that you know the length, receive the rest of the message
        message = b''
        while len(message) < message_length:
            part = self.connection.recv(message_length - len(message))
            if not part:
                raise Exception("Connection closed or error")
            message += part

        # Unpickle the complete message
        return pickle.loads(message)

    def send_data_to_agent(self, data):
        # Serialize your data
        data = pickle.dumps(data)

        #print("pickle message:", data, len(data))
        # Prefix the message with its length, packed into 4 bytes
        prefixed_data = struct.pack('!I', len(data)) + data

        # Send the prefixed message
        self.connection.sendall(prefixed_data)
        
    def step(self, action):
        # each step, there will be a common communication pattern between the agent and the environment
        # first the two patrons will be exchanged between the agent and the environment
        # after that, the agent will receive the game state and will send an action back to the environment
        # Here, I implemented to randomly choose the patrons, such that the agent learns to use all of them.
        # This could later be replaced to a more clever selection of patrons for maximizing performance.

        #print("Send action: ", action)
        self.send_data_to_agent(action)

        # get the next observation for the RL agent to process
        decoded_message = self.receive_data_from_agent()
        #print("Received message: ", decoded_message[0])
        if decoded_message[0] == "Gamestate":
            observation = decoded_message[1]
            reward = decoded_message[2]
            terminated = False      # has the game been ended due to the game rules

        else:
            observation = self.last_observation
            reward = decoded_message[1] * 2     # at the end of the game, we give a reward of 1 for winning and 0 for losing. ##UPDATED!!!! Bas changed this to 2 for winning, 0 for losing.
            terminated = True               # has the game been ended due to the game rules

        #print("Received observation: ", observation.shape)

        truncated = False       # has the game been ended due to external circumstances, e.g. time limit reached
        info = dict()           # storing additional information of the current game state or run

        self.last_observation = observation
        return observation, reward, terminated, truncated, info    
    
    def reset(self, seed=None, options=None):
        #print("reset")

        # We need the following line to seed self.np_random
        super().reset(seed=seed)


        if self.process is not None and psutil.pid_exists(self.process.pid):
            # Check if the process has not already ended
            if self.process.poll() is None:
                try:
                    # Try to terminate the process
                    self.process.terminate()
                    # Wait for the process to terminate
                    self.process.wait(timeout=5)
                except Exception as e:
                    pass
                    #print(f"Error while terminating process: {e}")

        # Close the socket connection
        if self.connection is not None:
            try:
                self.connection.close()
            except Exception as e:
                pass
                #print(f"Error while closing connection: {e}")


        self.sock.listen()
        # Note from the organizers: This part of the code has been commented out. The process is initiated during clashes with other bots in the run.sh script
        '''
        # create a game using the commandline. #TODO: The link in the next line needs to be changed to your path setup
        self.process = subprocess.Popen(['C:\\Users\\katar\\Downloads\\ToT-BoT 2\\Competition Submission\\SoT_RL_WORKING\\ScriptsOfTribute-Core-master\\GameRunner\\bin\\Release\\net7.0\\GameRunner',
                          "cmd:python ../../../../Bots/ExternalLanguageBotsUtils/Python/rl-bridge.py",
                          "MaxPrestigeBot",
                          "-n", "1", 
                          "-t", "1"], 
                          cwd="C:\\Users\\katar\\Downloads\\ToT-BoT 2\\Competition Submission\\SoT_RL_WORKING\\ScriptsOfTribute-Core-master\\GameRunner\\bin\\Release\\net7.0\\")
        '''
        # Wait for a connection, the python agent that takes part in the game will try to connect to this program
        #print('waiting for a connection')
        self.connection, client_address = self.sock.accept()
        #print('connection from', client_address)

        # get the next observation for the RL agent to process
        observation = self.receive_data_from_agent()[1]
        #print("Received observation: ", observation.shape)

        info = None
        self.last_observation = observation
        return observation, info
    
    def render(self):
        if self.render_mode == "test":
            return str(self.last_observation())
        
    
    def close(self):
        self.sock.close()
        if self.process is not None:
            os.kill(self.process.pid, signal.SIGTERM)  # Send the signal to all the process groups


if __name__ == "__main__":
    from stable_baselines3 import PPO

    direct = f"Junk Models"
    date = '7.19.2024'
    
    # Note from the organizers: This part of the code has been changed to fix the paths.
    models_dir = os.path.join(os.path.dirname(__file__), f"Models\\{direct}")
    logdir = os.path.join(os.path.dirname(__file__), f"Logs\\{direct}")

    if not os.path.exists(models_dir):
        os.makedirs(models_dir)
    if not os.path.exists(logdir):
        os.makedirs(logdir)

    env = TalesOfTribute()
    env.reset()

    #model = PPO("MlpPolicy", env, verbose=1, tensorboard_log=logdir)
    model = PPO.load(os.path.join(os.path.dirname(__file__), '4070_training_iterations'), env)

    TIMESTEPS = np.inf
    iterations = 0

    while True:
        iterations += 1
        model.learn(total_timesteps=TIMESTEPS, reset_num_timesteps=False, tb_log_name=f"PPO")
        model.save(f'{models_dir}/{date}/{iterations}_training_iterations')
