
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;
using UnityEngine.SceneManagement;

// Started with PurrNet docs and API
// Used ChatGPT to help with some of the code

public class StartRounds : NetworkBehaviour
{
    [Header("Ready Buttons")]
    [SerializeField] private Button[] startRound1Buttons;
    [SerializeField] private Button[] startRound2Buttons;
    [SerializeField] private Button[] startRound3Buttons;
    [SerializeField] private Button[] startRound4Buttons;


    [Header("UI")]
    [SerializeField] private GameObject profiles;
    [SerializeField] private GameObject roundContent;
    [SerializeField] private TextMeshProUGUI teamScoreText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultText;


    [Header("Submission Manager")]
    [SerializeField] private RoundsSubmissionManager submissionManager;


    // Keeps track of which players are ready
    private HashSet<PlayerID> readyPlayers = new HashSet<PlayerID>();

    // Checks if the player is ready and if the round has started
    private bool PlayerReady = false;
    private bool roundStarted = false;

    private int currentRound = 0;
    private int teamScore = 0;

    private const int RequiredPlayers = 2;


    private void Start()
    {
        ShowRoundButtons(1);
        UpdateScore(teamScore);
    }


    // Ready button methods
    public void OnStartRound1Clicked()
    {
        TryStartRound(1);
    }

    public void OnStartRound2Clicked()
    {
        TryStartRound(2);
    }

    public void OnStartRound3Clicked()
    {
        TryStartRound(3);
    }

    public void OnStartRound4Clicked()
    {
        TryStartRound(4);
    }


    // Checks if the player can start the round
    private void TryStartRound(int round)
    {
        if (PlayerReady == true)
        {
            return;
        }

        if (roundStarted == true)
        {
            return;
        }

        if (round != currentRound + 1)
        {
            return;
        }

        PlayerReady = true;

        SetButtons(GetButtonsForRound(round), false);

        SendReadyToServer(round);
    }


    // Sends ready information to the server
    [ServerRpc]
    private void SendReadyToServer(int round, RPCInfo info = default)
    {
        if (roundStarted == true)
        {
            return;
        }

        if (round != currentRound + 1)
        {
            return;
        }

        readyPlayers.Add(info.sender);

        // Starts the round when both players are ready
        if (readyPlayers.Count >= RequiredPlayers)
        {
            currentRound = round;
            roundStarted = true;

            readyPlayers.Clear();

            StartRound(round);
        }
    }


    // Starts the round for both players
    [ObserversRpc]
    private void StartRound(int round)
    {
        currentRound = round;
        roundStarted = true;
        PlayerReady = false;

        if (profiles != null)
        {
            profiles.SetActive(false);
        }

        if (roundContent != null)
        {
            roundContent.SetActive(true);
        }

        // Turns off all ready buttons
        SetButtons(startRound1Buttons, false);
        SetButtons(startRound2Buttons, false);
        SetButtons(startRound3Buttons, false);
        SetButtons(startRound4Buttons, false);

        if (submissionManager != null)
        {
            submissionManager.BeginRound(round);
        }
    }


    // Called when the round is finished
    public void FinishRound()
    {
        if (roundStarted == false)
        {
            return;
        }

        roundStarted = false;
        readyPlayers.Clear();

        FinishRound(currentRound, teamScore);
    }


    // Finishes the round for both players
    [ObserversRpc]
    private void FinishRound(int completedRound, int score)
    {
        currentRound = completedRound;
        teamScore = score;

        roundStarted = false;
        PlayerReady = false;

        if (roundContent != null)
        {
            roundContent.SetActive(false);
        }

        HideRoundButtons();
        UpdateScore(score);

        // Checks if all rounds are finished
        if (completedRound >= 4)
        {
            if (profiles != null)
            {
                profiles.SetActive(false);
            }

            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);
            }

            if (resultText != null)
            {
                if (score >= 3)
                {
                    resultText.text = "YOU WIN!\nFinal Score: " + score + "/4";
                }
                else
                {
                    resultText.text = "YOU LOSE!\nFinal Score: " + score + "/4";
                }
            }

            return;
        }

        // Shows profiles and the next round buttons
        if (profiles != null)
        {
            profiles.SetActive(true);
        }

        ShowRoundButtons(completedRound + 1);
    }


    // Shows the buttons for the next round
    private void ShowRoundButtons(int round)
    {
        HideRoundButtons();

        Button[] nextButtons = GetButtonsForRound(round);

        if (nextButtons == null)
        {
            return;
        }

        foreach (Button button in nextButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.interactable = true;
            }
        }
    }


    // Hides all round buttons
    private void HideRoundButtons()
    {
        SetButtonObjectsActive(startRound1Buttons, false);
        SetButtonObjectsActive(startRound2Buttons, false);
        SetButtonObjectsActive(startRound3Buttons, false);
        SetButtonObjectsActive(startRound4Buttons, false);
    }


    // Turns button objects on or off
    private void SetButtonObjectsActive(Button[] buttons, bool active)
    {
        if (buttons == null)
        {
            return;
        }

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }
    }


    // Gets the buttons for the selected round
    private Button[] GetButtonsForRound(int round)
    {
        if (round == 1)
        {
            return startRound1Buttons;
        }
        else if (round == 2)
        {
            return startRound2Buttons;
        }
        else if (round == 3)
        {
            return startRound3Buttons;
        }
        else if (round == 4)
        {
            return startRound4Buttons;
        }
        else
        {
            return null;
        }
    }


    // Turns buttons interactable or not
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


    // Adds a point to the team score
    public void AddTeamPoint()
    {
        teamScore++;

        UpdateScore(teamScore);
    }


    // Updates the score and display for both players
    [ObserversRpc]
    private void UpdateScore(int newScore)
    {
        teamScore = newScore;

        if (teamScoreText != null)
        {
            teamScoreText.text = "Score: " + teamScore + "/4";
        }
    }


    // Returns to the main menu
    public void ReturnToMainMenu()
    {
        RequestReturnToMainMenu();
    }


    // Tells the server to return to the main menu
    [ServerRpc(requireOwnership: false)]
    private void RequestReturnToMainMenu()
    {
        MainMenu();
    }


    // Loads the main menu for everyone
    [ObserversRpc]
    private void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
