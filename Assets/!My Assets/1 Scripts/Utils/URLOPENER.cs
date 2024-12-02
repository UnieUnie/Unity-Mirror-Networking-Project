using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLOPENER : MonoBehaviour
{
    string url = "";

    // quick and simple button method to open url
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
