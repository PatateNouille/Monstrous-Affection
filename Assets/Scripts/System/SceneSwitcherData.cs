using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loaded Singleton/Scene Switcher Data")]
public class SceneSwitcherData : ScriptableObject, Utility.ISingletonData<SceneSwitcher, SceneSwitcherData>
{
    public List<Utility.SceneReference> scenes = null;
}
