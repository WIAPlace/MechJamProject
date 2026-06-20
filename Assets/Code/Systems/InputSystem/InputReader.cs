using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///  Tool for utilizing the event system with the input actions
/// </summary>
[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, InputSystem.IPlayerActions, InputSystem.IUIActions
{
    private InputSystem _gameInput;

    ////////////////////////////////////////////////////////////////////////// Set Up
    /// House Keeping to make sure it starts up and ends correctly.
    /// Make sure in scripts if you subscribe to an event to unsubscibe on Disable/Destroy
    
    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new InputSystem();

            _gameInput.Player.SetCallbacks(this);
            _gameInput.UI.SetCallbacks(this);

            SetPlayer(); 
        }
    }
    private void OnDisable()
    {
        if (_gameInput != null)
        {
            // Ensure both action maps and the asset are disabled before this object is unloaded
            _gameInput.Player.Disable();
            _gameInput.UI.Disable();
            // Disable the whole asset (defensive) and dispose the generated object to destroy the underlying asset
            _gameInput.Disable();
        }
    }
    private void OnDestroy()
    {
        if (_gameInput != null)
        {
           // Ensure both action maps and the asset are disabled before this object is unloaded
            _gameInput.Player.Disable();
            _gameInput.UI.Disable();
            // Disable the whole asset (defensive) and dispose the generated object to destroy the underlying asset
            _gameInput.Disable();
        }
    }

    ////////////////////////////////////////////////////////////////////////// Switch Input Functions
    /// Changes which Events will react to inputs.
    
    public void SetPlayer() // GamePlay
    {
        _gameInput.Player.Enable();
        _gameInput.UI.Disable();
    }
    public void SetUI() // UI
    {
        _gameInput.Player.Disable();
        _gameInput.UI.Enable();
    }

    ////////////////////////////////////////////////////////////////////////// Events
    
    ////// Player Events ///////////////
    /// 
    


    ////// UI Events ///////////////////
    /// 



    ///////////////////////////////////////////////////////////////////////// Functions 
    
    ////// Implemented /////////////////
    ///
    



    ////// Not Implemented /////////////
    /// Interfaces will automaticaly be generated 
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnLook(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnAttack(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnCrouch(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPrevious(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnNext(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnNavigate(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnSubmit(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPoint(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnRightClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnMiddleClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnScrollWheel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTrackedDevicePosition(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTrackedDeviceOrientation(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }
}

    ////// Examples ////////////////////
    /// For refrence on how to write the diffrent call backs and events
    /*
        // Events

        public event Action<Vector2> MoveEvent; 
        public event Action JumpEvent;
        public event Action JumpCancelledEvent;
        public event Action PauseEvent;
        public event Action ResumeEvent;

        // Functions

        public void OnMove(InputAction.CallbackContext context) // Vector 2 
        {
            //Debug.Log(message: $"Phase:{context.phase},Value:{context.ReadValue<Vector2>()}");
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context) // Check if down then when up
        {
            if (context.phase == InputActionPhase.Performed)
            {
                JumpEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                JumpCancelledEvent?.Invoke();
            }
        }
        public void OnPause(InputAction.CallbackContext context) // PAUSE
        {
            if (context.phase == InputActionPhase.Performed)
            {
                PauseEvent.Invoke();
                SetUI();
            }
        }

        public void OnResume(InputAction.CallbackContext context) // RESUME
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ResumeEvent.Invoke();
                SetPlayer();
            }
        }
    */
