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
    else {
        // Convert to grayscale using luminosity method
        float gray = (0.21 * texelColor.r + 0.71 * texelColor.g + 0.07 * texelColor.b) / 4.0;        
        gl_FragColor = vec4(gray, gray, gray, 0.25);        
    }
}