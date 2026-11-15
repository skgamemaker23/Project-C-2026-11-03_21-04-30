
using UnityEngine;
using TMPro;
using PurrNet;

// Started with PurrNet docs and API
// Used ChatGPT to help with some of the code

public class GameModeChoose : NetworkBehaviour
{
    [Header("Gameplay Selection")]
    [SerializeField] private GameObject gameplayStyleSelection;

    [Header("Gameplay Panels")]
    [SerializeField] private GameObject gameMode1Panel;
    [SerializeField] private GameObject gameMode2Panel;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;

    // Stores each player's selected mode
    private int player1Mode = 0;
    private int player2Mode = 0;

    // Stores this player's local selection
    private int selectedMode = 0;

    // Checks if each player confirmed
    private bool player1Confirmed = false;
    private bool player2Confirmed = false;

    // Remembers the assigned players
    private PlayerID player1ID;
    private PlayerID player2ID;

    private bool player1Known = false;
    private bool player2Known = false;

    private bool gameStarted = false;


    protected override void OnSpawned(bool asServer)
    {
        if (gameplayStyleSelection != null)
        {
            gameplayStyleSelection.SetActive(true);
        }

        if (gameMode1Panel != null)
        {
            gameMode1Panel.SetActive(false);
        }

        if (gameMode2Panel != null)
        {
            gameMode2Panel.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = "Choose a gameplay style!";
        }
    }


    // Player chooses Game Mode 1
    public void SelectGameMode1()
    {
        if (gameStarted == true)
        {
            return;
        }

        selectedMode = 1;
        ChooseModeServer(1);
    }


    // Player chooses Game Mode 2
    public void SelectGameMode2()
    {
        if (gameStarted == true)
        {
            return;
        }

        selectedMode = 2;
        ChooseModeServer(2);
    }


    // Player confirms their choice
    public void ConfirmSelection()
    {
        if (gameStarted == true)
        {
            return;
        }

        if (selectedMode == 0)
        {
            if (statusText != null)
            {
                statusText.text = "Choose a gameplay style first!";
            }

            return;
        }

        ConfirmSelectionServer();
    }


    // Assigns the player based on who clicks first
    private int AssignPlayer(PlayerID sender)
    {
        if (player1Known == true && sender == player1ID)
        {
            return 1;
        }

        if (player2Known == true && sender == player2ID)
        {
            return 2;
        }

        // First player to click becomes Player 1
        if (player1Known == false)
        {
            player1ID = sender;
            player1Known = true;

            Debug.Log("PLAYER 1 assigned: " + player1ID);

            return 1;
        }

        // Second player to click becomes Player 2
        if (player2Known == false)
        {
            player2ID = sender;
            player2Known = true;

            Debug.Log("PLAYER 2 assigned: " + player2ID);

            return 2;
        }

        // No more players can be assigned
        Debug.LogWarning("Unknown player tried to join the game.");

        return 0;
    }


    // Sends the selected mode to the server
    [ServerRpc(requireOwnership: false)]
    private void ChooseModeServer(int mode, RPCInfo info = default)
    {
        if (gameStarted == true)
        {
            return;
        }

        if (mode < 1 || mode > 2)
        {
            return;
        }

        int playerNumber = AssignPlayer(info.sender);

        if (playerNumber == 1)
        {
            player1Mode = mode;
            player1Confirmed = false;
        }
        else if (playerNumber == 2)
        {
            player2Mode = mode;
            player2Confirmed = false;
        }
        else
        {
            return;
        }

        UpdateStatus();
    }


    // Sends confirmation to the server
    [ServerRpc(requireOwnership: false)]
    private void ConfirmSelectionServer(RPCInfo info = default)
    {
        if (gameStarted == true)
        {
            return;
        }

        int playerNumber = AssignPlayer(info.sender);

        if (playerNumber == 1)
        {
            if (player1Mode == 0)
            {
                return;
            }

            player1Confirmed = true;
        }
        else if (playerNumber == 2)
        {
            if (player2Mode == 0)
            {
                return;
            }

            player2Confirmed = true;
        }
        else
        {
            return;
        }

        UpdateStatus();
    }


    // Checks if both players agree
    private void UpdateStatus()
    {
        if (player1Known == false || player2Known == false)
        {
            UpdateStatusObservers("Waiting for the other player to choose...");
            return;
        }

        if (player1Mode == 0 || player2Mode == 0)
        {
            UpdateStatusObservers("Both players must choose a gameplay style.");
            return;
        }

        if (player1Mode != player2Mode)
        {
            UpdateStatusObservers("You chose different modes! Please agree on one.");
            return;
        }

        if (player1Confirmed == true && player2Confirmed == true)
        {
            gameStarted = true;
            StartSelectedMode(player1Mode);
            return;
        }

        if (player1Confirmed == true)
        {
            UpdateStatusObservers("Player 1 confirmed. Waiting for Player 2...");
        }
        else if (player2Confirmed == true)
        {
            UpdateStatusObservers("Player 2 confirmed. Waiting for Player 1...");
        }
        else
        {
            UpdateStatusObservers("You chose the same mode. Both players must confirm!");
        }
    }


    // Updates the status text for both players
    [ObserversRpc]
    private void UpdateStatusObservers(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }


    // Opens the selected gameplay mode
    [ObserversRpc]
    private void StartSelectedMode(int mode)
    {
        if (gameplayStyleSelection != null)
        {
            gameplayStyleSelection.SetActive(false);
        }

        if (gameMode1Panel != null)
        {
            gameMode1Panel.SetActive(mode == 1);
        }

        if (gameMode2Panel != null)
        {
            gameMode2Panel.SetActive(mode == 2);
        }
    }


    // Gets the assigned player number
    public int GetAssignedPlayerNumber(PlayerID player)
    {
        if (player1Known == true && player == player1ID)
        {
            return 1;
        }

        if (player2Known == true && player == player2ID)
        {
            return 2;
        }

        return 0;
    }
}
