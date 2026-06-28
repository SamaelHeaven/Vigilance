#version vigilance_300

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 direction;
uniform int radius;
uniform float sigma;

void main()
{
    vec2 texelStep = direction / vec2(textureSize(texture0, 0));
    float sum = 0.0;
    float weightSum = 0.0;
    for (int i = -radius; i <= radius; i++)
    {
        float weight = exp(-0.5 * float(i * i) / (sigma * sigma));
        sum += texture(texture0, fragTexCoord + texelStep * float(i)).a * weight;
        weightSum += weight;
    }
    finalColor = vec4(1.0, 1.0, 1.0, sum / weightSum);
}
