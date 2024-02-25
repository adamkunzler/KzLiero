// This is a fragment shader
#if defined(GL_ES)
precision mediump float;
#endif

uniform sampler2D texture0;
varying vec2 fragTexCoord;
varying vec4 fragColor;

uniform vec4 shade;

void main()
{
    vec4 texelColor = texture2D(texture0, fragTexCoord);
    
    // gun is grays >= 200...just return those suckers
    float twoHundred = 0.7843137254901961; //200.0 / 255.0;
    if(texelColor.r >= twoHundred) 
        gl_FragColor = texelColor * fragColor;
    else
        gl_FragColor = shade * texelColor;
}