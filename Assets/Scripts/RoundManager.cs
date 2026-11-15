
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

// Used PurrNet docs
// Some ChatGPT help

public class RoundsSubmissionManager : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private Button[] P1ProfileButtons; //for looking at the profiles/expanding
    [SerializeField] private Button[] P2ProfileButtons;
    [SerializeField] private Button[] P1CardButtons;//submitting the cards
    [SerializeField] private Button[] P2CardButtons;
    
    [SerializeField] private TextMeshProUGUI StatusText;

    [Header("StartRounds")]
    [SerializeField] private StartRounds readyManager;

    [Header("Player Assignment")]
    [SerializeField] private GameModeChoose gameModeChoose;


    // Checks if each player has submitted
    private bool P1Submitted = false;
    private bool P2Submitted = false;
    private bool roundFinish = false;

    // Player picks
    private int P1Card = 0;
    private int P2Card = 0;

    private int Round = 0;

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
        Round = roundNumber;

        P1Submitted = false;
        P2Submitted = false;
        roundFinish = false;

        P1Card = 0;
        P2Card = 0;

        RefreshButtons();

        if (StatusText != null)
        {
            if (Player1GoesFirst() == true)
            {
                StatusText.text = "ROUND " + Round + "\nWaiting for Player 1...";
            }
            else
            {
                StatusText.text = "ROUND " + Round + "\nWaiting for Player 2...";
            }
        }
    }


    // Player 1 starts rounds 1 and 3
    private bool Player1GoesFirst()
    {
        if (Round == 1 || Round == 3)
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
        SetButtons(P1CardButtons);
        SetButtons(P2CardButtons);
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
        SubmitCard(1);
    }

    public void SubmitVamp()
    {
        SubmitCard(2);
    }

    public void SubmitWolf()
    {
        SubmitCard(3);
    }

    public void SubmitGhost()
    {
        SubmitCard(4);
    }


    // Player 2 card buttons
    public void SubmitFairy()
    {
        SubmitCard(5);
    }

    public void SubmitZombie()
    {
        SubmitCard(6);
    }

    public void SubmitSkeleton()
    {
        SubmitCard(7);
    }

    public void SubmitDemon()
    {
        SubmitCard(8);
    }


    // Checks if a player can submit
    private bool CanPlayerSubmit(int playerNumber)
    {
        if (Round < 1 || Round > 4)
        {
            return false;
        }

        if (roundFinish == true)
        {
            return false;
        }

        if (playerNumber == 1)
        {
            if (P1Submitted == true)
            {
                return false;
            }

            if (Player1GoesFirst() == true)
            {
                return P2Submitted == false;
            }
            else
            {
                return P2Submitted == true;
            }
        }
        else if (playerNumber == 2)
        {
            if (P2Submitted == true)
            {
                return false;
            }

            if (Player1GoesFirst() == false)
            {
                return P1Submitted == false;
            }
            else
            {
                return P1Submitted == true;
            }
        }

        return false;
    }


    // Player submits a card
    private void SubmitCard(int card)
    {
        if (card < 1 || card > 8)
        {
            return;
        }

        SubmitCardServer(card);
    }


    // Sends card to server
    [ServerRpc(requireOwnership: false)]
    private void SubmitCardServer(int card, RPCInfo info = default)
    {
        if (roundFinish == true)
        {
            return;
        }

        if (card < 1 || card > 8)
        {
            return;
        }

        // Gets the player's assignment from GameModeChoose
        if (gameModeChoose == null)
        {
            Debug.LogError("GameModeChoose is not assigned in the Inspector!");
            return;
        }

        int playerNumber = gameModeChoose.GetAssignedPlayerNumber(info.sender);

        if (playerNumber == 0)
        {
            Debug.LogWarning("This player has not been assigned Player 1 or Player 2.");
            return;
        }

        // Makes sure Player 1 only selects Player 1 cards
        if (playerNumber == 1)
        {
            if (card < 1 || card > 4)
            {
                return;
            }

            if (CanPlayerSubmit(1) == false)
            {
                return;
            }

            if (player1Matched[card - 1] == true)
            {
                return;
            }

            P1Submitted = true;
            P1Card = card;
        }

        // Makes sure Player 2 only selects Player 2 cards
        else if (playerNumber == 2)
        {
            if (card < 5 || card > 8)
            {
                return;
            }

            if (CanPlayerSubmit(2) == false)
            {
                return;
            }

            if (player2Matched[card - 5] == true)
            {
                return;
            }

            P2Submitted = true;
            P2Card = card;
        }
        else
        {
            return;
        }

        RevealSubmittedCard(playerNumber, card);

        // Checks the cards when both players have submitted
        if (P1Submitted == true && P2Submitted == true)
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
            P1Submitted = true;
            P1Card = card;
        }
        else
        {
            P2Submitted = true;
            P2Card = card;
        }

        if (StatusText == null)
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

            StatusText.text =
                "Player " + playerNumber + " submitted: " +
                CardName(card) + "\n" +
                nextPlayer + ", choose your card!";
        }
    }


    // Checks if the cards are compatible
    private void CheckCompatibility()
    {
        if (roundFinish == true)
        {
            return;
        }

        if (P1Submitted == false || P2Submitted == false)
        {
            return;
        }

        bool compatible = false;

        if (P1Card == 4 && P2Card == 6)
        {
            compatible = true;
        }
        else if (P1Card == 3 && P2Card == 5)
        {
            compatible = true;
        }
        else if (P1Card == 2 && P2Card == 8)
        {
            compatible = true;
        }
        else if (P1Card == 1 && P2Card == 5)
        {
            compatible = true;
        }
        else if (P1Card == 4 && P2Card == 5)
        {
            compatible = true;
        }
        else if (P1Card == 1 && P2Card == 7)
        {
            compatible = true;
        }
        else if (P1Card == 4 && P2Card == 7)
        {
            compatible = true;
        }

        roundFinish = true;

        if (compatible == true)
        {
            if (readyManager != null)
            {
                readyManager.AddTeamPoint();
            }

            RemoveMatchedCharacters(P1Card, P2Card);
        }

        RoundResults(P1Card, P2Card, compatible);

        // Wait 4 seconds before starting the next round
        StartCoroutine(Delay());
    }


    // Waits before finishing the round
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(4f);

        if (readyManager != null)
        {
            readyManager.FinishRound();
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

        HideButton(P1CardButtons, player1Index);
        HideButton(P1ProfileButtons, player1Index);

        HideButton(P2CardButtons, player2Index);
        HideButton(P2ProfileButtons, player2Index);
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
    private void RoundResults(int firstCard, int secondCard, bool compatible)
    {
        P1Submitted = true;
        P2Submitted = true;
        roundFinish = true;

        string result = "";

        if (compatible == true)
        {
            result = "COMPATIBLE! +1 POINT!";
        }
        else
        {
            result = "NOT COMPATIBLE!";
        }

        if (StatusText != null)
        {
            StatusText.text =
                "ROUND " + Round + "\n" +
                CardName(firstCard) + " + " +
                CardName(secondCard) + "\n" +
                result;
        }
    }


    // Gets the name of a card
    private string CardName(int card)
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
