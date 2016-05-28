//=========================================================================
//
//	HDRSample
//
//		by MJP
//		09/20/08
//
//=========================================================================
//
//	File:		pp_Tonemap.fx
//
//	Desc:		Contains tone mapping routines used during post-processing.
//				The operator is derived from "Photographic Tone 
//				Reproduction for Digital Images" by Eric Reinhard.
//
//=========================================================================

float WhiteLevel = 5;
float LuminanceSaturation = 1;
float Bias = 0.5;

float g_fMiddleGrey = 0.6f;
float g_fMaxLuminance = 16.0f;

// Approximates luminance from an RGB value
float CalcLuminance(float3 color)
{
    return max(dot(color, float3(0.299f, 0.587f, 0.114f)), 0.0001f);
    //return max(dot(color, float3(0.212656f, 0.715158f, 0.072186f)), 0.0001f);
}

// Determines the color based on exposure settings (Auto key)
float3 CalcExposedColor(float3 color, float avgLuminance, float threshold)
{
    float exposure = 0;

    // Use geometric mean        
    avgLuminance = max(avgLuminance, 0.001f);

    float keyValue = 1.03f - (2.0f / (2 + log10(avgLuminance + 1)));

    float linearExposure = (keyValue / avgLuminance);
    exposure = log2(max(linearExposure, 0.0001f));

    exposure -= threshold;
    return exp2(exposure) * color;
}

// Determines the color based on exposure settings
float3 CalcExposedColor(float3 color, float avgLuminance, float threshold, float keyValue)
{
    float exposure = 0;

    // Use geometric mean        
    avgLuminance = max(avgLuminance, 0.001f);

    float linearExposure = (keyValue / avgLuminance);
    exposure = log2(max(linearExposure, 0.0001f));

    exposure -= threshold;
    return exp2(exposure) * color;
}

// Logarithmic mapping
float3 ToneMapLogarithmic(float3 color)
{
    float pixelLuminance = CalcLuminance(color);
    float toneMappedLuminance = log10(1 + pixelLuminance) / log10(1 + WhiteLevel);
    return toneMappedLuminance * pow(color / pixelLuminance, LuminanceSaturation);
}

// Drago's Logarithmic mapping
float3 ToneMapDragoLogarithmic(float3 color)
{
    float pixelLuminance = CalcLuminance(color);
    float toneMappedLuminance = log10(1 + pixelLuminance);
    toneMappedLuminance /= log10(1 + WhiteLevel);
    toneMappedLuminance /= log10(2 + 8 * ((pixelLuminance / WhiteLevel) * log10(Bias) / log10(0.5f)));
    return toneMappedLuminance * pow(color / pixelLuminance, LuminanceSaturation);
}

// Exponential mapping
float3 ToneMapExponential(float3 color)
{
    float pixelLuminance = CalcLuminance(color);
    float toneMappedLuminance = 1 - exp(-pixelLuminance / WhiteLevel);
    return toneMappedLuminance * pow(color / pixelLuminance, LuminanceSaturation);
}

// Applies Reinhard's basic tone mapping operator
float3 ToneMapReinhard(float3 color)
{
    float pixelLuminance = CalcLuminance(color);
    float toneMappedLuminance = pixelLuminance / (pixelLuminance + 1);
    return toneMappedLuminance * pow(color / pixelLuminance, LuminanceSaturation);
}

// Applies Reinhard's modified tone mapping operator
float3 ToneMapReinhardModified(float3 color)
{
    float pixelLuminance = CalcLuminance(color);
    float toneMappedLuminance = pixelLuminance * (1.0f + pixelLuminance / (g_fMaxLuminance * g_fMaxLuminance)) / (1.0f + pixelLuminance);
    return toneMappedLuminance * pow(color / pixelLuminance, LuminanceSaturation);
}
// Applies the filmic curve from John Hable's presentation
float3 ToneMapFilmicALU(float3 color)
{
    color = max(0, color - 0.004f);
    color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f) + 0.06f);

    // result has 1/2.2 baked in
    return pow(color, 2.2f);
}

// Function used by the Uncharted 2 tone mapping curve
float3 U2Func(float3 x)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;

    //float A = 0.22;
    //float B = 0.30;
    //float C = 0.10;
    //float D = 0.20;
    //float E = 0.01;
    //float F = 0.30;

    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

// Applies the Uncharted 2 filmic tone mapping curve
float3 ToneMapFilmicU2(float3 vColor)
{
    float LinearWhite = 11.2;

    float3 numerator = U2Func(vColor);
    float3 denominator = U2Func(LinearWhite);

    return numerator / denominator;
}

//float3 _toneDefault(float3 vColor, float fLumAvg)
//{
//    // Calculate the luminance of the current pixel
//    float fLumPixel = CalcLuminance(vColor);
//	
//	// Apply the modified operator (Eq. 4)
//    float fLumScaled = (fLumPixel * g_fMiddleGrey) / fLumAvg;
//    float fLumCompressed = (fLumScaled * (1 + (fLumScaled / (g_fMaxLuminance * g_fMaxLuminance)))) / (1 + fLumScaled);
//    return fLumCompressed * vColor / fLumPixel;
//}

float3 fToneMap(float3 vColor)
{
	// Get the calculated average luminance 
    float fLumAvg = tex2D(PointSampler1, float2(0.5f, 0.5f));
	//float fLumAvg = min(10, max(0.5, tex2D(PointSampler1, float2(0.5f, 0.5f)))).r;	

    //vColor = CalcExposedColor(vColor, fLumAvg, 0)

    // Calculate the luminance of the current pixel
    //float fLumPixel = CalcLuminance(vColor);
    //// Apply the modified operator (Eq. 2)
    //float fLumScaled = (fLumPixel * g_fMiddleGrey) / fLumAvg;
    //float3 color = vColor * fLumScaled / fLumPixel;

    //or
    //float3 color = vColor.rgb * (g_fMiddleGrey / (fLumAvg + 0.001f));

    //or
    float3 color = CalcExposedColor(vColor, fLumAvg, 0, g_fMiddleGrey);

    //return _toneDefault(vColor, fLumAvg);
    return ToneMapFilmicU2(color);
}