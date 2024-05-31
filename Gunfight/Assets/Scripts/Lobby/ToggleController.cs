using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : NetworkBehaviour
{
    public Toggle publicToggle;
    public Toggle cardToggle;
    public Toggle ffireToggle;

    public void OnPublicToggleValueChanged()
    {
        // Call a command to change the Toggle value on the server
        if(isServer)
            RpcSyncPublicToggleValue(publicToggle.isOn);
    }

    public void OnCardToggleValueChanged()
    {
        // Call a command to change the Toggle value on the server
        if(isServer)
            RpcSyncCardToggleValue(cardToggle.isOn);
    }

    [ClientRpc]
    void RpcSyncFFireToggleValue(bool value)
    {
        // Change the Toggle value on all clients
        ffireToggle.isOn = value;
    }

    public void OnFFireToggleValueChanged()
    {
        // Call a command to change the Toggle value on the server
        if (isServer)
            RpcSyncFFireToggleValue(ffireToggle.isOn);
    }

    [ClientRpc]
    void RpcSyncCardToggleValue(bool value)
    {
        // Change the Toggle value on all clients
        cardToggle.isOn = value;
    }

    [ClientRpc]
    void RpcSyncPublicToggleValue(bool value)
    {
        // Change the Toggle value on all clients
        publicToggle.isOn = value;
    }
}
