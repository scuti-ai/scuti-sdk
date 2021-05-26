using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeToSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField] private UnityEvent eventOnAccept;
	[SerializeField] private UnityEvent eventOnDiscard;
	[SerializeField] private Text acceptText;
	[SerializeField] private Text discardText;
	[SerializeField] private RectTransform brandRect;
	[SerializeField] private GameObject acceptImage;
	[SerializeField] private GameObject discardImage;
	private float rotationRange = 30;
	private float startingRotation = 180;
	private bool interactive = true;

	public void OnDrag(PointerEventData eventData)
	{
		if (!interactive) return;

		var value = eventData.delta.x / 50f;
		var rotation = value * 10;
		var temp = (brandRect.rotation.eulerAngles.z > startingRotation ? brandRect.rotation.eulerAngles.z - startingRotation : brandRect.rotation.eulerAngles.z + startingRotation) - rotation;
		temp = Mathf.Clamp(temp, startingRotation - rotationRange, startingRotation + rotationRange);

		if (temp < startingRotation) acceptText.color = new Color(acceptText.color.r, acceptText.color.g, acceptText.color.b, Mathf.Clamp01(acceptText.color.a + value));
		else if (temp > startingRotation) discardText.color = new Color(discardText.color.r, discardText.color.g, discardText.color.b, Mathf.Clamp01(discardText.color.a - value));

		brandRect.eulerAngles = new Vector3(0, 0, (brandRect.rotation.eulerAngles.z > startingRotation ? temp + startingRotation : temp - startingRotation));

		if (temp < startingRotation - 20) acceptImage.SetActive(true);
		else if (temp > startingRotation) acceptImage.SetActive(false);

		if (temp < startingRotation) discardImage.SetActive(false);
		else if (temp > startingRotation + 20) discardImage.SetActive(true);

		if (temp == startingRotation - rotationRange)
		{
			eventOnAccept.Invoke();
			interactive = false;
		}
		else if (temp == startingRotation + rotationRange)
		{
			eventOnDiscard.Invoke();
			interactive = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		interactive = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		brandRect.eulerAngles = new Vector3(0, 0, 0);
		acceptText.color = new Color(1, 1, 1, 0);
		discardText.color = new Color(1, 1, 1, 0);
		acceptImage.SetActive(false);
		discardImage.SetActive(false);
	}

	private void Start()
	{
		acceptText.color = new Color(1, 1, 1, 0);
		discardText.color = new Color(1, 1, 1, 0);
	}
}