using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Factory))]
public class ConsumeRecipes : MonoBehaviour
{
    Factory factory = null;

    private void Start()
    {
        factory = GetComponent<Factory>();

        factory.onRecipeCrafted += ConsumeRecipe;
    }

    private void OnDestroy()
    {
        if (factory != null)
        {
            factory.onRecipeCrafted -= ConsumeRecipe;
        }
    }

    void ConsumeRecipe(Factory.Recipe recipe, int index)
    {
        factory.recipes.RemoveAt(index);

        factory.SetSelectedRecipe(index);
    }
}
