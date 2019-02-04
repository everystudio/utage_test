using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialScreen //: MonoBehaviour 
{
	[RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Screen.SetResolution( 1280, 720, false, 60);
    }
}
