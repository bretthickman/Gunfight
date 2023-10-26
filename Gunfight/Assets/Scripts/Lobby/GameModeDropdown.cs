using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeDropdown : NetworkBehaviour
{
    public Dropdown dropdown;

    public void Start()
    {
        if (isLocalPlayer)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    public void OnDropdownValueChanged(int value)
    {
        // Call a command to change the Dropdown value on the server
        CmdChangeDropdownValue(value);
    }

    [Command]
    void CmdChangeDropdownValue(int value)
    {
        // Change the Dropdown value on the server
        RpcSyncDropdownValue(value);
    }

    [ClientRpc]
    void RpcSyncDropdownValue(int value)
    {
        // Change the Dropdown value on all clients
        dropdown.value = value;
    }
}
