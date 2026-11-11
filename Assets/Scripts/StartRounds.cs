
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;
using UnityEngine.SceneManagement;

//Started with Purrnet docs and API 
//Used ChatGPT to help with some of the code 
public class StartRounds : NetworkBehaviour
{
    [Header("Round 1 Ready Buttons")]
    [SerializeField] private Button[] startRound1Buttons;

    [Header("Round 2 Ready Buttons")]
    [SerializeField] private Button[] startRound2Buttons;

    [Header("Round 3 Ready Buttons")]
    [SerializeField] private Button[] startRound3Buttons;

    [Header("Round 4 Ready Buttons")]
    [SerializeField] private Button[] startRound4Buttons;

    [Header("Profile UI")]
    [SerializeField] private GameObject profiles;

    [Header("Shared Submission Screen")]
    [SerializeField] private GameObject round1Content;

    [Header("Submission Manager")]
    [SerializeField] private RoundsSubmissionManager submissionManager;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI teamScoreText;

    [Header("Win / Lose Screen")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    private readonly HashSet<PlayerID> readyPlayers =
        new HashSet<PlayerID>();

    private bool localPlayerIsReady;
    private bool roundHasStarted;

    private int currentRound = 0;
    private int teamScore = 0;

    private const int RequiredPlayers = 2;

    private void Start()
    {
        if (profiles != null)
            profiles.SetActive(true);

        if (round1Content != null)
            round1Content.SetActive(false);

        // Only Round 1 buttons appear at the beginning.
        ShowOnlyRoundButtons(1);

        UpdateScoreDisplay(teamScore);
    }

    // ---------------------------------
    // START BUTTON METHODS
    // ---------------------------------

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

    // ---------------------------------
    // READY SYSTEM
    // ---------------------------------

    private void TryStartRound(int round)
    {
        if (localPlayerIsReady || roundHasStarted)
            return;

        if (round != currentRound + 1)
            return;

        localPlayerIsReady = true;

        SetButtons(GetButtonsForRound(round), false);

        Debug.Log(
            "Player ready for Round " + round +
            ". Waiting for the other player..."
        );

        SendReadyToServer(round);
    }

    [ServerRpc]
    private void SendReadyToServer(
        int round,
        RPCInfo info = default)
    {
        if (roundHasStarted || round != currentRound + 1)
            return;

        readyPlayers.Add(info.sender);

        Debug.Log(
            "Round " + round + " ready players: " +
            readyPlayers.Count + "/" + RequiredPlayers
        );

        // Both players must be ready.
        if (readyPlayers.Count >= RequiredPlayers)
        {
            currentRound = round;
            roundHasStarted = true;

            readyPlayers.Clear();

            StartRoundForEveryone(round);
        }
    }

    // ---------------------------------
    // START ROUND FOR BOTH PLAYERS
    // ---------------------------------

    [ObserversRpc]
    private void StartRoundForEveryone(int round)
    {
        currentRound = round;
        roundHasStarted = true;
        localPlayerIsReady = false;

        if (profiles != null)
            profiles.SetActive(false);

        if (round1Content != null)
            round1Content.SetActive(true);

        // All ready buttons become unavailable during the round.
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
            Debug.LogError(
                "Submission Manager is not assigned!"
            );
        }

        Debug.Log("ROUND " + round + " STARTED!");
    }

    // ---------------------------------
    // FINISH ROUND
    // ---------------------------------

    public void FinishRound()
    {
        if (!roundHasStarted)
            return;

        roundHasStarted = false;
        readyPlayers.Clear();

        FinishRoundForEveryone(currentRound, teamScore);
    }

    [ObserversRpc]
    private void FinishRoundForEveryone(int completedRound, int score)
    {
        currentRound = completedRound;
        teamScore = score;
        roundHasStarted = false;
        localPlayerIsReady = false;

        // Hide the gameplay screen.
        if (round1Content != null)
            round1Content.SetActive(false);

        // Hide all round buttons.
        HideAllRoundButtons();

        UpdateScoreDisplay(score);

        // Check if all 4 rounds are finished.
        if (completedRound >= 4)
        {
            // Keep profiles hidden!
            if (profiles != null)
                profiles.SetActive(false);

            if (resultsPanel != null)
                resultsPanel.SetActive(true);

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

            Debug.Log(score >= 3 ? "YOU WIN!" : "YOU LOSE!");
            return;
        }

        // Only show profiles if another round remains.
        if (profiles != null)
            profiles.SetActive(true);

        ShowOnlyRoundButtons(completedRound + 1);

        Debug.Log("Round " + (completedRound + 1) + " is ready!");
    }
    // ---------------------------------
    // BUTTON VISIBILITY
    // ---------------------------------

    private void ShowOnlyRoundButtons(int round)
    {
        HideAllRoundButtons();

        Button[] nextButtons = GetButtonsForRound(round);

        if (nextButtons == null)
            return;

        foreach (Button button in nextButtons)
        {
            if (button == null)
                continue;

            button.gameObject.SetActive(true);
            button.interactable = true;
        }
    }

    private void HideAllRoundButtons()
    {
        SetButtonObjectsActive(startRound1Buttons, false);
        SetButtonObjectsActive(startRound2Buttons, false);
        SetButtonObjectsActive(startRound3Buttons, false);
        SetButtonObjectsActive(startRound4Buttons, false);
    }

    private void SetButtonObjectsActive(
        Button[] buttons,
        bool active)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }
    }

    private Button[] GetButtonsForRound(int round)
    {
        switch (round)
        {
            case 1:
                return startRound1Buttons;

            case 2:
                return startRound2Buttons;

            case 3:
                return startRound3Buttons;

            case 4:
                return startRound4Buttons;

            default:
                return null;
        }
    }

    private void SetButtons(
        Button[] buttons,
        bool interactable)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }

    // ---------------------------------
    // TEAM SCORE
    // ---------------------------------

    public void AddTeamPoint()
    {
        teamScore++;

        Debug.Log("TEAM SCORE: " + teamScore + "/4");

        UpdateScoreForEveryone(teamScore);
    }

    [ObserversRpc]
    private void UpdateScoreForEveryone(int newScore)
    {
        teamScore = newScore;
        UpdateScoreDisplay(newScore);
    }

    private void UpdateScoreDisplay(int score)
    {
        if (teamScoreText != null)
        {
            teamScoreText.text = "Score: " + score + "/4";
        }
    }
    public void ReturnToMainMenu()
    {
        RequestReturnToMainMenu();
    }

    [ServerRpc(requireOwnership: false)]
    private void RequestReturnToMainMenu()
    {
        LoadMainMenuForEveryone();
    }

    [ObserversRpc]
    private void LoadMainMenuForEveryone()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
