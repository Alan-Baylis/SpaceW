﻿/* Procedural planet generator.
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

/*
 * Precomputed Atmospheric Scattering
 * Copyright (c) 2008 INRIA
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
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Eric Bruneton
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2016
 */

Shader "SpaceEngine/Atmosphere/Atmosphere" 
{
	Properties
	{
		_Sun_Glare("Sun Glare", 2D) = "black" {}
		_Sun_Glare_Scale("Sun Glare Scale", Float) = 1.0
		_Sun_Glare_Color("Sun Glare Color", Color) = (1, 1, 1, 1)
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
	
		Pass 
		{
			ZWrite On
			ZTest Always  
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcColor

			Cull Off

			CGPROGRAM
			#include "UnityCG.cginc"		
			#include "HDR.cginc"
			#include "Atmosphere.cginc"
			#include "SpaceStuff.cginc"
			#include "Eclipses.cginc"

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag
					
			uniform sampler2D _Sun_Glare;
			uniform float _Sun_Glare_Scale;
			uniform float4 _Sun_Glare_Color;

			uniform float4x4 _Sun_WorldToLocal_1;
			uniform float4x4 _Sun_WorldToLocal_2;
			uniform float4x4 _Sun_WorldToLocal_3;
			uniform float4x4 _Sun_WorldToLocal_4;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 dir : TEXCOORD1;

				#ifdef LIGHT_1 
					float3 relativeDir_1 : TEXCOORD2;
				#endif

				#ifdef LIGHT_2
					float3 relativeDir_1 : TEXCOORD2;
					float3 relativeDir_2 : TEXCOORD3;
				#endif

				#ifdef LIGHT_3
					float3 relativeDir_1 : TEXCOORD2;
					float3 relativeDir_2 : TEXCOORD3;
					float3 relativeDir_3 : TEXCOORD4;
				#endif

				#ifdef LIGHT_4
					float3 relativeDir_1 : TEXCOORD2;
					float3 relativeDir_2 : TEXCOORD3;
					float3 relativeDir_3 : TEXCOORD4;
					float3 relativeDir_4 : TEXCOORD5;
				#endif
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;

				// apply this rotation to view dir to get relative viewdir
				#ifdef LIGHT_1 
					OUT.relativeDir_1 = mul(_Sun_WorldToLocal_1, OUT.dir); 
				#endif

				#ifdef LIGHT_2 
					OUT.relativeDir_1 = mul(_Sun_WorldToLocal_1, OUT.dir); 
					OUT.relativeDir_2 = mul(_Sun_WorldToLocal_2, OUT.dir); 
				#endif

				#ifdef LIGHT_3
					OUT.relativeDir_1 = mul(_Sun_WorldToLocal_1, OUT.dir); 
					OUT.relativeDir_2 = mul(_Sun_WorldToLocal_2, OUT.dir); 
					OUT.relativeDir_3 = mul(_Sun_WorldToLocal_3, OUT.dir); 
				#endif

				#ifdef LIGHT_4
					OUT.relativeDir_1 = mul(_Sun_WorldToLocal_1, OUT.dir); 
					OUT.relativeDir_2 = mul(_Sun_WorldToLocal_2, OUT.dir); 
					OUT.relativeDir_3 = mul(_Sun_WorldToLocal_3, OUT.dir); 
					OUT.relativeDir_4 = mul(_Sun_WorldToLocal_4, OUT.dir); 
				#endif
	
				OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
				OUT.uv = v.texcoord.xy;

				return OUT;
			}
			
			float3 OuterSunRadiance(float3 viewdir)
			{
				float3 data = viewdir.z > 0.0 ? (tex2D(_Sun_Glare, float2(0.5, 0.5) + viewdir.xy / _Sun_Glare_Scale).rgb * _Sun_Glare_Color) : float3(0, 0, 0);

				return pow(max(0, data), 2.2) * _Sun_Intensity;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPG = WCP + _Globals_Origin;

				float3 d = normalize(IN.dir);

				float sunColor = 0;
				float3 extinction = 0;
				float3 inscatter = 0;

				#ifdef ECLIPSES_ON
					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						float shadow = ShadowOuterColor(d, WCP, _Globals_Origin, Rt);
					#endif
				#endif

				#ifdef LIGHT_1
					sunColor += OuterSunRadiance(IN.relativeDir_1);

					float3 extinction1 = 0;

					#ifdef ECLIPSES_ON
						float eclipse1 = 1;

						eclipse1 = EclipseOuterShadow(_Sun_WorldSunDir_1, _Sun_Positions_1[0].w, d, WCP, _Globals_Origin, Rt);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_1, extinction1, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_2
					sunColor += OuterSunRadiance(IN.relativeDir_1);
					sunColor += OuterSunRadiance(IN.relativeDir_2);

					float3 extinction1 = 0;
					float3 extinction2 = 0;

					#ifdef ECLIPSES_ON
						float eclipse1 = 1;
						float eclipse2 = 1;

						eclipse1 *= EclipseOuterShadow(_Sun_WorldSunDir_1, _Sun_Positions_1[0].w, d, WCP, _Globals_Origin, Rt);
						eclipse2 *= EclipseOuterShadow(_Sun_WorldSunDir_2, _Sun_Positions_1[1].w, d, WCP, _Globals_Origin, Rt);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_1, extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_2, extinction2, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_3
					sunColor += OuterSunRadiance(IN.relativeDir_1);
					sunColor += OuterSunRadiance(IN.relativeDir_2);
					sunColor += OuterSunRadiance(IN.relativeDir_3);

					float3 extinction1 = 0;
					float3 extinction2 = 0;
					float3 extinction3 = 0;

					#ifdef ECLIPSES_ON
						float eclipse1 = 1;
						float eclipse2 = 1;
						float eclipse3 = 1;

						eclipse1 *= EclipseOuterShadow(_Sun_WorldSunDir_1, _Sun_Positions_1[0].w, d, WCP, _Globals_Origin, Rt);
						eclipse2 *= EclipseOuterShadow(_Sun_WorldSunDir_2, _Sun_Positions_1[1].w, d, WCP, _Globals_Origin, Rt);
						eclipse3 *= EclipseOuterShadow(_Sun_WorldSunDir_3, _Sun_Positions_1[2].w, d, WCP, _Globals_Origin, Rt);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_1, extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_2, extinction2, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_3, extinction3, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;
						inscatter *= eclipse3;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_4
					sunColor += OuterSunRadiance(IN.relativeDir_1);
					sunColor += OuterSunRadiance(IN.relativeDir_2);
					sunColor += OuterSunRadiance(IN.relativeDir_3);
					sunColor += OuterSunRadiance(IN.relativeDir_4);

					float3 extinction1 = 0;
					float3 extinction2 = 0;
					float3 extinction3 = 0;
					float3 extinction4 = 0;

					#ifdef ECLIPSES_ON
						float eclipse1 = 1;
						float eclipse2 = 1;
						float eclipse3 = 1;
						float eclipse4 = 1;

						eclipse1 *= EclipseOuterShadow(_Sun_WorldSunDir_1, _Sun_Positions_1[0].w, d, WCP, _Globals_Origin, Rt);
						eclipse2 *= EclipseOuterShadow(_Sun_WorldSunDir_2, _Sun_Positions_1[1].w, d, WCP, _Globals_Origin, Rt);
						eclipse3 *= EclipseOuterShadow(_Sun_WorldSunDir_3, _Sun_Positions_1[2].w, d, WCP, _Globals_Origin, Rt);
						eclipse4 *= EclipseOuterShadow(_Sun_WorldSunDir_4, _Sun_Positions_1[3].w, d, WCP, _Globals_Origin, Rt);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_1, extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_2, extinction2, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_3, extinction3, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldSunDir_4, extinction4, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;
						inscatter *= eclipse3;
						inscatter *= eclipse4;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;
					extinction += extinction4;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				return float4(0, 0, 0, 0);
			}		
			ENDCG
		}
	}
}