#include "Macros.fxh"
float4x4 View;
float4x4 Projection;
float2 ViewportScale;
float CurrentTime;
float NumberOfImages;
float2 Size;
float RotateTowardCamera;
texture t0;

sampler Sampler = sampler_state
{
    Texture = (t0);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float3 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
    float CreationTime : TEXCOORD1;
    float EndTime : TEXCOORD2;
    float2 Scale : TEXCOORD3;
    float3 Rotatation : TEXCOORD4;
    float3 Speed : TEXCOORD5;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
    float4 Color : TEXCOORD1;
};

float2 ComputeParticleSize()
{
    // Project the size into screen coordinates.
    return Size * Projection._m11;
}

float2x2 ComputeParticleRotation(float Rotatation)
{
    // Compute a 2x2 rotation matrix.
    float c = cos(Rotatation);
    float s = sin(Rotatation);
    return float2x2(c, -s, s, c);
}

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    float Age = CurrentTime - input.CreationTime;
    float Duration = input.CreationTime - input.EndTime;
    float normalizedAge = saturate(Age / Duration);
    float CurrentImage = floor(normalizedAge * NumberOfImages) % NumberOfImages;

	float3 tempPos = input.Position + input.Speed * Age;
    float2 UVPosition = (input.UV * 2 - 1);

    if (RotateTowardCamera)
    {
        float2 size = ComputeParticleSize();
        float2x2 rotation = ComputeParticleRotation(0);
        output.Position = mul(mul(float4(tempPos, 1), View), Projection);
        output.Position.xy += mul(UVPosition, rotation) * size * ViewportScale;
        output.UV = float2(input.UV.x / NumberOfImages + CurrentImage / NumberOfImages, input.UV.y);
    }
    else
    {
        matrix <float, 4, 4> rotationAroundY = {
               cos(input.Rotatation.y * Age), 0.0f, -sin(input.Rotatation.y * Age), 0.0f,
               0.0f, 1.0f, 0.0f, 0.0f,
               sin(input.Rotatation.y * Age), 0.0f, cos(input.Rotatation.y * Age), 0.0f,
               0.0f, 0.0f, 0.0f, 1.0f   
                           };

        float2x2 RotationAroundZ = ComputeParticleRotation(input.Rotatation.z * Age);

	    tempPos.xy += mul(mul(UVPosition, RotationAroundZ), rotationAroundY) * input.Scale * Size;
        output.Position = mul(mul(float4(tempPos, 1), View), Projection);
        output.UV = float2(input.UV.x / NumberOfImages + CurrentImage / NumberOfImages, 1 - input.UV.y);
    }
    
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : SV_Target
{
    float4 Color = SAMPLE_TEXTURE(Sampler, input.UV) * input.Color;
    clip(Color.a < 0.9f ? -1 : 1);
    return Color;
}

TECHNIQUE(Particles, ParticleVertexShader, ParticlePixelShader);
