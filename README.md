# Procedural game environment for training player skills

***
### Controls
* Move: Left/Right
* Crouch: Down
* Jump: Z
* Dash: Hold X while moving/jumping
* Fire: X
* Pause/Unpause game: Enter/return


***
### Structure
PCG Folders contain scripts or prefabs regarding the player modeling and procedural content generation of the levels.
Scripts/Mario contains the game logic
Scripts/Procedural genearation Contains level generation and player modeling logic
  - Models data
  - Controllers interact on the data
  - Handlers Handle actions executed from the code
  - Event manager used for handling game events
Scripts/Firebase contains database logic

Wanting to learn someone to play a video-game is tightly tight to the concept of literacy, which means the competence or knowledge in a specified area. In video-games, we call this game literacy, which means we want people to increase their skill in playing video games. The common way of teaching a user game literacy is through the use of tutorial's which teaches the player the base mechanics of the game. Tutorials in most situations are created by designers which forces players to complete some simple tasks in a certain order that learn them the base mechanics of the game. After completing the tutorial, the player plays through a series of levels, in which slowly the difficulty of the game scales up. The problem with this approach is that we have players of all kind of different skill levels in playing video games. We have to take the different skill levels of player's into account. If we do not take the different skill levels of players into account, they might quite the game because of boredom or being too frustrated because of not making any progress in the game. To solve this problem, we propose a method that adaptively changes the generation of game levels based on the player's performance in the game and try to target the difficulty of the levels based on the player's skill in playing video games. Our method focuses on generating game levels with game mechanics the user needs improvement on. Results show that our method is enabled to train people in their game literacy better than a baseline adaptive environment.
