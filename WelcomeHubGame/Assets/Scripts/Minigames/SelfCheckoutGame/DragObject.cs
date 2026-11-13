using UnityEngine;
using UnityEngine.InputSystem;

public class DragObject : MonoBehaviour
{
    public LayerMask dragLayers;

    [Range(0.0f, 100.0f)] 
    public float damping = 1.0f;

    [Range(0.0f, 100.0f)] 
    public float frequency = 5.0f;
    
    private TargetJoint2D targetJoint;

    private void Update()
    {
        var currentMouse = Mouse.current;
        var worldPos = Camera.main.ScreenToWorldPoint(currentMouse.position.value);

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            var targetCollider = Physics2D.OverlapPoint(worldPos, dragLayers);
            if (!targetCollider) return;
            
            var body = targetCollider.attachedRigidbody;
            if (!body) return;

            targetJoint = body.gameObject.AddComponent<TargetJoint2D>();
            targetJoint.dampingRatio = damping;
            targetJoint.frequency = frequency;

            targetJoint.anchor = targetJoint.transform.InverseTransformPoint(worldPos);
        }
        else if (currentMouse.leftButton.wasReleasedThisFrame)
        {
            Destroy(targetJoint);
            targetJoint = null;
            return;
        }

        if (targetJoint)
        {
            targetJoint.target = worldPos;
            // if (drawDragLine) Debug.DrawLine(targetJoint.transform.TransformPoint(targetJoint.anchor), worldPos, color);
        }
    }
}
