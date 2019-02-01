using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hides/disables the scrollable objects to preserve CPU power.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class HidingScrollView : MonoBehaviour, IScrollHitListener {

	[SerializeField] private ScrollableBounds upperBound;
	[SerializeField] private ScrollableBounds lowerBound;
	[SerializeField] private RectTransform containerObject;

	private const int SWAP_THRESHOLD_COUNT = 3; //number of minimum objects needed before the scrollable object will be swapped to the last/first place.

	private ScrollRect scrollRect;

	// Use this for initialization
	void Start () {
		this.upperBound.SetHitListener (this);
		this.lowerBound.SetHitListener (this);

		this.scrollRect = this.GetComponent<ScrollRect> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		
	private bool IsScrolling() {
		float valueY = Mathf.Abs (this.scrollRect.velocity.y);
		float valueX = Mathf.Abs (this.scrollRect.velocity.x);

		return (valueX > 0.0f || valueY > 0.0f);
	}

	public void OnTriggerHit (ScrollableObject scrollableObject, ScrollableBounds.BoundsType boundsType) {
		if (boundsType == ScrollableBounds.BoundsType.UPPER) {
			


		} else if (boundsType == ScrollableBounds.BoundsType.LOWER) {
			
		}
	}
}
