#version 330

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;
out vec2 texCoords;

void main()
{
    vec4 worldCoordinates = vec4(vPos, 1.0) * model;
    gl_Position = worldCoordinates * view * projection;

    FragPos = vec3(worldCoordinates);
    Normal = vNormal * mat3(transpose(inverse(model)));
    texCoords = vTexCoord;
}
