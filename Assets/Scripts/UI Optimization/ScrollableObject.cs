using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ScrollableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	/// <summary>
	/// Moves this oject to its last index
	/// </summary>
	public void MoveToLast() {
		//get the last child and use it as new position for the transform;
		Transform lastTransform = this.transform.parent.GetChild (this.transform.parent.childCount - 1);
		this.transform.SetSiblingIndex (this.transform.parent.childCount - 1);
	}


	/// <summary>
	/// Moves this object to its first index
	/// </summary>
	public void MoveToFirst() {
		this.transform.SetSiblingIndex (0);
	}

	public float GetHeight() {
		return this.GetComponent<RectTransform> ().sizeDelta.y;
	}

	public float GetWidth() {
		return this.GetComponent<RectTransform> ().sizeDelta.x;
	}

}
