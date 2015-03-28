using UnityEngine;
using System.Collections;

public class DataPrinter : MonoBehaviour
{
	public Vector3 center = new Vector3(0, 0, 0);
	public float scaling = 0.01f;
	public float hoverThreshold = 1f;		// LEAP relative, cm.
	public GameObject cursor;				// The object which gets moved when dragging.
	public GameObject clickLight;
	public float maximumClickLength = 0.2f;		// How quick the up-down-up seqence needs to be to considered as a click.
	public float minimumClickDelay = 0.1f;		// Clicks closer in time are considered as accidental clicks.
	public float maximumClickDistance = 0.4f;	// If a down finger travels farther than this then it will not be considered as a click.
	public float clickLightFadeSpeed = 1f;

	private static bool m_Created = false;
	private bool isCentered = false;
	private Vector3 previousFinalPosition = Vector3.zero;
	private float downTime = 0;
	private float previousClickTime = 0;
	private Vector3 downStartPosition = Vector3.zero;
	private bool isAlreadyDown = false;

	void Awake()
	{
		if (m_Created)
		{
			throw new UnityException("A LeapUnityBridge has already been created!");
		}
		m_Created = true;

		SetCenter();

	}
	void OnDestroy()
	{
		m_Created = false;
	}

	void SetCenter()
	{
		if (LeapInput.Frame != null && LeapInput.Frame.Pointables[0] != null)
		{
			center = Leap.UnityVectorExtension.ToUnity(LeapInput.Frame.Pointables[0].TipPosition);
			isCentered = true;
		}

		if (cursor != null) cursor.transform.position = new Vector3(0, -1, 0);
	}

	bool IsUp(Vector3 position)
	{
		if (position.y > hoverThreshold) return true;
		else return false;
	}

	void Update()
	{
		LeapInput.Update();
		if (LeapInput.Frame.Pointables[0] != null)
		{
			Vector3 trackedPosition = Leap.UnityVectorExtension.ToUnity(LeapInput.Frame.Pointables[0].TipPosition);
			Vector3 recenteredPosition = trackedPosition - center;
			Vector3 finalPosition = Vector3.zero;
			finalPosition.x = -recenteredPosition.x;
			finalPosition.y = -recenteredPosition.z;
			finalPosition.z = -recenteredPosition.y;

			if (IsUp(finalPosition)) // Finger UP.
			{
				if (isAlreadyDown) isAlreadyDown = false;
				transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				if (downTime > 0)
				{
					Vector2 fingerTravelXZ = new Vector2(finalPosition.x, finalPosition.z) - new Vector2(downStartPosition.x, downStartPosition.z);

					if (downTime < maximumClickLength &&
						(Time.time - previousClickTime) > minimumClickDelay &&
						fingerTravelXZ.magnitude < maximumClickDistance)
					{
						if (clickLight != null) clickLight.light.intensity = 1;
						previousClickTime = Time.time;
					}
					downTime = 0;
				}
			}
			else // Finger DOWN.
			{
				if (cursor != null)
				{
					Vector3 offset = (finalPosition - previousFinalPosition) * scaling;
					offset.y = 0;
					cursor.transform.position += offset;
				}
				transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

				downTime += Time.deltaTime;

				if (isAlreadyDown == false)
				{
					downStartPosition = finalPosition;
					isAlreadyDown = true;
				}
			}
			previousFinalPosition = finalPosition;
			transform.position = finalPosition * scaling;
		}

		if (isCentered == false || Input.GetKeyDown(KeyCode.C)) SetCenter();

		if (clickLight != null && clickLight.light.intensity > 0)
		{
			clickLight.light.intensity -= clickLightFadeSpeed * Time.deltaTime;
		}
	}
}