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

namespace UnityEngine
{
    public struct Matrix2x2d
    {
        public readonly double[,] m;

        public Matrix2x2d(double m00, double m01, double m10, double m11)
        {
            m = new double[2, 2];

            m[0, 0] = m00;
            m[0, 1] = m01;
            m[1, 0] = m10;
            m[1, 1] = m11;
        }

        public override int GetHashCode()
        {
            return m[0, 0].GetHashCode() + m[1, 0].GetHashCode() +
                   m[0, 1].GetHashCode() + m[1, 1].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix2x2d) { return Equals((Matrix2x2d)obj); }
            return false;
        }

        public bool Equals(Matrix2x2d other)
        {
            return this == other;
        }

        public static Matrix2x2d operator +(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kSum = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
                }
            }

            return kSum;
        }

        public static Matrix2x2d operator -(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kSum = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
                }
            }

            return kSum;
        }

        public static Matrix2x2d operator *(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kProd = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] + m1.m[iRow, 1] * m2.m[1, iCol];
                }
            }

            return kProd;
        }

        public static Vector2d operator *(Matrix2x2d m, Vector2d v)
        {
            var kProd = Vector2d.zero;

            kProd.x = m.m[0, 0] * v.x + m.m[0, 1] * v.y;
            kProd.y = m.m[1, 0] * v.x + m.m[1, 1] * v.y;

            return kProd;
        }

        public static Matrix2x2d operator *(Matrix2x2d m, double s)
        {
            var kProd = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kProd.m[iRow, iCol] = m.m[iRow, iCol] * s;
                }
            }

            return kProd;
        }

        public static bool operator ==(Matrix2x2d m1, Matrix2x2d m2)
        {
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return false;
                }
            }

            return true;
        }

        public static bool operator !=(Matrix2x2d m1, Matrix2x2d m2)
        {
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return m[0, 0] + "," + m[0, 1] + "\n" + 
                   m[1, 0] + "," + m[1, 1];
        }

        public Matrix2x2d Transpose()
        {
            var kTranspose = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kTranspose.m[iRow, iCol] = m[iCol, iRow];
                }
            }

            return kTranspose;
        }

        private double Determinant()
        {
            return m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1];
        }

        public bool Inverse(ref Matrix2x2d mInv, double tolerance = 1e-06)
        {
            var det = Determinant();

            if (System.Math.Abs(det) <= tolerance) { return false; }

            var invDet = 1.0 / det;

            mInv.m[0, 0] = m[1, 1] * invDet;
            mInv.m[0, 1] = -m[0, 1] * invDet;
            mInv.m[1, 0] = -m[1, 0] * invDet;
            mInv.m[1, 1] = m[0, 0] * invDet;

            return true;
        }

        public Matrix2x2d Inverse(double tolerance = 1e-06)
        {
            var kInverse = Matrix2x2d.identity;

            Inverse(ref kInverse, tolerance);

            return kInverse;
        }

        public static Matrix2x2d identity { get { return new Matrix2x2d(1, 0, 0, 1); } }
    }
}