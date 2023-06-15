using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterCtl : MonoBehaviour
{
    public static Client client;
    public Button Register;
    public InputField clientNameInputRegister;
    public InputField passwordInputRegister;
    public InputField reTypePasswordInputRegister;
    private string user, pass, retypePass;
    private int comfirmValue;


    public void RigisterUser()
    {
        user = clientNameInputRegister.text;
        pass = passwordInputRegister.text;
        retypePass = reTypePasswordInputRegister.text;
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(retypePass)) return;
        if (pass != retypePass) return;
        SceneManager.LoadScene(0);
    }
    
}
