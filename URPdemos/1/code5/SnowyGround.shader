Shader "Snowy/SnowyGround
"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _NormalMap("NormalMap",2D)="white"{}
        _RT("RT",2D)="white"{}

        _Roughness("Roughness",Range(0,5))=0.56
        _Metallic("Metallic",Range(0,2))=0.412
        _BumpScale("Bump Scale",Range(0,3))=1

        _Gloss("Gloss",Range(1,20))=5
        _SpecularColor("SpecularColor",Color)=(1,1,1,1)
        _BumpScale("Bump Scale",Range(1,10))=1.0
        _PlayerPosition("Player Position",Vector)=(1,1,1,1)
        _SnowRange("Snow Range",Vector)=(1,1,1,1)
        _TessellationUniform("TessellationUniform",Range(1,128))=10
        _DisplacementScale("DisplacementScale",Range(0,10))=1
        _TrailColor("Trail Color",Color)=(1,1,1,1)
        _TrailRange("Trail Range",Range(1,40))=7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"
                "Queue"="Geometry"}
        LOD 200
        HLSLINCLUDE
        
            #include "PBR.hlsl"
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
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 normalWS : TEXCOORD1; 
                float4 tangentWS : TEXCOORD2; 
                float4 bitangentWS : TEXCOORD3;
                float4 positionWS:TEXCOORD4;
                float colorBuff:TEXCOORD5;
                float3 bumpAdd:TEXCOORD6;
                
            };
        //    CBUFFER_START(UnityPerMaterial)
        //    TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
        //    CBUFFER_START(UnityPerMaterial)
            
            float4 _MainTex_ST;
           // float4 _BaseMap_ST;
            float _Roughness;
         //   float _Metallic;
         //   float _BumpScale;
       //     CBUFFER_END
            sampler2D _RT;
            float _Gloss;
            float4 _SpecularColor;
            float4 _PlayerPosition;
            float4 _SnowRange;
            float4 _TrailColor;
            float _DisplacementScale;
            float _TrailRange;
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
                OutputPatchConstant o;
                o.edge[0]=1;
                o.edge[1]=1;
                o.edge[2]=1;
                o.inside=1;
                if(distance(mul(UNITY_MATRIX_M,patch[0].vertex).xz,_PlayerPosition.xz)<=_TrailRange){
                o.edge[0] = _TessellationUniform;
                o.edge[1] = _TessellationUniform;
                o.edge[2] = _TessellationUniform;
                o.inside  = _TessellationUniform;
                }
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
            t2f dvert(v2h i){
                t2f o;
	            float3 positionWS = TransformObjectToWorld(i.vertex.xyz);
                o.positionWS=float4(positionWS,1);
	            o.uv.xy = TRANSFORM_TEX(i.uv, _BaseMap);
	            o.normalWS.xyz = normalize(TransformObjectToWorldNormal(i.normal.xyz));
	            o.tangentWS.xyz = normalize(TransformObjectToWorldDir(i.tangent.xyz));
	            o.bitangentWS.xyz = cross(o.normalWS.xyz, o.tangentWS.xyz) * i.tangent.w * unity_WorldTransformParams.w;
	            o.normalWS.w = positionWS.x;
	            o.tangentWS.w = positionWS.y;
	            o.bitangentWS.w = positionWS.z;
                float2 uv=float2(0.5*(-o.positionWS.x+_PlayerPosition.x)/_SnowRange.x+0.5,
                                 0.5*(-o.positionWS.z+_PlayerPosition.z)/_SnowRange.z+0.5);
                float height=(tex2Dlod(_RT,float4(uv,0,0)).w*2-1);
             //   if(uv.x>=0&&uv.x<=1&&uv.y>=0&&uv.y<=1)o.normalWS=tex2Dlod(_RT,float4(uv,0,0)).xyz*2-1;
                //if(height>=0.01||height<=-0.01)o.normalWS=tex2Dlod(_RT,float4(uv,0,0)).xyz*2-1;
                o.bumpAdd=float3(0,1,0);
                o.colorBuff=0;
                if(height>-0.99&&abs(height)>0.1){
                    o.bumpAdd=tex2Dlod(_RT,float4(uv,0,0)).xyz*2-1;
                    o.colorBuff=1;
                }
                //if(o.worldPos.y<=_PlayerPosition.y&&_PlayerPosition.y<=o.worldPos.y+3)o.worldPos.y+=height;
                //float sampleDis=0.2;
                //float h1=(tex2Dlod(_RT,float4(uv+float2(sampleDis,0),0,0)).w*2-1);
                //float h2=(tex2Dlod(_RT,float4(uv+float2(-sampleDis,0),0,0)).w*2-1);
                //float h3=(tex2Dlod(_RT,float4(uv+float2(0,sampleDis),0,0)).w*2-1);
                //float h4=(tex2Dlod(_RT,float4(uv+float2(0,-sampleDis),0,0)).w*2-1);
                //if(h1<0&&h2<0&&h3<0&&h4<0)o.bumpAdd=float3(0,1,0);
                

                o.positionWS.y+=height*_DisplacementScale;
                
	            o.positionCS = TransformWorldToHClip(o.positionWS);
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
   
        ENDHLSL
        Pass
        {
         Tags{
                "LightMode"="UniversalForward"
            }
           // Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma hull hullProgram
            #pragma domain ds
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            // make fog work

            float4 frag (t2f i) : SV_Target
            {
                float4 SHADOW_COORDS=TransformWorldToShadowCoord(i.positionWS);
                float shadow=MainLightRealtimeShadow(SHADOW_COORDS);

                float3 viewDir=SafeNormalize(GetCameraPositionWS()-i.positionWS);
                float3 bump=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv.xy),_BumpScale);
                if(i.colorBuff>0.02)bump=i.bumpAdd;
                float4 col=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv.xy);
                bump=mul(bump,real3x3(i.tangentWS.xyz,i.bitangentWS.xyz,i.normalWS.xyz));
                Light mainLight=GetMainLight();
                float4 lightColor=float4(mainLight.color,1.0);
                float3 lightDir=normalize(mainLight.direction);
                float3 outcolor=0;
          //  #ifdef _AdditionalLights
                int pixelLightCount=GetAdditionalLightsCount();
                for(int ind=0;ind<pixelLightCount;ind++){
                    Light light=GetAdditionalLight(ind,i.positionWS);
                    float3 ilightColor=light.color*(light.distanceAttenuation*light.shadowAttenuation);
                    PBR_result ip=PBR_Sp(col.xyz,_Metallic,_Roughness,bump,viewDir,normalize(light.direction),ilightColor);
                    outcolor+=ip.spe+ip.dif;
                  //  return float4(viewDir,1);
                    //float NdotL=saturate(dot(i.normalWS,light.direction));
                    //float3 ldif=light.color*((light.distanceAttenuation*light.shadowAttenuation)*NdotL)*col.rgb;
                    //float3 halfDir=normalize(light.direction+viewDir);
                    //float3 lspe=pow(saturate(dot(i.normalWS,halfDir)),15)*light.color*float3(0.8,0.8,0.8);
                    //outcolor+=ldif+lspe;
                }
         //  #endif
             //   return float4(col.xyz,1);
                PBR_result p=PBR_Sp(col.xyz,_Metallic,_Roughness,bump,viewDir,lightDir,lightColor.xyz);
                outcolor+=p.spe+p.dif;
                //col=ambient+diffuse;
                
                
                
                return float4(outcolor*shadow,1);
            }
            ENDHLSL
        }
      
        Pass
        {
            //ColorMask 0
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"
                  }
            Stencil{
                Ref [_StencilID]
                Comp [_SComp]
                Pass [_SOp]
            }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment DepthNormalsFragment
            #pragma hull hullProgram
            #pragma domain dds
                        
         
            Attributes tr(v2h input){
                Attributes o;
                o.texcoord=input.uv;
                o.positionOS=input.vertex;
                o.tangentOS=input.tangent;
                o.normal=input.normal;
                return o;
            }
            [domain("tri")]//同样需要定义图元
            Varyings dds (OutputPatchConstant tessFactors, const OutputPatch<v2h,3>patch,float3 bary :SV_DOMAINLOCATION)
            //bary:重心坐标
            {
                v2h v;
                v.vertex = patch[0].vertex*bary.x + patch[1].vertex*bary.y + patch[2].vertex*bary.z;
			    v.tangent = patch[0].tangent*bary.x + patch[1].tangent*bary.y + patch[2].tangent*bary.z;
			    v.normal = patch[0].normal*bary.x + patch[1].normal*bary.y + patch[2].normal*bary.z;
			    v.uv = patch[0].uv*bary.x + patch[1].uv*bary.y + patch[2].uv*bary.z;

                Varyings o = DepthNormalsVertex(tr(v));

                return o;
            }


           

            ENDHLSL
        }
    }
}
