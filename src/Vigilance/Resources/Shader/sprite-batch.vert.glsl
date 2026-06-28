#version vigilance_300

layout(location = 0) in vec2 vertexPosition;
layout(location = 1) in vec2 vertexTexCoord;
layout(location = 2) in vec2 instancePosition;
layout(location = 3) in vec2 instanceScale;
layout(location = 4) in float instanceRotation;
layout(location = 5) in vec2 instancePivotPoint;
layout(location = 6) in vec4 instanceTint;
layout(location = 7) in float instanceFlipX;
layout(location = 8) in float instanceFlipY;
layout(location = 9) in float instanceHasSource;
layout(location = 10) in vec4 instanceSource;

out vec2 fragTexCoord;
out vec4 fragColor;

uniform mat4 mvp;
uniform vec2 transformPosition;
uniform vec2 transformScale;
uniform float transformRotation;
uniform vec2 transformPivotPoint;
uniform vec2 textureSize;
uniform int flipY;

vec2 rotate(vec2 v, float deg)
{
    float rad = radians(deg);
    float s = sin(rad);
    float c = cos(rad);
    return vec2(v.x * c - v.y * s, v.x * s + v.y * c);
}

void main()
{
    vec4 source = instanceHasSource > 0.5 ? instanceSource : vec4(0, 0, textureSize);
    vec2 invTextureSize = 1.0 / textureSize;
    float sw = instanceFlipX > 0.5 ? -source.z : source.z;
    float sh = instanceFlipY > 0.5 ? -source.w : source.w;
    float sx = source.x;
    float sy = source.y < 0.0 ? source.y - sh : source.y;
    bool flipX = sw < 0.0;
    sw = abs(sw);
    float uLeft = sx * invTextureSize.x;
    float uRight = (sx + sw) * invTextureSize.x;
    float vTop = sy * invTextureSize.y;
    float vBottom = (sy + sh) * invTextureSize.y;
    float tx = flipX ? (1.0 - vertexTexCoord.x) : vertexTexCoord.x;
    float u = mix(uLeft, uRight, tx);
    float v = mix(vTop, vBottom, vertexTexCoord.y);
    v = flipY == 0 ? v : 1.0 - v;
    fragTexCoord = vec2(u, v);
    fragColor = instanceTint;
    vec2 position = instancePosition + transformPosition;
    vec2 scale = abs(instanceScale) * transformScale;
    float rotation = instanceRotation + transformRotation;
    vec2 pivotPoint = instancePivotPoint + transformPivotPoint;
    vec2 rotated = rotate(vertexPosition * scale - pivotPoint, rotation) + pivotPoint;
    gl_Position = vec4(rotated + position, 0.0, 1.0) * mvp;
}
