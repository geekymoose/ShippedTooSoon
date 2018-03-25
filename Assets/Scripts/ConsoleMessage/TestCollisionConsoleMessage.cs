using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollisionConsoleMessage : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("The player enter the collision message");
        }
    }
}
