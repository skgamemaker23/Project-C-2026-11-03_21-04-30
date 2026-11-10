
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PurrNet;

public class Round1ReadyManager : NetworkBehaviour
{
    [Header("Ready Buttons")]
    [SerializeField] private Button[] startRound1Buttons;

    [Header("Round 1 UI")]
    [SerializeField] private GameObject round1Content;

    private readonly HashSet<PlayerID> readyPlayers = new HashSet<PlayerID>();

    private bool localPlayerIsReady;
    private bool roundHasStarted;

    private const int RequiredPlayers = 2;

    private void Start()
    {
        // Round 1 should remain hidden until both players are ready.
        if (round1Content != null)
        {
            round1Content.SetActive(false);
        }
    }

    // Connect BOTH players' buttons to this method.
    public void OnStartRound1Clicked()
    {
        if (localPlayerIsReady || roundHasStarted)
            return;

        localPlayerIsReady = true;

        // Disable this player's Ready buttons.
        if (startRound1Buttons != null)
        {
            foreach (Button button in startRound1Buttons)
            {
                if (button != null)
                    button.interactable = false;
            }
        }

        Debug.Log("You are ready! Waiting for the other player...");

        SendReadyToServer();
    }

    // Send this player's Ready signal to the host.
    [ServerRpc()]
    private void SendReadyToServer(RPCInfo info = default)
    {
        if (roundHasStarted)
            return;

        // Count each network player only once.
        readyPlayers.Add(info.sender);

        Debug.Log("Players ready: " + readyPlayers.Count + "/" + RequiredPlayers);

        if (readyPlayers.Count >= RequiredPlayers)
        {
            roundHasStarted = true;

            Debug.Log("Both players are ready! Starting Round 1.");

            StartRound1ForEveryone();
        }
    }

    // Notify all connected players that Round 1 has started.
    [ObserversRpc()]
    private void StartRound1ForEveryone()
    {
        roundHasStarted = true;

        if (round1Content != null)
        {
            round1Content.SetActive(true);
        }

        Debug.Log("ROUND 1 STARTED!");
    }
}
