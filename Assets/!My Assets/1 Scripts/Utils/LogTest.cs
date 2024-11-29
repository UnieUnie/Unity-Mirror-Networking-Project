using Mirror.BouncyCastle.Asn1.BC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LogTest : MonoBehaviour
{
    public void TestMethod()
    {
        Debug.Log("Test Method Called");
    }

    public void TestMethod(string customLog)
    {
        Debug.Log(customLog);
    }
}
