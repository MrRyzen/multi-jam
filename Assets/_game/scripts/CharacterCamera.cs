using FishNet.Object;
using UnityEngine;

public class CharacterCamera : NetworkBehaviour
{
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)] [SerializeField] float yRotationLimit = 88f;

    private Vector2 rotation = Vector2.zero;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(!IsOwner)
            gameObject.SetActive(false);
    }

    public void UpdateWithInput(Vector2 rotationInput)
    {
        rotation.y += rotationInput.y;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

        transform.localRotation = yQuat; //Quaternions seem to rotate more consistently than EulerAngles. Sensitivity seemed to change slightly at certain degrees using Euler. transform.localEulerAngles = new Vector3(-rotation.y, rotation.x, 0);
    }
}