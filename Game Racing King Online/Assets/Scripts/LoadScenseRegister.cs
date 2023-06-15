using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScenseRegister : MonoBehaviour
{
    public Button buttonRegister;
   

    public void LoadSceneRegister()
    {
        SceneManager.LoadScene(1);
    }
}
