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
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
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
// Creation Date: 2017.03.28
// Creation Time: 2:17 PM
// Creator: zameran
#endregion

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Samplers;
using SpaceEngine.Core.Utilities.Gradients;
using SpaceEngine.Ocean;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    /// <summary>
    /// Class - extensions holder for a <see cref="Body"/>.
    /// </summary>
    public static class BodyExtensions
    {
        /// <summary>
        /// Get the deformation type enumerator.
        /// </summary>
        /// <param name="body">Target.</param>
        /// <returns>Returns <see cref="BodyDeformationType"/> of target body.</returns>
        public static BodyDeformationType GetBodyDeformationType(this Body body)
        {
            if (body is CelestialBody)
            {
                return BodyDeformationType.Spherical;
            }

            return BodyDeformationType.Flat;
        }
    }


    public class Body : Node<Body>, IBody, IUniformed<MaterialPropertyBlock>, IReanimateable, IRenderable<Body>
    {
        public Atmosphere Atmosphere;
        public OceanNode Ocean;
        public Ring Ring;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
        public int RenderQueueOffset = 0;

        public bool DrawGizmos = false;

        public bool AtmosphereEnabled = true;
        public bool OceanEnabled = true;
        public bool RingEnabled = true;
        public bool TerrainEnabled = true;

        public int GridResolution = 25;

        public float Amplitude = 32.0f;
        public float Frequency = 64.0f;

        public Mesh QuadMesh;

        public Shader ColorShader;

        public List<TerrainNode> TerrainNodes = new List<TerrainNode>(6);
        public List<TileSampler> TileSamplers = new List<TileSampler>();
        public List<Shadow> Shadows = new List<Shadow>(255);

        [HideInInspector]
        public double HeightZ = 0;

        public float Size = 6360000.0f;

        public Vector3 Offset { get; set; }
        public Vector3 Origin { get { return transform.position; } set { transform.position = value; } }

        public TCCommonParametersSetter TCCPS = null;

        public MaterialPropertyBlock MPB { get; set; }

        public MaterialTableGradientLut MaterialTable = new MaterialTableGradientLut();

        #region Node

        protected override void InitNode()
        {
            if (Atmosphere != null)
            {
                if (Atmosphere.ParentBody == null)
                    Atmosphere.ParentBody = this;
            }

            if (Ocean != null)
            {
                if (Ocean.ParentBody == null)
                    Ocean.ParentBody = this;

                // NOTE : Reinvert particular value to switch matrices for ocean rendering... Not used at the moment...
                //StartCoroutine(Ocean.InitializationFix());
            }

            if (Ring != null)
            {
                if (Ring.ParentBody == null)
                    Ring.ParentBody = this;
            }

            QuadMesh = MeshFactory.MakePlane(GridResolution, MeshFactory.PLANE.XY, true, false, false);
            QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            TileSamplers = new List<TileSampler>(GetComponentsInChildren<TileSampler>());
            TileSamplers.Sort(new TileSampler.Sort());

            MPB = new MaterialPropertyBlock();

            MaterialTable.GenerateLut();
        }

        protected override void UpdateNode()
        {
            // NOTE : Self - rendering!
            Render();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Helper.Destroy(QuadMesh);

            MaterialTable.DestroyLut();
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public virtual void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.InitUniforms(target);
            }
        }

        public virtual void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.SetUniforms(target);
            }

            if (Ring != null)
            {
                Ring.SetShadows(MPB, Shadows);
            }
        }

        public virtual void InitSetUniforms()
        {
            InitUniforms(MPB);
            SetUniforms(MPB);
        }

        #endregion

        #region IReanimateable

        public virtual void Reanimate()
        {
            foreach (var terrainNode in TerrainNodes)
            {
                if (Helper.Enabled(terrainNode))
                {
                    if (Atmosphere != null)
                    {
                        Atmosphere.InitUniforms(terrainNode.TerrainMaterial);
                        Atmosphere.SetUniforms(terrainNode.TerrainMaterial);
                    }
                }
            }
        }

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 0)
        {
            if (Atmosphere != null)
            {
                if (AtmosphereEnabled)
                {
                    Atmosphere.Render();
                }

                foreach (var sunGlare in GodManager.Instance.Sunglares)
                {
                    sunGlare.Atmosphere = Atmosphere;
                    sunGlare.Render();
                }
            }

            if (Ocean != null)
            {
                if (OceanEnabled)
                {
                    Ocean.Render();
                }
            }

            if (Ring != null)
            {
                if (RingEnabled)
                {
                    Ring.Render();
                }
            }

            // NOTE : Update controller and the draw. This can help avoid terrain nodes jitter...
            if (GodManager.Instance.ActiveBody == this)
                GodManager.Instance.UpdateControllerWrapper();

            for (int i = 0; i < TileSamplers.Count; i++)
            {
                if (Helper.Enabled(TileSamplers[i]))
                {
                    TileSamplers[i].UpdateSampler();
                }
            }

            if (TerrainEnabled)
            {
                for (int i = 0; i < TerrainNodes.Count; i++)
                {
                    if (Helper.Enabled(TerrainNodes[i]))
                    {
                        DrawTerrain(TerrainNodes[i]);
                    }
                }
            }

            ResetMPB();
        }

        #endregion

        protected virtual void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus != true) return;

            Reanimate();

            if (Atmosphere != null) Atmosphere.Reanimate();
            if (Ocean != null) Ocean.Reanimate();
        }

        protected virtual void ResetMPB()
        {
            MPB.Clear();

            // TODO : How to set these values per quad avoiding material property block and material uniforms?
            // NOTE : So, only these uniforms are variable per quad, but i don't know how to vary avoiding mpb and material uniforms, maybe instancing?
            //_Elevation_Tile
            //_Ortho_Tile
            //_Color_Tile
            //_Normals_Tile
            //_Deform_Offset
            //_Deform_Camera
            //_Deform_ScreenQuadCornerNorms
            //_Deform_ScreenQuadCorners
            //_Deform_ScreenQuadVericals
            //_Deform_TangentFrameToWorld

            InitSetUniforms();
        }

        public virtual List<string> GetKeywords()
        {
            var Keywords = new List<string>();

            return Keywords;
        }

        private void DrawTerrain(TerrainNode node)
        {
            // Get all the samplers attached to the terrain node. The samples contain the data need to draw the quad
            var samplers = node.Samplers.Where(sampler => sampler.enabled && sampler.StoreLeaf).ToList();

            // So, if doesn't have any samplers - do anything...
            if (samplers.Count == 0) return;

            // Find all the quads in the terrain node that need to be drawn
            node.FindDrawableQuads(node.TerrainQuadRoot, samplers);

            // The draw them
            node.DrawQuad(node.TerrainQuadRoot, samplers, QuadMesh, MPB);
        }
    }
}