﻿using System;
using UnityEngine;

namespace Scripts.Building
{
    public class EntryPoint : ICloneable
    {
        public string name;
        public Vector3 location;
        public Vector3 playerRotation;
        public bool movesOnStart;
        public Vector3 startMovement;

        public object Clone() => new EntryPoint
        {
            name = name,
            location = location,
            playerRotation = playerRotation,
            movesOnStart = movesOnStart,
            startMovement = startMovement,
        };
    }
}