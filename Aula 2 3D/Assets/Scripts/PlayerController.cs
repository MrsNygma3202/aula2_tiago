using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Assign the Input Action (Vector2) that represents Move (e.g. Player/Move). Use an InputActionReference from your input actions asset.")]
    [SerializeField] private InputActionReference moveAction = null;

    [Header("Movement")]
    [Tooltip("Force applied for movement (when using AddForce) or base torque multiplier (when using AddTorque).")]
    [SerializeField] private float moveForce = 10f;
    [Tooltip("Maximum horizontal speed (m/s). Set to 0 to disable clamping.")]
    [SerializeField] private float maxSpeed = 6f;
    [Tooltip("When enabled, uses AddTorque for a rolling feel. When disabled, uses AddForce for direct control.")]
    [SerializeField] private bool useTorque = true;
    [Tooltip("Additional multiplier applied to torque when using rolling behavior.")]
    [SerializeField] private float torqueMultiplier = 1.0f;

    [Header("Camera")]
    [Tooltip("If assigned, movement will be relative to this transform's forward/right on the XZ plane. If null, Camera.main will be used if available.")]
    [SerializeField] private Transform cameraTransform = null;
    [Tooltip("If true, movement input is interpreted relative to the camera. If false, world-space X/Z is used.")]
    [SerializeField] private bool useCameraRelative = true;

    // runtime
    private Rigidbody rb;
    private Vector2 moveInput = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        // Read input in Update for responsiveness
        if (moveAction != null && moveAction.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }
    }

    private void FixedUpdate()
    {
        // Convert input to world-space direction
        Vector3 desiredDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (desiredDirection.sqrMagnitude < 0.0001f)
            return; // no meaningful input

        if (useCameraRelative && cameraTransform != null)
        {
            // Flatten camera forward/right to XZ plane
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            desiredDirection = (camRight * moveInput.x + camForward * moveInput.y);
        }

        if (desiredDirection.sqrMagnitude > 1f)
            desiredDirection.Normalize();

        if (useTorque)
        {
            // Compute a torque axis that makes the sphere roll toward desiredDirection.
            // Cross product with up gives an axis perpendicular to movement on XZ plane.
            Vector3 torqueAxis = Vector3.Cross(desiredDirection, Vector3.up);
            Vector3 torque = torqueAxis * moveForce * torqueMultiplier;
            rb.AddTorque(torque, ForceMode.Force);
        }
        else
        {
            rb.AddForce(desiredDirection * moveForce, ForceMode.Force);
        }

        // Clamp horizontal speed if requested
        if (maxSpeed > 0f)
        {
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float hSpeed = horizontalVel.magnitude;
            if (hSpeed > maxSpeed)
            {
                Vector3 limited = horizontalVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
            }
        }
    }

    // Optional: expose a small runtime API for other scripts
    public void SetMoveAction(InputActionReference action)
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Disable();

        moveAction = action;

        if (moveAction != null && moveAction.action != null && isActiveAndEnabled)
            moveAction.action.Enable();
    }
}

