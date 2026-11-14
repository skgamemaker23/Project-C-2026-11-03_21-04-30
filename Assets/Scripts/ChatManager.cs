
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using PurrNet;
using PurrNet.Packing;

//code mostly from shelby in purrnet docs 
//some help with chatgpt to understand networks and debug 



// This stores the player's name and message
public struct ChatMessage : IPackedAuto
{
    public string name;
    public string message;
}

public class ChatManager : PurrMonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField chatInputField;
    public Transform messageContent;
    public ChatMessageUI messagePrefab;
    public ScrollRect chatScrollRect;
    public GameObject chatPanel;

    
    private void Awake()
    {
        // Hide the chat at the start
        chatPanel.SetActive(false);
    }

    
    

    // keybaard toggle for chat 
    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Press C to open or close chat
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            chatPanel.SetActive(!chatPanel.activeSelf);
        }

        // enter  sends message 
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (chatPanel.activeSelf)
            {
                SendMessage();
            }
        }
    }

    // Send the message to the server
    //most of this from shelby 
    private void SendMessage()
    {
        string typedMessage = chatInputField.text;

        // Check if the message is empty
        if (string.IsNullOrWhiteSpace(typedMessage))
        {
            return;
        }

        ChatMessage newMessage = new ChatMessage();

        newMessage.name = "";
        newMessage.message = typedMessage;

        // Send message through PurrNet
        InstanceHandler.NetworkManager.SendToServer<ChatMessage>(newMessage);

        // Clear the input field
        chatInputField.text = "";
    }

    // Subscribe to messages
    //also from purrnet docs and shelby
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<ChatMessage>(OnChatMessage, asServer);
    }

    // Unsubscribe from messages
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<ChatMessage>(OnChatMessage, asServer);
    }

    // Receive messages from players
    private void OnChatMessage(PlayerID player, ChatMessage data, bool asServer)
    {
        if (asServer)
        {
            // Get the sender's player ID
            data.name = player.ToString();

            // Send the message to everyone
            InstanceHandler.NetworkManager.SendToAll<ChatMessage>(data);
        }
        else
        {
            // Show the message in the chat
            ShowMessage(data.name, data.message);
        }
    }

    // Display the message on the screen
    private void ShowMessage(string playerName, string message)
    {
        ChatMessageUI newMessage = Instantiate(messagePrefab, messageContent);

        newMessage.SetMessage(playerName, message);

        // Update the chat layout
        Canvas.ForceUpdateCanvases();

        // Scroll to the newest message
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}
