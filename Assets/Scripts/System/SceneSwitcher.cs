using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : Utility.LoadedSingleton<SceneSwitcher, SceneSwitcherData>
{
    Rocket rocket = null;

    public int this[string name]
    {
        get => data.scenes.FirstOrDefault(s => s.name == name)?.buildIndex ?? -1;
    }

    void LoadAt(int index)
    {
        if (index < 0 || index >= data.scenes.Count || data.scenes[index].buildIndex == -1)
        {
            Debug.LogError("Tried to load unbinded scene");
            return;
        }
    }

    public void SetRocket(Rocket _rocket)
    {
        rocket = _rocket;
    }

    public void LoadScene(int buildIndex, bool async = true)
    {
        if (async) StartCoroutine(AsyncLoad(buildIndex));
        else
        {
            SceneManager.LoadScene(buildIndex);
        }
    }

    IEnumerator AsyncLoad(int buildIndex)
    {
        Planet.Instance.useAsUniqueInstance = false;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

        while (!loadOp.isDone || (rocket != null && !rocket.HighEnough))
        {
            yield return null;
        }

        Scene src = SceneManager.GetActiveScene();
        Scene dest = SceneManager.GetSceneAt(1);

        SceneManager.SetActiveScene(dest);
        Game.Instance.PopulatePlanet();

        Player.Instance.interaction.targets.Clear();

        SceneManager.MoveGameObjectToScene(MainCamera.Instance.gameObject, dest);
        MainCamera.Instance.UpdateFogSettings();

        if (rocket != null)
        {
            SceneManager.MoveGameObjectToScene(rocket.gameObject, dest);
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(src);

        while (!unloadOp.isDone)
        {
            yield return null;
        }

        Player.Instance.interaction.targets.Clear();

        if (rocket != null)
        {
            rocket.Land();

            rocket = null;
        }
    }
}
