using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ScreenLayer
{
    [SerializeField] public string name;
    [SerializeField] private Image imageRenderer;
    [SerializeField] private List<Sprite> sprites;

    public void SetSprite(int index)
    {
        SetVisible(true);
        if (index >= sprites.Count || index < 0) Debug.LogError("accessed sprite outside of sprite list bounds");
        imageRenderer.sprite = sprites[index];
    }

    public void SetSprite(string name)
    {
        SetVisible(true);
        foreach(Sprite sprite in sprites)
        {
            if (sprite.name.Split("_").Last() == name)
            {
                imageRenderer.sprite = sprite;
            }
        }
        Debug.LogError("name "+name+" not found in sprite list");
    }

    public void SetVisible(bool visible)
    {
        imageRenderer.enabled = visible;
    }
}