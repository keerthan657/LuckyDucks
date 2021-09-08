using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duck
{
    public Vector2 pos, vel, acc;
    public Transform duckTransform;
    public DNA dna;
    public float fitness;
    public int cnt;
    public bool completed, drowned;
    public GameObject gameobject;

    public static bool generationEnded;

    public Duck(Vector2 pos, Vector2 vel, Vector2 acc, Transform duckTransform, int numOfMovements, GameObject gameobject)
    {
        this.pos = pos;
        this.vel = vel;
        this.acc = acc;
        this.duckTransform = duckTransform;
        this.dna = new DNA(numOfMovements);
        this.fitness = 0f;
        this.cnt = 0;
        this.completed = false;
        this.drowned = false;
        this.gameobject = gameobject;
    }
    static Duck()
    {
        generationEnded = false;
    }

    public void addForce(Vector2 force)
    {
        this.acc += force;
    }

    public void update()
    {
        if(!generationEnded) {
            if(this.cnt<this.dna.numOfMovements) {
                this.addForce(this.dna.genes[this.cnt]);
                this.cnt++;
            }
            if(this.cnt==this.dna.numOfMovements)
            {
                // end of generation
                Debug.Log("Generation ended");

                // step1: calculate fitness of each duck
                Controller.evaluate();
                // step2: create mating pool for parents to be picked - selection
                Controller.createMatingPool();
                // step3: create evolved dna - crossover & mutation
                Controller.crossoverDNAs();
                // step4: reset the ducks' positions and velocities and acc
                Controller.resetPopulation();
                // step4 continued: restart generation
                Controller.restartGeneration();
            }

            if(!this.completed && !this.drowned) {
                this.vel += this.acc;
                this.pos += this.vel;

                this.acc *= Vector2.zero;

                duckTransform.position = pos;
            }
        }
    }
}