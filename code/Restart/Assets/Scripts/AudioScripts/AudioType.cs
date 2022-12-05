using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// For platforms that an object or a player can stand on or collide with. 
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AudioType : MonoBehaviour
{
    public SoundType soundType;
    public enum SoundType
    {
        WOOD, MUD, LEAVES, WATER
    }
}
