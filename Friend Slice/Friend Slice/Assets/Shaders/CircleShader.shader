Shader "Custom/circle" 
 {
     Properties
     {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _InnerColor ("InnerColor", Color) = (1,1,1,1)
         _BorderColor ("BorderColor", Color) = (1,1,1,1)
         _InnerRadiusU ("Inner Radius U", Float) = 0.15
         _HalfMinusInnerRadiusU ("Half Minus Inner Radius U", Float) = 0.35
 
     }
 
     CGINCLUDE
     #include "UnityCG.cginc"
     struct appdata_t
     {
         float4 vertex   : POSITION;
         float4 color    : COLOR;
         float2 texcoord : TEXCOORD0;
     };
  
     struct v2f
     {
         float4 vertex   : SV_POSITION;
         fixed4 color    : COLOR;
         half2 texcoord  : TEXCOORD0;
     };
            
     sampler2D _MainTex;
  
     v2f vert(appdata_t IN)
     {
         v2f OUT;
         OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
         OUT.texcoord = IN.texcoord;
         OUT.color = IN.color;
         return OUT;
     }
  
     half _InnerRadiusU;
     half _HalfMinusInnerRadiusU;
     half4 _InnerColor;
     half4 _BorderColor;
  
     fixed4 frag(v2f IN) : COLOR
     {
         half4 texcol = tex2D (_MainTex, IN.texcoord);       
                 
         half2 local_pos = IN.texcoord-fixed2(0.5,0.5);
         half dist = length(local_pos);
         half border_t = saturate((dist - _InnerRadiusU) / _HalfMinusInnerRadiusU);
         half4 col = lerp(_InnerColor,_BorderColor,border_t);
         texcol.xyz *= col;
         texcol.w = border_t >= 0.999 ? 0 : 1;
         return texcol;
     }
     ENDCG
  
     SubShader
     {
         Tags
         {
             "Queue"="Transparent"
             "IgnoreProjector"="True"
             "RenderType"="Transparent"
             "PreviewType"="Plane"
             "CanUseSpriteAtlas"="True"
         }
  
         Cull Off
         Lighting Off
         ZWrite Off
         Fog { Mode Off }
         Blend SrcAlpha OneMinusSrcAlpha
  
         Pass
         {
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile DUMMY PIXELSNAP_ON
         ENDCG
         }
     }
     Fallback "Sprites/Default"
 }