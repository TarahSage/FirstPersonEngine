# First Person Engine
This project is an open-source, first-person shooter framework, written in C# for Unity.
The repository is structured as a Unity project, and there are example levels included for demonstrating the gameplay components.

# Installation and Setup
Setup should be minimal. Clone this repo and place it in a folder of your choosing. You will need Unity 2020.1.5f1 or newer.
Then, add the project in Unity Hub, and import the project.

# Code Overview
The code is structured in the \Assets\Scripts folder. There are two major groups of code structure; "Components" and "Behaviors". These are contained in their respective folders.

Components represent a systemic, predictable API for interacting with the framework. These are meant to be added to Unity GameObjects, and to have functions called on them to perform behavior. A gameobject might have many different components on it. For example, a Gun Component might have a Shoot() function, and when called, will shoot a bullet into the world. These components also have data on them.

Behaviors are less restrictive, and represent the open ended nature of video game scripting. They are meant to be a bridge between the more rigid, systemic components, and the variety of actions that occur in games. For example, an Enemy Behavior might be responsible for playing animations, making sounds, and firing a gun (All of which are components, and therefore systemic). Behaviors often make a variety of calls to components to accomplish their tasks. Usually, only one of these is added to a given GameObject, and the naming of these behaviors is relevant to the specific behavior it implements. For example, a Sniper Enemy might have a SniperEnemyBehavior, and it's only used for them.

# Current Feature List
Shooting Components, used for shooting a bullet and dealing damage to damageables
* Bullet
* Damageable
* Gun
* Gun Data
* Zoomable Gun
* Flat Animated Gun
* Gun Selection

Player Components, for managing the first person player
* First Person Player
* Player Respawn Volume

Billboard Sprites Components, used for 2D sprites in 3d space, much like Doom or Wolfenstein 3D.
* Material Animation
* Rotatable
* Mesh Bounds

Enemy Components, used for managing enemies and their actions
* Enemy Manager
* Enemy Behavior
* Sprite Enemy Behavior
* Attack Token
* Bark

Level Components, used for level loading and lighting changes
* Level Manager
* Level Trigger
* Level Lighting Volume

Sound Components, used for managing the sounds within the game
* Sound Manager
* Sound
* Ambient Sound

Libraries, for helping with general tasks
* Custom Math
* Localizer
* Timer

# Future Work
* Clean up the TODOs
* Implement 3D variants for all the sprite-based components (flat animated gun, and example 3d enemy)
* Settings Manager system
* Save slots
* Enemy Corpses and Barks
* Player footsteps and Barks
