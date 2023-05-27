using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTapController : MonoBehaviour {

    [SerializeField] private LayerMask sushiMask;
    
    public void Tap(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Vector2 screenPositionOfTouch = context.ReadValue<Vector2>();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPositionOfTouch);
        bool didHit = Physics.Raycast(worldPosition, transform.forward, out RaycastHit hit, 
            Mathf.Infinity, sushiMask);
        if (didHit) {
            Sushi component = hit.transform.GetComponent<Sushi>();
            component.TappedOn();
        }
    }
}
