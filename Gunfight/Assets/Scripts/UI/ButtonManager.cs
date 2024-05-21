using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public Text ButtonInfo;

    public GameObject

            QuickButton,
            SingleButton,
            HostButton,
            JoinButton,
            BackButton;

    
    private Dictionary<Transform, TransformData> initialTransforms = new Dictionary<Transform, TransformData>();

    // Helper class to store RectTransform data
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Transform transform)
        {
            position = transform.GetComponent<RectTransform>().localPosition;
            rotation = transform.GetComponent<RectTransform>().localRotation;
            scale = transform.localScale;
        }
    }

    void Start()
    {
        init();
        StoreInitialPositions();
    }

    [SerializeField]
    private string[] Infos =
    {
        "",
        "Quick Start and Dive into the Action!",
        "Create a Lobby and Invite Players",
        "Join the Fun and Explore Existing Lobbies!",
        "Test Your Skills with Single-Player Mode!"
    };

    void StoreInitialPositions()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            initialTransforms.Add(QuickButton.transform, new TransformData(QuickButton.transform));
            initialTransforms.Add(SingleButton.transform, new TransformData(SingleButton.transform));
            initialTransforms.Add(HostButton.transform, new TransformData(HostButton.transform));
            initialTransforms.Add(JoinButton.transform, new TransformData(JoinButton.transform));
            initialTransforms.Add(BackButton.transform, new TransformData(BackButton.transform));
        }
    }

    public void init()
    {
        // Reset button positions
        foreach (var kvp in initialTransforms)
        {
            kvp.Key.GetComponent<RectTransform>().localPosition = kvp.Value.position;
            kvp.Key.GetComponent<RectTransform>().localRotation = kvp.Value.rotation;
            kvp.Key.localScale = kvp.Value.scale;

            Rigidbody rb = kvp.Key.gameObject.GetComponent<Rigidbody>();
            Transform shot = kvp.Key.gameObject.transform.Find("UIshot(Clone)");
            if (rb != null)
            {
                Destroy(rb);
            }
            if (shot != null)
            {
                Destroy(shot.gameObject);
            }
        }
    }

    public void HostLobbyButton()
    {
        Invoke("loadHostButton", 0.75f);
    }

    private void loadHostButton()
    {
        SteamLobby.Instance.HostLobby();
        PlayerPrefs.SetInt("isJoinedSingle", 0);
    }

    public void QuickStartButton()
    {
        Invoke("loadQuickStart", 0.75f);
    }

    private void loadQuickStart()
    {
        SteamLobby.Instance.QuickStart();
        PlayerPrefs.SetInt("isJoinedSingle", 0);
    }

    public void SinglePlayerButton()
    {
        Invoke("loadSinglePlayer", 0.75f);
    }

    private void loadSinglePlayer()
    {
        SteamLobby.Instance.HostLobby();
        PlayerPrefs.SetInt("isJoinedSingle", 1);
    }

    public void JoinLobbyButton()
    {
        Invoke("loadJoinLobby", 0.75f);
    }

    private void loadJoinLobby()
    {
        LobbiesListManager.instance.toLobbyList();
    }

    public void QuitGameToLobby()
    {
        Invoke("loadQuitGame", 0.75f);
    }

    private void loadQuitGame()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            // gameModeManager.currentGameMode.SetQuitClicked(true);
            gameModeManager.currentGameMode.QuitGame();
        }
    }

    public void onHoverQuick()
    {
        ButtonInfo.text = Infos[1];
    }

    public void onHoverHost()
    {
        ButtonInfo.text = Infos[2];
    }

    public void onHoverJoin()
    {
        ButtonInfo.text = Infos[3];
    }

    public void onHoverSingle()
    {
        ButtonInfo.text = Infos[4];
    }

    public void onLeaveButton()
    {
        ButtonInfo.text = Infos[0];
    }
}
