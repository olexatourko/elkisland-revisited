shader_type spatial;
render_mode unshaded;

uniform vec3 velocity = vec3(0, 0, 0);
uniform float max_velocity = 1.0;
uniform float stretch_factor = 1.0;

void vertex() {
	vec3 clamped_velocity = velocity;
	if (length(velocity) > max_velocity) {
		clamped_velocity = normalize(velocity) * max_velocity;
	}
	VERTEX -= clamped_velocity * (VERTEX.y + 1.0) * stretch_factor;
}

void fragment() {
	// Place fragment code here.
//	vec4 tmp = INV_VIEW_MATRIX * vec4(VERTEX.x, VERTEX.y, VERTEX.z, 1.0);
//	ALBEDO = vec3(tmp.x, tmp.y, tmp.z);
}
