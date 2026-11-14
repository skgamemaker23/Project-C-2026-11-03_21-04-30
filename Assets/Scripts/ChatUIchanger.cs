using UnityEngine;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text messageText;

    // Set the name and message for players 
    public void SetMessage(string playerName, string message)
    {
        playerNameText.text = playerName;
        messageText.text = message;
    }
}
