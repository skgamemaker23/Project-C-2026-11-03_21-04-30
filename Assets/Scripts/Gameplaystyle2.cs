
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

// Started with PurrNet docs and API
// Some ChatGPT help

public class Gameplay2 : NetworkBehaviour
{
    // UI
    [Header("UI")]
    [SerializeField] private GameObject Profiles;
    [SerializeField] private GameObject chooseMatchPanel;
    [SerializeField] private GameObject guessMatchPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;


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


    // Keeps track of which players are ready
    private HashSet<PlayerID> readyPlayers = new HashSet<PlayerID>();

    private bool localPlayerIsReady = false;
    private bool gameHasStarted = false;
    private bool guessingHasStarted = false;

    // Keeps track of the local player's choices
    private bool localHasChosen = false;
    private bool localHasGuessed = false;

    // Player choices
    private int player1Choice = -1;
    private int player2Choice = -1;

    // Player guesses
    private int player1Guess = -1;
    private int player2Guess = -1;

    // Checks if players have chosen
    private bool P1Choice = false;
    private bool P2Choice = false;

    // Checks if players have guessed
    private bool P1Guess = false;
    private bool P2Guess = false;

    private const int RequiredPlayers = 2;


    // Ready buttons
    public void OnP1ReadyClicked()
    {
        TryStartGame();
    }

    public void OnP2ReadyClicked()
    {
        TryStartGame();
    }


    // Checks if the players can start the game
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

        if (isHost == true)
        {
            if (P1ReadyButton != null)
            {
                P1ReadyButton.interactable = false;
            }
        }
        else
        {
            if (P2ReadyButton != null)
            {
                P2ReadyButton.interactable = false;
            }
        }

        Debug.Log("Player is ready!");

        SendReadyToServer();
    }


    // Sends Ready information to the server
    [ServerRpc(requireOwnership: false)]
    private void SendReadyToServer(RPCInfo info = default)
    {
        if (gameHasStarted == true)
        {
            return;
        }

        readyPlayers.Add(info.sender);

        Debug.Log("Ready players: " + readyPlayers.Count);

        // Start when both players are ready
        if (readyPlayers.Count >= RequiredPlayers)
        {
            Debug.Log("Both players are ready!");

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

        // Reset player choices
        player1Choice = -1;
        player2Choice = -1;

        // Reset player guesses
        player1Guess = -1;
        player2Guess = -1;

        // Reset choice checks
        P1Choice = false;
        P2Choice = false;

        // Reset guess checks
        P1Guess = false;
        P2Guess = false;

        // Turn ready buttons back on
        if (P1ReadyButton != null)
        {
            P1ReadyButton.interactable = true;
        }

        if (P2ReadyButton != null)
        {
            P2ReadyButton.interactable = true;
        }

        // Open the choose screen
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

        // Turn choose buttons on
        SetButtons(P1ChooseButtons, true);
        SetButtons(P2ChooseButtons, true);

        // Turn guess buttons off
        SetButtons(P1GuessButtons, false);
        SetButtons(P2GuessButtons, false);

        if (statusText != null)
        {
            statusText.text = "Choose your match!";
        }

        Debug.Log("Choose screen opened!");
    }


    // Player choice buttons
    public void ChooseWitch()
    {
        SendChoice(0);
    }

    public void ChooseVamp()
    {
        SendChoice(1);
    }

    public void ChooseWolf()
    {
        SendChoice(2);
    }

    public void ChooseGhost()
    {
        SendChoice(3);
    }

    public void ChooseFairy()
    {
        SendChoice(4);
    }

    public void ChooseZombie()
    {
        SendChoice(5);
    }

    public void ChooseSkeleton()
    {
        SendChoice(6);
    }

    public void ChooseDemon()
    {
        SendChoice(7);
    }


    // Sends the chosen profile to the server
    private void SendChoice(int choice)
    {
        if (gameHasStarted == false)
        {
            return;
        }

        if (guessingHasStarted == true)
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

        if (isHost == true)
        {
            SetButtons(P1ChooseButtons, false);
        }
        else
        {
            SetButtons(P2ChooseButtons, false);
        }

        Debug.Log("Sending choice: " + GetProfileName(choice));

        SendChoiceToServer(choice, isHost);
    }


    // Saves the choices on the server
    [ServerRpc(requireOwnership: false)]
    private void SendChoiceToServer(int choice, bool player1)
    {
        if (gameHasStarted == false || guessingHasStarted == true)
        {
            return;
        }

        if (choice < 0 || choice > 7)
        {
            return;
        }

        if (player1 == true)
        {
            if (P1Choice == true)
            {
                return;
            }

            player1Choice = choice;
            P1Choice = true;

            Debug.Log("Player 1 has chosen!");
        }
        else
        {
            if (P2Choice == true)
            {
                return;
            }

            player2Choice = choice;
            P2Choice = true;

            Debug.Log("Player 2 has chosen!");
        }

        // Open the guess screen when both players have chosen
        if (P1Choice == true && P2Choice == true)
        {
            Debug.Log("Both players have chosen!");

            GuessingTime();
        }
    }


    // Opens the guess screen
    [ObserversRpc]
    private void GuessingTime()
    {
        guessingHasStarted = true;

        if (chooseMatchPanel != null)
        {
            chooseMatchPanel.SetActive(false);
        }

        if (guessMatchPanel != null)
        {
            guessMatchPanel.SetActive(true);
        }

        // Turn choose buttons off
        SetButtons(P1ChooseButtons, false);
        SetButtons(P2ChooseButtons, false);

        // Turn guess buttons on
        SetButtons(P1GuessButtons, true);
        SetButtons(P2GuessButtons, true);

        if (statusText != null)
        {
            statusText.text = "Guess which profile the other player chose!";
        }

        Debug.Log("Guess screen opened!");
    }


    // Player guess buttons
    public void GuessWitch()
    {
        SendGuess(0);
    }

    public void GuessVamp()
    {
        SendGuess(1);
    }

    public void GuessWolf()
    {
        SendGuess(2);
    }

    public void GuessGhost()
    {
        SendGuess(3);
    }

    public void GuessFairy()
    {
        SendGuess(4);
    }

    public void GuessZombie()
    {
        SendGuess(5);
    }

    public void GuessSkeleton()
    {
        SendGuess(6);
    }

    public void GuessDemon()
    {
        SendGuess(7);
    }


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

        localHasGuessed = true;

        if (isHost == true)
        {
            SetButtons(P1GuessButtons, false);
        }
        else
        {
            SetButtons(P2GuessButtons, false);
        }

        Debug.Log("Sending guess: " + GetProfileName(guess));

        SendGuessToServer(guess, isHost);
    }


    // Saves the guesses on the server
    [ServerRpc(requireOwnership: false)]
    private void SendGuessToServer(int guess, bool player1)
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

        if (player1 == true)
        {
            if (P1Guess == true)
            {
                return;
            }

            player1Guess = guess;
            P1Guess = true;

            Debug.Log("Player 1 has guessed!");
        }
        else
        {
            if (P2Guess == true)
            {
                return;
            }

            player2Guess = guess;
            P2Guess = true;

            Debug.Log("Player 2 has guessed!");
        }

        // Show results after both players have guessed
        if (P1Guess == true && P2Guess == true)
        {
            Debug.Log("Both players have guessed!");

            ShowResults(
                player1Choice,
                player2Choice,
                player1Guess,
                player2Guess
            );
        }
    }


    // Shows the results for both players
    [ObserversRpc]
    private void ShowResults(int firstChoice, int secondChoice, int firstGuess, int secondGuess)
    {
        int player1Score = 0;
        int player2Score = 0;

        // Player 1 gets a point for guessing Player 2's choice
        if (firstGuess == secondChoice)
        {
            player1Score = 1;
        }

        // Player 2 gets a point for guessing Player 1's choice
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
            statusText.text = "Game finished!";
        }

        Debug.Log("Game Mode 2 finished!");
    }


    // Gets the profile name
    private string GetProfileName(int profile)
    {
        if (profile == 0)
        {
            return "Witch";
        }
        else if (profile == 1)
        {
            return "Vamp";
        }
        else if (profile == 2)
        {
            return "Wolf";
        }
        else if (profile == 3)
        {
            return "Ghost";
        }
        else if (profile == 4)
        {
            return "Fairy";
        }
        else if (profile == 5)
        {
            return "Zombie";
        }
        else if (profile == 6)
        {
            return "Skeleton";
        }
        else if (profile == 7)
        {
            return "Demon";
        }
        else
        {
            return "Unknown";
        }
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
