using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButtonHandler : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int index;
    public string type;

    [Header("Tuning")]
    [SerializeField] private float holdTime = 0.1f;      // seconds to lock hold
    [SerializeField] private float baseDeadZone = 20f;   // dp baseline

    // internals
    private Vector2 initialTouchPos;
    private bool isPointerDown = false;
    private bool holdActivated = false;
    private bool cancelledByDrag = false;
    private Coroutine holdCoroutine;
    private float deadZone;

    private void Awake()
    {
        float dpi = Screen.dpi;
        if (dpi <= 0) dpi = 160f;
        // gentler DPI scaling so deadZone isn't huge on high-DPI devices
        deadZone = baseDeadZone * Mathf.Sqrt(dpi / 160f);
        Debug.Log($"[HoldButtonHandler] deadZone={deadZone:F1}px, holdTime={holdTime:F2}s");
    }

    // -----------------------
    // Pointer / hold lifecycle
    // -----------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        initialTouchPos = eventData.position;
        isPointerDown = true;
        holdActivated = false;
        cancelledByDrag = false;

        Debug.Log($"[HoldButtonHandler] PointerDown idx={index} pos={eventData.position}");

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(HoldTimerCoroutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[HoldButtonHandler] PointerUp idx={index} holdActivated={holdActivated} cancelledByDrag={cancelledByDrag}");

        // If hold was activated, stop the hold action
        if (holdActivated)
        {
            EndHold();
        }
        else
        {
            // If hold never activated and not cancelled by a drag, this was a simple tap.
            // (If you want a single-tap buy behavior, trigger it here.)
            // Example: one-tap buys once (uncomment if desired):
            // If hold never activated and not cancelled by a drag, this was a simple tap.
            if (!cancelledByDrag)
            {
                if (type == "Upgrade")
                {
                    UpgradeManager.instance.OnUpgradeButtonDown(index);
                    UpgradeManager.instance.OnUpgradeButtonUp();
                }
                else if (type == "Power")
                {
                    Power1.instance.OnPowerButtonDown(index);
                    Power1.instance.OnPowerButtonUp();
                }
            }
        }

        isPointerDown = false;
        cancelledByDrag = false;

        if (holdCoroutine != null) { StopCoroutine(holdCoroutine); holdCoroutine = null; }
    }

    // -----------------------
    // Drag forwarding
    // -----------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        // If hold already activated, we want to consume/ignore drags.
        if (holdActivated)
        {
            // consume the event so ScrollRect doesn't also react
            eventData.Use();
            Debug.Log($"[HoldButtonHandler] BeginDrag consumed by active hold idx={index}");
            return;
        }

        // If hold isn't activated yet, treat this as a scrolling intent.
        // Cancel hold and forward begin drag to parent ScrollRect (if any).
        cancelledByDrag = true;
        CancelHoldTimer();

        ScrollRect sr = GetComponentInParent<ScrollRect>();
        if (sr != null)
        {
            // Forward BeginDrag to the ScrollRect so it starts scrolling
            ExecuteEvents.ExecuteHierarchy(sr.gameObject, eventData, ExecuteEvents.beginDragHandler);
            Debug.Log($"[HoldButtonHandler] Forwarded BeginDrag to ScrollRect for idx={index}");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (holdActivated)
        {
            // If hold activated, ignore/consume drag (prevents scroll)
            eventData.Use();
            return;
        }

        // If dragging started before hold activation, forward drag to parent ScrollRect
        if (cancelledByDrag)
        {
            ScrollRect sr = GetComponentInParent<ScrollRect>();
            if (sr != null)
            {
                ExecuteEvents.ExecuteHierarchy(sr.gameObject, eventData, ExecuteEvents.dragHandler);
                // no debug flood
            }
        }
        else
        {
            // still within potential hold; but detect large movement and cancel early
            float distance = Vector2.Distance(initialTouchPos, eventData.position);
            if (distance > deadZone)
            {
                cancelledByDrag = true;
                CancelHoldTimer();

                ScrollRect sr = GetComponentInParent<ScrollRect>();
                if (sr != null)
                    ExecuteEvents.ExecuteHierarchy(sr.gameObject, eventData, ExecuteEvents.beginDragHandler);

                Debug.Log($"[HoldButtonHandler] Movement exceeded deadZone ({distance:F1}px > {deadZone:F1}px). Cancelling hold and forwarding drag.");
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (holdActivated)
        {
            // consume
            eventData.Use();
            Debug.Log($"[HoldButtonHandler] EndDrag consumed by active hold idx={index}");
            return;
        }

        if (cancelledByDrag)
        {
            ScrollRect sr = GetComponentInParent<ScrollRect>();
            if (sr != null)
                ExecuteEvents.ExecuteHierarchy(sr.gameObject, eventData, ExecuteEvents.endDragHandler);
            Debug.Log($"[HoldButtonHandler] Forwarded EndDrag to ScrollRect for idx={index}");
        }
    }

    // -----------------------
    // Hold timer & activation
    // -----------------------
    private IEnumerator HoldTimerCoroutine()
    {
        yield return new WaitForSeconds(holdTime);

        // only activate if pointer still down and not cancelled by drag
        if (isPointerDown && !cancelledByDrag)
        {
            holdActivated = true;
            Debug.Log($"[HoldButtonHandler] HOLD ACTIVATED idx={index}");

            // start hold action
            if (type == "Upgrade")
                UpgradeManager.instance.OnUpgradeButtonDown(index);
            else if (type == "Power")
                Power1.instance.OnPowerButtonDown(index);
        }
        else
        {
            Debug.Log("[HoldButtonHandler] HoldTimer ended but was cancelled or pointer up.");
        }
    }

    private void CancelHoldTimer()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private void EndHold()
    {
        // stop hold action
        if (type == "Upgrade")
            UpgradeManager.instance.OnUpgradeButtonUp();
        else if (type == "Power")
            Power1.instance.OnPowerButtonUp();

        holdActivated = false;
        CancelHoldTimer();
        Debug.Log($"[HoldButtonHandler] HOLD ENDED idx={index}");
    }
}
