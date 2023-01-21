shader_type spatial;

uniform sampler2D wind_sway_weight;

uniform float sway_speed = 1.0;
uniform float sway_strength = 0.05;
uniform float sway_phase_len = 4.0;

void vertex() {
    float strength = texture(wind_sway_weight, UV).r * sway_strength;
    VERTEX.x += sin(VERTEX.x * sway_phase_len * 1.123 + TIME * sway_speed) * strength;
    VERTEX.y += sin(VERTEX.y * sway_phase_len + TIME * sway_speed * 1.12412) * strength * 0.25;
    VERTEX.z += sin(VERTEX.z * sway_phase_len * 0.9123 + TIME * sway_speed * 1.3123) * strength;
}

void fragment() {
//	ALBEDO = vec3(1.0, 1.0, 1.0) * texture(wind_sway_weight, UV, 0.0).rgb;
	ALBEDO = vec3(0.01, 0.06, 0.05);
	METALLIC = 0.0;
}
