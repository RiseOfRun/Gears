using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Axle : MonoBehaviour
{
    [FormerlySerializedAs("DefaultMovingGearPrefab")] public Gear DefaultGearPrefab;
    [FormerlySerializedAs("TargetMovingGear")] [SerializeField] public Gear TargetGear;
    [HideInInspector] public Gear CurrentGear;
}
