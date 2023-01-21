shader_type spatial;
render_mode unshaded;

uniform sampler2D noise;

void vertex() {
	float offset = cos(TIME + (VERTEX.y * 4.0)) * 0.25;
	offset += cos(TIME * 1.2 + (VERTEX.y * 8.0)) * 0.25;
	VERTEX.xz += NORMAL.xz * texture(noise, VERTEX.xy).xy * offset;
}

void fragment() {
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	ALBEDO = vec3(0, 0, 0);
	ALPHA = clamp(1.0 - (fresnel * 0.25), 0.0, 1.0);
}