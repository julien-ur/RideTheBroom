
Shader "Hidden/radialBlur"
{
 
/*    Properties
    {
        _MainTex ("Input", RECT) = "white" {}
        tDiffuse ("Base (RGB)", 2D) = "white" {}
        fX ("fX", Float) = 0.5
        fY ("fY", Float) = 0.5
        fExposure ("fExposure", Float) = 0.6
        fDecay ("fDecay", Float) = 0.93
        fDensity ("fDensity", Float) = 0.96
        fWeight ("fWeight", Float) = 0.4
        fClamp ("fClamp", Float) = 1.0
        //iSamples ("iSamples", Int) = 20
    }

    SubShader
    {
        Pass
        {
                ZTest Always Cull Off ZWrite Off
                Fog { Mode off }

            Tags { "RenderType"="Opaque" }
            LOD 200
            Cull Off
             
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            
            uniform sampler2D _MainTex;
            sampler2D tDiffuse;
            float fX,fY,fExposure,fDecay,fDensity,fWeight,fClamp,iSamples;
            uniform half _iWidth;
            uniform half _iHeight;
             
            struct Input
            {
                float2 uvtDiffuse;
                float4 screenPos;
            };
             
            //void surf (Input IN, inout SurfaceOutput o)
            half4 frag (v2f_img i) : COLOR
            {
                half4 color = tex2D(_MainTex, i.uv);
                //return color;

                int iSamples=100;
                float2 vUv = i.uv;
                //vUv *= float2(1,1); // repeat?
                //half2 dir = 0.5 * half2(_iHeight,_iWidth) - i.uv;

                //float2 deltaTextCoord = float2(vUv - float2(fX,fY));

                float2 deltaTextCoord = 0.5 * half2(_iHeight,_iWidth) - i.uv;
                fClamp = sqrt(deltaTextCoord.x*deltaTextCoord.x + deltaTextCoord.y*deltaTextCoord.y);

                deltaTextCoord *= 1.0 /  float(iSamples) * fDensity;
                float2 coord = vUv;
                float illuminationDecay = 1.0;
                float4 FragColor = color;

                for(int j=0; j < iSamples ; j++)
                {
                    coord -= deltaTextCoord;
                    float4 texel = tex2D(_MainTex, coord + deltaTextCoord);
                    texel *= illuminationDecay * fWeight;
                    FragColor += texel;
                    illuminationDecay *= fDecay;
                }

                FragColor *= fExposure;
                
                FragColor = clamp(FragColor, 0.0, fClamp);
                float4 c = FragColor;

                return lerp(color, c, fClamp);
                //o.Albedo = c.rgb;
                //o.Alpha = c.a;
            }

            ENDCG
        }
    }
}*/







      Properties {
        _MainTex ("Input", RECT) = "white" {}
        _BlurStrength ("", Float) = 0.5
        _BlurWidth ("", Float) = 0.5
    }
        SubShader {
            Pass {
                ZTest Always Cull Off ZWrite Off
                Fog { Mode off }
           
        CGPROGRAM
       
        #pragma vertex vert_img
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
     
        #include "UnityCG.cginc"
     
        uniform sampler2D _MainTex;
        uniform sampler2D _CameraDepthTexture;
        uniform half _BlurStrength;
        uniform half _BlurWidth;
        uniform half _iWidth;
        uniform half _iHeight;
        uniform half _ClearRadius;
     
        half4 frag (v2f_img i) : COLOR {

            #if UNITY_UV_STARTS_AT_TOP
            if (_MainTex_TexelSize.y < 0)
                    i.uv.y = 1-i.uv.y;
            #endif

            //float2 depth_uv = i.uvgrab.xy / i.uvgrab.w;
            half4 depth_col = tex2D( _CameraDepthTexture, i.uv );
            float depth = 1 - Linear01Depth(depth_col.r);
            //float depth = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.uvgrab)).r);


            half4 color = tex2D(_MainTex, i.uv);
           
            // some sample positions
            half samples[10] = {-0.08,-0.05,-0.03,-0.02,-0.01,0.01,0.02,0.03,0.05,0.08};
           
            //vector to the middle of the screen
            half2 dir = 0.5 * half2(_iHeight,_iWidth) - i.uv;
           
            //distance to center
            half dist = sqrt(dir.x*dir.x + dir.y*dir.y);
           
            if(dist < _ClearRadius) return color;

            //normalize direction
            dir = dir/dist;
           
            //additional samples towards center of screen
            half4 sum = color;
            for(int n = 0; n < 10; n++)
            {
                //sum += tex2D(_CameraDepthTexture, i.uv);
                sum += tex2D(_MainTex, i.uv + dir * dir * samples[n] * _BlurWidth * _iWidth * depth);
                //sum += tex2D(_MainTex, dir * samples[n] * _BlurWidth * _iWidth);
                //sum += tex2D(_MainTex, i.uv + half2(samples[n],samples[9-n]));
                //sum += tex2D(_MainTex, i.uv + dir * samples[n] * _BlurWidth * _iWidth);
            }
           
            //eleven samples...
            sum *= (1.0/11.0);
           
            //weighten blur depending on distance to screen center
            half t = dist * _BlurStrength / _iWidth;
            t = clamp(t, 0.0, 1);
           
            //blend original with blur
            return lerp(color, sum, 1);
        }
        ENDCG
            }
        }
        }

