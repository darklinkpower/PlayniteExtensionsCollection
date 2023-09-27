sampler2D input : register(s0);

float Exposure : register(c0); // Controls brightness
float MaxLuminance : register(c1); // Controls maximum value of luminance possible

float4 main(float2 texCoord : TEXCOORD) : SV_TARGET
{
    // Sample the original color from the input texture
    float4 color = tex2D(input, texCoord);

    // Calculate the luminance of the color
    float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));

    // Adjust the luminance to control exposure
    luminance = min(luminance * Exposure, MaxLuminance);

    // Create a grayscale color with adjusted luminance
    color.rgb = float3(luminance, luminance, luminance);

    return color;
}