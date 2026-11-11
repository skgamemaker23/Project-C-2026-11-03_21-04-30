
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;
//used purrnet docs/API 
//some chatgpt help :C 
public class RoundsSubmissionManager : NetworkBehaviour
{
    [Header("Player 1 Submit Buttons - Host")]
    [SerializeField] private Button[] player1Buttons;

    [Header("Player 2 Submit Buttons - Joiner")]
    [SerializeField] private Button[] player2Buttons;

    [Header("Player 1 Open Profile Buttons")]
    [SerializeField] private Button[] player1ProfileButtons;

    [Header("Player 2 Open Profile Buttons")]
    [SerializeField] private Button[] player2ProfileButtons;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI revealText;

    [Header("Score Manager")]
    [SerializeField] private StartRounds readyManager;

    private bool player1Submitted;
    private bool player2Submitted;
    private bool roundComplete;

    private int player1Card;
    private int player2Card;
    private int currentRound;

    private readonly bool[] player1Matched = new bool[4];
    private readonly bool[] player2Matched = new bool[4];

    private void Start()
    {
        RefreshButtons();
    }

    protected override void OnSpawned()
    {
        RefreshButtons();
    }

    // BEGIN ROUND

    public void BeginRound(int roundNumber)
    {
        currentRound = roundNumber;

        player1Submitted = false;
        player2Submitted = false;
        roundComplete = false;

        player1Card = 0;
        player2Card = 0;

        RefreshButtons();

        if (revealText != null)
        {
            revealText.text =
                "ROUND " + currentRound + "\n" +
                (Player1GoesFirst()
                    ? "Waiting for Player 1..."
                    : "Waiting for Player 2...");
        }

        Debug.Log("Submission system ready for Round " + currentRound);
    }

    private bool Player1GoesFirst()
    {
        return currentRound == 1 || currentRound == 3;
    }

    // BUTTON REFRESH

    private void RefreshButtons()
    {
        SetAvailableButtons(player1Buttons);
        SetAvailableButtons(player2Buttons);
    }

    private void SetAvailableButtons(Button[] buttons)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null && button.gameObject.activeSelf)
                button.interactable = true;
        }
    }

    // PLAYER 1 BUTTONS

    public void SubmitWitch() => SubmitPlayer1Card(1);
    public void SubmitVamp() => SubmitPlayer1Card(2);
    public void SubmitWolf() => SubmitPlayer1Card(3);
    public void SubmitGhost() => SubmitPlayer1Card(4);

    // PLAYER 2 BUTTONS

    public void SubmitFairy() => SubmitPlayer2Card(5);
    public void SubmitZombie() => SubmitPlayer2Card(6);
    public void SubmitSkeleton() => SubmitPlayer2Card(7);
    public void SubmitDemon() => SubmitPlayer2Card(8);

    // TURN CHECKS

    private bool CanPlayer1Submit()
    {
        if (currentRound < 1 || currentRound > 4)
            return false;

        if (roundComplete || player1Submitted)
            return false;

        if (Player1GoesFirst())
            return !player2Submitted;

        return player2Submitted;
    }

    private bool CanPlayer2Submit()
    {
        if (currentRound < 1 || currentRound > 4)
            return false;

        if (roundComplete || player2Submitted)
            return false;

        if (!Player1GoesFirst())
            return !player1Submitted;

        return player1Submitted;
    }

    // PLAYER 1 SUBMISSION

    private void SubmitPlayer1Card(int card)
    {
        if (!isHost || !CanPlayer1Submit())
            return;

        SubmitPlayer1CardServer(card);
    }

    [ServerRpc(requireOwnership: false)]
    private void SubmitPlayer1CardServer(int card)
    {
        if (!CanPlayer1Submit())
            return;

        if (card < 1 || card > 4)
            return;

        if (player1Matched[card - 1])
            return;

        player1Submitted = true;
        player1Card = card;

        RevealSubmittedCard(1, card);

        if (player2Submitted)
            CheckCompatibility();
    }

    // PLAYER 2 SUBMISSION

    private void SubmitPlayer2Card(int card)
    {
        if (isHost || !CanPlayer2Submit())
            return;

        SubmitPlayer2CardServer(card);
    }

    [ServerRpc(requireOwnership: false)]
    private void SubmitPlayer2CardServer(int card)
    {
        if (!CanPlayer2Submit())
            return;

        if (card < 5 || card > 8)
            return;

        if (player2Matched[card - 5])
            return;

        player2Submitted = true;
        player2Card = card;

        RevealSubmittedCard(2, card);

        if (player1Submitted)
            CheckCompatibility();
    }

    // REVEAL SUBMISSION

    [ObserversRpc]
    private void RevealSubmittedCard(int playerNumber, int card)
    {
        if (playerNumber == 1)
        {
            player1Submitted = true;
            player1Card = card;
        }
        else
        {
            player2Submitted = true;
            player2Card = card;
        }

        if (revealText == null)
            return;

        int firstPlayer = Player1GoesFirst() ? 1 : 2;

        if (playerNumber == firstPlayer)
        {
            string nextPlayer =
                playerNumber == 1 ? "Player 2" : "Player 1";

            revealText.text =
                "Player " + playerNumber +
                " submitted: " + GetCardName(card) +
                "\n" + nextPlayer + ", choose your card!";
        }
    }

    // COMPATIBILITY CHECK

    private void CheckCompatibility()
    {
        if (roundComplete || !player1Submitted || !player2Submitted)
            return;

        bool compatible =
            (player1Card == 4 && player2Card == 6) ||
            (player1Card == 3 && player2Card == 5) ||
            (player1Card == 2 && player2Card == 8) ||
            (player1Card == 1 && player2Card == 5) ||
            (player1Card == 4 && player2Card == 5) ||
            (player1Card == 1 && player2Card == 7) ||
            (player1Card == 4 && player2Card == 7);

        roundComplete = true;

        if (compatible)
        {
            if (readyManager != null)
                readyManager.AddTeamPoint();
            else
                Debug.LogError("Round1ReadyManager is not assigned!");

            RemoveMatchedCharacters(player1Card, player2Card);
        }

        ShowRoundResult(player1Card, player2Card, compatible);

        // IMPORTANT: Wait before returning to Profiles.
        StartCoroutine(FinishRoundAfterDelay());
    }

    private IEnumerator FinishRoundAfterDelay()
    {
        yield return new WaitForSeconds(4f);

        if (readyManager != null)
            readyManager.FinishRound();
        else
            Debug.LogError("Round1ReadyManager is not assigned!");
    }

    // REMOVE MATCHED CHARACTERS

    [ObserversRpc]
    private void RemoveMatchedCharacters(int firstCard, int secondCard)
    {
        int player1Index = firstCard - 1;
        int player2Index = secondCard - 5;

        player1Matched[player1Index] = true;
        player2Matched[player2Index] = true;

        HideButton(player1Buttons, player1Index);
        HideButton(player1ProfileButtons, player1Index);

        HideButton(player2Buttons, player2Index);
        HideButton(player2ProfileButtons, player2Index);

        Debug.Log(
            "Removed matched characters: " +
            GetCardName(firstCard) + " and " +
            GetCardName(secondCard)
        );
    }

    private void HideButton(Button[] buttons, int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Length)
            return;

        if (buttons[index] != null)
            buttons[index].gameObject.SetActive(false);
    }

    // DISPLAY RESULT

    [ObserversRpc]
    private void ShowRoundResult(
        int firstCard,
        int secondCard,
        bool compatible)
    {
        player1Submitted = true;
        player2Submitted = true;
        roundComplete = true;

        string result = compatible
            ? "COMPATIBLE! +1 POINT!"
            : "NOT COMPATIBLE!";

        if (revealText != null)
        {
            revealText.text =
                "ROUND " + currentRound + "\n" +
                GetCardName(firstCard) + " + " +
                GetCardName(secondCard) + "\n" +
                result;
        }

        Debug.Log("[ROUND " + currentRound + "] " + result);
    }

    // CARD NAMES

    private string GetCardName(int card)
    {
        switch (card)
        {
            case 1: return "Witch";
            case 2: return "Vamp";
            case 3: return "Wolf";
            case 4: return "Ghost";
            case 5: return "Fairy";
            case 6: return "Zombie";
            case 7: return "Skeleton";
            case 8: return "Demon";
            default: return "Unknown";
        }
    }
}
