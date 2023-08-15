﻿using System.IO;

namespace NeeView
{
    // TODO: これ意味ないように見えるのだが？
    // NOTE: Streamを保持することで開放されないようにしている？けどその役目を果たしているのか？
    public sealed class WrappingStream : Stream
    {
        private Stream BaseStream { get; set; }

        public WrappingStream(Stream stream) => BaseStream = stream;

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public override void Flush() => BaseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        public override void SetLength(long value) => BaseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseStream.Dispose();
                //BaseStream = null;
            }

            base.Dispose(disposing);
        }
    }
}
