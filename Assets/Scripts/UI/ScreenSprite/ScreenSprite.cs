using System.Collections.Generic;
using UnityEngine;

public class ScreenSprite : MonoBehaviour
{
    [SerializeField] private List<ScreenLayer> screenLayers = new();
    [SerializeField] GameObject main;

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
            }
        }
        Debug.LogError("layer "+layer+" not found in layers", this.gameObject);
    }

    void SetLayerVisible(int layer, bool visible)
    {
        screenLayers[layer].SetVisible(visible);
    }

    void Start()
    {
        SetSprite("hair", "short");
    }
}
