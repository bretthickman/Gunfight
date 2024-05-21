using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIColorChanger : MonoBehaviour
{
    [SerializeField] private Color baseColor;

    void Start()
    {
        // Load baseColor from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("BaseColorR") && PlayerPrefs.HasKey("BaseColorG") && PlayerPrefs.HasKey("BaseColorB"))
        {
            float r = PlayerPrefs.GetFloat("BaseColorR");
            float g = PlayerPrefs.GetFloat("BaseColorG");
            float b = PlayerPrefs.GetFloat("BaseColorB");
            baseColor = new Color(r, g, b);
        }
        SetBaseColor(baseColor);
    }

    void OnValidate()
    {
        if (SceneManager.GetActiveScene().name == "Start")
        {
            SetBaseColor(baseColor);
        }
    }


    void ChangeColors()
    {
        ChangeColorsRecursively(transform);
    }

    void ChangeColorsRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Check if the child is named "Background"
            if (child.name.Equals("Background", System.StringComparison.OrdinalIgnoreCase) ||
                child.name.Equals("LoadScreen", System.StringComparison.OrdinalIgnoreCase))
            {
                Image backgroundImage = child.GetComponent<Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.color = baseColor;
                }

                // Change color for all nested child GameObjects with Image components
                ChangeNestedChildColors(child);
            }

            Transform sprite = FindChildIgnoreCase(child, "sprite");
            if (sprite != null && sprite.childCount > 0)
            {
                Transform firstChild = sprite.GetChild(0);

                if (firstChild.name.Equals("Layer1", System.StringComparison.OrdinalIgnoreCase))
                {
                    // If the first game object under sprite is Layer1
                    Transform layer2 = FindChildIgnoreCase(sprite, "Layer2");
                    Transform layer4 = FindChildIgnoreCase(sprite, "Layer4");

                    if (layer2 != null)
                    {
                        Image layer2Image = layer2.GetComponent<Image>();
                        if (layer2Image != null)
                        {
                            Color newColor = new Color(baseColor.r / 3.1f, baseColor.g / 5f, baseColor.b / 3.8f);
                            layer2Image.color = newColor;
                        }
                    }

                    if (layer4 != null)
                    {
                        Image layer4Image = layer4.GetComponent<Image>();
                        if (layer4Image != null)
                        {
                            Color newColor = new Color(baseColor.r / 1.465f, baseColor.g / 1.49f, baseColor.b / 1.48f);
                            layer4Image.color = newColor;
                        }
                    }
                }
                else
                {
                    // If the first game object under sprite is not Layer1
                    Transform layer3 = FindChildIgnoreCase(sprite, "Layer3");
                    Transform layer1UnderSprite = FindChildIgnoreCase(sprite, "Layer1");

                    if (layer3 != null)
                    {
                        Image layer3Image = layer3.GetComponent<Image>();
                        if (layer3Image != null)
                        {
                            Color newColor = new Color(baseColor.r / 3.1f, baseColor.g / 5f, baseColor.b / 3.8f);
                            layer3Image.color = newColor;

                            if (layer1UnderSprite != null)
                            {
                                Image layer1Image = layer1UnderSprite.GetComponent<Image>();
                                if (layer1Image != null)
                                {
                                    newColor = new Color(baseColor.r / 3.1f, baseColor.g / 3f, baseColor.b / 3.8f);
                                    layer1Image.color = newColor;
                                }
                            }
                        }
                    }
                }
            }

            // Recursive call to handle nested children
            if (child.childCount > 0)
            {
                ChangeColorsRecursively(child);
            }
        }
    }


    void ChangeNestedChildColors(Transform parent)
    {
        foreach (Transform nestedChild in parent)
        {
            Image imageComponent = nestedChild.GetComponent<Image>();
            if (imageComponent != null)
            {
                Color newColor = new Color(baseColor.r / 2.0f, baseColor.g / 2.0f, baseColor.b / 2.0f, baseColor.a);
                imageComponent.color = newColor;
            }

            // Recursive call to handle nested children
            ChangeNestedChildColors(nestedChild);
        }
    }

    void ChangeColorsAndSave()
    {
        ChangeColors();

        // Save the baseColor to PlayerPrefs
        PlayerPrefs.SetFloat("BaseColorR", baseColor.r);
        PlayerPrefs.SetFloat("BaseColorG", baseColor.g);
        PlayerPrefs.SetFloat("BaseColorB", baseColor.b);
        PlayerPrefs.Save();
    }

    // Optionally, add a public method to set the base color from outside and update the colors
    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        ChangeColorsAndSave();
    }

    // Helper method to find a child with case-insensitive name matching
    Transform FindChildIgnoreCase(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }
        return null;
    }
}
