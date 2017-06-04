Shader "Custom/Premultiplied" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    } 
    SubShader {
        Tags { "Queue"="Transparent" }
        Blend One OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        ZWrite Off
        Cull Off
        Pass {
            SetTexture [_MainTex] {
   	  	       constantColor (1,1,1,1)
               combine texture
            } 
        }
    }
}