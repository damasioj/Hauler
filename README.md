# Master's Thesis: Hauler Simulation

This project is being developed for a Master's thesis that studies the implementation of behaviours in subjects using Machine Learning. This scenario attempts to simulate an agent (hauler) that must physically move objects within the scene.

The agent uses ray casting to perceive its environment and must avoid obstacles. For every trial, the obstacles are repositioned. The objective is for the agent to "push" the target object to the goal without hitting boundaries.

# Unity ML-Agents SDK

The project uses Unity ML-Agents to train agents.

# Goals

The following are the goals of this project:

- [x] Move the object to the target location
- [ ] Move objects with differing mass and dimensions (1 object / trial)
- [ ] Move multiple objects to the goal

# Requirements

To run this environment, you will need:
- Python 3.6.1 or greater
	- mlagents version 0.16.1
- Unity 2019.2.0f1 or greater

# Current limitations / issues

## Object dimension information

Ideally we would like to provide the agent with the object's dimensions, but this seems to be more complicated than initially thought. The dimensions of a box, for instance, can be defined with 8 given vectors (1 vector for each point), but if we change the target object to a ball the number of vectors can be virtually infinite. 

One of the better proposed solutions to these types of problems is by using multi-model reinforcement learning (MMRL). For this project it's unlikely that this will be used due to the lack of time. Another possible solution is creating an enum that will represent the different object shapes. The enum would obviously be very limiting, and adding a new shape would require retraining, but it would at least provide a few options to work with.