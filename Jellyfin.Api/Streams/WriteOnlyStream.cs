using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Api.Streams
{
    /// <summary>
    /// https://blog.stephencleary.com/2016/11/ziparchive-on-write-only-streams.html.
    /// </summary>
    public class WriteOnlyStream : Stream
    {
        private readonly Stream _stream;
        private long _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteOnlyStream"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public WriteOnlyStream(Stream stream)
        {
            _stream = stream;
        }

        /// <inheritdoc/>
        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public override bool CanRead => _stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => _stream.Length;

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Kestrel only allows us to perform async I/O, so convert this synchronous request into an async one.
            this.WriteAsync(buffer, offset, count).Wait();
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult) => _stream.EndWrite(asyncResult);

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            this.WriteAsync(new byte[] { value }, 0, 1, CancellationToken.None).Wait();
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _position += count;
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _position += buffer.Length;
            return _stream.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _stream.FlushAsync().Wait();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }
    }
}
