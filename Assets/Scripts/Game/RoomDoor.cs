using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomDoor : MonoBehaviour {

    public bool isOpen = true;

    public void openDoor() {
        this.isOpen = true;

		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

        Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

        //soso.enabled = false;
		coco.enabled = false;

        //anim.SetTrigger("openDoor");
    }

	public void closeDoor() {
        this.isOpen = false;

		SpriteRenderer soso = this.GetComponent<SpriteRenderer>();
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

        Assert.IsNotNull(soso, "Unable to recover TheDoors renderer buddy!");
		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

        //soso.enabled = true;
		coco.enabled = true;
        
        //anim.SetTrigger("closeDoor");
    }
}
