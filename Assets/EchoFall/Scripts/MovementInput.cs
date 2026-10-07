using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    public struct MovementCommand
    {
        public float move;
        public bool down, jumpPressed, jumpReleased, dashPressed, resetPressed;
    }

    public sealed class MovementInput : MonoBehaviour
    {
        public InputActionAsset actions;
        public bool allowReset = true;
        InputActionAsset instance;
        InputAction move, jump, dash, reset;
        bool jumpPressed, jumpReleased, dashPressed, resetPressed;
        public bool HasMovementIntent => (move != null && Mathf.Abs(move.ReadValue<Vector2>().x) > .3f) ||
            jumpPressed || dashPressed || (jump != null && jump.IsPressed()) || (dash != null && dash.IsPressed());

        void OnEnable()
        {
            if (actions == null) return;
            instance = Instantiate(actions);
            move = instance.FindAction("Movement/Move", true);
            jump = instance.FindAction("Movement/Jump", true);
            dash = instance.FindAction("Movement/Dash", true);
            reset = instance.FindAction("Movement/Reset", true);
            jump.performed += JumpPressed;
            jump.canceled += JumpReleased;
            dash.performed += DashPressed;
            reset.performed += ResetPressed;
            instance.Enable();
        }

        void JumpPressed(InputAction.CallbackContext _) => jumpPressed = true;
        void JumpReleased(InputAction.CallbackContext _) => jumpReleased = true;
        void DashPressed(InputAction.CallbackContext _) => dashPressed = true;
        void ResetPressed(InputAction.CallbackContext _) => resetPressed = true;

        public MovementCommand Consume()
        {
            Vector2 direction = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
            var command = new MovementCommand
            {
                // Preserve the browser's digital left/right movement on a stick too.
                move = Mathf.Abs(direction.x) > .3f ? Mathf.Sign(direction.x) : 0f,
                down = direction.y < -.4f,
                jumpPressed = jumpPressed, jumpReleased = jumpReleased,
                dashPressed = dashPressed, resetPressed = resetPressed && allowReset
            };
            ClearEdges();
            return command;
        }

        void ClearEdges() => jumpPressed = jumpReleased = dashPressed = resetPressed = false;
        void OnApplicationFocus(bool focused) { if (!focused) ClearEdges(); }

        void OnDisable()
        {
            if (instance != null)
            {
                jump.performed -= JumpPressed;
                jump.canceled -= JumpReleased;
                dash.performed -= DashPressed;
                reset.performed -= ResetPressed;
                instance.Disable();
                Destroy(instance);
            }
            instance = null;
            move = jump = dash = reset = null;
            ClearEdges();
        }
    }
}
