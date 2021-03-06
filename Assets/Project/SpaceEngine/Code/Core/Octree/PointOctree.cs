﻿#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
//  
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//  
// Creation Date: 2017.05.31
// Creation Time: 12:52 AM
// Creator: zameran
#endregion

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Octree
{
    public class PointOctree<T> where T : class
    {
        /// <summary>
        /// The total amount of objects currently in the tree.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Root node of the octree.
        /// </summary>
        PointOctreeNode<T> RootNode;

        /// <summary>
        /// Size that the octree was on creation.
        /// </summary>
        private readonly float InitialSize;

        /// <summary>
        /// Minimum side length.
        /// </summary>
        readonly float MinSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
        /// <param name="initialWorldPos">Position of the centre of the initial node.</param>
        /// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
        public PointOctree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Debug.LogWarning(string.Format("Minimum node size must be at least as big as the initial world size. Was: {0} Adjusted to: {1}", minNodeSize, initialWorldSize));

                minNodeSize = initialWorldSize;
            }

            Count = 0;
            InitialSize = initialWorldSize;
            MinSize = minNodeSize;
            RootNode = new PointOctreeNode<T>(InitialSize, MinSize, initialWorldPos);
        }

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="position">Position of the object.</param>
        public void Add(T obj, Vector3 position)
        {
            // Safety check against infinite/excessive growth
            byte count = 0;

            while (!RootNode.Add(obj, position))
            {
                Grow(position - RootNode.Center);

                if (++count > 32)
                {
                    Debug.LogError(string.Format("Aborted Add operation as it seemed to be going on forever ({0}) attempts at growing the octree.", count - 1));

                    return;
                }
            }

            Count++;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            var removed = RootNode.Remove(obj);

            // See if we can shrink the octree down now that we've removed the item...
            if (removed)
            {
                Count--;

                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Return objects that are within maxDistance of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(Ray ray, float maxDistance)
        {
            var collidingWith = new List<T>();

            RootNode.GetNearby(ref ray, ref maxDistance, collidingWith);

            return collidingWith.ToArray();
        }

        /// <summary>
        /// Draws node boundaries visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
        /// </summary>
        public void DrawAllBounds()
        {
            RootNode.DrawAllBounds();
        }

        /// <summary>
        /// Draws the bounds of all objects in the tree visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
        /// </summary>
        public void DrawAllObjects()
        {
            RootNode.DrawAllObjects();
        }

        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        public void Grow(Vector3 direction)
        {
            sbyte xDirection = direction.x >= 0 ? (sbyte)1 : (sbyte)-1;
            sbyte yDirection = direction.y >= 0 ? (sbyte)1 : (sbyte)-1;
            sbyte zDirection = direction.z >= 0 ? (sbyte)1 : (sbyte)-1;

            PointOctreeNode<T> oldRoot = RootNode;

            var half = RootNode.SideLength / 2;
            var newLength = RootNode.SideLength * 2;

            var newCenter = RootNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            RootNode = new PointOctreeNode<T>(newLength, MinSize, newCenter);

            // Create 7 new octree children to go with the old root as children of the new root...
            var rootPos = GetRootPosIndex(xDirection, yDirection, zDirection);

            PointOctreeNode<T>[] children = new PointOctreeNode<T>[8];

            for (byte i = 0; i < 8; i++)
            {
                if (i == rootPos)
                {
                    children[i] = oldRoot;
                }
                else
                {
                    xDirection = i % 2 == 0 ? (sbyte)-1 : (sbyte)1;
                    yDirection = i > 3 ? (sbyte)-1 : (sbyte)1;
                    zDirection = (i < 2 || (i > 3 && i < 6)) ? (sbyte)-1 : (sbyte)1;

                    children[i] = new PointOctreeNode<T>(RootNode.SideLength, MinSize, newCenter + new Vector3(xDirection * half, 
                                                                                                               yDirection * half, 
                                                                                                               zDirection * half));
                }
            }

            RootNode.SetChildren(children);
        }

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
        /// </summary>
        public void Shrink()
        {
            RootNode = RootNode.ShrinkIfPossible(InitialSize);
        }

        /// <summary>
        /// Used when growing the octree. 
        /// Works out where the old root node would fit inside a new, larger root node.
        /// </summary>
        /// <param name="xDir">X direction of growth [1 || -1].</param>
        /// <param name="yDir">Y direction of growth [1 || -1].</param>
        /// <param name="zDir">Z direction of growth [1 || -1].</param>
        /// <returns>Octant, where the root node should be.</returns>
        private static byte GetRootPosIndex(sbyte xDir, sbyte yDir, sbyte zDir)
        {
            byte result = xDir > 0 ? (byte)1 : (byte)0;

            if (yDir < 0) result += 4;
            if (zDir > 0) result += 2;

            return result;
        }
    }
}