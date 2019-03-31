using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollableBounds : MonoBehaviour {

	public enum BoundsType {
		UPPER,
		LOWER
	}

	[SerializeField] private BoundsType boundsType;
	private IScrollHitListener scrollHitListener;

	// Use this for initialization
	void Start () {

	}

	public void SetHitListener(IScrollHitListener scrollHitListener) {
		this.scrollHitListener = scrollHitListener;
	}

	void OnTriggerEnter2D(Collider2D other) {
		ScrollableObject scrollObject = other.gameObject.GetComponent<ScrollableObject> ();
		if (this.scrollHitListener != null && scrollObject != null) {
			this.scrollHitListener.OnTriggerHit (scrollObject, this.boundsType);
		}
	}
}

public interface IScrollHitListener {
	void OnTriggerHit (ScrollableObject scrollableObject, ScrollableBounds.BoundsType boundsType);
}
