using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public Transform[] FFASpawnPoints;
    public Transform[] SPSpawnPoints;
    public Transform[] GFSpawnPoints;
    public int mapWidth;
    public int mapHeight;
    // this is used to make up difference between 
    // tile height and walkable height duo to 75% top-down view
    public int heightOffset;
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
