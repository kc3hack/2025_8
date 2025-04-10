using UnityEngine;
using UnityEngine.EventSystems;

namespace refactor
{
	public class SupportHandler : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData pointerData)
		{
			Debug.Log(gameObject.name + " がクリックされた!");
		}
	}
}