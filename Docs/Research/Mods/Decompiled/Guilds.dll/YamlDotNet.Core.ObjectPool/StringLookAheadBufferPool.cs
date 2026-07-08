using System;

namespace YamlDotNet.Core.ObjectPool;

internal static class StringLookAheadBufferPool
{
	internal readonly struct BufferWrapper : IDisposable
	{
		public readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer Buffer;

		private readonly ObjectPool<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer> pool;

		public BufferWrapper(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer buffer, ObjectPool<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer> pool)
		{
			Buffer = buffer;
			this.pool = pool;
		}

		public override string ToString()
		{
			return Buffer.ToString();
		}

		public void Dispose()
		{
			pool.Return(Buffer);
		}
	}

	private static readonly ObjectPool<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer> Pool = ObjectPool.Create(new DefaultPooledObjectPolicy<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer>());

	public static BufferWrapper Rent(string value)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer = Pool.Get();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer.Value = value;
		return new BufferWrapper(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer, Pool);
	}
}
