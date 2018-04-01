using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomDoor : MonoBehaviour {

    public void openDoor() {
		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

        Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

        //soso.enabled = false;
		coco.enabled = false;
        anim.SetBool("isOpen", true);
    }

	public void closeDoor() {
		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

        Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

        //soso.enabled = true;
		coco.enabled = true;
        anim.SetBool("isOpen", false);
    }
}
