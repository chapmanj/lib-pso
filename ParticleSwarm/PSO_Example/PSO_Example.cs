using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleSwarmLibrary;

namespace PSO_Example
{
    class PSO_Example
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing PSO");

            //defines the number of variables needed
            //here, {x,y}
            //if more are needed, add them to the testPositionVector
            //ex. {x1, y1, z1, x2, y2, z2, ...}
            List<float> testPositionVector = new List<float>() { 0, 0 };

            //defines the boundaries for each variable in an NxM dimensional list of lists
            //ex. {{xmin, xmax}, { ymin, ymax}}
            List<List<float>> testPositionBoundaries = new List<List<float>>(){
                new List<float>(){ -3f, 3f}, 
                new List<float>() { -2f, 2f }};

            //defines the maximum move rate of the particles in the swarm
            //choose wisely so that particles do not move too quickly around the solution space
            float maximumparticlevelocity = 0.0001f;

            //create and initilize the swarm
            //define the objective function based on list like the previously defined testPositionVector
            //here refer to the testObjective method
            PSO mySwarm = new PSO(testPositionVector, testPositionBoundaries, maximumparticlevelocity, testObjective);

            //define a seed based on the computers current time
            //to use a unique seed each run and record it use the following line.
            //(int)DateTime.Now.Ticks & 0x0000FFFF;
            int seed = 3611;// 10210;

            //reset the swarms random number generator; used for repeatability
            //here defined above
            mySwarm.setRNGseed(seed);

            //reset rho values tht balance personal and population cognition
            //here, more weight is placed on personal cognition
            //rho1 + rho2 should sum to 4 if using constriction (K)
            //uncomment
            //mySwarm.setRhoValues(2.25f, 1.75f); 

            //use constriction parameter (K) to modify amount of modification of velocity updates
            //uncomment
            //mySwarm.setK(true);

            //run your swarm for some number of iterations with some number of particles
            List<float> output = mySwarm.Run(100000, 10);//minimize

            Console.Write("Best found solution: (");
            for (int i = 0; i < output.Count; i++)
                Console.Write(mySwarm.getGlobalBestPositionVector()[i] + " ");
            Console.Write(") " + mySwarm.getGlobalBestObjectiveFunctionValue());
            Console.Write("\tseed= " + seed);

            Console.WriteLine("\r\n\r\nglobal min = (0.094262, -0.71509) -1.031514");

            Console.ReadKey();  //wait for input
        }

        static float testObjective(List<float> positionvector)
        {
            //This is based on the six hump camelback function with two decision variables 
            //where x lies between +-3 and y lies between +-2.
            //The objective is to minimize z.  
            //The global minimum is -1.031514 @ (0.094262, -0.71509)

            return (float)((4f - 2.1f
                * Math.Pow(positionvector[0], 2) + (Math.Pow(positionvector[0], 4) / 3))
                * Math.Pow(positionvector[0], 2) + positionvector[0]
                * positionvector[1] + (-4f + 4f
                * Math.Pow(positionvector[1], 2))
                * Math.Pow(positionvector[1], 2)
                //-1
                );
        }

    }
}
