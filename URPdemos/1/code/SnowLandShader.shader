Shader "SnowGround/SnowLand"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap("NormalMap",2D)="white"{}
        _RT("RT",2D)="white"{}
        _Gloss("Gloss",Range(1,20))=5
        _SpecularColor("SpecularColor",Color)=(1,1,1,1)
        _BumpScale("Bump Scale",Range(1,10))=1.0
        _PlayerPosition("Player Position",Vector)=(1,1,1,1)
        _SnowRange("Snow Range",Vector)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"
                "Queue"="Geometry"}
        LOD 200

        Pass
        {
           // Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal:NORMAL;
                float4 tangent:TANGENT;
            };

            struct v2f
            {              
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 scrPos:TEXCOORD1;
                float4 viewPos:TEXCOORD2;
                float3 normalWS:NORMAL_WS;
                float4 tangentWS:TANGENT_WS;
                float3 worldPos:TEXCOORD6;
                
            };
        //    CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _RT;
            float _Gloss;
            float4 _MainTex_ST;
            float4 _SpecularColor;
            float4 _PlayerPosition;
            float4 _SnowRange;
            float _BumpScale;
      //      CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                //float3 binormal=cross(v.normal.xyz,v.tangent.xyz)*v.tangent.w;
                //float3x3 rotation=float3x3(v.tangent.xyz,binormal,v.normal.xyz);
                ////VertexPositionInputs vertexInput=GetVertexPositionInputs(v.vertex.xyz);
                ////o.vertex = vertexInput.positionCS;
                //o.vertex=mul(UNITY_MATRIX_MVP,v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.viewPos=mul(UNITY_MATRIX_MV,v.vertex);
                //o.scrPos=ComputeScreenPos(o.vertex);
                //o.worldPos=mul(unity_ObjectToWorld,v.vertex).xyz;

                const VertexPositionInputs vertexInput=GetVertexPositionInputs(v.vertex.xyz);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.vertex=vertexInput.positionCS;
                o.worldPos=vertexInput.positionWS;
                const VertexNormalInputs vertexNormalInputs=GetVertexNormalInputs(v.normal,v.tangent);
                real sign=v.tangent.w*GetOddNegativeScale();
                o.normalWS=vertexNormalInputs.normalWS;
                o.tangentWS=real4(vertexNormalInputs.tangentWS,sign);
                o.scrPos=ComputeScreenPos(o.vertex);
                o.viewPos=mul(UNITY_MATRIX_MV,v.vertex);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                


                float3 bump=UnpackNormalScale(tex2D(_NormalMap,i.uv.xy),_BumpScale);
                float4 col=tex2D(_MainTex,i.uv.xy);
                float sgn=i.tangentWS.w;
                float3 bitangent=sgn*cross(i.normalWS.xyz,i.tangentWS.xyz);
                bump=mul(bump,real3x3(i.tangentWS.xyz,bitangent.xyz,i.normalWS.xyz));
                Light mainLight=GetMainLight();
                float4 lightColor=float4(mainLight.color,1.0);
                float3 lightDir=normalize(mainLight.direction);
                float4 ambient=float4(SampleSH(bump),1.0)*col;
                float4 diffuse=saturate(dot(lightDir,bump))*lightColor*col;
                float3 viewDir=SafeNormalize(GetCameraPositionWS()-i.vertex);
                float3 halfDir=SafeNormalize(viewDir+lightDir);
                float4 specular=pow(saturate(dot(halfDir,bump)),_Gloss)*lightColor*_SpecularColor;
                col=ambient+diffuse+specular;
                //col=ambient+diffuse;
                
                float2 uv=float2(0.5*(-i.worldPos.x+_PlayerPosition.x)/_SnowRange.x+0.5,
                                 0.5*(-i.worldPos.z+_PlayerPosition.z)/_SnowRange.z+0.5);
                if(uv.x<0||uv.x>1||uv.y<0||uv.y>1){
                    return float4(col.xyz,1);
                }
                float4 colAdd=tex2D(_RT,uv);
                
                return float4(col.xyz+colAdd.xyz,1);
            }
            ENDHLSL
        }
       
    }
}
