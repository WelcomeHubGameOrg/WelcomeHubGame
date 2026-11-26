using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragObject : MonoBehaviour
{
    public LayerMask dragLayers;
    public LayerMask barcodeLayers;

    // to prevent the background UI from interfering
    [SerializeField] GraphicRaycaster backgroundUICaster;

    [Range(0.0f, 100.0f)] 
    public float damping = 1.0f;

    [Range(0.0f, 100.0f)] 
    public float frequency = 5.0f;
    
    private TargetJoint2D targetJoint;

    private void Update()
    {
        var currentMouse = Mouse.current;
        var worldPos = Camera.main.ScreenToWorldPoint(currentMouse.position.value);
        var targetCollider = Physics2D.OverlapPoint(worldPos, dragLayers);
        
        if (backgroundUICaster)
        {
            bool blockUI = (targetJoint) || (targetCollider);
            backgroundUICaster.enabled = !blockUI;
        }

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            if (Physics2D.OverlapPoint(worldPos, barcodeLayers)) return;
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
            if (!targetJoint) return;
            
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
