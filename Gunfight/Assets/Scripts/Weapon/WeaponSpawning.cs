using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class WeaponSpawning : NetworkBehaviour
{

    [SerializeField] private GameObject[] weapons;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private int numPerWeapons;

    private List<Vector3> tileLoc = new List<Vector3>();
    private List<GameObject> spawnedWeapons = new List<GameObject>();

    private void Start()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if(gameModeManager == null)
        {
            Debug.Log("Weapon spawner failed to find gamemodemanager.");
        }
        else
        {
            if(gameModeManager.currentGameMode is not GunfightMode)
            {
                SpawnWeapons();
            }
        }
    }

    // Call this function to spawn weapons
    public void SpawnWeapons()
    {
        if (!isServer)
            return;

        if (tileLoc.Count == 0)
            FindTileLocations();

        foreach (GameObject weapon in weapons)
        {
            for (int i = 0; i < numPerWeapons; i++)
            {
                Vector3 spawnPosition = tileLoc[Random.Range(0, tileLoc.Count)];
                Quaternion spawnRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                GameObject weaponInstance = Instantiate(weapon, spawnPosition, spawnRotation);
                spawnedWeapons.Add(weaponInstance);
                NetworkServer.Spawn(weaponInstance);
            }
        }
    }

    // Call this function to delete all weapons with the "Weapon" tag
    public void DeleteWeapons()
    {
        foreach (GameObject weapon in spawnedWeapons)
        {
            if (weapon != null && weapon.CompareTag("Weapon"))
            {
                NetworkServer.Destroy(weapon);
            }
        }
        spawnedWeapons.Clear();
    }

    private void FindTileLocations()
    {
        tileLoc.Clear();
        foreach (var pos in tileMap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 place = tileMap.CellToWorld(localPlace);
            if (tileMap.HasTile(localPlace))
            {
                tileLoc.Add(place);
            }
        }
    }

    public GameObject[] GetWeapons()
    {
        return weapons;
    }
}
