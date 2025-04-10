using UnityEngine;
using R3;

namespace refactor
{
    public class InGamePresenter : MonoBehaviour
    {
        public const int MAX_X = 6;
        public const int MAX_Z = 6;
        [SerializeField] private GameObject _supportObj;
        [SerializeField] private GameObject _supportParentObj;

        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            for (int x = 0; x < MAX_X; x++)
            {
                for (int z = 0; z < MAX_Z; z++)
                {
                    var supportObj = Instantiate(_supportObj, _supportParentObj.transform);
                    supportObj.transform.localPosition = new Vector3(x, 0.015f, z);
                    var supportHandler = supportObj.GetComponentInChildren<SupportHandler>();
                    supportHandler.Initialize(x, z);
                }
            }
        }

        private void Bind()
        {

        }
    }
}