shader_type spatial;
render_mode unshaded;

uniform vec3 velocity = vec3(0, 0, 0);
uniform float max_velocity = 1.0;
uniform float stretch_factor = 1.0;
uniform sampler2D noise;

void vertex() {
	// Movement skew
	vec3 clamped_velocity = velocity;
	if (length(velocity) > max_velocity) {
		clamped_velocity = normalize(velocity) * max_velocity;
	}
	VERTEX += clamped_velocity * (VERTEX.y + 1.0) * stretch_factor;
	
	// Shadow effect
	float offset = cos(TIME + (VERTEX.y * 4.0)) * 0.25;
	offset += cos(TIME * 1.2 + (VERTEX.y * 8.0)) * 0.25;
	VERTEX.xz += NORMAL.xz * texture(noise, VERTEX.xy).xy * offset;
}

void fragment() {
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	ALBEDO = vec3(0, 0, 0);
	ALPHA = clamp(1.0 - (fresnel * 0.25), 0.0, 1.0);
}