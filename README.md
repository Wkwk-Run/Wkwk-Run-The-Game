<!-- PROJECT LOGO -->
<div align="center">
    <img src="https://github.com/Wkwk-Run/Wkwk-Run-The-Game/blob/main/Images/logo.png"
 alt="Logo" width="200" height="200">
</div>
<br/>

<p align="center">
<img alt="GitHub contributors" src="https://img.shields.io/github/contributors/Wkwk-Run/Wkwk-Run-The-Game">
    </p>
   

<p align="center">
‘Maju. Bermain bersama, lari dan menertawakannya’ <br/>
It means 'Go forward, play together, run and laugh at it'. <br/>
WKWK Run is a multi-player game android-based platform compatible to play on mobile devices.
    </p>
<br/>

<img src=https://github.com/Wkwk-Run/Wkwk-Run-The-Game/blob/main/Images/All%20conto.png>

# About
WKWK RUN is a multiplayer game that can be played by up to 5 players. The game is intended for children aged over 13 years (IGRS 13+). By taking the theme of iconic and popular things in Indonesia, such as Bocil (kids), Mak Mak, and other unique phenomena. Using plots of iconic places in Indonesia such as Borobudur, Traditional house, and others. The ultimate goal of this game is to promote unique things and hype that only exists in Indonesia.

# How To Play
Players can choose 2 modes of play options. 
- Random play mode
    - If the player chooses the Random play mode, the system will automatically pair randomly for the players in the game set. 
- Play With Friends mode
    - If the player chooses the Play With Friends mode, the player will be given a code by the system.
    - Then other player can join the room with the code 
<p>
The gameplay in the WKWK RUN game requires players to race with other players to be able to get 1st, 2nd, or 3rd Place at the Finish line. Avoid obstacles that can make players delay in playing the game to obstacles that make players die.
</p>

# Game Design Document
<p>
https://docs.google.com/document/d/1j6ov218TL7z7kKG_mCwD1cUjjYhOghIIkOSAiI2kOkA/edit?usp=sharing{/google_docs}
</p>

# Trello
https://trello.com/b/R9qKY1gA/wpg-5

# Network Flow Documentation
This game uses homemade framework with .NET, see [Server](https://github.com/Wkwk-Run/Wkwk-Run-The-Game/tree/main/Wkwk-Server) and [Client](https://github.com/Wkwk-Run/Wkwk-Run-The-Game/blob/main/WkWk-Run_Unity-Project/Assets/Script/General/Client.cs)<br/>
The connection process is carried out from verification, here using 2 pieces of encryption (symmetry and asymmetry). After the verification process is complete, the normal communication between the user and the server can run.
<br\>
<h3 align="center">
    User Verification / Login
    </h3>
<div align="center">
<img src=https://github.com/Wkwk-Run/Wkwk-Run-The-Game/blob/main/Images/Verification_Login.jpg>
    </div>

<h3 align="center">
    User Normal Communication
    </h3>
<div align="center">
<img src=https://github.com/Wkwk-Run/Wkwk-Run-The-Game/blob/main/Images/Normal%20Communication.jpg>
    </div>
