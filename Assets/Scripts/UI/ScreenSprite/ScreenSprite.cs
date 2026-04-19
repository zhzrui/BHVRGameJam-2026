using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreenSprite : MonoBehaviour
{
    [SerializeField] private List<ScreenLayer> screenLayers = new();
    [SerializeField] GameObject main;

    static Dictionary<string, string[]> shortcuts = new Dictionary<string, string[]>() {
        ["stare"] = new string[] {"hands hide", "mouth kind", "eyes normal"},
        ["hungry"] = new string[] {"hands cutlery", "mouth drool", "eyes squint"},
        ["speak"] = new string[] {"mouth speak", "eyes normal"},
        ["annoyed"] = new string[] {"hands clasped", "mouth pout", "eyes normal"},
        ["outtro"] = new string[] {"hands hide", "mouth tongueout", "eyes squint"},
    };

    void SetVisible(bool visible)
    {
        main.SetActive(visible);
    }

    void SetSprite(string layer, string sprite)
    {
        foreach(ScreenLayer screenLayer in screenLayers)
        {
            if (screenLayer.name == layer)
            {
                screenLayer.SetSprite(sprite);
                return;
            }
        }
        Debug.LogError("layer "+layer+" not found in layers", this.gameObject);
    }

    void SetSprite(int layer, int sprite)
    {
        screenLayers[layer].SetSprite(sprite);
    }

    void SetLayerVisible(string layer, bool visible)
    {
        foreach(ScreenLayer screenLayer in screenLayers)
        {
            if (screenLayer.name == layer)
            {
                screenLayer.SetVisible(visible);
                return;
            }
        }
        Debug.LogError("layer "+layer+" not found in layers", this.gameObject);
    }

    void SetLayerVisible(int layer, bool visible)
    {
        screenLayers[layer].SetVisible(visible);
    }

    public void UpdateSprites(string[] spriteUpdates)
    {
        foreach (string spriteUpdate in spriteUpdates)
        {
            string[] spriteUpdateValues = spriteUpdate.Split(" ");
            if (spriteUpdateValues.Count() < 2)
            {
                Debug.LogError("SpriteUpdate not enough arguments");
            }

            // special cases
            switch (spriteUpdateValues[0], spriteUpdateValues[1])
            {
                case ("shortcut", _):
                    UpdateSprites(shortcuts[spriteUpdateValues[1]]); // recursion. uh oh.
                    break;
                case ("main", "hide"):
                    SetVisible(false);
                    break;
                case ("main", "show"):
                    SetVisible(true);
                    break;
                case (_, "hide"):
                    SetLayerVisible(spriteUpdateValues[0], false);
                    break;
                case (_, "show"):
                    SetLayerVisible(spriteUpdateValues[0], true);
                    break;
                default:
                    SetSprite(spriteUpdateValues[0], spriteUpdateValues[1]);
                    break;
            }
        }
    }
}
