using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct VectorD3
{
    public double x;
    public double y;
    public double z;

    public VectorD3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static VectorD3 operator +(VectorD3 a, VectorD3 b)
    {
        return new VectorD3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static VectorD3 operator -(VectorD3 a, VectorD3 b)
    {
        return new VectorD3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static VectorD3 operator *(VectorD3 a, double b)
    {
        return new VectorD3(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3 operator *(Quaternion q, VectorD3 v)
    {
        return q * (Vector3)v;
    }

    public static VectorD3 operator /(VectorD3 a, double b)
    {
        return new VectorD3(a.x / b, a.y / b, a.z / b);
    }

    public static explicit operator Vector3(VectorD3 dv)
    {
        return new Vector3((float)dv.x, (float)dv.y, (float)dv.z);
    }

    public static explicit operator VectorD3(Vector3 v)
    {
        return new VectorD3((double)v.x, (double)v.y, (double)v.z);
    }

    public static explicit operator VectorD3(Vector3Int v)
    {
        return new VectorD3((double)v.x, (double)v.y, (double)v.z);
    }

    public static double Distance(VectorD3 a, VectorD3 b)
    {
        return (b - a).magnitude();
    }
    
    public double magnitude()
    {
        return Math.Sqrt(x*x+ y*y + z*z);
    }

    public VectorD3 normalized()
    {
        return this / this.magnitude();
    }

    public double Dot(VectorD3 other)
    {
        return x * other.x + y * other.y + z * other.z;
    }

    public static readonly VectorD3 zero = new VectorD3(0, 0, 0);
}

[Serializable]
public struct VectorD2
{
    public double x;
    public double y;

    public VectorD2(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public static VectorD2 operator +(VectorD2 a, VectorD2 b)
    {
        return new VectorD2(a.x + b.x, a.y + b.y);
    }

    public static VectorD2 operator -(VectorD2 a, VectorD2 b)
    {
        return new VectorD2(a.x - b.x, a.y - b.y);
    }

    public static VectorD2 operator *(VectorD2 a, double b)
    {
        return new VectorD2(a.x * b, a.y * b);
    }

    public static readonly VectorD2 zero = new VectorD2(0, 0);
}