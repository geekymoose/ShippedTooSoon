using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerMessage : MonoBehaviour {

    public GameObject post_itPanel;
    

    private void Start()
    {
        //post_itPanel = GameObject.Find("PanelMessageRoom1");
    }

    //activate error panel post-it
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("activate post it panel");
            post_itPanel.SetActive(true);
        }
    }
}
