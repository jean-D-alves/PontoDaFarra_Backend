using System;

namespace PontoDaFarra.services
{
    public readonly record struct ImageFormat(string ContentType, string Extension);

    public static class ImageSignatureValidator
    {
        public static ImageFormat? Detect(Stream stream)
        {
            Span<byte> header = stackalloc byte[12];
            var read = 0;

            while (read < header.Length)
            {
                var count = stream.Read(header[read..]);
                if (count == 0) break;
                read += count;
            }

            if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return new ImageFormat("image/jpeg", ".jpg");

            if (read >= 8 && header[..8].SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                return new ImageFormat("image/png", ".png");

            if (read >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header[8..12].SequenceEqual("WEBP"u8))
                return new ImageFormat("image/webp", ".webp");

            return null;
        }
    }
}
