using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tessera;

// Simple demonstration of generating an infinite amount of tiles on demand.
// Tessera Pro comes with a more general and sophisticated class for this, InfiniteGenerator.
[RequireComponent(typeof(TesseraGenerator))]
public class SlidingGenerator : MonoBehaviour
{
    private TesseraGenerator generator;

    public TesseraTile skyBox;

    public float delay = 3.0f;

    void Start()
    {
        generator = GetComponent<TesseraGenerator>();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            // Using StartGenerate in this way will resume the coroutine when the generation is done.
            // This has less stutter than calling generator.Generate.
            yield return generator.StartGenerate(new TesseraGenerateOptions
            {
                // Create new tiles unparented, so they don't move when we move the generator
                onCreate = instance => TesseraGenerator.Instantiate(instance, null),
            });

            // Shift the generator along z-axis
            generator.transform.Translate(0, 0, generator.cellSize.z * generator.size.z, Space.Self);

            // Change the skybox. This ensures the first generation is open on one side, but
            // later generations are open at both front and back.
            generator.skyBox = skyBox;

            // Sleep for a bit
            yield return new WaitForSeconds(delay);

        }
    }
}
