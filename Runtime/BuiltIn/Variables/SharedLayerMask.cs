﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner
{
    [Serializable]
    public class SharedLayerMask : SharedVariable<LayerMask>
    {
        public static implicit operator SharedLayerMask(LayerMask value)
        {
            return new SharedLayerMask {Value = value};
        }
        
        public static implicit operator SharedLayerMask(int value)
        {
            return new SharedLayerMask {Value = value};
        }
    }
}