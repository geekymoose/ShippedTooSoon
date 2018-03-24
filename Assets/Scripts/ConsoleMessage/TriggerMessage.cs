using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMessage : MonoBehaviour {

    private GameObject post_itPanel;
    
    private void Start()
    {
        post_itPanel = GameObject.FindGameObjectWithTag("PostPanel1");
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
