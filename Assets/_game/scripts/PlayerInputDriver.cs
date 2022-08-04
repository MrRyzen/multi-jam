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
        public Vector2 moveVector;
        public Vector3 cameraInput;
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

    public CharacterCamera characterCamera;
    private EntityController _characterController;

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

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick; // Could also be in Awake
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
            moveVector = _moveInput,
            cameraInput = _cameraInput
        };
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            GetInputData(out MoveInputData md);
            updateMovement(md, false);
        }

        if (base.IsServer)
        {
            updateMovement(default, true);
            ReconcileData rd = new(transform.position, transform.rotation);
            Reconciliation(rd, true);
        }
    }

    #endregion

    #region Prediction Callbacks

    [Replicate]
    private void updateMovement(MoveInputData md, bool asServer, bool replaying = false)
    {
        characterCamera.UpdateWithInput(md.cameraInput);
        _characterController.UpdateWithInput(md.cameraInput, md.moveVector, md.jump);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
    }

    #endregion

    #region UnityEventCallbacks

    public void OnMovement(InputAction.CallbackContext context)
    {
       _moveInput = context.ReadValue<Vector2>();
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {

        _cameraInput = context.ReadValue<Vector2>() * sensitivity;

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            _cameraInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
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
