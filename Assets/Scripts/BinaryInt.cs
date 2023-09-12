using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BinaryInt
{
    public bool[] bits;

    public BinaryInt(bool[] bits)
    {
        this.bits = bits;
    }

    public static BinaryInt operator +(BinaryInt a, BinaryInt b)
    {
        BinaryInt result = BinaryInt.zero;

        bool carry = false;
        for (int i = 0; i < a.bits.Length; i++)
        {
            var aBit = a.bits[i];
            var bBit = b.bits[i];
            if ((aBit ^ bBit) ^ carry)
            {
                result = result.rConcat(true);
            }
            else
            {
                result = result.rConcat(false);
            }

            if (((aBit || bBit) && carry) || aBit && bBit)
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

    public static BinaryInt operator -(BinaryInt a, BinaryInt b)
    {
        BinaryInt result = BinaryInt.zero;

        result = a + b*-1;
        return result;
    }

    public static BinaryInt operator *(BinaryInt a, int x)
    {
        if (x != 1 && x != -1)
        {
            Debug.LogError("Type BinaryInt can only be multiplied by values -1 and 1");
        }
        if (x == -1)
        {
            return a.Inverse() + BinaryInt.one;
        }
        return a;
    }

    public static explicit operator int(BinaryInt b)
    {
        int result = 0;
        for (int i = 0; i < b.bits.Length; i++) if (b.bits[i]) result += (int)Mathf.Pow(2, i);
        return result;
    }

    public static explicit operator string(BinaryInt b)
    {
        string result = "";
        for (int i = 0; i < b.bits.Length; i++) if (b.bits[i]) result += "1"; else result += "0";
        return result;
    }

    public override string ToString()
    {
        string st = (string)this;
        string result = "";
        bool end = false;
        for (int i = this.bits.Length - 1; i >= 0; i--) 
        {
            if (this.bits[i]) end = true;
            if (end)
            {
                if (this.bits[i]) result += "1";
                else result += "0";
            }
        }
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

    public BinaryInt rConcat(bool b)
    {
        bool[] nArr = new bool[this.bits.Length];
        for (int i = 0; i < this.bits.Length-1; i++)
        {
            nArr[i] = this.bits[i + 1];
        }
        nArr[this.bits.Length-1] = b;

        return new BinaryInt(nArr);
    }

    public BinaryInt Inverse()
    {
        bool[] nArr = new bool[this.bits.Length];
        for (int i = 1; i < this.bits.Length; i++)
        {
            nArr[i] = !this.bits[i];
        }

        return new BinaryInt(nArr);
    }

    public BinaryInt Reverse()
    {
        if ((int)this == 0)
        {
            return this;
        }
        BinaryInt result = BinaryInt.zero;
        bool end = false;
        for (int i = this.bits.Length - 1; i >= 0; i--)
        {
            if (this.bits[i]) end = true;
            if (end)
            {
                if (this.bits[i]) result.rConcat(true);
                else result += result.rConcat(false);
            }
        }
        
        while (result.bits[0] != false)
        {
            result.rConcat(false);
        }

        return result;
    }

    public static readonly BinaryInt zero = new BinaryInt(new bool[32]);
    public static readonly BinaryInt one = new BinaryInt(new bool[] { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false });
}
