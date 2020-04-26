using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Game : UniqueInstance<Game>
{
    [SerializeField]
    public bool isMenu = false;

    [SerializeField]
    bool populatePlanet = true;

    [SerializeField]
    public Monster monster = null;

    [SerializeField]
    public Rocket rocket = null;

    [SerializeField]
    public List<string> startingStuff = null;

    [HideInInspector]
    Planet curPlanet = null;
    public Planet CurPlanet 
    {
        get
        { 
            if (curPlanet == null)
            {
                curPlanet = Planet.Instance;
            }

            return curPlanet;
        }
    }

    PlanetGenData[] planetGens = null;

    bool tutorialCompleted = false;

    void Start()
    {
        planetGens = Resources.LoadAll<PlanetGenData>("Generation/");

        tutorialCompleted = PlayerPrefs.GetInt("Built Rocket") == 1;

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        if (isMenu)
        {
            Rocket rocket = FindObjectOfType<RocketPad>().CraftRocket();

            rocket.AddFuel(rocket.fuelCapacity);

            UI.Instance.rocketProgress.SetProgress(1f);
        }
    }

    void Update()
    {
        
    }

    public void PopulatePlanet()
    {
        if (!populatePlanet) return;
        
        CurPlanet.Populate(planetGens[Random.Range(0, planetGens.Length)]);
    }

    public void Launch()
    {
        SceneSwitcher.Instance.LoadScene(SceneSwitcher.Instance["Game"]);

        Destroy(gameObject);
    }

    public void Lose()
    {
        SceneSwitcher.Instance.LoadScene(SceneSwitcher.Instance["Menu"], false);
    }
}
