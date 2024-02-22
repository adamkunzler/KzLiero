// This is a fragment shader
#if defined(GL_ES)
precision mediump float;
#endif

uniform sampler2D texture0;
varying vec2 fragTexCoord;
varying vec4 fragColor;

void main()
{
    vec4 texelColor = texture2D(texture0, fragTexCoord);
    if(texelColor.rgb == vec3(0.0, 0.0, 0.0)) // Black color
        discard; // Discard makes the pixel transparent
    else
        gl_FragColor = texelColor * fragColor; // Apply the texture color
}