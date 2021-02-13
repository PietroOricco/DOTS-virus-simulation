# Infectious disease simulator with Unity DOTS

Features

- Display on screen multiple “humans”
	- Humans are represented with a 2d sprite symbol.
	- The color of the symbol changes with the “state” in which the human is.
	- The state could be "susceptible" -> “infected” -> ("asymptomatic" || "symptomatic") -> (“healed” || ”dead”)

- Display a map -> autogenerated/fixed? Starting from a manually defined set of cells that are repeated randomly to form the map
	- Places: Houses, offices, parks, supermarkets, pubs/restaurants
	- Places are connected by streets
	- Humans are spawned randomly, they have one home

- Humans movement based on different factors -> pathfinding:
	Needs: hunger -> supermarket, social interactions -> pub , sport -> park, work -> offices, fatigue -> home

- Contagion, probability based formula based on different factors (where to find it/how to generate the formula?):
	- Distance
	- Time
	- Behavior
		- Mask 
		- social distancing (0-> collision, 1-> huge distance)
		- infected social responsibility (quarantine respect)
	- Health -> (“healed”/”dead”)/factor in contagion probability 
	- Inside/outside -> how to find a formula?

- Display graphs on contagion status and stats
	Contagion vs time

