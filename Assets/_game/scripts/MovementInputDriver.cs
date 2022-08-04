using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class MovementInputDriver : NetworkBehaviour
{
    #region Types.

    public struct MoveInputData
    {
        public Vector2 moveVector;
        public Vector3 moveDirection;
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

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private bool _jump;
    [SerializeField] public float jumpSpeed = 6f;
    [SerializeField] public float speed = 8f;
    [SerializeField] public float gravity = -9.8f; // negative acceleration in y - remember physics?

    #endregion

    private void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick; // Could also be in Awake
        _characterController = GetComponent(typeof(CharacterController)) as CharacterController;
        _jump = false;
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
            moveVector = _moveInput,
            moveDirection = _moveDirection
        };
    }
    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            GetInputData(out MoveInputData md);
            Move(md, false);
        }

        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation);
            Reconciliation(rd, true);
        }
    }

    #endregion

    #region Prediction Callbacks

    [Replicate]
    private void Move(MoveInputData md, bool asServer, bool replaying = false)
    {
        if (md.grounded)
        {
            md.moveDirection.x = md.moveVector.x;
            md.moveDirection.y = 0;
            md.moveDirection.z = md.moveVector.y;

            md.moveDirection *= speed;
            if (md.jump)
            {
                md.moveDirection.y = jumpSpeed;
            }
        }

        md.moveDirection.y += gravity * (float)base.TimeManager.TickDelta; // gravity is negative...
        _characterController.Move(md.moveDirection * (float)base.TimeManager.TickDelta);
        _moveDirection = md.moveDirection;
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
        if (!base.IsOwner)
            return;
        _moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!base.IsOwner)
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