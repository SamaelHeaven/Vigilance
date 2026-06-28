#version vigilance_300

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;

void main()
{
    float alpha = texture(texture0, fragTexCoord).a;
    finalColor = vec4(1.0, 1.0, 1.0, alpha);
}
