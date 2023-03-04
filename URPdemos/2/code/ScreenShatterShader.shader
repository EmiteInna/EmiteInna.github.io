Shader "Summer/ScreenShatterShader"
{
    Properties
    {

        [Header(Main Texture Setting)]
        [Space(5)]
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        _NormalMap("Normal Map",2D)="white"{}
        _ShatterMap("Shatter Map",2D)="white"{}
        
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalMap);           SAMPLER(sampler_NormalMap);
        TEXTURE2D(_ShatterMap);           SAMPLER(sampler_ShatterMap);

        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float _BumpScale;
        CBUFFER_END

        struct a2v{
            float3 vertex : POSITION;    
   
            float2 texCoord : TEXCOORD0;  
        };
        struct v2f{
            float4 pos : POSITION;            
            float2 uv : TEXCOORD0;          
            float3 worldPos : TEXCOORD1;      
            float3 worldNormal : TEXCOORD2;    
            float3 worldTangent : TEXCOORD3;    
            float3 worldBiTangent : TEXCOORD4;  
            half4 color : COLOR0;               
        };
        
        v2f vert(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex);
            o.uv = v.texCoord;

            return o;
        }
        ENDHLSL
        Pass
        {
            Name "Screen Shatter Post Processing"
    
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            inline half3 GammaToLinearSpace (half3 sRGB)
            {
                // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);

                // Precise version, useful for debugging.
                //return half3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
            }
            inline half3 LinearToGammaSpace (half3 linRGB)
            {
                linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
                // An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);

                // Exact version, useful for debugging.
                //return half3(LinearToGammaSpaceExact(linRGB.r), LinearToGammaSpaceExact(linRGB.g), LinearToGammaSpaceExact(linRGB.b));
            }
            half4 frag(v2f i) : SV_TARGET
            {   
                float4 colOriginal=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                float4 mk=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv);
                mk.rgb=LinearToGammaSpace(mk.rgb);
               // return float4(mk.x,mk.x,mk.x,0);
             //   float test=abs(mk.x-i.uv.y);
             //   return float4(1,1,1,1)*test;
                float2 bump=(mk.xy*2-1)*_BumpScale;
                float4 colShatter=SAMPLE_TEXTURE2D(_ShatterMap,sampler_ShatterMap,float2(i.uv.x,mk.b)+bump);

                return lerp(colOriginal,colShatter,mk.a);
          
            }
            ENDHLSL
        }
     
        //Pass
        //{
        //    Tags {"LightMode" = "DepthNormals"}
        //    HLSLPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment fragD
        //    half4 fragD(v2f i):SV_Target{
        //        return half4(0,0,0,1);
        //    }
        //    ENDHLSL
        //}
    }
}