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
// Creation Date: 2017.09.10
// Creation Time: 5:04 PM
// Creator: zameran
#endregion

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Bodies;

using System;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public class DebugGUIAtmosphereSetup : DebugGUI
    {
        public Body Body { get { return GodManager.Instance.ActiveBody; } }

        public Atmosphere Atmosphere { get { return Body.Atmosphere; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Atmosphere Setup");
        }

        protected override void UI(int id)
        {
            GUILayoutExtensions.VerticalBoxed("Controls: ", GUISkin, () =>
            {
                GUILayout.Space(20);

                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    DrawApplyButton(() => { });
                });
            });

            GUILayout.Space(5);

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            if (Body != null && Helper.Enabled(Body))
            {
                if (Atmosphere != null && Helper.Enabled(Body.Atmosphere) && Body.AtmosphereEnabled)
                {
                    GUILayoutExtensions.VerticalBoxed("Realtime parameters: ", GUISkin, () =>
                    {
                        GUILayout.Space(20);

                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.VerticalBoxed("Preset: ", GUISkin, () =>
                            {
                                GUILayout.Space(20);

                                Atmosphere.AtmosphereBase = (AtmosphereBase)GUILayout.SelectionGrid((int)Atmosphere.AtmosphereBase, System.Enum.GetNames(typeof(AtmosphereBase)), 2);
                            });

                            GUILayout.Space(10);

                            GUILayoutExtensions.SliderWithField("Density: ", 0.0f, 1.0f, ref Atmosphere.Density);
                            GUILayoutExtensions.SliderWithField("Radius Hold (Terrain Radius)", 0.0f, 2048.0f, ref Atmosphere.TerrainRadiusHold, "0.00");
                            GUILayoutExtensions.SliderWithField("Scale", 0.01f, 16.0f, ref Atmosphere.Scale, "0.000");
                            GUILayoutExtensions.SliderWithField("Height: ", 0.0f, Body.Size / 512.0f, ref Atmosphere.Height);
                            GUILayoutExtensions.SliderWithField("Aerial Perspective Offset", 0.0f, 4096.0f, ref Atmosphere.AerialPerspectiveOffset, "0.00");
                            GUILayoutExtensions.SliderWithField("Extinction Ground Fade", 0.000025f, 0.1f, ref Atmosphere.ExtinctionGroundFade, "0.000000");
                            GUILayoutExtensions.SliderWithField("HDR Exposure", 0.0f, 1.0f, ref Atmosphere.HDRExposure, "0.00");
                        });
                    });

                    GUILayout.Space(5);
                }
                else
                {
                    GUILayoutExtensions.DrawBadHolder("Atmosphere Parameters: ", "No Atmosphere!?", GUISkin);
                }
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Body parameters: ", "No Body!?", GUISkin);
            }

            GUILayout.EndScrollView();
        }

        protected void DrawApplyButton(Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button("Apply", options))
            {
                if (action != null) action();
            }
        }
    }
}