
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
    // Ready buttons
    [Header("Round 1 Ready Buttons")]
    [SerializeField] private Button[] startRound1Buttons;

    [Header("Round 2 Ready Buttons")]
    [SerializeField] private Button[] startRound2Buttons;

    [Header("Round 3 Ready Buttons")]
    [SerializeField] private Button[] startRound3Buttons;

    [Header("Round 4 Ready Buttons")]
    [SerializeField] private Button[] startRound4Buttons;


    // Player profiles
    [Header("Profile UI")]
    [SerializeField] private GameObject profiles;


    // Gameplay screen
    [Header("Shared Submission Screen")]
    [SerializeField] private GameObject roundContent;


    // Submission manager
    [Header("Submission Manager")]
    [SerializeField] private RoundsSubmissionManager submissionManager;


    // Score
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI teamScoreText;


    // Win and lose screen
    [Header("Win / Lose Screen")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultText;


    // Keeps track of who is ready
    private HashSet<PlayerID> readyPlayers = new HashSet<PlayerID>();

    private bool localPlayerIsReady = false;
    private bool roundHasStarted = false;

    private int currentRound = 0;
    private int teamScore = 0;

    private const int RequiredPlayers = 2;


    private void Start()
    {
        

        ShowOnlyRoundButtons(1);
        UpdateScoreDisplay(teamScore);
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
        if (localPlayerIsReady == true)
        {
            return;
        }

        if (roundHasStarted == true)
        {
            return;
        }

        if (round != currentRound + 1)
        {
            return;
        }

        localPlayerIsReady = true;

        SetButtons(GetButtonsForRound(round), false);

        SendReadyToServer(round);
    }


    // Sends ready information to the server
    [ServerRpc]
    private void SendReadyToServer(int round, RPCInfo info = default)
    {
        if (roundHasStarted == true)
        {
            return;
        }

        if (round != currentRound + 1)
        {
            return;
        }

        readyPlayers.Add(info.sender);

        // Start when both players are ready
        if (readyPlayers.Count >= RequiredPlayers)
        {
            currentRound = round;
            roundHasStarted = true;

            readyPlayers.Clear();

            StartRoundForEveryone(round);
        }
    }


    // Starts the round for both players
    [ObserversRpc]
    private void StartRoundForEveryone(int round)
    {
        currentRound = round;
        roundHasStarted = true;
        localPlayerIsReady = false;

        if (profiles != null)
        {
            profiles.SetActive(false);
        }

        if (roundContent != null)
        {
            roundContent.SetActive(true);
        }

        // Turn off all ready buttons
        SetButtons(startRound1Buttons, false);
        SetButtons(startRound2Buttons, false);
        SetButtons(startRound3Buttons, false);
        SetButtons(startRound4Buttons, false);

        if (submissionManager != null)
        {
            submissionManager.BeginRound(round);
        }
        else
        {
        }

    }


    // Called when the round is finished
    public void FinishRound()
    {
        if (roundHasStarted == false)
        {
            return;
        }

        roundHasStarted = false;
        readyPlayers.Clear();

        FinishRoundForEveryone(currentRound, teamScore);
    }


    // Finishes the round for both players
    [ObserversRpc]
    private void FinishRoundForEveryone(int completedRound, int score)
    {
        currentRound = completedRound;
        teamScore = score;

        roundHasStarted = false;
        localPlayerIsReady = false;

        if (roundContent != null)
        {
            roundContent.SetActive(false);
        }

        HideAllRoundButtons();
        UpdateScoreDisplay(score);

        // Check if all rounds are finished
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

            if (score >= 3)
            {
            }
            

            return;
        }

        // Show profiles and the next round buttons
        if (profiles != null)
        {
            profiles.SetActive(true);
        }

        ShowOnlyRoundButtons(completedRound + 1);

    }


    // Shows the buttons for the next round
    private void ShowOnlyRoundButtons(int round)
    {
        HideAllRoundButtons();

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
    private void HideAllRoundButtons()
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

        Debug.Log("TEAM SCORE: " + teamScore + "/4");

        UpdateScoreForEveryone(teamScore);
    }


    // Updates the score for both players
    [ObserversRpc]
    private void UpdateScoreForEveryone(int newScore)
    {
        teamScore = newScore;

        UpdateScoreDisplay(newScore);
    }


    // Shows the score on the screen
    private void UpdateScoreDisplay(int score)
    {
        if (teamScoreText != null)
        {
            teamScoreText.text = "Score: " + score + "/4";
        }
    }


    // Return to main menu
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
