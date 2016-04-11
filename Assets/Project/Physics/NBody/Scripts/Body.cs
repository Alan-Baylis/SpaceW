﻿#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;

using UnityEngine;

namespace NBody
{
    /// <summary>
    /// Represents a massive body in the simulation. 
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Returns the radius defined for the given mass value. 
        /// </summary>
        /// <param name="mass">The mass to calculate a radius for.</param>
        /// <returns>The radius defined for the given mass value.</returns>
        public static double GetRadius(double mass)
        {
            // We assume all bodies have the same density so volume is directly 
            // proportion to mass. Then we use the inverse of the equation for the 
            // volume of a sphere to solve for the radius. The end result is arbitrarily 
            // scaled and added to a constant so the Body is generally visible. 
            return 10 * Math.Pow(3 * mass / (4 * Math.PI), 1 / 3.0) + 10;
        }

        /// <summary>
        /// The spatial location of the body. 
        /// </summary>
        public pVector3d Location = pVector3d.Zero;
        
        /// <summary>
        /// The velocity of the body. 
        /// </summary>
        public pVector3d Velocity = pVector3d.Zero;

        /// <summary>
        /// The acceleration accumulated for the body during a single simulation 
        /// step. 
        /// </summary>
        public pVector3d Acceleration = pVector3d.Zero;

        /// <summary>
        /// The mass of the body. 
        /// </summary>
        public double Mass;

        /// <summary>
        /// The radius of the body. 
        /// </summary>
        public double Radius
        {
            get
            {
                return GetRadius(Mass);
            }
        }

        /// <summary>
        /// Constructs a body with the given mass. All other properties are assigned 
        /// default values of zero. 
        /// </summary>
        /// <param name="mass">The mass of the new body.</param>
        public Body(double mass)
        {
            Mass = mass;
        }

        /// <summary>
        /// Constructs a body with the given location, mass, and velocity. 
        /// Unspecified properties are assigned default values of zero except for
        /// mass, which is given the value 1e6.
        /// </summary>
        /// <param name="location">The location of the new body.</param>
        /// <param name="mass">The mass of the new body.</param>
        /// <param name="velocity">The velocity of the new body.</param>
        public Body(pVector3d location, double mass = 1e6, pVector3d velocity = new pVector3d())
            : this(mass)
        {
            Location = location;
            Velocity = velocity;
        }

        /// <summary>
        /// Updates the properties of the body such as location, velocity, and 
        /// applied acceleration. This method should be invoked at each time step. 
        /// </summary>
        public void Update()
        {
            Simulate(out Location, ref Acceleration);
        }

        public void Simulate(out pVector3d location, ref pVector3d acceleration)
        {
            double speed = Velocity.Magnitude();

            location = this.Location;

            if (speed > World.C)
            {
                Velocity = World.C * Velocity.Unit();
                speed = World.C;
            }

            if (speed == 0)
                Velocity += acceleration * World.S;
            else
            {
                // Apply relativistic velocity addition. 
                pVector3d parallelAcc = pVector3d.Projection(acceleration, Velocity);
                pVector3d orthogonalAcc = pVector3d.Rejection(acceleration, Velocity);

                double alpha = Math.Sqrt(1 - Math.Pow(speed / World.C, 2));

                Velocity = (Velocity + parallelAcc + alpha * orthogonalAcc) /
                           (1 + pVector3d.Dot(Velocity, acceleration) /
                           (World.C * World.C));
            }

            location += Velocity;
            acceleration = pVector3d.Zero;
        }

        /// <summary>
        /// Rotates the body along an arbitrary axis. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of rotation</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(pVector3d point, pVector3d direction, double angle)
        {
            Location = Location.Rotate(point, direction, angle);

            // To rotate velocity and acceleration we have to adjust for the starting 
            // point for the axis of rotation. This way the vectors are effectively 
            // rotated about their own starting points. 
            Velocity += point;
            Velocity = Velocity.Rotate(point, direction, angle);
            Velocity -= point;

            Acceleration += point;
            Acceleration = Acceleration.Rotate(point, direction, angle);
            Acceleration -= point;
        }
    }
}