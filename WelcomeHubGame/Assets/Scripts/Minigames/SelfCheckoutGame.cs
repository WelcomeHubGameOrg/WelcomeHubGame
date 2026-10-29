using UnityEngine;

public class SelfCheckoutGame : MinigameController
{
    public Rigidbody holdObject;
    public Rigidbody cursorRb;
    public SpringJoint cursorSpring;

    private float _grabDepth;
    private Vector3 _mouseWorld;
    private Ray _mouseRay;
    
    public Camera mainCamera;

    private void Update()
    {
        if (holdObject)
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = _grabDepth;
            _mouseWorld = mainCamera.ScreenToWorldPoint(screenPos);
        }
        
        if ((Input.mousePosition.y > Screen.height || Input.mousePosition.x > Screen.width ||
             Input.mousePosition.x < 0 || Input.mousePosition.y < 0) && holdObject)
        {
            Release();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            _mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(_mouseRay, out RaycastHit hit) && hit.rigidbody)
            {
                Grab(hit);
            }
        }
        
        if (Input.GetMouseButtonUp(0) && holdObject)
        {
            Release();
        }
    }

    private void FixedUpdate()
    {
        cursorRb.MovePosition(_mouseWorld);
    }

    private void Release()
    {
        cursorSpring.connectedBody = null;
        holdObject = null;
    }

    private void Grab(RaycastHit hit)
    {
        holdObject = hit.rigidbody;
        
        Plane grabPlane = new Plane(Vector3.forward, Vector3.zero);
        
        Vector3 grabPoint = hit.point;
        if (grabPlane.Raycast(_mouseRay, out float distance))
        {
            grabPoint = _mouseRay.GetPoint(distance);
        }
                
        _grabDepth = Vector3.Dot(grabPoint - mainCamera.transform.position, mainCamera.transform.forward);
        _mouseWorld = grabPoint;
        cursorRb.position = grabPoint;
        cursorSpring.connectedAnchor = holdObject.transform.InverseTransformPoint(grabPoint);
        cursorSpring.connectedBody = holdObject;
        holdObject.WakeUp();
    }
}
