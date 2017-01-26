using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a static randomizer class. It is needed because if the enemies are created
//simultaneously, their RNG values would be exactly the same because System.Random
//uses the system's current time as its seed number.
public class RandomNumberGenerator
{

    private static System.Random random;


    static RandomNumberGenerator()
    {
        random = new System.Random();
    }

    public static int NextRandom(int min, int max)
    {
        lock (random)
        {
            return random.Next(min, max);
        }
    }

    public static int NextRandom(int i)
    {
        lock (random)
        {
            return random.Next(i);
        }
    }

}
