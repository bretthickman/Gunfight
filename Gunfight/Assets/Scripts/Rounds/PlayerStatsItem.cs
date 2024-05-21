using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsItem : MonoBehaviour
{
    public TMP_Text wins;
    public RawImage PlayerIcon;

    public int Team;
    public int ConnectionID;
    public ulong PlayerSteamID;
    private bool AvatarReceived;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    // Start is called before the first frame update
    void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    public void SetPlayerStats(int rounds)
    {
        if (!AvatarReceived) { GetPlayerIcon(); }
        wins.text = "Wins: " + rounds;
    }

    void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayerSteamID);
        if (ImageID == -1)
            return;

        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayerSteamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
            return;
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarReceived = true;
        return texture;
    }

    public void SetTeamStats(int rounds, Color teamColor)
    {
        wins.text = "Wins: " + rounds;
        PlayerIcon.color = teamColor;
    }
}
