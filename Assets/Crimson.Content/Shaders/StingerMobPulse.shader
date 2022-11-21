// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "StingerMobPulse"
{
	Properties
	{
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset]_Metallic("Metallic", 2D) = "white" {}
		[NoScaleOffset][Normal]_Normal("Normal", 2D) = "bump" {}
		[NoScaleOffset]_Occlusion("Occlusion", 2D) = "white" {}
		[NoScaleOffset]_Emissive("Emissive", 2D) = "white" {}
		_EmissiveMax("EmissiveMax", Range( 0 , 1)) = 0.9071487
		_EmissiveMin("EmissiveMin", Range( 0 , 1)) = 0.1880044
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform sampler2D _Albedo;
		uniform sampler2D _Emissive;
		uniform float _EmissiveMin;
		uniform float _EmissiveMax;
		uniform sampler2D _Metallic;
		uniform sampler2D _Occlusion;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal7 = i.uv_texcoord;
			float4 Normal14 = tex2D( _Normal, uv_Normal7 );
			o.Normal = Normal14.rgb;
			float2 uv_Albedo5 = i.uv_texcoord;
			float4 Albedo11 = tex2D( _Albedo, uv_Albedo5 );
			o.Albedo = Albedo11.rgb;
			float2 uv_Emissive10 = i.uv_texcoord;
			float4 color38 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float PulsePower34 = saturate( (min( _EmissiveMin , _EmissiveMax ) + (saturate( _SinTime.w ) - 0.0) * (max( _EmissiveMin , _EmissiveMax ) - min( _EmissiveMin , _EmissiveMax )) / (1.0 - 0.0)) );
			float4 lerpResult35 = lerp( tex2D( _Emissive, uv_Emissive10 ) , color38 , PulsePower34);
			float4 Emissive16 = lerpResult35;
			o.Emission = Emissive16.rgb;
			float2 uv_Metallic6 = i.uv_texcoord;
			float4 tex2DNode6 = tex2D( _Metallic, uv_Metallic6 );
			float Metallness12 = tex2DNode6.r;
			o.Metallic = Metallness12;
			float Rougness13 = tex2DNode6.a;
			o.Smoothness = Rougness13;
			float2 uv_Occlusion8 = i.uv_texcoord;
			float AO15 = tex2D( _Occlusion, uv_Occlusion8 ).r;
			o.Occlusion = AO15;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19100
Node;AmplifyShaderEditor.CommentaryNode;33;-620.7351,-782.557;Inherit;False;1488.125;323.9377;Comment;9;34;26;27;32;31;28;29;30;37;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;24;-627,-411.063;Inherit;False;1427.241;1326.645;Comment;20;13;38;36;35;16;15;14;12;11;7;3;9;10;8;4;6;5;2;1;39;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;23;867.7249,-346.5645;Inherit;False;605.5554;643.9733;Comment;7;17;18;19;20;21;22;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;17;917.7249,-296.5645;Inherit;False;11;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;919.3503,-212.0423;Inherit;False;14;Normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;19;920.9758,-132.3963;Inherit;False;16;Emissive;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;20;935.6046,-42.9978;Inherit;False;12;Metallness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;940.4809,44.77528;Inherit;False;13;Rougness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;950.2335,130.9229;Inherit;False;15;AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1218.281,-173.5912;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;StingerMobPulse;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;5;-220,-361.063;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-220.0986,-169.7411;Inherit;True;Property;_TextureSample1;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-214.0579,220.4889;Inherit;True;Property;_TextureSample3;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-210.2267,417.1921;Inherit;True;Property;_TextureSample4;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-215.6396,19.43263;Inherit;True;Property;_TextureSample2;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;145.6469,-355.1839;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;166.7773,-184.514;Inherit;False;Metallness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;187.908,46.29674;Inherit;False;Normal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;30;129.1167,-675.0455;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-203.6168,-675.7644;Inherit;False;Property;_EmissiveMin;EmissiveMin;6;0;Create;True;0;0;0;False;0;False;0.1880044;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-201.1378,-575.0923;Inherit;False;Property;_EmissiveMax;EmissiveMax;5;0;Create;True;0;0;0;False;0;False;0.9071487;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;31;129.1166,-578.7131;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;32;383.6067,-732.557;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;27;-411.36,-728.6172;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;26;-570.7354,-710.7434;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;666.048,-721.1743;Inherit;False;PulsePower;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;671.4492,-572.1329;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;582.6334,342.8417;Inherit;True;Emissive;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;35;336.8619,345.1026;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;120.2713,644.7084;Inherit;True;34;PulsePower;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;38;-139.3104,620.939;Inherit;False;Constant;_Color0;Color 0;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;155.6741,224.842;Inherit;False;AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;13;446.9935,-142.7581;Inherit;True;Rougness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;39;227.4581,-31.983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-577,-361.3304;Inherit;True;Property;_Albedo;Albedo;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;30a913b658c474e47b161ec9552155a4;30a913b658c474e47b161ec9552155a4;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;2;-573.18,-159.8549;Inherit;True;Property;_Metallic;Metallic;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;d5a8e3ac942574a4780a36be928297c3;b5871d40a7cd07b4f99a0b216100e017;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;3;-570,26.5197;Inherit;True;Property;_Normal;Normal;2;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;95a99d73143a68c4d985869964ab8319;95a99d73143a68c4d985869964ab8319;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;4;-560,220.937;Inherit;True;Property;_Occlusion;Occlusion;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;d0d3b03923065d44cb88368c982cfc42;d0d3b03923065d44cb88368c982cfc42;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;9;-556.1685,414.6121;Inherit;True;Property;_Emissive;Emissive;4;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;d425ca4bf9949834d9599f58d89a1ec3;d425ca4bf9949834d9599f58d89a1ec3;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
WireConnection;0;0;17;0
WireConnection;0;1;18;0
WireConnection;0;2;19;0
WireConnection;0;3;20;0
WireConnection;0;4;21;0
WireConnection;0;5;22;0
WireConnection;5;0;1;0
WireConnection;6;0;2;0
WireConnection;8;0;4;0
WireConnection;10;0;9;0
WireConnection;7;0;3;0
WireConnection;11;0;5;0
WireConnection;12;0;6;1
WireConnection;14;0;7;0
WireConnection;30;0;29;0
WireConnection;30;1;28;0
WireConnection;31;0;29;0
WireConnection;31;1;28;0
WireConnection;32;0;27;0
WireConnection;32;3;30;0
WireConnection;32;4;31;0
WireConnection;27;0;26;4
WireConnection;34;0;37;0
WireConnection;37;0;32;0
WireConnection;16;0;35;0
WireConnection;35;0;10;0
WireConnection;35;1;38;0
WireConnection;35;2;36;0
WireConnection;15;0;8;1
WireConnection;13;0;6;4
WireConnection;39;0;6;4
ASEEND*/
//CHKSM=F16DFACF9510B70BC6323542ADE58C93545BACD8