using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using UnityEngine.InputSystem;

public struct ChatMessage : IPackedAuto
{
    public string name;
    public string message;
}

public class ChatManager : PurrMonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ChatMessage message = new ChatMessage
            {
                name = InstanceHandler.NetworkManager.localPlayer.ToString(),
                message = "Hello World!"
            };

            InstanceHandler.NetworkManager.SendToServer<ChatMessage>(message);
        }
    }

    // Subscribe to ChatMessage events as either the Server, Client, or both
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<ChatMessage>(OnChatMessage, asServer);
    }

    // Unsubscribe to ChatMessage events as either the Server, Client, or both
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<ChatMessage>(OnChatMessage, asServer);
    }

    // Called when a ChatMessage broadcast is sent from either the Server or a Client
    private void OnChatMessage(PlayerID player, ChatMessage data, bool asServer)
    {
        if (asServer)   // The broadcast was sent to the Server from a Client
        {
            // Send the broadcast down to the Clients
            InstanceHandler.NetworkManager.SendToAll<ChatMessage>(data);
        }
        else    // The broadcast was sent to the Clients from the Server
        {
            Debug.Log($"Received {data.message} from {data.name}!");
        }
    }
}