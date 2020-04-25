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

    public void SetFromRecipe(Factory.Recipe recipe)
    {
        if (recipe == null)
        {
            title.text = "Out Of Order";
        }
        else
        {
            title.text = recipe.name;
        }

        UI.MakeItemInfos(input, recipe?.input);
        UI.MakeItemInfos(output, recipe?.output);
    }
}
