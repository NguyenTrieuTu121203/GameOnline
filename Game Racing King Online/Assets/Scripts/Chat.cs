using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public int unitID;
    [SerializeField]
    List<Message> messageList = new List<Message> ();
    public GameObject chatPanal, textObject;
    public InputField chatBox;
    private int countMassage=20;
    private void Update()
    {
        if (chatBox.text!="")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendToChat(chatBox.text);
                PlayerControls.client.Send("Chat|" + unitID +"|" + chatBox.text);
                chatBox.text = "";
            }
        }
    }

    public void SendToChat(string text)
    {
        if (messageList.Count > countMassage)
        {
            Destroy(messageList[0].textObj.gameObject);
            messageList.Remove(messageList[0]);
        }
        Message newMessage = new Message();

        newMessage.text = text;
        GameObject newText = Instantiate(textObject, chatPanal.transform);
        newMessage.textObj = newText.GetComponent<Text>();
        newMessage.textObj.text= newMessage.text;
        messageList.Add(newMessage);
    }
}


[System.Serializable]
public class Message
{
    public string text;
    public Text textObj;    
}