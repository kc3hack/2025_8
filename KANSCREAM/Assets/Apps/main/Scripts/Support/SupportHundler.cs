using UnityEngine;
using UnityEngine.EventSystems;

namespace refactor
{
	public class SupportHandler : MonoBehaviour, IPointerClickHandler
	{
		private int _posX;
		private int _posZ;
		[SerializeField] private InGamePresenter _presenter;
		public void Initialize(int x, int z)
		{
			_posX = x;
			_posZ = z;
		}
		public void OnPointerClick(PointerEventData pointerData)
		{
			Debugger.Log($"ボタンが押された座標: {pointerData.pointerCurrentRaycast.gameObject.name} posX: {_posX} posZ: {_posZ}");
			// _presenter.SetModelState(_posX, _posZ);
		}
	}
}