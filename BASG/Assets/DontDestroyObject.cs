using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour {
    public bool IsLoader = false;
    public GameObject gm;

    void Awake()
    {
       
        if(IsLoader)
        {
            gm = GameObject.Find("Gamemanager");

        }
        else
        {
            DontDestroyOnLoad(this);
        }
        
    }
}
