Shader "SnowGround/SnowLandGeo
"
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
        _TessellationUniform("TessellationUniform",Range(1,128))=10
        _DisplacementScale("DisplacementScale",Range(1,10))=1
        _TrailColor("Trail Color",Color)=(1,1,1,1)
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
            #pragma hull hullProgram
            #pragma domain ds
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

            struct v2h
            {              
                float4 vertex : INTERNALTESSPOS;
                float2 uv : TEXCOORD0;
                float4 normal:NORMAL;
                float4 tangent:TANGENT;
                
            };
            struct t2f
            {              
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 viewPos:TEXCOORD2;
                float3 normalWS:NORMAL_WS;
                float4 tangentWS:TANGENT_WS;
                float3 worldPos:TEXCOORD1;
                float colorBuff:TEXCOORD4;
                
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
            float4 _TrailColor;
            float _BumpScale;
            float _DisplacementScale;
      //      CBUFFER_END

            v2h vert (appdata v)
            {
                v2h o;
                o.vertex=v.vertex;
                o.uv=v.uv;
                o.normal=v.normal;
                o.tangent=v.tangent;
                return o;
            }
          
     //#ifdef UNITY_CAN_COMPILE_TESSELLATION

            struct OutputPatchConstant{
                float edge[3]:SV_TessFactor;
                float inside:SV_InsideTessFactor;
            };

            float _TessellationUniform;
            OutputPatchConstant hsconst (InputPatch<v2h,3> patch){
                //定义曲面细分的参数
                OutputPatchConstant o;
                o.edge[0] = _TessellationUniform;
                o.edge[1] = _TessellationUniform;
                o.edge[2] = _TessellationUniform;
                o.inside  = _TessellationUniform;
                return o;
            }

            [domain("tri")]//确定图元，quad,triangle等
            [partitioning("fractional_odd")]//拆分edge的规则，equal_spacing,fractional_odd,fractional_even
            [outputtopology("triangle_cw")]
            [patchconstantfunc("hsconst")]//一个patch一共有三个点，但是这三个点都共用这个函数
            [outputcontrolpoints(3)]      //不同的图元会对应不同的控制点
            v2h hullProgram (InputPatch<v2h,3> patch,uint id : SV_OutputControlPointID){
                //定义hullshaderV函数
                return patch[id];
            }
            t2f dvert(v2h v){
                t2f o;
                const VertexPositionInputs vertexInput=GetVertexPositionInputs(v.vertex.xyz);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.vertex=vertexInput.positionCS;
                o.worldPos=vertexInput.positionWS;
                const VertexNormalInputs vertexNormalInputs=GetVertexNormalInputs(v.normal,v.tangent);
                real sign=v.tangent.w*GetOddNegativeScale();
                o.normalWS=vertexNormalInputs.normalWS;
                o.tangentWS=real4(vertexNormalInputs.tangentWS,sign);
                
                float2 uv=float2(0.5*(-o.worldPos.x+_PlayerPosition.x)/_SnowRange.x+0.5,
                                 0.5*(-o.worldPos.z+_PlayerPosition.z)/_SnowRange.z+0.5);
                float height=tex2Dlod(_RT,float4(uv,0,0)).x*_DisplacementScale*2-1;

                //float3 normal=float3(0,0,0);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(1,0,0,0)).x*_DisplacementScale*float3(-1,1,0);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(1,-1,0,0)).x*_DisplacementScale*float3(-1,1,1);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(-1,0,0,0)).x*_DisplacementScale*float3(1,1,0);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(-1,-1,0,0)).x*_DisplacementScale*float3(1,1,1);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(0,1,0,0)).x*_DisplacementScale*float3(0,1,-1);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(0,-1,0,0)).x*_DisplacementScale*float3(0,1,1);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(1,1,0,0)).x*_DisplacementScale*float3(-1,1,-1);
                //normal+=tex2Dlod(_RT,float4(uv,0,0)+float4(-1,1,0,0)).x*_DisplacementScale*float3(1,1,-1);
                //normal=normalize(normal);

                if(o.worldPos.y<=_PlayerPosition.y&&_PlayerPosition.y<=o.worldPos.y+3)o.worldPos.y+=height;
                o.colorBuff=max(0,-height);
                o.vertex=mul(UNITY_MATRIX_VP,float4(o.worldPos,1));
                o.viewPos=mul(UNITY_MATRIX_V,float4(o.worldPos,1));
                return o;
            } 

            [domain("tri")]//同样需要定义图元
            t2f ds (OutputPatchConstant tessFactors, const OutputPatch<v2h,3>patch,float3 bary :SV_DOMAINLOCATION)
            //bary:重心坐标
            {
                v2h v;
                v.vertex = patch[0].vertex*bary.x + patch[1].vertex*bary.y + patch[2].vertex*bary.z;
			    v.tangent = patch[0].tangent*bary.x + patch[1].tangent*bary.y + patch[2].tangent*bary.z;
			    v.normal = patch[0].normal*bary.x + patch[1].normal*bary.y + patch[2].normal*bary.z;
			    v.uv = patch[0].uv*bary.x + patch[1].uv*bary.y + patch[2].uv*bary.z;

                t2f o = dvert (v);

                return o;
            }
   
            float4 frag (t2f i) : SV_Target
            {
                float3 viewDir=SafeNormalize(GetCameraPositionWS()-i.vertex);
                //return tex2D(_RT,uv);
                float3 bump=UnpackNormalScale(tex2D(_NormalMap,i.uv.xy),_BumpScale);
                float4 col=tex2D(_MainTex,i.uv.xy);
                col=lerp(col,_TrailColor,i.colorBuff);
                float sgn=i.tangentWS.w;
                float3 bitangent=sgn*cross(i.normalWS.xyz,i.tangentWS.xyz);
                bump=mul(bump,real3x3(i.tangentWS.xyz,bitangent.xyz,i.normalWS.xyz));
                bump=lerp(bump,i.normalWS,i.colorBuff);
                Light mainLight=GetMainLight();
                float4 lightColor=float4(mainLight.color,1.0);
                float3 lightDir=normalize(mainLight.direction);
                float4 ambient=float4(SampleSH(bump),1.0)*col;
                float4 diffuse=saturate(dot(lightDir,bump))*lightColor*col;
                
                float3 halfDir=SafeNormalize(viewDir+lightDir);
                float4 specular=pow(saturate(dot(halfDir,bump)),_Gloss)*lightColor*_SpecularColor;
                col=ambient+diffuse+specular;
                //col=ambient+diffuse;
                
                
                
                return float4(col.xyz,1);
            }
            ENDHLSL
        }
       
    }
}
