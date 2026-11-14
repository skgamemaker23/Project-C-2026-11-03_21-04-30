
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

// Used PurrNet docs
// Some ChatGPT help 

public class RoundsSubmissionManager : NetworkBehaviour
{
    [Header("Player 1 Buttons")]
    [SerializeField] private Button[] player1Buttons;

    [Header("Player 2 Buttons")]
    [SerializeField] private Button[] player2Buttons;

    [Header("Player Profile Buttons")]
    [SerializeField] private Button[] player1ProfileButtons;
    [SerializeField] private Button[] player2ProfileButtons;

    [Header("Round Text")]
    [SerializeField] private TextMeshProUGUI revealText;

    [Header("Score Manager")]
    [SerializeField] private StartRounds readyManager;


    // Checks if each player has submitted
    private bool player1Submitted = false;
    private bool player2Submitted = false;
    private bool roundComplete = false;

    // Stores which cards the players picked
    private int player1Card = 0;
    private int player2Card = 0;

    private int currentRound = 0;

    // Remembers which characters already matched
    private bool[] player1Matched = new bool[4];
    private bool[] player2Matched = new bool[4];


    private void Start()
    {
        RefreshButtons();
    }


    protected override void OnSpawned()
    {
        RefreshButtons();
    }


    // Starts a new round
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
            if (Player1GoesFirst() == true)
            {
                revealText.text = "ROUND " + currentRound + "\nWaiting for Player 1...";
            }
            else
            {
                revealText.text = "ROUND " + currentRound + "\nWaiting for Player 2...";
            }
        }

        Debug.Log("Round " + currentRound + " started!");
    }


    // Player 1 starts rounds 1 and 3
    private bool Player1GoesFirst()
    {
        if (currentRound == 1 || currentRound == 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // Makes the buttons usable again
    private void RefreshButtons()
    {
        SetButtons(player1Buttons);
        SetButtons(player2Buttons);
    }


    private void SetButtons(Button[] buttons)
    {
        if (buttons == null)
        {
            return;
        }

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                if (button.gameObject.activeSelf == true)
                {
                    button.interactable = true;
                }
            }
        }
    }


    // Player 1 card buttons
    public void SubmitWitch()
    {
        SubmitPlayer1Card(1);
    }

    public void SubmitVamp()
    {
        SubmitPlayer1Card(2);
    }

    public void SubmitWolf()
    {
        SubmitPlayer1Card(3);
    }

    public void SubmitGhost()
    {
        SubmitPlayer1Card(4);
    }


    // Player 2 card buttons
    public void SubmitFairy()
    {
        SubmitPlayer2Card(5);
    }

    public void SubmitZombie()
    {
        SubmitPlayer2Card(6);
    }

    public void SubmitSkeleton()
    {
        SubmitPlayer2Card(7);
    }

    public void SubmitDemon()
    {
        SubmitPlayer2Card(8);
    }


    // Checks if Player 1 can submit
    private bool CanPlayer1Submit()
    {
        if (currentRound < 1 || currentRound > 4)
        {
            return false;
        }

        if (roundComplete == true || player1Submitted == true)
        {
            return false;
        }

        if (Player1GoesFirst() == true)
        {
            if (player2Submitted == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (player2Submitted == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    // Checks if Player 2 can submit
    private bool CanPlayer2Submit()
    {
        if (currentRound < 1 || currentRound > 4)
        {
            return false;
        }

        if (roundComplete == true || player2Submitted == true)
        {
            return false;
        }

        if (Player1GoesFirst() == false)
        {
            if (player1Submitted == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (player1Submitted == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    // Player 1 submits their card
    private void SubmitPlayer1Card(int card)
    {
        if (isHost == true)
        {
            if (CanPlayer1Submit() == true)
            {
                SubmitPlayer1CardServer(card);
            }
        }
    }


    [ServerRpc(requireOwnership: false)]
    private void SubmitPlayer1CardServer(int card)
    {
        if (CanPlayer1Submit() == false)
        {
            return;
        }

        if (card < 1 || card > 4)
        {
            return;
        }

        if (player1Matched[card - 1] == true)
        {
            return;
        }

        player1Submitted = true;
        player1Card = card;

        RevealSubmittedCard(1, card);

        if (player2Submitted == true)
        {
            CheckCompatibility();
        }
    }


    // Player 2 submits their card
    private void SubmitPlayer2Card(int card)
    {
        if (isHost == false)
        {
            if (CanPlayer2Submit() == true)
            {
                SubmitPlayer2CardServer(card);
            }
        }
    }


    [ServerRpc(requireOwnership: false)]
    private void SubmitPlayer2CardServer(int card)
    {
        if (CanPlayer2Submit() == false)
        {
            return;
        }

        if (card < 5 || card > 8)
        {
            return;
        }

        if (player2Matched[card - 5] == true)
        {
            return;
        }

        player2Submitted = true;
        player2Card = card;

        RevealSubmittedCard(2, card);

        if (player1Submitted == true)
        {
            CheckCompatibility();
        }
    }


    // Shows which player submitted
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
        {
            return;
        }

        int firstPlayer = 1;

        if (Player1GoesFirst() == false)
        {
            firstPlayer = 2;
        }

        if (playerNumber == firstPlayer)
        {
            string nextPlayer = "";

            if (playerNumber == 1)
            {
                nextPlayer = "Player 2";
            }
            else
            {
                nextPlayer = "Player 1";
            }

            revealText.text =
                "Player " + playerNumber + " submitted: " +
                GetCardName(card) + "\n" +
                nextPlayer + ", choose your card!";
        }
    }


    // Checks if the cards are compatible
    private void CheckCompatibility()
    {
        if (roundComplete == true)
        {
            return;
        }

        if (player1Submitted == false || player2Submitted == false)
        {
            return;
        }

        bool compatible = false;

        if (player1Card == 4 && player2Card == 6)
        {
            compatible = true;
        }
        else if (player1Card == 3 && player2Card == 5)
        {
            compatible = true;
        }
        else if (player1Card == 2 && player2Card == 8)
        {
            compatible = true;
        }
        else if (player1Card == 1 && player2Card == 5)
        {
            compatible = true;
        }
        else if (player1Card == 4 && player2Card == 5)
        {
            compatible = true;
        }
        else if (player1Card == 1 && player2Card == 7)
        {
            compatible = true;
        }
        else if (player1Card == 4 && player2Card == 7)
        {
            compatible = true;
        }

        roundComplete = true;

        if (compatible == true)
        {
            if (readyManager != null)
            {
                readyManager.AddTeamPoint();
            }
            else
            {
                Debug.LogError("Score manager is missing!");
            }

            RemoveMatchedCharacters(player1Card, player2Card);
        }

        ShowRoundResult(player1Card, player2Card, compatible);

        // Wait 4 seconds before starting the next round
        StartCoroutine(FinishRoundAfterDelay());
    }


    // Waits before finishing the round
    private IEnumerator FinishRoundAfterDelay()
    {
        yield return new WaitForSeconds(4f);

        if (readyManager != null)
        {
            readyManager.FinishRound();
        }
        else
        {
            Debug.LogError("Score manager is missing!");
        }
    }


    // Removes characters that already matched
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

        Debug.Log("Removed " + GetCardName(firstCard) + " and " + GetCardName(secondCard));
    }


    // Hides a button
    private void HideButton(Button[] buttons, int index)
    {
        if (buttons == null)
        {
            return;
        }

        if (index < 0 || index >= buttons.Length)
        {
            return;
        }

        if (buttons[index] != null)
        {
            buttons[index].gameObject.SetActive(false);
        }
    }


    // Shows the round result
    [ObserversRpc]
    private void ShowRoundResult(int firstCard, int secondCard, bool compatible)
    {
        player1Submitted = true;
        player2Submitted = true;
        roundComplete = true;

        string result = "";

        if (compatible == true)
        {
            result = "COMPATIBLE! +1 POINT!";
        }
        else
        {
            result = "NOT COMPATIBLE!";
        }

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


    // Gets the name of a card
    private string GetCardName(int card)
    {
        if (card == 1)
        {
            return "Witch";
        }
        else if (card == 2)
        {
            return "Vamp";
        }
        else if (card == 3)
        {
            return "Wolf";
        }
        else if (card == 4)
        {
            return "Ghost";
        }
        else if (card == 5)
        {
            return "Fairy";
        }
        else if (card == 6)
        {
            return "Zombie";
        }
        else if (card == 7)
        {
            return "Skeleton";
        }
        else if (card == 8)
        {
            return "Demon";
        }
        else
        {
            return "Unknown";
        }
    }
}
