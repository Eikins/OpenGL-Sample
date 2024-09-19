using GLSample.Rendering;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace GLSample.AssetLoaders
{
    public class TextureLoader
    {
        public static GLTexture LoadRGBA8Texture2DFromFile(GL gl, string path, bool useMips = true)
        {
            return LoadTexture2DFromFile<Rgba32>(gl, path, useMips);
        }

        public static GLTexture LoadRGB8Texture2DFromFile(GL gl, string path, bool useMips = true)
        {
            return LoadTexture2DFromFile<Rgb24>(gl, path, useMips);
        }

        public static GLTexture LoadTexture2DFromFile<TPixel>(GL gl, string path, bool useMips = true)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Image<TPixel> image = Image.Load<TPixel>(path);
            image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));

            var pixels = new TPixel[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            uint mipCount = 1;
            if (useMips)
            {
                // Compute the maximum number of mipmaps.
                mipCount = (uint) MathF.Floor(MathF.Log2(Math.Max(image.Width, image.Height))) + 1;
            }

            var textureDescriptor = new GLTextureDescriptor((uint)image.Width, (uint)image.Height, SizedInternalFormat.Rgba8, mipCount);
            var texture = new GLTexture(gl, textureDescriptor);
            texture.SetFilterMode(FilterMode.Linear);
            texture.SetWrapMode(WrapMode.Clamp);

            texture.SetData<TPixel>(pixels, PixelFormat.Rgba, PixelType.UnsignedByte);
            if (useMips && textureDescriptor.mipCount > 1)
            {
                texture.GenerateAllMips();
            }

            return texture;
        }
    }
}
