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
    public struct Matrix4x4d
    {
        public readonly double[,] m;

        public Matrix4x4d(double m00,
            double m01,
            double m02,
            double m03,
            double m10,
            double m11,
            double m12,
            double m13,
            double m20,
            double m21,
            double m22,
            double m23,
            double m30,
            double m31,
            double m32,
            double m33)
        {
            m = new double[4, 4];

            m[0, 0] = m00;
            m[0, 1] = m01;
            m[0, 2] = m02;
            m[0, 3] = m03;
            m[1, 0] = m10;
            m[1, 1] = m11;
            m[1, 2] = m12;
            m[1, 3] = m13;
            m[2, 0] = m20;
            m[2, 1] = m21;
            m[2, 2] = m22;
            m[2, 3] = m23;
            m[3, 0] = m30;
            m[3, 1] = m31;
            m[3, 2] = m32;
            m[3, 3] = m33;
        }

        public Matrix4x4d(Matrix4x4 mat)
        {
            m = new double[4, 4];

            m[0, 0] = mat.m00;
            m[0, 1] = mat.m01;
            m[0, 2] = mat.m02;
            m[0, 3] = mat.m03;
            m[1, 0] = mat.m10;
            m[1, 1] = mat.m11;
            m[1, 2] = mat.m12;
            m[1, 3] = mat.m13;
            m[2, 0] = mat.m20;
            m[2, 1] = mat.m21;
            m[2, 2] = mat.m22;
            m[2, 3] = mat.m23;
            m[3, 0] = mat.m30;
            m[3, 1] = mat.m31;
            m[3, 2] = mat.m32;
            m[3, 3] = mat.m33;
        }

        public override int GetHashCode()
        {
            return m[0, 0].GetHashCode() + m[1, 0].GetHashCode() + m[2, 0].GetHashCode() + m[3, 0].GetHashCode() +
                   m[0, 1].GetHashCode() + m[1, 1].GetHashCode() + m[2, 1].GetHashCode() + m[3, 1].GetHashCode() +
                   m[0, 2].GetHashCode() + m[1, 2].GetHashCode() + m[2, 2].GetHashCode() + m[3, 2].GetHashCode() +
                   m[0, 3].GetHashCode() + m[1, 3].GetHashCode() + m[2, 3].GetHashCode() + m[3, 3].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix4x4d) { return Equals((Matrix4x4d)obj); }
            return false;
        }

        public bool Equals(Matrix4x4d other)
        {
            return this == other;
        }

        public static Matrix4x4d operator +(Matrix4x4d m1, Matrix4x4d m2)
        {
            var kSum = Matrix4x4d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] + m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] + m2.m[0, 1];
            kSum.m[0, 2] = m1.m[0, 2] + m2.m[0, 2];
            kSum.m[0, 3] = m1.m[0, 3] + m2.m[0, 3];

            kSum.m[1, 0] = m1.m[1, 0] + m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] + m2.m[1, 1];
            kSum.m[1, 2] = m1.m[1, 2] + m2.m[1, 2];
            kSum.m[1, 3] = m1.m[1, 3] + m2.m[1, 3];

            kSum.m[2, 0] = m1.m[2, 0] + m2.m[2, 0];
            kSum.m[2, 1] = m1.m[2, 1] + m2.m[2, 1];
            kSum.m[2, 2] = m1.m[2, 2] + m2.m[2, 2];
            kSum.m[2, 3] = m1.m[2, 3] + m2.m[2, 3];

            kSum.m[3, 0] = m1.m[3, 0] + m2.m[3, 0];
            kSum.m[3, 1] = m1.m[3, 1] + m2.m[3, 1];
            kSum.m[3, 2] = m1.m[3, 2] + m2.m[3, 2];
            kSum.m[3, 3] = m1.m[3, 3] + m2.m[3, 3];
#else
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix4x4d operator -(Matrix4x4d m1, Matrix4x4d m2)
        {
            var kSum = Matrix4x4d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] - m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] - m2.m[0, 1];
            kSum.m[0, 2] = m1.m[0, 2] - m2.m[0, 2];
            kSum.m[0, 3] = m1.m[0, 3] - m2.m[0, 3];

            kSum.m[1, 0] = m1.m[1, 0] - m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] - m2.m[1, 1];
            kSum.m[1, 2] = m1.m[1, 2] - m2.m[1, 2];
            kSum.m[1, 3] = m1.m[1, 3] - m2.m[1, 3];

            kSum.m[2, 0] = m1.m[2, 0] - m2.m[2, 0];
            kSum.m[2, 1] = m1.m[2, 1] - m2.m[2, 1];
            kSum.m[2, 2] = m1.m[2, 2] - m2.m[2, 2];
            kSum.m[2, 3] = m1.m[2, 3] - m2.m[2, 3];

            kSum.m[3, 0] = m1.m[3, 0] - m2.m[3, 0];
            kSum.m[3, 1] = m1.m[3, 1] - m2.m[3, 1];
            kSum.m[3, 2] = m1.m[3, 2] - m2.m[3, 2];
            kSum.m[3, 3] = m1.m[3, 3] - m2.m[3, 3];
#else
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix4x4d operator *(Matrix4x4d m1, Matrix4x4d m2)
        {
            var kProd = Matrix4x4d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m1.m[0, 0] * m2.m[0, 0] + m1.m[0, 1] * m2.m[1, 0] + m1.m[0, 2] * m2.m[2, 0] + m1.m[0, 3] * m2.m[3, 0];
            kProd.m[0, 1] = m1.m[0, 0] * m2.m[0, 1] + m1.m[0, 1] * m2.m[1, 1] + m1.m[0, 2] * m2.m[2, 1] + m1.m[0, 3] * m2.m[3, 1];
            kProd.m[0, 2] = m1.m[0, 0] * m2.m[0, 2] + m1.m[0, 1] * m2.m[1, 2] + m1.m[0, 2] * m2.m[2, 2] + m1.m[0, 3] * m2.m[3, 2];
            kProd.m[0, 3] = m1.m[0, 0] * m2.m[0, 3] + m1.m[0, 1] * m2.m[1, 3] + m1.m[0, 2] * m2.m[2, 3] + m1.m[0, 3] * m2.m[3, 3];

            kProd.m[1, 0] = m1.m[1, 0] * m2.m[0, 0] + m1.m[1, 1] * m2.m[1, 0] + m1.m[1, 2] * m2.m[2, 0] + m1.m[1, 3] * m2.m[3, 0];
            kProd.m[1, 1] = m1.m[1, 0] * m2.m[0, 1] + m1.m[1, 1] * m2.m[1, 1] + m1.m[1, 2] * m2.m[2, 1] + m1.m[1, 3] * m2.m[3, 1];
            kProd.m[1, 2] = m1.m[1, 0] * m2.m[0, 2] + m1.m[1, 1] * m2.m[1, 2] + m1.m[1, 2] * m2.m[2, 2] + m1.m[1, 3] * m2.m[3, 2];
            kProd.m[1, 3] = m1.m[1, 0] * m2.m[0, 3] + m1.m[1, 1] * m2.m[1, 3] + m1.m[1, 2] * m2.m[2, 3] + m1.m[1, 3] * m2.m[3, 3];

            kProd.m[2, 0] = m1.m[2, 0] * m2.m[0, 0] + m1.m[2, 1] * m2.m[1, 0] + m1.m[2, 2] * m2.m[2, 0] + m1.m[2, 3] * m2.m[3, 0];
            kProd.m[2, 1] = m1.m[2, 0] * m2.m[0, 1] + m1.m[2, 1] * m2.m[1, 1] + m1.m[2, 2] * m2.m[2, 1] + m1.m[2, 3] * m2.m[3, 1];
            kProd.m[2, 2] = m1.m[2, 0] * m2.m[0, 2] + m1.m[2, 1] * m2.m[1, 2] + m1.m[2, 2] * m2.m[2, 2] + m1.m[2, 3] * m2.m[3, 2];
            kProd.m[2, 3] = m1.m[2, 0] * m2.m[0, 3] + m1.m[2, 1] * m2.m[1, 3] + m1.m[2, 2] * m2.m[2, 3] + m1.m[2, 3] * m2.m[3, 3];

            kProd.m[3, 0] = m1.m[3, 0] * m2.m[0, 0] + m1.m[3, 1] * m2.m[1, 0] + m1.m[3, 2] * m2.m[2, 0] + m1.m[3, 3] * m2.m[3, 0];
            kProd.m[3, 1] = m1.m[3, 0] * m2.m[0, 1] + m1.m[3, 1] * m2.m[1, 1] + m1.m[3, 2] * m2.m[2, 1] + m1.m[3, 3] * m2.m[3, 1];
            kProd.m[3, 2] = m1.m[3, 0] * m2.m[0, 2] + m1.m[3, 1] * m2.m[1, 2] + m1.m[3, 2] * m2.m[2, 2] + m1.m[3, 3] * m2.m[3, 2];
            kProd.m[3, 3] = m1.m[3, 0] * m2.m[0, 3] + m1.m[3, 1] * m2.m[1, 3] + m1.m[3, 2] * m2.m[2, 3] + m1.m[3, 3] * m2.m[3, 3];
#else
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] + m1.m[iRow, 1] * m2.m[1, iCol] + m1.m[iRow, 2] * m2.m[2, iCol] + m1.m[iRow, 3] * m2.m[3, iCol];
                }
            }
#endif

            return kProd;
        }

        public static Vector3d operator *(Matrix4x4d m, Vector3d v)
        {
            var fInvW = 1.0 / (m.m[3, 0] * v.x + m.m[3, 1] * v.y + m.m[3, 2] * v.z + m.m[3, 3]);

            return new Vector3d
            {
                x = (m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z + m.m[0, 3]) * fInvW,
                y = (m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z + m.m[1, 3]) * fInvW,
                z = (m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z + m.m[2, 3]) * fInvW
            };
        }

        public static Vector4d operator *(Matrix4x4d m, Vector4d v)
        {
            return new Vector4d
            {
                x = m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z + m.m[0, 3] * v.w,
                y = m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z + m.m[1, 3] * v.w,
                z = m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z + m.m[2, 3] * v.w
            };
        }

        public static Matrix4x4d operator *(Matrix4x4d m, double s)
        {
            var kProd = Matrix4x4d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m.m[0, 0] * s;
            kProd.m[0, 1] = m.m[0, 1] * s;
            kProd.m[0, 2] = m.m[0, 2] * s;
            kProd.m[0, 3] = m.m[0, 3] * s;

            kProd.m[1, 0] = m.m[1, 0] * s;
            kProd.m[1, 1] = m.m[1, 1] * s;
            kProd.m[1, 2] = m.m[1, 2] * s;
            kProd.m[1, 3] = m.m[1, 3] * s;

            kProd.m[2, 0] = m.m[2, 0] * s;
            kProd.m[2, 1] = m.m[2, 1] * s;
            kProd.m[2, 2] = m.m[2, 2] * s;
            kProd.m[2, 3] = m.m[2, 3] * s;

            kProd.m[3, 0] = m.m[3, 0] * s;
            kProd.m[3, 1] = m.m[3, 1] * s;
            kProd.m[3, 2] = m.m[3, 2] * s;
            kProd.m[3, 3] = m.m[3, 3] * s;
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

        public static bool operator ==(Matrix4x4d m1, Matrix4x4d m2)
        {
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return false;
                }
            }

            return true;
        }

        public static bool operator !=(Matrix4x4d m1, Matrix4x4d m2)
        {
            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return true;
                }
            }

            return false;
        }

        public static implicit operator Matrix4x4(Matrix4x4d m)
        {
            var matrix = Matrix4x4.identity;

            matrix.SetRow(0, m.GetRow(0));
            matrix.SetRow(1, m.GetRow(1));
            matrix.SetRow(2, m.GetRow(2));
            matrix.SetRow(3, m.GetRow(3));

            return matrix;
        }

        public static implicit operator Matrix4x4d(Matrix4x4 m)
        {
            var matrix = Matrix4x4d.identity;

            matrix.SetRow(0, m.GetRow(0));
            matrix.SetRow(1, m.GetRow(1));
            matrix.SetRow(2, m.GetRow(2));
            matrix.SetRow(3, m.GetRow(3));

            return matrix;
        }


        public override string ToString()
        {
            return m[0, 0] + "," + m[0, 1] + "," + m[0, 2] + "," + m[0, 3] + "\n" +
                   m[1, 0] + "," + m[1, 1] + "," + m[1, 2] + "," + m[1, 3] + "\n" +
                   m[2, 0] + "," + m[2, 1] + "," + m[2, 2] + "," + m[2, 3] + "\n" +
                   m[3, 0] + "," + m[3, 1] + "," + m[3, 2] + "," + m[3, 3];
        }

        public Matrix4x4d Transpose()
        {
            var kTranspose = Matrix4x4d.identity;

            for (byte iRow = 0; iRow < 4; iRow++)
            {
                for (byte iCol = 0; iCol < 4; iCol++)
                {
                    kTranspose.m[iRow, iCol] = m[iCol, iRow];
                }
            }

            return kTranspose;
        }

        private double Minor(int r0, int r1, int r2, int c0, int c1, int c2)
        {
            return m[r0, c0] * (m[r1, c1] * m[r2, c2] - m[r2, c1] * m[r1, c2]) -
                   m[r0, c1] * (m[r1, c0] * m[r2, c2] - m[r2, c0] * m[r1, c2]) +
                   m[r0, c2] * (m[r1, c0] * m[r2, c1] - m[r2, c0] * m[r1, c1]);
        }

        private double Determinant()
        {
            return m[0, 0] * Minor(1, 2, 3, 1, 2, 3) - m[0, 1] * Minor(1, 2, 3, 0, 2, 3) + m[0, 2] * Minor(1, 2, 3, 0, 1, 3) - m[0, 3] * Minor(1, 2, 3, 0, 1, 2);
        }

        private Matrix4x4d Adjoint()
        {
            return new Matrix4x4d(Minor(1, 2, 3, 1, 2, 3), -Minor(0, 2, 3, 1, 2, 3), Minor(0, 1, 3, 1, 2, 3), -Minor(0, 1, 2, 1, 2, 3),
                                  -Minor(1, 2, 3, 0, 2, 3), Minor(0, 2, 3, 0, 2, 3), -Minor(0, 1, 3, 0, 2, 3), Minor(0, 1, 2, 0, 2, 3),
                                  Minor(1, 2, 3, 0, 1, 3), -Minor(0, 2, 3, 0, 1, 3), Minor(0, 1, 3, 0, 1, 3), -Minor(0, 1, 2, 0, 1, 3),
                                  -Minor(1, 2, 3, 0, 1, 2), Minor(0, 2, 3, 0, 1, 2), -Minor(0, 1, 3, 0, 1, 2), Minor(0, 1, 2, 0, 1, 2));
        }

        public Matrix4x4d Inverse()
        {
            return Adjoint() * (1.0f / Determinant());
        }

        public Vector4d GetColumn(int iCol)
        {
            return new Vector4d(m[0, iCol], m[1, iCol], m[2, iCol], m[3, iCol]);
        }

        public void SetColumn(int iCol, Vector4d v)
        {
            m[0, iCol] = v.x;
            m[1, iCol] = v.y;
            m[2, iCol] = v.z;
            m[3, iCol] = v.w;
        }

        public Vector4d GetRow(int iRow)
        {
            return new Vector4d(m[iRow, 0], m[iRow, 1], m[iRow, 2], m[iRow, 3]);
        }

        public void SetRow(int iRow, Vector4d v)
        {
            m[iRow, 0] = v.x;
            m[iRow, 1] = v.y;
            m[iRow, 2] = v.z;
            m[iRow, 3] = v.w;
        }

        public Matrix4x4 ToMatrix4x4()
        {
            var mat = new Matrix4x4
            {
                m00 = (float)m[0, 0],
                m01 = (float)m[0, 1],
                m02 = (float)m[0, 2],
                m03 = (float)m[0, 3],
                m10 = (float)m[1, 0],
                m11 = (float)m[1, 1],
                m12 = (float)m[1, 2],
                m13 = (float)m[1, 3],
                m20 = (float)m[2, 0],
                m21 = (float)m[2, 1],
                m22 = (float)m[2, 2],
                m23 = (float)m[2, 3],
                m30 = (float)m[3, 0],
                m31 = (float)m[3, 1],
                m32 = (float)m[3, 2],
                m33 = (float)m[3, 3]
            };


            return mat;
        }

        public Matrix3x3d ToMatrix3x3d()
        {
            var mat = Matrix3x3d.identity;

            mat.m[0, 0] = m[0, 0];
            mat.m[0, 1] = m[0, 1];
            mat.m[0, 2] = m[0, 2];
            mat.m[1, 0] = m[1, 0];
            mat.m[1, 1] = m[1, 1];
            mat.m[1, 2] = m[1, 2];
            mat.m[2, 0] = m[2, 0];
            mat.m[2, 1] = m[2, 1];
            mat.m[2, 2] = m[2, 2];

            return mat;
        }

        public static Matrix4x4d ToMatrix4x4d(Matrix4x4 matf)
        {
            var mat = Matrix4x4d.identity;

            mat.m[0, 0] = matf.m00;
            mat.m[0, 1] = matf.m01;
            mat.m[0, 2] = matf.m02;
            mat.m[0, 3] = matf.m03;
            mat.m[1, 0] = matf.m10;
            mat.m[1, 1] = matf.m11;
            mat.m[1, 2] = matf.m12;
            mat.m[1, 3] = matf.m13;
            mat.m[2, 0] = matf.m20;
            mat.m[2, 1] = matf.m21;
            mat.m[2, 2] = matf.m22;
            mat.m[2, 3] = matf.m23;
            mat.m[3, 0] = matf.m30;
            mat.m[3, 1] = matf.m31;
            mat.m[3, 2] = matf.m32;
            mat.m[3, 3] = matf.m33;

            return mat;
        }

        public static Matrix4x4d Translate(Vector3d v)
        {
            return new Matrix4x4d(1, 0, 0, v.x, 0, 1, 0, v.y, 0, 0, 1, v.z, 0, 0, 0, 1);
        }

        public static Matrix4x4d Translate(Vector3 v)
        {
            return new Matrix4x4d(1, 0, 0, v.x, 0, 1, 0, v.y, 0, 0, 1, v.z, 0, 0, 0, 1);
        }

        public static Matrix4x4d Scale(Vector3d v)
        {
            return new Matrix4x4d(v.x, 0, 0, 0, 0, v.y, 0, 0, 0, 0, v.z, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4d Scale(Vector3 v)
        {
            return new Matrix4x4d(v.x, 0, 0, 0, 0, v.y, 0, 0, 0, 0, v.z, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4d RotateX(double angle)
        {
            var ca = System.Math.Cos(angle * MathUtility.Deg2Rad);
            var sa = System.Math.Sin(angle * MathUtility.Deg2Rad);

            return new Matrix4x4d(1, 0, 0, 0, 0, ca, -sa, 0, 0, sa, ca, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4d RotateY(double angle)
        {
            var ca = System.Math.Cos(angle * MathUtility.Deg2Rad);
            var sa = System.Math.Sin(angle * MathUtility.Deg2Rad);

            return new Matrix4x4d(ca, 0, sa, 0, 0, 1, 0, 0, -sa, 0, ca, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4d RotateZ(double angle)
        {
            var ca = System.Math.Cos(angle * MathUtility.Deg2Rad);
            var sa = System.Math.Sin(angle * MathUtility.Deg2Rad);

            return new Matrix4x4d(ca, -sa, 0, 0, sa, ca, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4d Rotate(Vector3 rotation)
        {
            var x = new Quaternion4d(new Vector3d(1, 0, 0), rotation.x * MathUtility.Deg2Rad);
            var y = new Quaternion4d(new Vector3d(0, 1, 0), rotation.y * MathUtility.Deg2Rad);
            var z = new Quaternion4d(new Vector3d(0, 0, 1), rotation.z * MathUtility.Deg2Rad);

            return (z * y * x).ToMatrix4x4d();
        }

        public static Matrix4x4d Rotate(Vector3d rotation)
        {
            var x = new Quaternion4d(new Vector3d(1, 0, 0), rotation.x * MathUtility.Deg2Rad);
            var y = new Quaternion4d(new Vector3d(0, 1, 0), rotation.y * MathUtility.Deg2Rad);
            var z = new Quaternion4d(new Vector3d(0, 0, 1), rotation.z * MathUtility.Deg2Rad);

            return (z * y * x).ToMatrix4x4d();
        }

        public static Matrix4x4d Perspective(double fovy, double aspect, double zNear, double zFar)
        {
            var f = 1.0 / System.Math.Tan((fovy * MathUtility.Deg2Rad) / 2.0);

            return new Matrix4x4d(f / aspect, 0, 0, 0, 0, f, 0, 0, 0, 0, (zFar + zNear) / (zNear - zFar), (2.0 * zFar * zNear) / (zNear - zFar), 0, 0, -1, 0);
        }

        public static Matrix4x4d Ortho(double xRight, double xLeft, double yTop, double yBottom, double zNear, double zFar)
        {
            var tx = -(xRight + xLeft) / (xRight - xLeft);
            var ty = -(yTop + yBottom) / (yTop - yBottom);
            var tz = -(zFar + zNear) / (zFar - zNear);

            return new Matrix4x4d(2.0 / (xRight - xLeft), 0, 0, tx, 0, 2.0 / (yTop - yBottom), 0, ty, 0, 0, -2.0 / (zFar - zNear), tz, 0, 0, 0, 1);
        }

        public static Matrix4x4d identity { get { return new Matrix4x4d(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1); } }
    }
}