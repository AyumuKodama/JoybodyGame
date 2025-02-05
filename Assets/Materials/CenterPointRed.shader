Shader "Custom/CenterPointRed"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
    }

    SubShader  
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
        ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
        Pass  
        {
            CGPROGRAM  
            #pragma vertex vert_img  
            #pragma fragment frag  
            #include "UnityCG.cginc" 

            fixed4 _Color;

            fixed4 frag (v2f_img i) : SV_Target  
            {
                return _Color;  
            }

            ENDCG  
        }
    }
}
