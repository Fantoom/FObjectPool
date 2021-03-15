using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FObjectPool
{
	public class ConcurrentObjectPool<TObject>
	{

		private ConcurrentQueue<TObject> pool;
		private SemaphoreSlim getSemaphore;
		private SemaphoreSlim addSemaphore;


		public event Action<TObject> OnAdd = delegate { };
		public event Action<TObject> OnGet = delegate { };
		private Func<TObject> objectCreation;
		private int maxCount = int.MaxValue;

		public int MaxCount
		{
			get { return maxCount; }
		}

		public ConcurrentObjectPool()
		{
			pool = new ConcurrentQueue<TObject>();
			getSemaphore = new SemaphoreSlim(0);
			addSemaphore = new SemaphoreSlim(maxCount);
		}

		public ConcurrentObjectPool(IEnumerable<TObject> initialObjects)
		{
			pool = new ConcurrentQueue<TObject>(initialObjects);
			getSemaphore = new SemaphoreSlim(initialObjects.Count());
			addSemaphore = new SemaphoreSlim(maxCount);
		}

		public ConcurrentObjectPool(Func<TObject> objectCreation)
		{
			if (objectCreation is null)
			{
				throw new ArgumentNullException(nameof(objectCreation));
			}

			this.objectCreation = objectCreation;
			pool = new ConcurrentQueue<TObject>();
			getSemaphore = new SemaphoreSlim(0);
			addSemaphore = new SemaphoreSlim(maxCount);
		}

		public ConcurrentObjectPool(int maxCount)
		{
			pool = new ConcurrentQueue<TObject>();
			this.maxCount = maxCount;
			getSemaphore = new SemaphoreSlim(0);
			addSemaphore = new SemaphoreSlim(maxCount);
		}

		public ConcurrentObjectPool(Func<TObject> objectCreation = null, IEnumerable<TObject> initialObjects = null, int maxCount = int.MaxValue)
		{
			if (initialObjects != null && initialObjects.Count() < maxCount)
			{
				throw new ArgumentException("Maximum items count can not be less than initial objects count", nameof(maxCount));
			}

			this.maxCount = maxCount;
			this.objectCreation = objectCreation ?? this.objectCreation;
			if (initialObjects != null)
			{
				pool = new ConcurrentQueue<TObject>(initialObjects);
				getSemaphore = new SemaphoreSlim(pool.Count);
			}
			else
			{
				pool = new ConcurrentQueue<TObject>();
			}
			getSemaphore = new SemaphoreSlim(initialObjects != null ? initialObjects.Count() : 0);
			addSemaphore = new SemaphoreSlim(maxCount - (initialObjects != null ? initialObjects.Count() : 0));
		}

		public async Task FillPool(int count, int millisecondsTimeout = 0, CancellationToken token = default)
		{
			if (objectCreation is null)
			{
				throw new Exception($"{nameof(objectCreation)} is not set");
			}

			if (count > maxCount)
			{
				throw new ArgumentException("Count can not be greater than maximum items count", nameof(count));
			}

			foreach (var item in Enumerable.Range(0, count).Select(x => objectCreation()))
			{
				await AddObjectAsync(item, millisecondsTimeout, token);
			}
		}

		public async Task FillPool(IEnumerable<TObject> objects, int millisecondsTimeout = 0, CancellationToken token = default)
		{
			if (objects.Count() > maxCount)
			{
				throw new ArgumentException("Objects count can not be greater than max count", nameof(objects));
			}
			foreach (var item in objects)
			{
				if (!await AddObjectAsync(item, millisecondsTimeout, token))
				{
					break;
				}
			}
		}

		public async Task<ConcurrentObjectPoolItem<TObject>> GetObjectAsync(int millisecondsTimeout = -1, CancellationToken token = default)
		{
			TObject obj = default;
			if (pool.Count == 0 && objectCreation != null)
			{
				obj = objectCreation();
			}
			else
			{
				if (await getSemaphore.WaitAsync(millisecondsTimeout, token))
				{
					if (pool.TryDequeue(out obj))
					{
						addSemaphore.Release();
					}
				}
			}
			OnGet(obj);
			return new ConcurrentObjectPoolItem<TObject>(this, obj);
		}

		public async Task<bool> AddObjectAsync(TObject objectToAdd, int millisecondsTimeout = -1, CancellationToken token = default)
		{

			if (await addSemaphore.WaitAsync(millisecondsTimeout, token))
			{
				if (!pool.Contains(objectToAdd) && pool.Count < maxCount)
				{
					pool.Enqueue(objectToAdd);
					OnAdd(objectToAdd);
					getSemaphore.Release();
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}

}
