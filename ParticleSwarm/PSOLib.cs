using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleSwarmLibrary
{
    public class PSO
    {
        #region class variables
        //the set of particles used in the swarm
        private List<Particle> particles = new List<Particle>();
        
        //the random number generator used in the swarm
        private Random rng { set; get; }

        //the structure of the position vector
        private List<float> positionVectorStructure = new List<float>();

        //the search boundaries for this swarm {(min x, min y, min z)(max x, max y, max z)}
        private List<List<float>> positionBoundaries = new List<List<float>>();

        //the maximum particle velocity vector
        public float maxvelocity { get; private set; }

        //the delegate for the user defined objective function calculation
        Func<List<float>, float> objectivefunctionmethod;

        //the best found objective function value (assuming minmization objective)
        private float globalBestObjectiveFunctionValue = float.MaxValue;

        //the best found position
        private List<float> globalBestPositionVector = new List<float>();

        //cognition parameters
        public float rho1 { get; private set; }
        public float rho2 { get; private set; }
        public float rho { get; private set; }

        //constriction parameter
        public float K { get; private set; }


        #endregion

        public PSO(List<float> Position_Vector_Structure, List<List<float>> Position_Boundaries, float maximum_velocity, Func<List<float>, float> objective_function_method)//Delegate objective_function)
        {
            positionVectorStructure = Position_Vector_Structure.ToList();
            positionBoundaries = Position_Boundaries;
            maxvelocity = maximum_velocity;

            objectivefunctionmethod = objective_function_method;
            
            //these properties are initialized with these default values.
            //If approprite, change these propeties from calling function before executing "Run" 
            rng = new Random();
            rho1 = 2;
            rho2 = 2;
            rho = rho1 + rho2;
            K =  (float)(Math.Abs(2 - rho - (float)Math.Sqrt(Math.Pow(rho, 2) - 4 * (rho))));

            Console.WriteLine("Verbose Output");
            Console.WriteLine("rng={0}\trho1={1}\trho2={2}\trho={3}\tK={4}\tmaxVel={5}", rng.Next(), rho1, rho2, rho, K, maxvelocity);
            Console.Write("pos_struct (");
            for (int i = 0; i < positionVectorStructure.Count; i++)
                Console.Write(positionVectorStructure[i] + " ");
            Console.WriteLine(")");
            Console.Write("boundary: (");
            for (int i = 0; i < positionBoundaries.Count; i++)
                Console.Write(positionBoundaries[i] + " ");
            Console.WriteLine(")");        
        
        }

        #region gets and sets ===
        public void setRhoValues(float Rho1, float Rho2)
        {
            //todo: put some value checks in
            rho1 = Rho1;
            rho2 = Rho2;
        }
        public void setK(bool useK)
        {
            K = useK == true ? K : 1;
        }
        public void setRNGseed(int seed)
        {
            rng = new Random(seed);
        }
        public List<float> getGlobalBestPositionVector()
        {
            return globalBestPositionVector.ToList();
        }
        public List<List<float>> getPositionBoundaries()
        {
            return positionBoundaries.ToList();
        }
        public float getGlobalBestObjectiveFunctionValue()
        {
            return globalBestObjectiveFunctionValue;
        }
        #endregion

        #region methods ===
        public List<float> Run(ulong NumberOfIterations, int NumberOfParticles)
        {
            //make particles
            makeParticles(NumberOfParticles);

            #region main_pso
            Console.WriteLine("running swarm...");
            //run the pso for NumberOfIterations 
            //where each particle is evaluated
            for (ulong i = 0; i < NumberOfIterations; i++)
            {
                    //evaluate particles
                for (int p = 0; p < particles.Count; p++)
                    particles[p].evaluate();
                
                //update global bests value and location
                for (int p = 0; p < particles.Count; p++)
                {
                    if (particles[p].getObjectiveFunctionValue() < globalBestObjectiveFunctionValue)
                    {
                        globalBestObjectiveFunctionValue = particles[p].getObjectiveFunctionValue();

                        globalBestPositionVector = particles[p].getPositionVector().ToList();

                        //optional feedback
                        Console.WriteLine("new best: " + particles[p].ToString());
                    }

                }
            }
            #endregion

            return globalBestPositionVector;
        }

        private void makeParticles(int NumberOfParticles)
        {
            for (int p = 0; p < NumberOfParticles; p++)
            {
                //create and initialize particle
                particles.Add(new Particle(this, p, rng, positionVectorStructure, objectivefunctionmethod));

                //evaluate particles
                float particlesobjectivefunctionvalue = objectivefunctionmethod(particles.Last().getPositionVector());

                //update global bests value and location
                if (particlesobjectivefunctionvalue < globalBestObjectiveFunctionValue)
                {
                    globalBestObjectiveFunctionValue = particlesobjectivefunctionvalue;
                    globalBestPositionVector = particles[(int)p].getPositionVector();
                }

                //feedback
                Console.WriteLine(particles[p].ToString()); ;
            }
        }
        #endregion
    }
    class Particle
    {
        #region class variables
        //to access the swarm and its parameters
        private PSO swarm;

        //this particles identification number
        public int Number { get; private set; }

        //passed in random number generator (all uses will come from the same rng string)
        private Random RNG;
        
        //the position vector is used in the objective function calculation
        private List<float> positionVector;

        //the velocity vector is used to modify the position vector
        private List<float> velocityVector;

        //the value of the objective function calculated with the positionvector 
        private float objectivefunctionvalue;

        //the best found objective function value/ position vector
        public float personalBestObjectiveFunctionValue { get; private set; }
        public List<float> personalBestPositionVector { get; private set; }

        //the delegate reference to the USER defined objective function
        Func<List<float>, float> objective_function;
        #endregion

        public Particle(PSO parent_swarm, int number, Random rng, List<float> position_Vector_Structure, Func<List<float>, float> objective_function_method)
        {
            swarm = parent_swarm;
            Number = number;
            this.RNG = rng;
            
            //copies the position vector structure 
            positionVector = position_Vector_Structure.ToList();

            //throws exception if position vectors and bounds do not work together
            checkPositionAndBoundaryStructure(position_Vector_Structure);

            //to match the swarm.boundaries with the position vectors
            int degree = 0;

            //randomizes a position vector based on the boundaries set in the parent swarm
            for (int p = 0; p < positionVector.Count; p++)
            {
                //random position = min_degree + randomNumber * (max_degree - min_degree)
                positionVector[p] = swarm.getPositionBoundaries()[degree][0]
                    + (float)RNG.NextDouble() * (swarm.getPositionBoundaries()[degree][1]
                    - swarm.getPositionBoundaries()[degree][0]);

                degree = degree < swarm.getPositionBoundaries().Count - 1 ? degree++ : 0;
            }


            //randomizes the velocity vector based on the velocity limitations set in the parent swarm
            velocityVector = new List<float>();
            for(int i = 0; i < positionVector.Count; i++)
                velocityVector.Add((float)RNG.NextDouble() * 2 * parent_swarm.maxvelocity - parent_swarm.maxvelocity);

            //store reference to USER defined objective function value function
            objective_function = objective_function_method;

            //store the current location as the best found 
            //(because this particle has had no other locations)
            personalBestPositionVector = getPositionVector();
            
            //evaluate this particle with the USER defined objective value function
            personalBestObjectiveFunctionValue = objective_function(getPositionVector());

        }

        private void checkPositionAndBoundaryStructure(List<float> position_Vector_Structure)
        {
            if (position_Vector_Structure.Count % swarm.getPositionBoundaries().Count != 0)
                throw new ArgumentException("position vector and search bounds do not match");
        }

        #region gets and sets
        internal List<float> getPositionVector()
        {
            return positionVector.ToList();
        }
        public float getObjectiveFunctionValue()
        {
            return objectivefunctionvalue;
        }
        #endregion

        #region methods
        public float evaluate()
        {
            //to match the swarm.boundaries with the position vectors
            int degree = 0;

            //calculate new velocity and modify position vectors
            for (int p = 0; p < positionVector.Count(); p++)
            {
                //calc new velocity vectors
                velocityVector[p] = (swarm.K * 
                    (velocityVector[p]
                        + swarm.rho1 * (float)RNG.NextDouble() * (personalBestPositionVector[p] - positionVector[p])
                        + swarm.rho2 * (float)RNG.NextDouble() * (swarm.getGlobalBestPositionVector()[p] - positionVector[p])));

                //check velocity magnitude in negative and positive directions
                velocityVector[p] = velocityVector[p] > swarm.maxvelocity ? swarm.maxvelocity : velocityVector[p];
                velocityVector[p] = velocityVector[p] < -swarm.maxvelocity ? -swarm.maxvelocity : velocityVector[p];

                //modify position vector
                positionVector[p] += velocityVector[p];

                //check position bounds 
                //wrap around: if agent is at max, set it to min
                positionVector[p] = positionVector[p] < swarm.getPositionBoundaries()[degree][0] ? swarm.getPositionBoundaries()[degree][1] : positionVector[p];
                positionVector[p] = positionVector[p] > swarm.getPositionBoundaries()[degree][1] ? swarm.getPositionBoundaries()[degree][0] : positionVector[p];

                //set to bound: do not allow 
                //positionVector[p] = positionVector[p] < swarm.getPositionBoundaries()[degree][0] ? swarm.getPositionBoundaries()[degree][0] : positionVector[p];
                //positionVector[p] = positionVector[p] > swarm.getPositionBoundaries()[degree][1] ? swarm.getPositionBoundaries()[degree][1] : positionVector[p];

                degree = degree < swarm.getPositionBoundaries().Count - 1 ? degree++ : 0;
            }
            
            //evaluate objective function defined by user 
            objectivefunctionvalue = objective_function(getPositionVector());

            //update personal best objective function value and position
            if (objectivefunctionvalue < personalBestObjectiveFunctionValue)
            {
                personalBestObjectiveFunctionValue = objectivefunctionvalue;
                personalBestPositionVector = getPositionVector();
                
            }

            //return the current objective function value
            return objectivefunctionvalue;
        }
        public override string ToString()
        {
            string tostring = Number + " ( ";
            for (int i = 0; i < positionVector.Count; i++)
                tostring += positionVector[i] + " ";
            tostring += " )< ";
            for (int i = 0; i < velocityVector.Count; i++)
                tostring += velocityVector[i] + " ";
            tostring += " >= " + objectivefunctionvalue + "\r";
            return tostring;
        }
        #endregion
    }

}
