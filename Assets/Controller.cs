using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Controller : MonoBehaviour
{
    public GameObject duckPrefab;
    public GameObject finish;
    public GameObject obstacle;
    private static Transform finishTransform;

    // variable to change total number of ducks spawned
    public static int totalDucks = 200;
    public static Duck[] ducks = new Duck[totalDucks];

    // variable for lifespan & mutation-rate
    public static int numOfMovements = 500;
    public static float mutationRate;

    private static DNA[] matingPool;
    private static System.Random random;
    public static bool mutationEnabled = true;

    // to enable'disable mutation via editor
    public bool setMutationEnabled = true;

    public static int generationCount;

    public TMP_Text infoText, labelText;
    public Toggle toggleInput;
    public Slider lifeSpanSlider;

    void Start()
    {
        finishTransform = finish.GetComponent<Transform>();
        random = new System.Random();

        mutationRate = 0.001f;
        generationCount = 1;

        // generate some num of ducks
        for(int i=0; i<totalDucks; i++)
        {
            // for each duck, instantiate it
            GameObject duckObj = Instantiate(duckPrefab, transform.position, Quaternion.identity);
            // get it's transform
            Transform duckTransform = duckObj.GetComponent<Transform>();
            // store it in array after creating new object - give it random velocity at first
            ducks[i] = new Duck(new Vector2(transform.position.x, transform.position.y),
                         Vector2.zero, Vector2.zero, duckTransform, numOfMovements, duckObj);
        }
    }

    void Update()
    {
        updateInfoText();
        updateLabelText();
        mutationEnabled = setMutationEnabled;

        // update each duck
        for(int i=0; i<totalDucks; i++)
        {
            ducks[i].update();

            float dist1 = Vector2.Distance(ducks[i].duckTransform.position, finishTransform.position);
            if(dist1<0.5f)
            {
                ducks[i].completed = true;
                // ducks[i].gameobject.GetComponent<SpriteRenderer>().color = Color.green;
            }
            float dist2 = Vector2.Distance(ducks[i].duckTransform.position, obstacle.GetComponent<Transform>().position);
            if(dist2<0.5f)
            {
                ducks[i].drowned = true;
                // ducks[i].gameobject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    public void onRestartButtonPress()
    {
        int lifeSpan = (int)lifeSpanSlider.value;
        numOfMovements = lifeSpan;
        print("Restarting");

        // clear previous sprites
        for(int i=0; i<totalDucks; i++)
        {
            Destroy(ducks[i].gameobject);
        }
        // start all-over again
        Start();
    }

    public void updateLabelText()
    {
        if(toggleInput.isOn)
        {
            setMutationEnabled = true;
        }
        else
        {
            setMutationEnabled = false;
        }

        string myText = "Mutation\nLifespan: "+numOfMovements;
        labelText.text = myText;
    }

    public void updateInfoText()
    {
        string myText = "Generation: "+generationCount+"\nNum of Ducks: "+totalDucks;
        infoText.text = myText;
    }

    public static Vector2 GetRandV2(float speed, float spread)
    {
        // spread on both directions
        float angle = Random.Range(180-spread, 180+spread);

        if(Random.Range(0f,1f) > 0.5f)
        {
            angle = (angle+180)%360;
        }

        // print(angle);

        float x = Mathf.Cos(Mathf.Deg2Rad * angle) * speed;
        float y = Mathf.Sin(Mathf.Deg2Rad * angle) * speed;

        return(new Vector2(x, y));
    }

    public static void evaluate()
    {
        // print("Generation: "+generationCount);
        generationCount++;

        float maxFitness = ducks[0].fitness;
        for(int i=0; i<totalDucks; i++)
        {
            Duck.generationEnded = true;
        }
        for(int i=0; i<totalDucks; i++)
        {
            float tempFitness = calcFitness(ducks[i]);
            if(tempFitness > maxFitness)
            {
                maxFitness = tempFitness;
            }
        }
        // normalize the fitness values
        for(int i=0; i<totalDucks; i++)
        {
            ducks[i].fitness /= maxFitness;
        }
        // reward those ducks who reached finish & punish those who drowned
        for(int i=0; i<totalDucks; i++)
        {
            if(ducks[i].completed==true)
            {
                ducks[i].fitness *= 3;
            }
            if(ducks[i].drowned==true)
            {
                ducks[i].fitness /= 3;
            }
        }
    }

    public static float calcFitness(Duck duck)
    {
        float dist = Vector2.Distance(duck.duckTransform.position, finishTransform.position);
        duck.fitness = 1/dist;
        return duck.fitness;
    }

    public static void resetPopulation()
    {
        for(int i=0; i<totalDucks; i++)
        {
            ducks[i].vel = ducks[i].pos = ducks[i].acc = Vector2.zero;
            ducks[i].duckTransform.position = Vector2.zero;
        }
    }

    public static void createMatingPool()
    {
        // print("Creating mating pool");

        // have a pool of genes -> with more fitness ones more and less fitness ones less
        // make that array global, then can be accessible by crossoverDNAs()

        List<DNA> matingPoolList = new List<DNA>();

        for(int i=0; i<totalDucks; i++)
        {
            int n = (int)(ducks[i].fitness * 100);
            for(int j=0; j<n; j++)
            {
                matingPoolList.Add(ducks[i].dna);
            }
        }
        matingPool = matingPoolList.ToArray();

        /* Initial approach -> some values were null
        matingPool = new DNA[numOfMovements];
        // populate matingPool
        int j=0;
        while(j!=numOfMovements)
        {
            for(int i=0; i<totalDucks; i++)
            {
                float currFitness = ducks[i].fitness;
                // if this duck has high fitness, have a greater chance of selecting it in mating-pool
                if(currFitness>0.5f)
                {
                    if(Random.value > 0.2f)
                    {
                        matingPool[j] = ducks[i].dna;
                    }
                }
                // else, have a lesser chance of selecting
                else
                {
                    if(Random.value > 0.9f)
                    {
                        matingPool[j] = ducks[i].dna;
                    }
                }
                j=j+1;
            }
        }
        */
    }

    public static void crossoverDNAs()
    {
        // print("Crossing over");

        // for all ducks, pick two random parents from mating-pool
        // do crossover of parent's genes and put new_gene to that duck
        for(int i=0; i<totalDucks; i++)
        {
            // pick random parents
            DNA DNA1 = matingPool[random.Next(matingPool.Length)];//(int)(Random.value * matingPool.Length)];
            DNA DNA2 = matingPool[random.Next(matingPool.Length)];//(int)(Random.value * matingPool.Length)];
            // crossover - at alternate positions
            // i.e, pick one from DNA1, other from DNA2, and alternating
            DNA evolvedDNA = new DNA(numOfMovements);
            // print("For duck"+i+", DNA1="+DNA1+", DNA2="+DNA2+", ev="+evolvedDNA);

            for(int j=0; j<numOfMovements; j++)
            {
                //print("doing for: "+j);
                evolvedDNA.genes[j] = (j%2==0)?(DNA1.genes[j]):(DNA2.genes[j]);
            }
            // update to that evolved DNA
            ducks[i].dna = evolvedDNA;

            // mutate duck's DNA
            if(mutationEnabled)
            {
                mutate(ducks[i].dna);
            }
        }
    }

    public static void mutate(DNA dna)
    {
        // mutationRate% of dna's genes should be mutated (randomized)
        Vector2[] mutatedGenes = dna.genes;

        for(int i=0; i<mutatedGenes.Length; i++)
        {
            if(Random.value < mutationRate)
            {
                mutatedGenes[i] = GetRandV2(0.5f, 180);
                mutatedGenes[i] *= 0.001f;
                print("mutated");
            }
        }
    }

    public static void restartGeneration()
    {
        for(int i=0; i<totalDucks; i++)
        {
            ducks[i].cnt = 0;
            ducks[i].completed = false;
            ducks[i].drowned = false;
        }
        Duck.generationEnded = false;
    }
}
