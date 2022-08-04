using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public class EntityController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor motor;
    public Transform cameraFollowPoint;

    [Header("Misc")]
    public List<Collider> ignoredColliders = new List<Collider>();
    public Vector3 gravity = new Vector3(0, -9.8f, 0);
    public float drag = 0.1f;

    [Header("Stable Movement")]
    public float stableMovementSharpness = 15f;
    public float maxStableMoveSpeed = 10f;

    [Header("Air Movement")]
    public float airAccelerationSpeed = 0.2f;
    public float maxAirMoveSpeed = 15f;


    public bool isGrounded;

    private Vector3 _lookVector = new();
    private Vector3 _moveVector = new();
    private bool _jump = false;

    private Vector3 _planarDirection { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        motor.CharacterController = this;
        _planarDirection = motor.CharacterForward;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (ignoredColliders.Count == 0)
        {
            return true;
        }

        if (ignoredColliders.Contains(coll))
        {
            return false;
        }

        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (motor.GroundingStatus.IsStableOnGround && !motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!motor.GroundingStatus.IsStableOnGround && motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Quaternion rotationFromInput = Quaternion.Euler(motor.CharacterUp * _lookVector.x);
        _planarDirection = rotationFromInput * _planarDirection;
        _planarDirection = Vector3.Cross(motor.CharacterUp, Vector3.Cross(_planarDirection, motor.CharacterUp));
        currentRotation  = Quaternion.LookRotation(_planarDirection, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 moveVectorNormalized = motor.InitialSimulationRotation * _moveVector;
        // Ground movement
        if (motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(moveVectorNormalized, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveVectorNormalized.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * maxStableMoveSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableMovementSharpness * deltaTime));
        }
        else
        {
            // Add move input
            if (moveVectorNormalized.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = moveVectorNormalized * airAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < maxAirMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, maxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }

            // Gravity
            currentVelocity += gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (drag * deltaTime)));
        }
    }

    public void UpdateWithInput(Vector2 cameraInput, Vector2 moveVector, bool jump)
    {
        _lookVector = new Vector3(cameraInput.x, 0, 0);
        _moveVector = new Vector3(moveVector.x, 0, moveVector.y);
        _jump = jump;
    }

    private void OnLeaveStableGround()
    {
        isGrounded = false;
    }

    private void OnLanded()
    {
        isGrounded = true;
    }

}
