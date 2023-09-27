using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ExtraMetadataLoader.Shaders
{
    public class SemiWhiteGrayscaleEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty(
            "Input", typeof(SemiWhiteGrayscaleEffect), 0);

        public static readonly DependencyProperty ExposureProperty =
            DependencyProperty.Register("Exposure", typeof(float), typeof(SemiWhiteGrayscaleEffect),
                new UIPropertyMetadata(1.0f, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty MaxLuminanceProperty =
            DependencyProperty.Register("MaxLuminance", typeof(float), typeof(SemiWhiteGrayscaleEffect),
                new UIPropertyMetadata(1.0f, PixelShaderConstantCallback(1)));

        public SemiWhiteGrayscaleEffect()
        {
            var assembly = typeof(SemiWhiteGrayscaleEffect).Assembly;
            var assemblyShortName = assembly.ToString().Split(',')[0];
            PixelShader = new PixelShader
            {
                UriSource = new Uri($"pack://application:,,,/{assemblyShortName};component/Shaders/SemiWhiteGrayscaleShader.ps")
            };

            UpdateShaderValue(ExposureProperty);
            UpdateShaderValue(MaxLuminanceProperty);
            UpdateShaderValue(InputProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public float Exposure
        {
            get => (float)GetValue(ExposureProperty);
            set => SetValue(ExposureProperty, value);
        }

        public float MaxLuminance
        {
            get => (float)GetValue(MaxLuminanceProperty); 
            set => SetValue(MaxLuminanceProperty, value);
        }
    }
}