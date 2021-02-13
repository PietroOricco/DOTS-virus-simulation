# Infectious disease simulator with Unity DOTS

INTRODUCTION
------------

2D simulation of a configurable viral pandemic developed under Unity 2019 ECS DOTS Environment.
Each entity replicates the human behaviour, characterized by some primary basic needs such as going to work, at home to rest, to the grocery shop but also social needs like need for a run in a park or visiting friends.
The purpose of the project is to simulate the trend of a viral infection enhancing the parallelism provided by ECS: many entities are spawn at the beginning, the majority is non-infected. The intention is to show how a potential viral infection can spread among the population of entities according both to viral parameters but also on entities behaviour.
<br/>

HOW TO CONFIGURE THE SIMULATION
------------
./Conf/conf.json is the configuration file, the user can set up many parameters for modeling the simulation, such as 
- number of humans i.e. population
- number of initial infects
- time scale i.e. speed-up time factor
- probability of death 
- length (among min and max value) of every state of infection
- map to use (see folder ./Conf/Maps)
- lockdown, set to true to see an implementation of a partial lockdown

<br/>

PERFORMANCES
------------
For avoiding main memory bottleneck, we suggest to execute the simulation with at least 16 GB DRAM and the following parameters less than a cap value:
- numberOfHumans at most 250k-300k
- timeScale at most 60

<br />

HOW TO USE
-----------
- Upper right corner shows stats
- Key "Q" -> Zoom In
- Key "E" -> Zoom Out
- W-A-S-D for xy movement on the map 

<br/> 

ENTITIES
------------
- Humans are represented with a 2d sprite symbol.
  
- The color of the symbol changes with the “state” in which the human is.
  
- The simulation follows the SEIRS model, it means that the contagion follow a state machine of the kind:
	- S susceptible
	- E exposed
	- I infected, modeled as Symptomatic and Asymptomatic
	- R recovered or removed, the entity may also die
	- S susceptible, after some time after recovery, an entity may be infected again
  
- Humans movement based on pathfinding in order to satisfy needs, each time a human needs to satisfy a demand, the pathfinding algorithm drives the human to the closest place where to satisfy  the necessity:<br/>

_ | Home | Park | Pub | Supermarket | Office
:------: | :------: | :------: | :------: | :------: | :------:
  Fatigue | X | | | |
  Hunger | X | | X | |
  Sportivity |  | X | | |
  Sociality |  | X | X | |
  Grocery |  | | | X |
  Work |  | | | | X |

<br/>

CONTAGION
------------
Contagion, probability based formula that keeps into account several parameters such as:
- Distance, 2 meters is the value of risk 
- Time, 15 minutes are enough to make infection happen
- Social Responsibility (that represents several factors about social behaviours during a pandemic, mainly mask usage and social distancing)
<br/>
