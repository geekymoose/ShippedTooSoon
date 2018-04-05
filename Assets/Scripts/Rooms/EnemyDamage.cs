using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyDamage : MonoBehaviour {

	[Tooltip("Amount of damage enemy is doing to the player when enter")]
	public float damage = 1.0f;


    private void OnTriggerEnter2D(Collider2D other) {
		Debug.Log("OnTriggerEnter");
        if(other.gameObject.CompareTag("Player")) {
			PlayerHealth player = other.gameObject.GetComponent<PlayerHealth>();
			Assert.IsNotNull(player, "Player tag without PlayerHealth script");
			player.takeDammage(this.damage);
		}
	}

    private void OnCollisionEnter2D(Collision2D other) {
		Debug.Log("OnColliderEnter2D");
        if(other.gameObject.CompareTag("Player")) {
			PlayerHealth player = other.gameObject.GetComponent<PlayerHealth>();
			Assert.IsNotNull(player, "Player tag without PlayerHealth script");
			player.takeDammage(this.damage);
		}
	}
}
