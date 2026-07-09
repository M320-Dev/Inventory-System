using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace M320.InventorySystem.Samples.Demo
{
    public sealed class InventoryManagerInput : MonoBehaviour
    {
        private InputAction _takeAction;
        private InputAction _dropAction;

        public event Action<Vector2> Take;
        public event Action<Vector2> Drop;

        private Action<InputAction.CallbackContext> _takeHandler;
        private Action<InputAction.CallbackContext> _dropHandler;

        private Camera _mainCamera;
        private readonly Mouse _mouse = Mouse.current;

        private void Awake()
        {
            _mainCamera = Camera.main;

            _takeAction = new("Take", InputActionType.Button);
            _takeAction.AddBinding("<Mouse>/leftButton");
            _takeHandler = context => Take?.Invoke(CalculatePointerPosition());

            _dropAction = new("Drop", InputActionType.Button);
            _dropAction.AddBinding("<Mouse>/rightButton");
            _dropHandler = context => Drop?.Invoke(CalculatePointerPosition());
        }

        private void OnEnable()
        {
            if (_takeAction != null) 
            {
                _takeAction.performed += _takeHandler;
                _takeAction.Enable();
            }
            if (_dropAction != null)
            {
                _dropAction.performed += _dropHandler;
                _dropAction.Enable();
            }
        }
        private void OnDisable()
        {
            if (_takeAction != null)
            {
                _takeAction.Disable();
                _takeAction.performed -= _takeHandler;
            }
            if (_dropAction != null)
            {
                _dropAction.Disable();
                _dropAction.performed -= _dropHandler;
            }
        }

        private Vector2 CalculatePointerPosition() 
        {
            return _mainCamera.ScreenToWorldPoint(_mouse.position.value);
        }
    }
}
