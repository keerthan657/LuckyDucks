using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA
{
    public Vector2[] genes;
    public int numOfMovements;

    public DNA(int numOfMovements)
    {
        this.numOfMovements = numOfMovements;
        this.genes = new Vector2[numOfMovements];

        // initially given random values
        for(int i=0; i<numOfMovements; i++)
        {
            this.genes[i] = Controller.GetRandV2(0.5f, 180);
            this.genes[i] *= 0.001f;
        }
    }

    public void setGenePos(Vector2 x, int pos)
    {
        if(pos < this.numOfMovements)
        {
            this.genes[pos] = x;
        }
    }
}
