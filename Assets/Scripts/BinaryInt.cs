using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BinaryInt
{
    public bool[] bits;

    public BinaryInt(bool[] bits = null)
    {
        if (bits == null)
        {
            this.bits = zero;
        }
        else
        {
            this.bits = bits;
        }
    }

    public static BinaryInt operator +(BinaryInt a, BinaryInt b)
    {
        BinaryInt result = new BinaryInt();

        bool carry = false;
        for (int i = 0; i < a.bits.Length; i++)
        {
            var aBit = a.bits[i];
            var bBit = b.bits[i];
            if ((aBit ^ bBit) ^ carry)
            {
                result.bits[i] = true;
            }
            if (aBit && bBit)
            {
                carry = true;
            }
            else
            {
                carry = false;
            }
        }
        return result;
    }

    public static explicit operator int(BinaryInt b)
    {
        int result = 0;
        for (int i = 0; i < b.bits.Length; i++) if (b.bits[i]) result += (int)Mathf.Pow(2, i);
        return result;
    }

    public BinaryInt Concat(bool b)
    {
        bool[] nArr = new bool[this.bits.Length];
        for (int i = 1; i < this.bits.Length; i++)
        {
            nArr[i] = this.bits[i-1];
        }
        nArr[0] = b;

        return new BinaryInt(nArr);
    }

    public static readonly bool[] zero = new bool[32];
}
