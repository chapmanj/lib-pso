# lib-pso
implementation of a particle swarm optimization.

## Motivation 🤓
This library stems from learning about meta-heuristics, specifically pso, in an Adaptive Optimization course at Auburn University.
After becoming familar with the mechanics, I decided to generalize the framework to increase accessability.

## Useage 📖
An example use case Visual Studio console project is included named PSO_Example (with the solution) .
Import the library into your C# project.

 1. Define your encoding.
 
    Generally, the pso is applied to multi-dimensional continuous space problems. 
    The List is n-dimension in size containing the input values.
 
 2. Define your objective function.
 
    This is the value you are attempting to optimize ( maximize / minimize ).
   
 3. Set the search parameters.
 
    These include the bounds on the input values, maximum particle velocity, and  psuedo-random number generator `seed`.
    There are other PSO specific values such as `rho1`, `rho2`, (weights) and `K` that can set. 
   
The returned solution will be the best solution found during the search.

## Future 👨‍💻
 - Use arrays instead of Lists. 
 
   I chose lists to allow for any size of information to be used without the need to specify size.
