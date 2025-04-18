using UnityEngine;
using UnityEngine.EventSystems;

namespace refactor
{
	public class SupportHandler : MonoBehaviour, IPointerClickHandler
	{
		private int _posX;
		private int _posZ;
		private BoardManager _boardManager;
		public void Initialize(int x, int z)
		{
			_posX = x;
			_posZ = z;

			// ゲームシーン上のBoardManagerを取得
			_boardManager = GameObject.Find("GameSystem").GetComponent<BoardManager>();
			// 取得できなかった場合はエラーログを出力
			if (_boardManager == null)
			{
				Debugger.Log("ReversiBoardが見つかりませんでした。");
				return;
			}
		}

		/// <summary>
		/// マウスでクリックしたときの処理
		/// クリックした座標を取得し、駒を置くメソッドを呼び出す
		/// </summary>
		/// <param name="pointerData"></param>
		public void OnPointerClick(PointerEventData pointerData)
		{
			// Debugger.Log($"ボタンが押された座標: {pointerData.pointerCurrentRaycast.gameObject.name} posX: {_posX} posZ: {_posZ}");
			_boardManager.SetUpPiece(_posX, _posZ);
		}
	}
}