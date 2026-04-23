using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SFXLabLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float HoldTime = 0.5f;
    public float MaxMoveDistance = 25f;
    public Action OnLongPress;

    float pressStart = -1f;
    Vector2 pressPos;
    bool fired;

    public void OnPointerDown(PointerEventData e)
    {
        pressStart = Time.unscaledTime;
        pressPos = e.position;
        fired = false;
    }

    public void OnPointerUp(PointerEventData e)
    {
        pressStart = -1f;
    }

    public void OnDrag(PointerEventData e)
    {
        // Any real drag cancels the hold — user is adjusting the slider, not holding to reset.
        if (Vector2.Distance(e.position, pressPos) > MaxMoveDistance)
            pressStart = -1f;
    }

    void Update()
    {
        if (pressStart < 0f || fired) return;
        if (Time.unscaledTime - pressStart >= HoldTime)
        {
            fired = true;
            pressStart = -1f;
            OnLongPress?.Invoke();
        }
    }
}
