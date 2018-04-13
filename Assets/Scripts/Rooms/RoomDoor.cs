using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomDoor : MonoBehaviour {

    public void openDoor() {
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

		coco.enabled = false;
        anim.SetBool("isOpen", true);

        // TODO: Bug. This is called each frames (But shouldn't)
		// AkSoundEngine.PostEvent("fx_door", gameObject);
    }

	public void closeDoor() {
		BoxCollider2D coco = this.GetComponent<BoxCollider2D>();
        Animator anim = this.GetComponent<Animator>();

		Assert.IsNotNull(coco, "Unable to recover TheDoors collider buddy!");
        Assert.IsNotNull(anim, "Unable to recover TheDoors animator buddy!");

		coco.enabled = true;
        anim.SetBool("isOpen", false);

		AkSoundEngine.PostEvent("fx_door", gameObject);
    }
}
