using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Gear : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int GearType;
    public Color HighLightColor;
    public bool isStatic = false;
    [SerializeField] private float defaultRotationSpeed;
    [SerializeField] private float movementSpeed = 1;
    [HideInInspector] public float RotationSpeed => defaultRotationSpeed / ((CircleCollider2D) collider).radius;
    [HideInInspector] public float RotationDirection = 0;
    [HideInInspector] public bool moving;
    [HideInInspector] public Collider2D collider;
    private Vector2 target;
    private SpriteRenderer sr;
    private Camera mainCamera;


    private bool InDrag;
    private Transform defaultParent;
    private int defaultOrder;
    private Gear lastDetectedGear;
    private int dragOrder = 20;
    private int highlightOrder = 21;

    [Header("Sounds")] public AudioSource PutSound;
    public AudioSource CatchSound;
    public AudioSource SwapSound;

    private void Awake()
    {
        mainCamera = Camera.main;
        sr = GetComponentInChildren<SpriteRenderer>();
        collider = GetComponentInChildren<Collider2D>();
        defaultOrder = sr.sortingOrder;
    }

    IEnumerator Move()
    {
        var goal = target;
        moving = true;

        while (moving && !InDrag && GameField.Instance.GameInProgress && goal == target)
        {
            if ((Vector2) transform.position != goal)
            {
                transform.position = Vector2.MoveTowards(transform.position, goal,
                    movementSpeed * Time.deltaTime);
            }
            else
            {
                moving = false;
            }

            yield return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        InDrag = true;
        CatchSound.Play();
        defaultParent = transform.parent;
        sr.sortingOrder = dragOrder;
        collider.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        transform.position = (Vector2) mainCamera.ScreenToWorldPoint(eventData.position);
        Gear gear = CheckGearOverPointer(eventData);

        if (lastDetectedGear != null && lastDetectedGear != gear)
        {
            lastDetectedGear.HighLight(false);
        }

        lastDetectedGear = gear;
        if (lastDetectedGear != null)
        {
            lastDetectedGear.HighLight(true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InDrag = false;
        if (!CanDrag())
        {
            return;
        }

        collider.enabled = true;
        if (lastDetectedGear == null)
        {
            target = defaultParent.position;
            PutSound.Play();
            StartCoroutine(Move());
        }
        else
        {
            SwapGears(this, lastDetectedGear);
            lastDetectedGear.HighLight(false);
            lastDetectedGear = null;
        }

        sr.sortingOrder = defaultOrder;
    }

    private void SwapGears(Gear gear1, Gear gear2)
    {
        var parent1 = gear1.transform.parent;
        var parent2 = gear2.transform.parent;
        SwapSound.Play();
        (gear1.transform.parent, gear2.transform.parent) = (gear2.transform.parent, gear1.transform.parent);

        gear1.target = parent2.transform.position;
        gear2.target = parent1.transform.position;

        gear1.StartCoroutine(Move());
        gear2.StartCoroutine(gear2.Move());

        parent1.GetComponent<Axle>().CurrentGear = gear2;
        parent2.GetComponent<Axle>().CurrentGear = gear1;
    }

    private void HighLight(bool activate)
    {
        if (activate)
        {
            sr.color = HighLightColor;
            sr.sortingOrder = highlightOrder;
        }
        else
        {
            sr.color = Color.white;
            sr.sortingOrder = defaultOrder;
        }
    }

    private Gear CheckGearOverPointer(PointerEventData eventData)
    {
        var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(eventData.position), Vector3.zero,
            float.MaxValue,
            LayerMask.GetMask("Gear"));
        if (hit.collider == null)
        {
            return null;
        }

        return hit.collider.GetComponentInParent<Gear>();
    }

    private bool CanDrag()
    {
        return !isStatic && GameField.Instance.GameInProgress;
    }
}