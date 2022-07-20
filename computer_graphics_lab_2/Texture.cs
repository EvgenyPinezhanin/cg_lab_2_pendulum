using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Pendulum
{
    class Texture
    {
        private int Handle;
        private readonly List<byte> pixels;
        private readonly int Width, Height;

        public Texture(string imagePath)
        {
            Handle = GL.GenTexture();

            Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            Width = image.Width;
            Height = image.Height;
            pixels = new List<byte>(4 * image.Width * image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(image[x, y].R);
                    pixels.Add(image[x, y].G);
                    pixels.Add(image[x, y].B);
                    pixels.Add(image[x, y].A);
                }
            }
        }

        public void TextParameterWrap(int _TextureWrapModeS, int _TextureWrapModeT)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, _TextureWrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, _TextureWrapModeT);
        }

        public void TextParameterFilter(int _TextureMinFilter, int _TextureMagFilter)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, _TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, _TextureMagFilter);
        }

        public void GenerateTexture()
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteTexture(Handle);

                disposedValue = true;
            }
        }

        ~Texture()
        {
            GL.DeleteTexture(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
