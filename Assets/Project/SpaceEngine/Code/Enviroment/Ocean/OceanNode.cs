﻿using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy.Eventit;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using System;
using System.Collections;
using UnityEngine;

namespace SpaceEngine.Ocean
{
    /// <summary>
    /// An AbstractTask to draw a flat or spherical ocean. This class provides the functions and data to draw a flat projected grid but nothing else.
    /// </summary>
    public abstract class OceanNode : Node<OceanNode>, IUniformed<Material>, IUniformed<MaterialPropertyBlock>, IReanimateable, IEventit, IRenderable<OceanNode>
    {
        public Body ParentBody;

        public Shader OceanShader;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
        public int RenderQueueOffset = 0;

        [SerializeField]
        protected Material OceanMaterial;

        [SerializeField]
        protected Color UpwellingColor = new Color(0.039f, 0.156f, 0.47f);

        /// <summary>
        /// Sea level in meters.
        /// </summary>
        public float OceanLevel = 5.0f;

        /// <summary>
        /// The maximum altitude at which the ocean must be displayed.
        /// </summary>
        [SerializeField]
        public float ZMin = 20000.0f;

        /// <summary>
        /// Size of each grid in the projected grid. (number of pixels on screen).
        /// </summary>
        [SerializeField]
        protected int Resolution = 4;

        Mesh[] ScreenMeshGrids;
        Matrix4x4d OldLocalToOcean;

        Vector3d ux = Vector3d.zero;
        Vector3d uy = Vector3d.zero;
        Vector3d uz = Vector3d.zero;
        Vector3d oo = Vector3d.zero;

        protected Vector4 Offset;

        /// <summary>
        /// If the ocean should be draw. To minimize depth fighting the ocean is not draw when the camera is far away. 
        /// Instead the terrain shader should render the ocean areas directly on the terrain
        /// </summary>
        public bool DrawOcean { get; protected set; }

        public Vector3d Origin { get { return ParentBody != null ? ParentBody.Origin : transform.position; } }

        protected const string FFT_KEYWORD = "OCEAN_FFT";
        protected const string WHITECAPS_KEYWORD = "OCEAN_WHITECAPS";

        /// <summary>
        /// Concrete classes must provide a function that returns the variance of the waves need for the BRDF rendering of waves.
        /// </summary>
        /// <returns></returns>
        public abstract float GetMaxSlopeVariance();

        #region OceanNode

        protected abstract void InitOceanNode();

        protected abstract void UpdateOceanNode();

        #endregion

        #region Node

        protected override void InitNode()
        {
            InitOceanNode();

            OceanMaterial = MaterialHelper.CreateTemp(OceanShader, "Ocean");

            ParentBody.Atmosphere.InitUniforms(OceanMaterial);

            OldLocalToOcean = Matrix4x4d.identity;
            Offset = Vector4.zero;

            // Create the projected grid. The resolution is the size in pixels of each square in the grid. 
            // If the squares are small the size of the mesh will exceed the max verts for a mesh in Unity. In this case split the mesh up into smaller meshes.
            Resolution = Mathf.Max(1, Resolution);

            // The number of squares in the grid on the x and y axis
            var NX = Screen.width / Resolution;
            var NY = Screen.height / Resolution;
            var numGrids = 1;

            const int MAX_VERTS = 65000;

            // The number of meshes need to make a grid of this resolution
            if (NX * NY > MAX_VERTS)
            {
                numGrids += (NX * NY) / MAX_VERTS;
            }

            ScreenMeshGrids = new Mesh[numGrids];

            // Make the meshes. The end product will be a grid of verts that cover the screen on the x and y axis with the z depth at 0. 
            // This grid is then projected as the ocean by the shader
            for (byte i = 0; i < numGrids; i++)
            {
                NY = Screen.height / numGrids / Resolution;

                ScreenMeshGrids[i] = MeshFactory.MakeOceanPlane(NX, NY, (float)i / (float)numGrids, 1.0f / (float)numGrids);
                ScreenMeshGrids[i].bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
            }
        }

        protected override void UpdateNode()
        {
            UpdateOceanNode();

            OceanMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            // Calculates the required data for the projected grid
            var cameraToWorld = GodManager.Instance.CameraToWorld;
            var oceanFrame = cameraToWorld * Vector3d.zero; // Camera in local space

            var radius = ParentBody.Size;
            var trueAltitude = Vector3d.Distance(GodManager.Instance.WorldCameraPos, Origin) - radius;

            if ((radius > 0.0 && trueAltitude > ZMin) ||
                (radius < 0.0 && new Vector2d(oceanFrame.y, oceanFrame.z).Magnitude() < -radius - ZMin))
            {
                OldLocalToOcean = Matrix4x4d.identity;
                Offset = Vector4.zero;
                DrawOcean = false;

                return;
            }

            DrawOcean = true;

            UpdateMatrices();
        }

        private void UpdateMatrices()
        {
            var cameraToWorld = GodManager.Instance.CameraToWorld;
            var worldToLocal = new Matrix4x4d(1.0, 0.0, 0.0, -Origin.x,
                                              0.0, 1.0, 0.0, -Origin.y,
                                              0.0, 0.0, 1.0, -Origin.z,
                                              0.0, 0.0, 0.0, 1.0);
            var cameraToLocal = worldToLocal * cameraToWorld;
            var localToCamera = cameraToLocal.Inverse();
            var oceanFrame = cameraToLocal * Vector3d.zero;
            var radius = ParentBody.Size;

            uz = oceanFrame.Normalized();

            if (OldLocalToOcean != Matrix4x4d.identity)
            {
                ux = new Vector3d(OldLocalToOcean.m[1, 0], OldLocalToOcean.m[1, 1], OldLocalToOcean.m[1, 2]).Cross(uz).Normalized();
            }
            else
            {
                ux = Vector3d.forward.Cross(uz).Normalized();
            }

            uy = uz.Cross(ux);
            oo = (uz * radius);

            var localToOcean = new Matrix4x4d(ux.x, ux.y, ux.z, -ux.Dot(oo),
                                              uy.x, uy.y, uy.z, -uy.Dot(oo),
                                              uz.x, uz.y, uz.z, -uz.Dot(oo),
                                              0.0, 0.0, 0.0, 1.0);
            var cameraToOcean = localToOcean * cameraToLocal;
            //var worldToOcean = localToOcean * worldToLocal;   // Useless at the moment...

            if (OldLocalToOcean != Matrix4x4d.identity)
            {
                var delta = localToOcean * (OldLocalToOcean.Inverse() * Vector3d.zero);

                Offset += VectorHelper.MakeFrom(delta, 0.0f);
            }

            if (Mathf.Max(Mathf.Abs(Offset.x), Mathf.Abs(Offset.y)) > 20000.0f)
            {
                Offset.x = 0.0f;
                Offset.y = 0.0f;
            }

            OldLocalToOcean = localToOcean;

            var oc = cameraToOcean * Vector3d.zero;
            var h = oc.z;

            if (double.IsNaN(h)) { h = 1.0; }

            var offset = new Vector3d(-Offset.x, -Offset.y, h);

            var sunDirection = ParentBody.Atmosphere.GetSunDirection(ParentBody.Atmosphere.Suns[0]);
            var oceanSunDirection = localToOcean.ToMatrix3x3d() * sunDirection;

            var sphereDirection = (localToCamera * Vector3d.zero).Normalized();   // Direction to center of planet			
            var OHL = (localToCamera * Vector3d.zero).Magnitude();                // Distance to center of planet
            var rHorizon = Math.Sqrt(OHL * OHL - radius * radius);                // Distance to the horizon, i.e distance to ocean sphere tangent

            // Theta equals angle to horizon, now all that is left to do is check the view direction against this angle
            var cosTheta = rHorizon / (OHL);
            var sinTheta = Math.Sqrt(1 - cosTheta * cosTheta);

            OceanMaterial.SetVector("_SphereDirection", sphereDirection.ToVector3());
            OceanMaterial.SetFloat("_CosTheta", (float)cosTheta);
            OceanMaterial.SetFloat("_SinTheta", (float)sinTheta);

            OceanMaterial.SetVector("_Ocean_SunDir", oceanSunDirection.ToVector3());
            OceanMaterial.SetMatrix("_Ocean_CameraToOcean", cameraToOcean.ToMatrix4x4());
            OceanMaterial.SetMatrix("_Ocean_OceanToCamera", cameraToOcean.Inverse().ToMatrix4x4());
            OceanMaterial.SetVector("_Ocean_CameraPos", offset.ToVector3());
            OceanMaterial.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            OceanMaterial.SetVector("_Ocean_ScreenGridSize", new Vector2((float)Resolution / (float)Screen.width, (float)Resolution / (float)Screen.height));
            OceanMaterial.SetFloat("_Ocean_Radius", radius);

            // TODO : Complete ocean matrices calculation for a cyllindrical worlds...
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            Eventit();

            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            UnEventit();

            Helper.Destroy(OceanMaterial);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed<Material>

        public virtual void InitUniforms(Material target)
        {
            if (target == null) return;
        }

        public virtual void SetUniforms(Material target)
        {
            if (target == null) return;

            target.SetFloat("_Ocean_Sigma", GetMaxSlopeVariance());
            target.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            target.SetFloat("_Ocean_DrawBRDF", DrawOcean ? 0.0f : 1.0f);
            target.SetFloat("_Ocean_Level", OceanLevel);
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public virtual void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;
        }

        public virtual void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            target.SetFloat("_Ocean_Sigma", GetMaxSlopeVariance());
            target.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            target.SetFloat("_Ocean_DrawBRDF", DrawOcean ? 0.0f : 1.0f);
            target.SetFloat("_Ocean_Level", OceanLevel);
        }

        #endregion

        #region IUniformed

        public virtual void InitSetUniforms()
        {
            InitUniforms(OceanMaterial);
            SetUniforms(OceanMaterial);
        }

        #endregion

        #region IReanimateable

        public void Reanimate()
        {
            InitNode();
        }

        #endregion

        #region Eventit

        public bool isEventit { get; set; }

        public void Eventit()
        {
            if (isEventit) return;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent += OnAtmosphereBaked;

            isEventit = true;
        }

        public void UnEventit()
        {
            if (!isEventit) return;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent -= OnAtmosphereBaked;

            isEventit = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event must be overridden in every inherited class, at any level of inheritance.
        /// Should provide <see cref="UpdateNode"/> call.
        /// </summary>
        /// <param name="celestialBody">Celestial body.</param>
        /// <param name="atmosphere">Atmosphere.</param>
        protected abstract void OnAtmosphereBaked(Body celestialBody, Atmosphere atmosphere);

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 0)
        {
            if (DrawOcean == false) return;

            foreach (var mesh in ScreenMeshGrids)
            {
                if (mesh == null) break;

                Graphics.DrawMesh(mesh, Matrix4x4.identity, OceanMaterial, layer, CameraHelper.Main(), 0, ParentBody.MPB);
            }
        }

        #endregion

        /// <summary>
        /// Inverting <see cref="ZMin"/> with interval to switch the matrices for rendering.
        /// </summary>
        /// <returns><see cref="Yielders.EndOfFrame"/></returns>
        [Obsolete("Don't use it! Looks like a useless bugfix.")]
        public IEnumerator InitializationFix()
        {
            ZMin *= -1.0f;
            yield return Yielders.EndOfFrame;
            ZMin *= -1.0f;
        }
    }
}