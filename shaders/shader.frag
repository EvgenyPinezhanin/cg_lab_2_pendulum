#version 330

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};
 
struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

uniform Light light;
uniform Material material;
uniform vec3 viewPos;

in vec3 FragPos;
in vec3 Normal;
in vec2 texCoords;

out vec4 FragColor;

void main()
{
    // 1. Ambient component
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, texCoords));

    // 2. Diffuse component
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, texCoords));

    // 3. Specular component
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoords));

    // 4. Attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
                              light.quadratic * (distance * distance));

    // We apply attenuation by multiplying our components by it.
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;

    // Returning our final color of this fragment
    FragColor = vec4((ambient + diffuse + specular), 1.0);
    // FragColor = texture(material.diffuse, texCoords);
}
