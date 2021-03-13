using System;
using System.Collections.Generic;
using System.Text;

namespace FObjectPool
{
	public class ObjectPoolItem<TItem> : IDisposable
	{
		public ObjectPool<TItem> Pool { get; private set; }
		public TItem item { get; private set; }

		public TItem Item
		{
			get
			{
				if (!disposed)
					return item;
				else
					throw new ObjectDisposedException(nameof(Item), "Object is reverted back to pool");
			}
			private set => item = value;
		}

		private bool disposed = false;

		public ObjectPoolItem(ObjectPool<TItem> pool, TItem item)
		{
			Pool = pool;
			Item = item;
		}

		public void BackToPool()
		{
			Dispose();
		}

		public void Dispose()
		{
			Pool.AddObject(Item);
			disposed = true;
			item = default;
		}
	}
}
