using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoBubble : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI title = null;

    [SerializeField]
    GameObject input = null;

    [SerializeField]
    GameObject output = null;

    public void SetTitleFromRecipe(Factory.Recipe recipe)
    {
        if (recipe == null)
        {
            title.text = "No Recipe";
        }
        else
        {
            title.text = recipe.name;
        }
    }

    public void SetFromRecipe(Factory.Recipe recipe)
    {
        SetTitleFromRecipe(recipe);
        
        UI.MakeItemInfos(input, recipe?.input);
        UI.MakeItemInfos(output, recipe?.output);
    }

    public void SetFromRecipeInventory(Factory.Recipe recipe, Inventory inventory)
    {
        SetTitleFromRecipe(recipe);

        UI.MakeItemInfos(input, recipe?.input.Select(info => (info.name, info.count, (uint?)inventory.Count(info.name))).ToList());
        UI.MakeItemInfos(output, recipe?.output);
    }
}
