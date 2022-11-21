// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stinger/PackedStandart"
{
	Properties
	{
		_BaseColor("BaseColor", 2D) = "white" {}
		_ORMAORoughMetal("ORM (AO Rough Metal)", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_AOpower("AO power", Range( 0 , 1)) = 1
		_DelMetall("DelMetall", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform sampler2D _ORMAORoughMetal;
		uniform float4 _ORMAORoughMetal_ST;
		uniform float _DelMetall;
		uniform float _AOpower;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = tex2D( _Normal, uv_Normal ).rgb;
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			o.Albedo = tex2D( _BaseColor, uv_BaseColor ).rgb;
			float2 uv_ORMAORoughMetal = i.uv_texcoord * _ORMAORoughMetal_ST.xy + _ORMAORoughMetal_ST.zw;
			float4 tex2DNode5 = tex2D( _ORMAORoughMetal, uv_ORMAORoughMetal );
			float delmetall18 = saturate( ( ( _DelMetall - 0.5 ) * 1000.0 ) );
			float lerpResult19 = lerp( tex2DNode5.b , 0.0 , delmetall18);
			o.Metallic = lerpResult19;
			o.Smoothness = tex2DNode5.g;
			o.Occlusion = ( tex2DNode5.r * ( 1.0 - _AOpower ) );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19100
Node;AmplifyShaderEditor.TexturePropertyNode;2;-613,-127.5;Inherit;True;Property;_BaseColor;BaseColor;0;0;Create;True;0;0;0;False;0;False;None;99aa2b13187123449abb89f5298e3d3b;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;3;-381,-115.5;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;6;-604,296.5;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;None;fb43d8e51ac53564ba27d2033a0c5985;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;7;-372.6144,293.7231;Inherit;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;4;-607,102.5;Inherit;True;Property;_ORMAORoughMetal;ORM (AO Rough Metal);1;0;Create;True;0;0;0;False;0;False;None;0ceaa83d48f8d8642b6f772d08bfe270;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;5;-371,91.5;Inherit;True;Property;_TextureSample1;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;0.7034912,-175.9665;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;361,-2;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Stinger/PackedStandart;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-363.2965,-343.9665;Inherit;False;Property;_AOpower;AO power;3;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;11;-65.29649,-303.9665;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;14;376.8512,-323.8583;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;541.8512,-320.8583;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1000;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;17;763.8512,-324.8583;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;936.8512,-317.8583;Inherit;False;delmetall;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;172.8512,209.1417;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;20;-54.1488,369.1417;Inherit;False;18;delmetall;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;94.85118,-324.8583;Inherit;False;Property;_DelMetall;DelMetall;4;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
WireConnection;3;0;2;0
WireConnection;7;0;6;0
WireConnection;5;0;4;0
WireConnection;10;0;5;1
WireConnection;10;1;11;0
WireConnection;0;0;3;0
WireConnection;0;1;7;0
WireConnection;0;3;19;0
WireConnection;0;4;5;2
WireConnection;0;5;10;0
WireConnection;11;0;9;0
WireConnection;14;0;12;0
WireConnection;16;0;14;0
WireConnection;17;0;16;0
WireConnection;18;0;17;0
WireConnection;19;0;5;3
WireConnection;19;2;20;0
ASEEND*/
//CHKSM=B4A817052E85B4A19ABA1ACDCA467E9272DD3116