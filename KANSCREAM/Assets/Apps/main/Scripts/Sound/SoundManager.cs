using UnityEngine;

namespace refactor
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _gameBGM;
        [SerializeField] private AudioSource _winBGM;
        [SerializeField] private AudioSource _loseBGM;
        [SerializeField] private AudioSource _betraySE;
        [SerializeField] private AudioSource _shineSE;
        [SerializeField] private AudioSource _screamBGM;

        /// <summary>
        /// ゲーム開始時にBGMを再生するメソッド
        /// </summary>
        /// <param name="gameState">ゲームの状態</param>
        public void PlayBGM(InGameModel.GameState gameState)
        {
            Debugger.Log("PlayBGM：" + gameState);
            switch (gameState)
            {
                case InGameModel.GameState.Start:
                    StopBGM();
                    break;
                case InGameModel.GameState.BeforeScream:
                    StopBGM();
                    _gameBGM.Play();
                    break;
                case InGameModel.GameState.AfterScream:
                    _screamBGM.Play();
                    break;
                case InGameModel.GameState.Result:
                    StopBGM();
                    break;
            }
        }

        /// <summary>
        /// 勝敗が決定した時にBGMを再生するメソッド
        /// 勝った場合は勝利BGMを、負けた場合は敗北BGMを再生する
        /// </summary>
        public void PlayResultBGM(bool isWin)
        {
            StopBGM();
            if (isWin)
            {
                _winBGM.Play();
            }
            else
            {
                _loseBGM.Play();
            }
        }

        /// <summary>
        /// 全てのBGMを停止するメソッド
        /// </summary>
        private void StopBGM()
        {
            _gameBGM.Stop();
            _winBGM.Stop();
            _loseBGM.Stop();
            _screamBGM.Stop();
        }
    }
}
