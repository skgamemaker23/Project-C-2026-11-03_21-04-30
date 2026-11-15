using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

// Started with PurrNet docs and API
// Used ChatGPT to help with some of the code

public class Gameplay2 : NetworkBehaviour
{
    // UI
    [Header("Basic UI")]
    [SerializeField] private GameObject Profiles;
    [SerializeField] private GameObject chooseMatchPanel;
    [SerializeField] private GameObject guessMatchPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;


    //BUTTONS

    // Choose buttons
    [Header("Choose Buttons")]
    [SerializeField] private Button[] P1ChooseButtons;
    [SerializeField] private Button[] P2ChooseButtons;


    // Guess buttons
    [Header("Guess Buttons")]
    [SerializeField] private Button[] P1GuessButtons;
    [SerializeField] private Button[] P2GuessButtons;


    // Ready buttons
    [Header("Ready Buttons")]
    [SerializeField] private Button P1ReadyButton;
    [SerializeField] private Button P2ReadyButton;


    // Keeps track of ready players
    //chat hekped with thsi
    private HashSet<PlayerID> readyPlayers = new HashSet<PlayerID>();


    // Player IDs
    private PlayerID hostPlayerId;
    private PlayerID clientPlayerId;

    private bool hostPlayerKnown = false;
    private bool clientPlayerKnown = false;


    // Game checks - make sure both players ready before moving on  
    private bool localPlayerIsReady = false;
    private bool gameHasStarted = false;
    private bool guessingHasStarted = false;

    private bool localHasChosen = false;
    private bool localHasGuessed = false;


    // Player choices
    private int player1Choice = 0;
    private int player2Choice = 0;


    // Player guesses
    private int player1Guess = 0;
    private int player2Guess = 0;


    // Checks if players have chosen
    private bool P1Choice = false;
    private bool P2Choice = false;


    // Checks if players have guessed
    private bool P1Guess = false;
    private bool P2Guess = false;


    private const int RequiredPlayers = 2;


    private void Start()
    {
        // Show profiles first
        if (Profiles != null)
        {
            Profiles.SetActive(true);
        }

        // Hide the other screens
        if (chooseMatchPanel != null)
        {
            chooseMatchPanel.SetActive(false);
        }

        if (guessMatchPanel != null)
        {
            guessMatchPanel.SetActive(false);
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        // Turn off choice and guess buttons
        SetButtons(P1ChooseButtons, false);
        SetButtons(P2ChooseButtons, false);

        SetButtons(P1GuessButtons, false);
        SetButtons(P2GuessButtons, false);

        if (statusText != null)
        {
            statusText.text = "Read the profiles, then click Choose Match when ready!";
        }
    }


    // Ready button methods
    public void OnP1Ready()
    {
        TryStartGame();
    }

    public void OnP2Ready()
    {
        TryStartGame();
    }


    // Checks if players can start
    private void TryStartGame()
    {
        if (localPlayerIsReady == true)
        {
            return;
        }

        if (gameHasStarted == true)
        {
            return;
        }

        localPlayerIsReady = true;

        // Turn off ready buttons
        if (P1ReadyButton != null)
        {
            P1ReadyButton.interactable = false;
        }

        if (P2ReadyButton != null)
        {
            P2ReadyButton.interactable = false;
        }

        SendReadyToServer();
    }


    // Sends ready information to the server
    [ServerRpc(requireOwnership: false)]
    private void SendReadyToServer(RPCInfo info = default)
    {
        if (gameHasStarted == true)
        {
            return;
        }

        if (NetworkManager.main == null)
        {
            return;
        }

        // Find the host player
        hostPlayerId = NetworkManager.main.localPlayer;
        hostPlayerKnown = true;

        // Find the client player
        if (info.sender != hostPlayerId)
        {
            clientPlayerId = info.sender;
            clientPlayerKnown = true;
        }

        readyPlayers.Add(info.sender);

        if (readyPlayers.Count == 1)
        {
            UpdateStatus("Waiting for the other player...");
        }

        // Start when both players are ready
        if (readyPlayers.Count >= RequiredPlayers)
        {
            if (clientPlayerKnown == false)
            {
                return;
            }

            readyPlayers.Clear();
            gameHasStarted = true;

            StartGame();
        }
    }


    // Opens the choose screen
    [ObserversRpc]
    private void StartGame()
    {
        gameHasStarted = true;
        guessingHasStarted = false;

        localPlayerIsReady = false;
        localHasChosen = false;
        localHasGuessed = false;

        // Reset choices
        player1Choice = 0;
        player2Choice = 0;

        // Reset guesses
        player1Guess = 0;
        player2Guess = 0;

        // Reset checks
        P1Choice = false;
        P2Choice = false;

        P1Guess = false;
        P2Guess = false;

        // Hide profiles
        if (Profiles != null)
        {
            Profiles.SetActive(false);
        }

        // Show choose screen
        if (chooseMatchPanel != null)
        {
            chooseMatchPanel.SetActive(true);
        }

        if (guessMatchPanel != null)
        {
            guessMatchPanel.SetActive(false);
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        SetButtons(P1ChooseButtons, true);
        SetButtons(P2ChooseButtons, true);

        SetButtons(P1GuessButtons, false);
        SetButtons(P2GuessButtons, false);

        if (statusText != null)
        {
            statusText.text = "Choose your match!";
        }
    }


    // Player choice buttons
    public void ChooseWitch() { SendChoice(0); }
    public void ChooseVamp() { SendChoice(1); }
    public void ChooseWolf() { SendChoice(2); }
    public void ChooseGhost() { SendChoice(3); }
    public void ChooseFairy() { SendChoice(4); }
    public void ChooseZombie() { SendChoice(5); }
    public void ChooseSkeleton() { SendChoice(6); }
    public void ChooseDemon() { SendChoice(7); }


    // Sends the chosen profile to the server
    private void SendChoice(int choice)
    {
        if (gameHasStarted == false || guessingHasStarted == true)
        {
            return;
        }

        if (localHasChosen == true)
        {
            return;
        }

        if (choice < 0 || choice > 7)
        {
            return;
        }

        localHasChosen = true;

        SendChoiceToServer(choice);
    }


    // Saves the choices on the server
    [ServerRpc(requireOwnership: false)]
    private void SendChoiceToServer(int choice, RPCInfo info = default)
    {
        if (gameHasStarted == false || guessingHasStarted == true)
        {
            return;
        }

        if (choice < 0 || choice > 7)
        {
            return;
        }

        if (hostPlayerKnown == false || clientPlayerKnown == false)
        {
            return;
        }

        // Check which player sent the choice
        if (info.sender == hostPlayerId)
        {
            if (P1Choice == true)
            {
                return;
            }

            player1Choice = choice;
            P1Choice = true;
        }
        else if (info.sender == clientPlayerId)
        {
            if (P2Choice == true)
            {
                return;
            }

            player2Choice = choice;
            P2Choice = true;
        }
        else
        {
            return;
        }

        ChoiceAccepted(info.sender);

        // Open guess screen when both players have chosen
        if (P1Choice == true && P2Choice == true)
        {
            GuessingTime();
        }
        else
        {
            UpdateStatus("Waiting for the other player to choose...");
        }
    }


    // Turns off buttons after a player chooses
    [ObserversRpc]
    private void ChoiceAccepted(PlayerID player)
    {
        if (NetworkManager.main != null &&
            NetworkManager.main.localPlayer == player)
        {
            localHasChosen = true;

            SetButtons(P1ChooseButtons, false);
            SetButtons(P2ChooseButtons, false);
        }
    }


    // Opens the guess screen
    [ObserversRpc]
    private void GuessingTime()
    {
        guessingHasStarted = true;
        localHasGuessed = false;

        if (chooseMatchPanel != null)
        {
            chooseMatchPanel.SetActive(false);
        }

        if (guessMatchPanel != null)
        {
            guessMatchPanel.SetActive(true);
        }

        SetButtons(P1ChooseButtons, false);
        SetButtons(P2ChooseButtons, false);

        SetButtons(P1GuessButtons, true);
        SetButtons(P2GuessButtons, true);

        if (statusText != null)
        {
            statusText.text = "Guess what the other player chose!";
        }
    }


    // Player guess buttons
    public void GuessWitch() { SendGuess(0); }
    public void GuessVamp() { SendGuess(1); }
    public void GuessWolf() { SendGuess(2); }
    public void GuessGhost() { SendGuess(3); }
    public void GuessFairy() { SendGuess(4); }
    public void GuessZombie() { SendGuess(5); }
    public void GuessSkeleton() { SendGuess(6); }
    public void GuessDemon() { SendGuess(7); }


    // Sends the guess to the server
    private void SendGuess(int guess)
    {
        if (guessingHasStarted == false)
        {
            return;
        }

        if (localHasGuessed == true)
        {
            return;
        }

        if (guess < 0 || guess > 7)
        {
            return;
        }

        SendGuessToServer(guess);
    }


    // Saves the guesses on the server
    [ServerRpc(requireOwnership: false)]
    private void SendGuessToServer(int guess, RPCInfo info = default)
    {
        if (guessingHasStarted == false)
        {
            return;
        }

        if (P1Choice == false || P2Choice == false)
        {
            return;
        }

        if (guess < 0 || guess > 7)
        {
            return;
        }

        if (hostPlayerKnown == false || clientPlayerKnown == false)
        {
            return;
        }

        // Check which player sent the guess
        if (info.sender == hostPlayerId)
        {
            if (P1Guess == true)
            {
                return;
            }

            player1Guess = guess;
            P1Guess = true;
        }
        else if (info.sender == clientPlayerId)
        {
            if (P2Guess == true)
            {
                return;
            }

            player2Guess = guess;
            P2Guess = true;
        }
        else
        {
            return;
        }

        GuessAccepted(info.sender);

        // Show results when both players have guessed
        if (P1Guess == true && P2Guess == true)
        {
            ShowResults(
                player1Choice,
                player2Choice,
                player1Guess,
                player2Guess
            );
        }
        else
        {
            UpdateStatus("Waiting for the other player to guess...");
        }
    }


    // Turns off buttons after a player guesses
    [ObserversRpc]
    private void GuessAccepted(PlayerID player)
    {
        if (NetworkManager.main != null &&
            NetworkManager.main.localPlayer == player)
        {
            localHasGuessed = true;

            SetButtons(P1GuessButtons, false);
            SetButtons(P2GuessButtons, false);
        }
    }


    // Shows the results
    [ObserversRpc]
    private void ShowResults(int firstChoice, int secondChoice, int firstGuess, int secondGuess)
    {
        int player1Score = 0;
        int player2Score = 0;

        // Check Player 1's guess
        if (firstGuess == secondChoice)
        {
            player1Score = 1;
        }

        // Check Player 2's guess
        if (secondGuess == firstChoice)
        {
            player2Score = 1;
        }

        if (chooseMatchPanel != null)
        {
            chooseMatchPanel.SetActive(false);
        }

        if (guessMatchPanel != null)
        {
            guessMatchPanel.SetActive(false);
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        if (resultsText != null)
        {
            resultsText.text =
                "GAME RESULTS\n\n" +
                "Player 1 chose: " + GetProfileName(firstChoice) + "\n" +
                "Player 2 chose: " + GetProfileName(secondChoice) + "\n\n" +
                "Player 1 guessed: " + GetProfileName(firstGuess) + "\n" +
                "Player 2 guessed: " + GetProfileName(secondGuess) + "\n\n" +
                "Player 1 Score: " + player1Score + "\n" +
                "Player 2 Score: " + player2Score + "\n\n";

            if (player1Score > player2Score)
            {
                resultsText.text += "PLAYER 1 WINS!";
            }
            else if (player2Score > player1Score)
            {
                resultsText.text += "PLAYER 2 WINS!";
            }
            else
            {
                resultsText.text += "IT'S A TIE!";
            }
        }

        if (statusText != null)
        {
            statusText.text = "";
        }
    }


    // Updates the status text
    [ObserversRpc]
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }


    // Gets the profile name
    private string GetProfileName(int profile)
    {
        if (profile == 0) return "Witch";
        else if (profile == 1) return "Vamp";
        else if (profile == 2) return "Wolf";
        else if (profile == 3) return "Ghost";
        else if (profile == 4) return "Fairy";
        else if (profile == 5) return "Zombie";
        else if (profile == 6) return "Skeleton";
        else if (profile == 7) return "Demon";
        else return "Unknown";
    }


    // Turns buttons on or off
    private void SetButtons(Button[] buttons, bool interactable)
    {
        if (buttons == null)
        {
            return;
        }

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}
