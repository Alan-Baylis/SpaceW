﻿#region License
//
// Procedural planet renderer.
// Copyright (c) 2008-2011 INRIA
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Proland is distributed under a dual-license scheme.
// You can obtain a specific license from Inria: proland-licensing@inria.fr.
//
// Authors: Justin Hawkins 2014.
// Modified by Denis Ovchinnikov 2015-2017
#endregion

#define MATICES_UNROLL

namespace UnityEngine
{
    public struct Matrix3x3d
    {
        public readonly double[,] m;

        public Matrix3x3d(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            m = new double[3, 3];

            m[0, 0] = m00;
            m[0, 1] = m01;
            m[0, 2] = m02;
            m[1, 0] = m10;
            m[1, 1] = m11;
            m[1, 2] = m12;
            m[2, 0] = m20;
            m[2, 1] = m21;
            m[2, 2] = m22;
        }

        public override int GetHashCode()
        {
            return m[0, 0].GetHashCode() + m[1, 0].GetHashCode() + m[2, 0].GetHashCode() +
                   m[0, 1].GetHashCode() + m[1, 1].GetHashCode() + m[2, 1].GetHashCode() +
                   m[0, 2].GetHashCode() + m[1, 2].GetHashCode() + m[2, 2].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3x3d) { return Equals((Matrix3x3d)obj); }
            return false;
        }

        public bool Equals(Matrix3x3d other)
        {
            return this == other;
        }

        public static Matrix3x3d operator +(Matrix3x3d m1, Matrix3x3d m2)
        {
            var kSum = Matrix3x3d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] + m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] + m2.m[0, 1];
            kSum.m[0, 2] = m1.m[0, 2] + m2.m[0, 2];

            kSum.m[1, 0] = m1.m[1, 0] + m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] + m2.m[1, 1];
            kSum.m[1, 2] = m1.m[1, 2] + m2.m[1, 2];

            kSum.m[2, 0] = m1.m[2, 0] + m2.m[2, 0];
            kSum.m[2, 1] = m1.m[2, 1] + m2.m[2, 1];
            kSum.m[2, 2] = m1.m[2, 2] + m2.m[2, 2];
#else
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix3x3d operator -(Matrix3x3d m1, Matrix3x3d m2)
        {
            var kSum = Matrix3x3d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] - m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] - m2.m[0, 1];
            kSum.m[0, 2] = m1.m[0, 2] - m2.m[0, 2];

            kSum.m[1, 0] = m1.m[1, 0] - m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] - m2.m[1, 1];
            kSum.m[1, 2] = m1.m[1, 2] - m2.m[1, 2];

            kSum.m[2, 0] = m1.m[2, 0] - m2.m[2, 0];
            kSum.m[2, 1] = m1.m[2, 1] - m2.m[2, 1];
            kSum.m[2, 2] = m1.m[2, 2] - m2.m[2, 2];
#else
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix3x3d operator *(Matrix3x3d m1, Matrix3x3d m2)
        {
            var kProd = Matrix3x3d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m1.m[0, 0] * m2.m[0, 0] + m1.m[0, 1] * m2.m[1, 0] + m1.m[0, 2] * m2.m[2, 0];
            kProd.m[0, 1] = m1.m[0, 0] * m2.m[0, 1] + m1.m[0, 1] * m2.m[1, 1] + m1.m[0, 2] * m2.m[2, 1];
            kProd.m[0, 2] = m1.m[0, 0] * m2.m[0, 2] + m1.m[0, 1] * m2.m[1, 2] + m1.m[0, 2] * m2.m[2, 2];

            kProd.m[1, 0] = m1.m[1, 0] * m2.m[0, 0] + m1.m[1, 1] * m2.m[1, 0] + m1.m[1, 2] * m2.m[2, 0];
            kProd.m[1, 1] = m1.m[1, 0] * m2.m[0, 1] + m1.m[1, 1] * m2.m[1, 1] + m1.m[1, 2] * m2.m[2, 1];
            kProd.m[1, 2] = m1.m[1, 0] * m2.m[0, 2] + m1.m[1, 1] * m2.m[1, 2] + m1.m[1, 2] * m2.m[2, 2];

            kProd.m[2, 0] = m1.m[2, 0] * m2.m[0, 0] + m1.m[2, 1] * m2.m[1, 0] + m1.m[2, 2] * m2.m[2, 0];
            kProd.m[2, 1] = m1.m[2, 0] * m2.m[0, 1] + m1.m[2, 1] * m2.m[1, 1] + m1.m[2, 2] * m2.m[2, 1];
            kProd.m[2, 2] = m1.m[2, 0] * m2.m[0, 2] + m1.m[2, 1] * m2.m[1, 2] + m1.m[2, 2] * m2.m[2, 2];
#else
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] + m1.m[iRow, 1] * m2.m[1, iCol] + m1.m[iRow, 2] * m2.m[2, iCol];
                }
            }
#endif

            return kProd;
        }

        public static Vector3d operator *(Matrix3x3d m, Vector3d v)
        {
            return new Vector3d
            {
                x = m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z,
                y = m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z,
                z = m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z
            };
        }

        public static Matrix3x3d operator *(Matrix3x3d m, double s)
        {
            var kProd = Matrix3x3d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m.m[0, 0] * s;
            kProd.m[0, 1] = m.m[0, 1] * s;
            kProd.m[0, 2] = m.m[0, 2] * s;

            kProd.m[1, 0] = m.m[1, 0] * s;
            kProd.m[1, 1] = m.m[1, 1] * s;
            kProd.m[1, 2] = m.m[1, 2] * s;

            kProd.m[2, 0] = m.m[2, 0] * s;
            kProd.m[2, 1] = m.m[2, 1] * s;
            kProd.m[2, 2] = m.m[2, 2] * s;
#else
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    kProd.m[iRow, iCol] = m.m[iRow, iCol] * s;
                }
            }
#endif

            return kProd;
        }

        public static bool operator ==(Matrix3x3d m1, Matrix3x3d m2)
        {
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return false;
                }
            }

            return true;
        }

        public static bool operator !=(Matrix3x3d m1, Matrix3x3d m2)
        {
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return m[0, 0] + "," + m[0, 1] + "," + m[0, 2] + "\n" +
                   m[1, 0] + "," + m[1, 1] + "," + m[1, 2] + "\n" +
                   m[2, 0] + "," + m[2, 1] + "," + m[2, 2];
        }

        public Matrix3x3d Transpose()
        {
            var kTranspose = Matrix3x3d.identity;

            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    kTranspose.m[iRow, iCol] = m[iCol, iRow];
                }
            }

            return kTranspose;
        }

        private double Determinant()
        {
            var fCofactor00 = m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1];
            var fCofactor10 = m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2];
            var fCofactor20 = m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0];

            var fDet = m[0, 0] * fCofactor00 + m[0, 1] * fCofactor10 + m[0, 2] * fCofactor20;

            return fDet;
        }

        public bool Inverse(ref Matrix3x3d mInv, double tolerance = 1e-06)
        {
            // Invert a 3x3 using cofactors. 
            // This is about 8 times faster than the Numerical Recipes code which uses Gaussian elimination.
            mInv.m[0, 0] = m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1];
            mInv.m[0, 1] = m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2];
            mInv.m[0, 2] = m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1];
            mInv.m[1, 0] = m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2];
            mInv.m[1, 1] = m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0];
            mInv.m[1, 2] = m[0, 2] * m[1, 0] - m[0, 0] * m[1, 2];
            mInv.m[2, 0] = m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0];
            mInv.m[2, 1] = m[0, 1] * m[2, 0] - m[0, 0] * m[2, 1];
            mInv.m[2, 2] = m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];

            var fDet = m[0, 0] * mInv.m[0, 0] + m[0, 1] * mInv.m[1, 0] + m[0, 2] * mInv.m[2, 0];

            if (System.Math.Abs(fDet) <= tolerance) { return false; }

            var fInvDet = 1.0 / fDet;

#if (MATICES_UNROLL)
            mInv.m[0, 0] *= fInvDet;
            mInv.m[0, 1] *= fInvDet;
            mInv.m[0, 2] *= fInvDet;

            mInv.m[1, 0] *= fInvDet;
            mInv.m[1, 1] *= fInvDet;
            mInv.m[1, 2] *= fInvDet;

            mInv.m[2, 0] *= fInvDet;
            mInv.m[2, 1] *= fInvDet;
            mInv.m[2, 2] *= fInvDet;
#else
            for (byte iRow = 0; iRow < 3; iRow++)
            {
                for (byte iCol = 0; iCol < 3; iCol++)
                {
                    mInv.m[iRow, iCol] *= fInvDet;
                }
            }
#endif

            return true;
        }

        public Matrix3x3d Inverse(double tolerance = 1e-06)
        {
            var kInverse = Matrix3x3d.identity;

            Inverse(ref kInverse, tolerance);

            return kInverse;
        }

        public Vector3d GetColumn(int iCol)
        {
            return new Vector3d(m[0, iCol], m[1, iCol], m[2, iCol]);
        }

        public void SetColumn(int iCol, Vector3d v)
        {
            m[0, iCol] = v.x;
            m[1, iCol] = v.y;
            m[2, iCol] = v.z;
        }

        public Vector3d GetRow(int iRow)
        {
            return new Vector3d(m[iRow, 0], m[iRow, 1], m[iRow, 2]);
        }

        public void SetRow(int iRow, Vector3d v)
        {
            m[iRow, 0] = v.x;
            m[iRow, 1] = v.y;
            m[iRow, 2] = v.z;
        }

        public Matrix4x4d ToMatrix4x4d()
        {
            return new Matrix4x4d(m[0, 0], m[0, 1], m[0, 2], 0.0, m[1, 0], m[1, 1], m[1, 2], 0.0, m[2, 0], m[2, 1], m[2, 2], 0.0, 0.0, 0.0, 0.0, 0.0);
        }

        public Matrix4x4 ToMatrix4x4()
        {
            var mat = new Matrix4x4
            {
                m00 = (float)m[0, 0],
                m01 = (float)m[0, 1],
                m02 = (float)m[0, 2],
                m10 = (float)m[1, 0],
                m11 = (float)m[1, 1],
                m12 = (float)m[1, 2],
                m20 = (float)m[2, 0],
                m21 = (float)m[2, 1],
                m22 = (float)m[2, 2]
            };

            return mat;
        }

        public static Matrix3x3d identity { get { return new Matrix3x3d(1, 0, 0, 0, 1, 0, 0, 0, 1); } }
    }
}