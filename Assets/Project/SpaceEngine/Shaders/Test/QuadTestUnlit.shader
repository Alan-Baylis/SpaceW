﻿// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

Shader "SpaceEngine/Test/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_PlanetUVSampler("Planet UV Sampler (RGBA)", 2D) = "white" {}
		_QuadTexture1("QuadTexture 1 (RGB)", 2D) = "white" {}
		_QuadTexture2("QuadTexture 2 (RGB)", 2D) = "white" {}
		_QuadTexture3("QuadTexture 3 (RGB)", 2D) = "white" {}
		_QuadTexture4("QuadTexture 4 (RGB)", 2D) = "white" {}
		_DrawNormals("Draw Normals?", Range(0, 1)) = 0.0
		_DrawQuadTree("Draw Quad Tree?", Range(0, 1)) = 0.0
	}
	SubShader
	{
		Pass
		{
			Name "Quad"
			Tags
			{ 
				"Queue" = "Geometry" 
				"RenderType" = "Opaque" 
				"IgnoreProjector" = "True" 
			}

			ZWrite On
			ZTest LEqual 
			Cull Back
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag
			//#pragma geometry geom 
			// TODO : Move to another shader, vert - move vertices, geom - debug lines, frag - debug lines coloring.

			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF

			#include "UnityCG.cginc"
			#include "../SpaceStuff.cginc"
			#include "../Eclipses.cginc"
			#include "../TCCommon.cginc"
			#include "../HDR.cginc"
			#include "../Atmosphere.cginc"
			#include "../LogarithmicDepthBuffer.cginc"
			#include "../Ocean/OceanBRDF.cginc"

			struct appdata_full_compute 
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;

				uint id : SV_VertexID;
			};

			struct v2fg
			{
				float2 uv0 : TEXCOORD0;
				float3 normal0 : NORMAL0;
				float4 vertex0 : POSITION0;
				float4 vertex1 : POSITION1;
				float4 vertex2 : POSITION2;
				float4 vertex3 : POSITION3;
				float4 tangent0 : TANGENT0;
				float depth : DEPTH;

				float3 direction : TEXCOORD1;
				float3 relativeDirection : TEXCOORD2;
			};

			uniform float _DrawNormals;
			uniform float _DrawQuadTree;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;
			uniform sampler2D _PlanetUVSampler;

			uniform sampler2D _QuadTexture1;
			uniform sampler2D _QuadTexture2;
			uniform sampler2D _QuadTexture3;
			uniform sampler2D _QuadTexture4;

			uniform StructuredBuffer<OutputStruct> data;
			uniform StructuredBuffer<QuadGenerationConstants> quadGenerationConstants;

			uniform float4x4 _TRS;

			uniform float _LODLevel;
			uniform float _ID;
			uniform float _TerrainMaxHeight;

			uniform float _Ocean_Sigma;
			uniform float3 _Ocean_Color;
			uniform float _Ocean_DrawBRDF;
			uniform float _Ocean_Level;

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}

			inline float4 GroundFinalColorWithoutAtmosphere(float4 terrainColor, float3 p, float n, float3 WSD)
			{
				float cTheta = dot(n, -WSD);

				return terrainColor * max(cTheta, 0);
			}

			inline float4 GroundFinalColorWithAtmosphere(float4 terrainColor, float3 p, float3 n, float3 d, float3 WSD, float4 WSPR)
			{	
				float3 sunL = 0;
				float3 skyE = 0;
				float3 extinction = 0;
				float3 position = p; // NOTE : We need unshifted position for shadows stuff...

				// Unpack height from noise value...
				float height = DenormalizeMaximumHeight(terrainColor.a, _TerrainMaxHeight);
				float3 V = normalize(p);
				float3 P = V * max(length(p), Rg + 10.0);
				float3 v = normalize(P - _Globals_WorldCameraPos);

				p += _Globals_Origin;

				float cTheta = dot(n, -WSD);

				#ifdef OCEAN_ON
					if(height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
					{
						n = float3(0, 0, 1);
					}
				#endif
	
				SunRadianceAndSkyIrradiance(p, n, WSD, sunL, skyE);

				#ifdef ECLIPSES_ON
					float eclipse = 1;

					float3 invertedLightDistance = rsqrt(dot(WSPR.xyz, WSPR.xyz));
					float3 lightPosition = WSPR.xyz * invertedLightDistance;

					float lightAngularRadius = asin(WSPR.w * invertedLightDistance);

					eclipse *= EclipseShadow(p, lightPosition, lightAngularRadius);

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						float shadow = ShadowColor(float4(position, 1));
					#endif
				#endif

				float4 inscatter = InScattering(_Globals_WorldCameraPos_Offsetted, p, WSD, extinction, 1.0);

				#ifdef ECLIPSES_ON
					inscatter *= eclipse;

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif
				#endif

				#ifdef SHINE_ON
					inscatter += float4(SkyShineRadiance(p, d), 0.0);
				#endif

				float3 groundColor = 1.5 * RGB2Reflectance(terrainColor).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;

				#ifdef OCEAN_ON
					if(height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
					{
						groundColor = OceanRadiance(WSD, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color);
					}
				#endif

				#ifdef ECLIPSES_ON
					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						extinction = 1 * _ExtinctionGroundFade + (1 - _ExtinctionGroundFade) * extinction * eclipse * shadow;
					#else
						extinction = 1 * _ExtinctionGroundFade + (1 - _ExtinctionGroundFade) * extinction * eclipse;
					#endif
				#endif

				float4 finalColor = float4(groundColor, 1) * float4(extinction, 1) + inscatter;
				
				return finalColor;
			}

			void Account(in float4 terrainColor, out float4 scatteringColor, float3 p, float3 n, float3 d) //AtmosphereInToTheAccount
			{
				scatteringColor = 0;

				#ifdef LIGHT_1
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[0], _Sun_Positions_1[0]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[0]);
					#endif
				#endif

				#ifdef LIGHT_2	
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[0], _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[1], _Sun_Positions_1[1]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[0]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[1]);
					#endif
				#endif

				#ifdef LIGHT_3
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[0], _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[1], _Sun_Positions_1[1]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[2], _Sun_Positions_1[2]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[0]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[1]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[2]);
					#endif
				#endif

				#ifdef LIGHT_4
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[0], _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[1], _Sun_Positions_1[1]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[2], _Sun_Positions_1[2]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldDirections_1[3], _Sun_Positions_1[3]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[0]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[1]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[2]);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldDirections_1[3]);
					#endif
				#endif
			}

			float4 GetOverlayColorForID(float id)
			{
				if (_DrawQuadTree < 1.0) return float4(0.0, 0.0, 0.0, 0.0);

				if (id == 0) return float4(1.0, 0.0, 0.0, 0.0);
				else if (id == 1) return float4(0.0, 1.0, 0.0, 0.0);
				else if (id == 2) return float4(0.0, 0.0, 1.0, 0.0);
				else if (id == 3) return float4(0.0, 0.5, 0.5, 0.0);

				return float4(0.0, 0.0, 0.0, 0.0);
			}

			void vert(in appdata_full_compute v, out v2fg o)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 position = data[v.id].position;
				float4 cubePosition = data[v.id].cubePosition;

				float3 normal = tex2Dlod(_NormalTexture, v.texcoord) * 10;

				position.w = 1.0;
				position.xyz += patchCenter;

				cubePosition.w = 1.0;
				cubePosition.xyz += patchCenter;

				v.vertex = position;
				v.tangent = float4(FindTangent(normal, 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;

				o.uv0 = v.texcoord;
				o.normal0 =  mul(_TRS, v.normal);
				o.vertex0 = UnityObjectToClipPos(v.vertex);
				o.vertex1 = mul(unity_ObjectToWorld, v.vertex); // TODO : Apply Origin vector. //NOTE : Bug here!!!!!111
				o.vertex2 = v.vertex;
				o.vertex3 = cubePosition;
				o.tangent0 = v.tangent;
				o.depth = 1;

				o.direction = normalize(((_Globals_WorldCameraPos_Offsetted + _Globals_Origin) - mul(_Globals_CameraToWorld, o.vertex1)).xyz);
				o.relativeDirection = (_Globals_WorldCameraPos_Offsetted - o.vertex1.xyz);
				//Log. depth
				//o.vertex0.z = log2(max(1e-6, 1.0 + o.vertex0.w)) * (2.0 / log2(_ProjectionParams.z + 1.0)) - 1.0;
				//o.vertex0.z *= o.vertex0.w;
				//o.depth = log2(1.0 + o.vertex0.w) * (0.5 * (2.0 / log2(_ProjectionParams.z + 1.0)));

				//Log. depth new; FarPlane = _ProjectionParams.z | _ProjectionParams.w;
				//FarPlane = UNITY_MATRIX_P[2].w / (UNITY_MATRIX_P[2].z + 1.0);
				//o.vertex0.z = log2(max(1e-6, 1.0 + o.vertex0.w)) * FCoef(1e+2) - 1.0;
				//o.depth = 1.0 + o.vertex0.w;
			}

			[maxvertexcount(4)]
			void geom(line v2fg Input[2], inout LineStream<v2fg> OutputStream)
			{
				float3 P = Input[0].vertex2.xyz;
				float3 N = Input[0].normal0.xyz;

				float4 PositionA = UnityObjectToClipPos(float4(P, 1.0));
				float4 PositionB = UnityObjectToClipPos(float4(P + N * 1000, 1.0));

				v2fg a = Input[0];
				v2fg b = Input[0];

				a.vertex0 = PositionA;
				b.vertex0 = PositionB;

				OutputStream.Append(Input[0]);
				OutputStream.Append(Input[1]);

				OutputStream.Append(a);
				OutputStream.Append(b);
			}

			void frag(in v2fg IN, out float4 outDiffuse : COLOR)//, out float depth : DEPTH)
			{		
				QuadGenerationConstants constants = quadGenerationConstants[0];

				float3 normal = tex2D(_NormalTexture, IN.uv0).rgb;
				//normal.z = sqrt(1.0 - dot(normal.xy, normal.xy));

				float3x3 _TBN = CotangentFrame(normal, IN.vertex2.xyz, IN.uv0);

				normal = mul(_TRS, normal);
				//normal = mul(_TBN, normal);

				float3 vertexNormal = IN.normal0.xyz;

				float4 scatteringColor = 0;
				fixed4 terrainColor = tex2D(_HeightTexture, IN.uv0);
				fixed4 uvSamplerColor = tex2D(_PlanetUVSampler, IN.uv0); // Calculating UV's in fragment to prevent artifacts... CHEATER!
				fixed4 outputNormal = fixed4(normal, 1);

				float height = tex2D(_HeightTexture, IN.uv0).a;
				float slope = tex2D(_NormalTexture, IN.uv0).a;

				Account(terrainColor, scatteringColor, IN.vertex1.xyz, normal, IN.direction);

				//outDiffuse = triplanar;
				//outDiffuse = uvSamplerColor;
				//outDiffuse = lerp(scatteringColor, outputNormal, _DrawNormals);
				outDiffuse = lerp(lerp(scatteringColor, outputNormal, _DrawNormals), GetOverlayColorForID(_ID), _DrawQuadTree);
				//depth = log2(IN.depth) * (0.5 * FCoef(1e+2));
				//depth = 0;
			}

			ENDCG
		}

		/*
		float3 triplanarVector = abs(IN.vertex2.xyz);
		float3 triplanarUV = triplanarVector;
		triplanarVector = normalize(max(triplanarVector, 0.00001));
		float totalTriplanarVector = (triplanarVector.x + triplanarVector.y + triplanarVector.z);

		triplanarVector /= totalTriplanarVector;

		float4 triplanar = 0;
		float4 qt1 = tex2D(_QuadTexture1, triplanarUV.xy * 0.1);
		triplanar += (triplanarVector.x * qt1);

		float4 qt2 = tex2D(_QuadTexture2, triplanarUV.yz * 0.1);
		triplanar += (triplanarVector.y * qt2);

		float4 qt3 = tex2D(_QuadTexture3, triplanarUV.zx * 0.1);
		triplanar += (triplanarVector.z * qt3);
		*/

	}
}