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
            Image<Rgba32> image = (Image<Rgba32>) Image.Load(path);
            image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));

            var pixels = new Rgba32[image.Size.Width * image.Size.Height];
            image.CopyPixelDataTo(pixels);

            uint mipCount = 1;
            if (useMips)
            {
                // Compute the maximum number of mipmaps.
                mipCount = (uint) MathF.Floor(MathF.Log2(Math.Max(image.Size.Width, image.Size.Height))) + 1;
            }

            var textureDescriptor = new GLTextureDescriptor((uint)image.Size.Width, (uint)image.Size.Height, SizedInternalFormat.Rgba8, mipCount);
            var texture = new GLTexture(gl, textureDescriptor);
            texture.SetFilterMode(FilterMode.Linear);
            texture.SetWrapMode(WrapMode.Clamp);

            texture.SetData<Rgba32>(pixels, PixelFormat.Rgba, PixelType.UnsignedByte);
            if (useMips && textureDescriptor.mipCount > 1)
            {
                texture.GenerateAllMips();
            }

            return texture;
        }
    }
}
