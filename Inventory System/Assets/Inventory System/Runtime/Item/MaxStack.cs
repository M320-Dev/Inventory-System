using System;
using UnityEngine;

namespace InventorySystem.Runtime
{
    [Serializable]
    public struct MaxStack
    {
        [SerializeField] private bool m_isStackable;
        [SerializeField, Min(1)] private int m_value;

        public readonly bool IsStackable => m_isStackable;

        private MaxStack(int value) 
        {
            if (value < 1) throw new ArgumentOutOfRangeException(
                nameof(value), value, "MaxStack value must be positive.");

            m_isStackable = value > 1;
            m_value = value;
        }

        public static MaxStack Unstackable = new(1);

        public static implicit operator MaxStack(int value) => new(value);
        public static implicit operator int(MaxStack maxStack) => maxStack.m_value;
    }
}
