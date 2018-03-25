using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomDoor : MonoBehaviour {
	private bool isOpen = false;

	public void openDoor() {
		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();

		Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");

		soso.enabled = false;
		coco.enabled = false;
	}

	public void closeDoor() {
		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();

		Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");

		soso.enabled = true;
		coco.enabled = true;
	}
}
