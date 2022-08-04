using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputDriver : NetworkBehaviour
{
    #region Types.
    public struct MoveInputData
    {
        public Vector2 moveInput;
        public Vector2 cameraInput;
        public bool jump;
        public bool grounded;
    }

    public struct ReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public ReconcileData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    #endregion

    #region Fields

    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    [Range(0.1f, 9f)] [SerializeField] float sensitivity = 2f;

    private Vector2 _cameraInput;
    private Vector2 _moveInput;
    private bool _jump;

    public CharacterCamera characterCamera;
    private EntityController _characterController;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        _characterController = GetComponent<EntityController>();
        _jump = false;
         Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
    }

    #region Movement Processing

    private void GetInputData(out MoveInputData moveData)
    {
        moveData = new MoveInputData
        {
            jump = _jump,
            grounded = _characterController.isGrounded,
            moveInput = _moveInput,
            cameraInput = _cameraInput
        };
    }
    private void TimeManager_OnTick()
    {
        if (IsOwner)
        {
            Reconciliation(default, false);
            GetInputData(out MoveInputData md);
            UpdateMovement(md, false);
        }

        if (IsServer)
        {
            UpdateMovement(default, true);
            ReconcileData rd = new ReconcileData(_characterController.motor.InitialSimulationPosition, _characterController.motor.InitialSimulationRotation);
            Reconciliation(rd, true);
        }
    }

    #endregion

    #region Prediction Callbacks
    [Replicate]
    private void UpdateMovement(MoveInputData md, bool asServer, bool replaying = false)
    {
        characterCamera.UpdateWithInput(md.cameraInput);
        _characterController.UpdateWithInput(md.cameraInput, md.moveInput, md.jump);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        _characterController.motor.SetPositionAndRotation(rd.Position, rd.Rotation);
    }

    #endregion

    #region UnityEventCallbacks

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;
        _cameraInput = context.ReadValue<Vector2>() * sensitivity;

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            _cameraInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;
        if (context.started || context.performed)
        {
            _jump = true;
        }
        else if (context.canceled)
        {
            _jump = false;
        }
    }

    #endregion
}
