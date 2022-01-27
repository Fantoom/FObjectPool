using System;
using System.Collections.Generic;
using System.Linq;

namespace FObjectPool
{
	public class ObjectPool<TObject>
	{
		private Queue<TObject> pool;
		public event Action<TObject> OnAdd = delegate { };
		public event Action<TObject> OnGet = delegate { };
		private Func<TObject>? objectCreation;
		private int maxCount = int.MaxValue;

		public int MaxCount
		{
			get { return maxCount; }
			set { maxCount = value > MaxCount ? value : MaxCount; }
		}

		public ObjectPool()
		{
			pool = new Queue<TObject>();
		}

		public ObjectPool(IEnumerable<TObject> initialObjects)
		{
			pool = new Queue<TObject>(initialObjects);
		}

		public ObjectPool(Func<TObject> objectCreation)
		{
			if (objectCreation is null)
			{
				throw new ArgumentNullException(nameof(objectCreation));
			}

			this.objectCreation = objectCreation;
			pool = new Queue<TObject>();
		}

		public ObjectPool(int maxCount)
		{
			pool = new Queue<TObject>();
			this.maxCount = maxCount;
 		}

		public ObjectPool(Func<TObject>? objectCreation = null, IEnumerable<TObject>? initialObjects = null, int maxCount = int.MaxValue)
		{

			if (initialObjects != null && initialObjects.Count() < maxCount)
			{
				throw new ArgumentException("Maximum items count can not be less than initial objects count", nameof(maxCount));
			}

			this.maxCount = maxCount;
			this.objectCreation = objectCreation ?? this.objectCreation;
			if(initialObjects != null)
			{ 
				pool = new Queue<TObject>(initialObjects);
			}
			else
            {
				pool = new Queue<TObject>();
			}
		}

		public void FillPool(int count)
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
				AddObject(item);
			}
		}

		public void FillPool(IEnumerable<TObject> objects)
		{
			if (objects.Count() > maxCount)
			{
				throw new ArgumentException("Objects count can not be greater than max count", nameof(objects));
			}
			foreach (var item in objects)
			{
				if (!AddObject(item))
				{
					break;
				}
			}
		}

		public ObjectPoolItem<TObject> GetObject()
		{
			TObject obj = default!;
			if (pool.Count == 0)
			{
				if (objectCreation != null)
				{
					obj = objectCreation();
				}
			}
			else 
			{
				obj = pool.Dequeue();
			}
			OnGet(obj);
			return new ObjectPoolItem<TObject>(this, obj);
		}

		public bool AddObject(TObject objectToAdd)
		{
			if (!pool.Contains(objectToAdd) && pool.Count < maxCount)
			{
				pool.Enqueue(objectToAdd);
				OnAdd(objectToAdd);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
