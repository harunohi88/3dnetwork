// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LowPoly/SimpleWater"
{
	Properties
	{
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 0
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_ShalowColor("Shalow Color", Color) = (1,1,1,0)
		_WaterDepth("Water Depth", Float) = 0
		_WaterFalloff("Water Falloff", Float) = 0
		_WaterSpecular("Water Specular", Float) = 0
		_WaterSmoothness("Water Smoothness", Float) = 0
		_Distortion("Distortion", Float) = 0.5
		_Foam("Foam", 2D) = "white" {}
		_FoamDepth("Foam Depth", Float) = 0
		_FoamFalloff("Foam Falloff", Float) = 0
		_FoamSpecular("Foam Specular", Float) = 0
		_FoamSmoothness("Foam Smoothness", Float) = 0
		_WavesAmplitude("WavesAmplitude", Float) = 0.01
		_WavesAmount("WavesAmount", Float) = 8.87
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
		LOD 300
		
		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			// URP keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float4 _WaterNormal_ST;
				float4 _Foam_ST;
				float4 _DeepColor;
				float4 _ShalowColor;
				float _NormalScale;
				float _WaterDepth;
				float _WaterFalloff;
				float _WaterSpecular;
				float _WaterSmoothness;
				float _Distortion;
				float _FoamDepth;
				float _FoamFalloff;
				float _FoamSpecular;
				float _FoamSmoothness;
				float _WavesAmount;
				float _WavesAmplitude;
			CBUFFER_END

			TEXTURE2D(_WaterNormal);
			SAMPLER(sampler_WaterNormal);
			TEXTURE2D(_Foam);
			SAMPLER(sampler_Foam);

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				float4 screenPos : TEXCOORD4;
				float eyeDepth : TEXCOORD5;
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;
				
				// Wave animation
				float3 positionOS = input.positionOS.xyz;
				float waveOffset = sin((_WavesAmount * positionOS.z) + _Time.y) * _WavesAmplitude;
				positionOS += input.normalOS * waveOffset;
				
				VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
				
				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				output.uv = TRANSFORM_TEX(input.uv, _WaterNormal);
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
				output.screenPos = ComputeScreenPos(output.positionCS);
				output.eyeDepth = -TransformWorldToView(output.positionWS).z;
				
				return output;
			}

			float4 frag(Varyings input) : SV_Target
			{
				// Screen space calculations
				float2 screenUV = input.screenPos.xy / input.screenPos.w;
				float sceneDepth = SampleSceneDepth(screenUV);
				float sceneZ = LinearEyeDepth(sceneDepth, _ZBufferParams);
				float depthDifference = abs(sceneZ - input.eyeDepth);
				
				// Water normal calculation with panning
				float2 uv1 = input.uv + _Time.y * float2(-0.03, 0);
				float2 uv2 = input.uv + _Time.y * float2(0.04, 0.04);
				
				float3 normal1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal, sampler_WaterNormal, uv1), _NormalScale);
				float3 normal2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal, sampler_WaterNormal, uv2), _NormalScale);
				float3 waterNormal = BlendNormal(normal1, normal2);
				
				// Convert normal to world space
				float3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
				float3x3 tangentToWorld = float3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
				float3 normalWS = normalize(mul(waterNormal, tangentToWorld));
				
				// Water depth and color
				float waterDepthMask = saturate(pow((depthDifference + _WaterDepth), _WaterFalloff));
				float4 waterColor = lerp(_DeepColor, _ShalowColor, waterDepthMask);
				
				// Foam calculation
				float2 foamUV = TRANSFORM_TEX(input.uv, _Foam) + _Time.y * float2(-0.01, 0.01);
				float foamNoise = SAMPLE_TEXTURE2D(_Foam, sampler_Foam, foamUV).r;
				float foamMask = saturate(pow((depthDifference + _FoamDepth), _FoamFalloff)) * foamNoise;
				float4 finalColor = lerp(waterColor, float4(1, 1, 1, 1), foamMask);
				
				// Screen distortion for refraction
				float2 distortedScreenUV = screenUV + waterNormal.xy * _Distortion * 0.01;
				distortedScreenUV = clamp(distortedScreenUV, 0, 1);
				float3 sceneColor = SampleSceneColor(distortedScreenUV);
				
				// Blend with scene
				finalColor.rgb = lerp(finalColor.rgb, sceneColor, waterDepthMask * 0.8);
				
				// Lighting
				Light mainLight = GetMainLight();
				float3 lightDir = normalize(mainLight.direction);
				float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
				
				// Specular
				float specular = lerp(_WaterSpecular, _FoamSpecular, foamMask);
				float smoothness = lerp(_WaterSmoothness, _FoamSmoothness, foamMask);
				
				float3 halfVector = normalize(lightDir + viewDir);
				float NdotH = saturate(dot(normalWS, halfVector));
				float specularReflection = pow(NdotH, (1 - smoothness) * 128) * specular;
				
				finalColor.rgb += specularReflection * mainLight.color;
				finalColor.a = lerp(0.8, 1.0, foamMask);
				
				return finalColor;
			}
			ENDHLSL
		}
		
		Pass
		{
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }
			
			ZWrite On
			ColorMask 0
			Cull Back
			
			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
				float _WavesAmount;
				float _WavesAmplitude;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
			};

			Varyings DepthOnlyVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				
				// Wave animation
				float3 positionOS = input.positionOS.xyz;
				float waveOffset = sin((_WavesAmount * positionOS.z) + _Time.y) * _WavesAmplitude;
				positionOS += input.normalOS * waveOffset;
				
				VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
				output.positionCS = vertexInput.positionCS;
				
				return output;
			}

			float4 DepthOnlyFragment(Varyings input) : SV_Target
			{
				return 0;
			}
			ENDHLSL
		}
	}
	
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
/*ASEBEGIN
Version=14006
357;92;969;673;-152.21;1759.334;2.21029;True;False
Node;AmplifyShaderEditor.CommentaryNode;152;-2053.601,-256.6997;Float;False;828.5967;315.5001;Screen depth difference to get intersection and fading effect with terrain and objects;4;89;2;1;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-2003.601,-153.1996;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;1;-1781.601,-155.6997;Float;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-1574.201,-110.3994;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;151;-935.9057,-1082.484;Float;False;1281.603;457.1994;Blend panning normals to fake noving ripples;7;19;23;24;21;22;17;48;;1,1,1,1;0;0
Node;AmplifyShaderEditor.AbsOpNode;89;-1389.004,-112.5834;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;-863.7005,-467.5007;Float;False;1113.201;508.3005;Depths controls and colors;11;87;94;12;13;156;157;11;88;10;6;143;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;155;-1106.507,7.515848;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;153;-843.9032,402.718;Float;False;1083.102;484.2006;Foam controls and texture;9;116;105;106;115;111;110;112;113;114;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-885.9058,-1005.185;Float;False;0;17;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;154;-922.7065,390.316;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;19;-610.9061,-919.9849;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.04,0.04;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-722.2024,526.6185;Float;False;Property;_FoamDepth;Foam Depth;15;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-813.7005,-128.1996;Float;False;Property;_WaterDepth;Water Depth;9;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;22;-613.2062,-1032.484;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.03,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;158;-1075.608,-163.0834;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-557.3063,-795.3858;Float;False;Property;_NormalScale;Normal Scale;6;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-256.3054,-814.2847;Float;True;Property;_WaterNormal;Water Normal;5;0;Create;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-636.2005,-79.20019;Float;False;Property;_WaterFalloff;Water Falloff;10;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-269.2061,-1024.185;Float;True;Property;_Normal2;Normal2;5;0;Create;None;True;0;True;bump;Auto;True;Instance;17;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-632.0056,-204.5827;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-531.4025,588.5187;Float;False;Property;_FoamFalloff;Foam Falloff;16;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-542.0016,452.718;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;106;-793.9032,700.119;Float;False;0;105;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;110;-357.2024,461.6185;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;150;467.1957,-1501.783;Float;False;985.6011;418.6005;Get screen color for refraction and disturbe it with normals;7;96;97;98;65;149;164;165;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;175;1170.833,-82.95044;Float;False;Property;_WavesAmount;WavesAmount;20;0;Create;8.87;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;116;-573.2014,720.3181;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.01,0.01;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;11;-455.0999,-328.3;Float;False;Property;_ShalowColor;Shalow Color;8;0;Create;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;-697.5002,-417.5007;Float;False;Property;_DeepColor;Deep Color;7;0;Create;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;24;170.697,-879.6849;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;87;-455.8059,-118.1832;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;168;1141.033,-4.350647;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;149;487.4943,-1188.882;Float;False;1;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;97;710.096,-1203.183;Float;False;Property;_Distortion;Distortion;13;0;Create;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;1344.033,-30.35065;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;157;-149.1077,-261.9834;Float;False;1;0;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GrabScreenPosition;164;511.3026,-1442.425;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;105;-304.4021,674.9185;Float;True;Property;_Foam;Foam;14;0;Create;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;113;-136.0011,509.618;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;170;1297.033,146.6494;Float;False;1;0;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;94;-249.5044,-96.98394;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;156;-151.0076,-354.5834;Float;False;1;0;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;13;60.50008,-220.6998;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;888.1974,-1279.783;Float;False;2;2;0;FLOAT3;0.0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;165;814.6503,-1385.291;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;171;1504.033,-54.35065;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;108;58.99682,146.0182;Float;False;Constant;_Color0;Color 0;-1;0;Create;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;80.19891,604.0181;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;117;323.797,77.91843;Float;False;3;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;1041.296,-1346.683;Float;False;2;2;0;FLOAT2;0.0,0;False;1;FLOAT3;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;143;95.69542,-321.0839;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;167;1565.033,74.64935;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;166;1667.033,-148.3506;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;753.9969,-565.4819;Float;False;Property;_WaterSpecular;Water Specular;11;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;65;1232.797,-1350.483;Float;False;Global;_WaterGrab;WaterGrab;-1;0;Create;Object;-1;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;132;914.3978,-199.48;Float;False;Property;_FoamSmoothness;Foam Smoothness;18;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;1809.033,7.649353;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT3;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;26;920.1959,-279.1855;Float;False;Property;_WaterSmoothness;Water Smoothness;12;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;161;660.4934,-750.6837;Float;False;1;0;COLOR;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;162;1312.293,-894.3823;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;756.1969,-467.1806;Float;False;Property;_FoamSpecular;Foam Specular;17;0;Create;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;173;1952.033,-76.35065;Float;False;Property;_WavesAmplitude;WavesAmplitude;19;0;Create;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;1139.597,-182.68;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;130;955.7971,-465.8806;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;1925.033,-250.3506;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;93;1559.196,-1006.285;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1838.601,-748.1998;Float;False;True;6;Float;ASEMaterialInspector;0;0;StandardSpecular;ASESampleShaders/Water/SimpleWater;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;3;False;0;0;Translucent;0.5;True;False;0;False;Opaque;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;True;1;16;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0.0,0,0;False;7;FLOAT3;0.0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0.0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;2;0
WireConnection;3;0;1;0
WireConnection;3;1;2;4
WireConnection;89;0;3;0
WireConnection;155;0;89;0
WireConnection;154;0;155;0
WireConnection;19;0;21;0
WireConnection;22;0;21;0
WireConnection;158;0;89;0
WireConnection;17;1;19;0
WireConnection;17;5;48;0
WireConnection;23;1;22;0
WireConnection;23;5;48;0
WireConnection;88;0;158;0
WireConnection;88;1;6;0
WireConnection;115;0;154;0
WireConnection;115;1;111;0
WireConnection;110;0;115;0
WireConnection;110;1;112;0
WireConnection;116;0;106;0
WireConnection;24;0;23;0
WireConnection;24;1;17;0
WireConnection;87;0;88;0
WireConnection;87;1;10;0
WireConnection;149;0;24;0
WireConnection;174;0;175;0
WireConnection;174;1;168;3
WireConnection;157;0;11;0
WireConnection;105;1;116;0
WireConnection;113;0;110;0
WireConnection;94;0;87;0
WireConnection;156;0;12;0
WireConnection;13;0;156;0
WireConnection;13;1;157;0
WireConnection;13;2;94;0
WireConnection;98;0;149;0
WireConnection;98;1;97;0
WireConnection;165;0;164;0
WireConnection;171;0;174;0
WireConnection;171;1;170;0
WireConnection;114;0;113;0
WireConnection;114;1;105;1
WireConnection;117;0;13;0
WireConnection;117;1;108;0
WireConnection;117;2;114;0
WireConnection;96;0;165;0
WireConnection;96;1;98;0
WireConnection;143;0;94;0
WireConnection;166;0;171;0
WireConnection;65;0;96;0
WireConnection;169;0;166;0
WireConnection;169;1;167;0
WireConnection;161;0;117;0
WireConnection;162;0;143;0
WireConnection;133;0;26;0
WireConnection;133;1;132;0
WireConnection;133;2;114;0
WireConnection;130;0;104;0
WireConnection;130;1;131;0
WireConnection;130;2;114;0
WireConnection;172;0;169;0
WireConnection;172;1;173;0
WireConnection;93;0;161;0
WireConnection;93;1;65;0
WireConnection;93;2;162;0
WireConnection;0;0;93;0
WireConnection;0;1;24;0
WireConnection;0;3;130;0
WireConnection;0;4;133;0
WireConnection;0;11;172;0
ASEEND*/
//CHKSM=41DF6B041279D108F48301F85BDA2B786ACD0194
