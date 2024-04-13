using System.Collections;
using System.Collections.Generic;
using Tessera;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


// This script simply kicks off the generator.
// By default the generator doesn't do anything, which is not useful for a sample
public class GenerateOnStart : MonoBehaviour
{
    void Start()
    {
        Generate();
    }

    public void Clear()
    {
        var multipassGenerator = GetComponent<TesseraMultipassGenerator>();

        if (multipassGenerator != null)
        {
            multipassGenerator.Clear();
        }
        else
        {

            var generators = GetComponents<TesseraGenerator>();

            generators[0].Clear();
        }
    }

    public void Generate()
    { 
        var multipassGenerator = GetComponent<TesseraMultipassGenerator>();
        if (multipassGenerator != null)
        {
            multipassGenerator.Generate();
        }
        else
        {

            var generators = GetComponents<TesseraGenerator>();

            foreach (var generator in generators)
            {
                generator.Generate();
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GenerateOnStart))]
public class GenerateOnStartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Regenerate"))
        {
            var t = target as GenerateOnStart;
            t.Clear();
            var multipassGenerator = t.GetComponent<TesseraMultipassGenerator>();
            if (multipassGenerator != null)
            {
                multipassGenerator.Generate();
            }
            else
            {

                var generators = t.GetComponents<TesseraGenerator>();

                foreach (var generator in generators)
                {
                    generator.Generate();
                }
            }
        }
    }
}
#endif
