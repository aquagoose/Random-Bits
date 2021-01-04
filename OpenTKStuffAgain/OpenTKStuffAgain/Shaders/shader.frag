#version 330

in vec2 texCoord;

out vec4 outputColor;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main() {
	outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord, 0.2), 0.2);
}