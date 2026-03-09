#define HIGH_PRECISION_VERTEX

#include "sh_Utils.h"
#include "sh_Masking.h"

layout(location = 2) in highp vec2 v_TexCoord;

layout(std140, set = 0, binding = 0) uniform m_NoteGlowParameters
{
    mediump float innerPortion;
    mediump float cornerRadius;
};

layout(location = 0) out vec4 o_Colour;

highp float roundedBoxSDF(highp vec2 center, highp vec2 halfSize, highp float radius)
{
    return length(max(abs(center) - halfSize + radius, 0.0)) - radius;
}

void main(void)
{
    highp vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    highp vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    highp vec2 uv = pixelPos * 2.0 - 1.0;

    highp vec2 halfSize = vec2(innerPortion);
    highp float cr = min(cornerRadius, innerPortion);

    highp float dist = roundedBoxSDF(uv, halfSize, cr);

    highp float glowExtent = max(1.0 - innerPortion, 0.001);
    highp float t = clamp(max(dist, 0.0) / glowExtent, 0.0, 1.0);
    highp float alpha = exp(-3.0 * t * t);

    highp vec2 edgeDist = vec2(1.0) - abs(uv);
    highp float edgeFade = smoothstep(0.0, 0.08, min(edgeDist.x, edgeDist.y));
    alpha *= edgeFade;

    o_Colour = getRoundedColor(vec4(vec3(1.0), alpha), v_TexCoord);
}
