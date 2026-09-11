using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Events : MonoBehaviour, IInteractable
{
    /*private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interact();
        }
    }*/

    public abstract void Interact();
}
