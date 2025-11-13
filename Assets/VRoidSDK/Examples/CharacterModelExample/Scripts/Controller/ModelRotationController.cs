using UnityEngine;
using UnityEngine.InputSystem;

namespace VRoidSDK.Examples.CharacterModelExample.Controller
{
    public class ModelRotationController : MonoBehaviour
    {
        private Vector2 _prevPosition;
        private bool _isHolding;

        public void OnPoint(InputValue value)
        {
            var position = value.Get<Vector2>();
            if (_isHolding)
            {
                var delta = position - _prevPosition;
                transform.rotation *= Quaternion.Euler(0, -delta.x / Screen.width * 180, 0);
            }
            _prevPosition = position;
        }

        public void OnClick(InputValue value)
        {
            _isHolding = value.Get<float>() != 0;
        }

        public void Reset()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
