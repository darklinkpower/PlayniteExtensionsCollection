using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineChecker.Tests
{
	internal class TestableItemCollection<T> : IItemCollection<T> where T : DatabaseObject, new()
	{
		private List<T> _items;

		public TestableItemCollection(List<T> items)
		{
			_items = items;
		}

		public int UpdateCount { get; private set; }

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public bool ContainsItem(Guid id)
		{
			throw new NotImplementedException();
		}

		public GameDatabaseCollection CollectionType { get; }

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			_items.Add(item);
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public int Count { get; }
		public bool IsReadOnly { get; }

		public T Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public List<T> Get(IList<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public T Add(string itemName)
		{
			var alreadyExists = _items.FirstOrDefault(x => x.Name == itemName);
			if (alreadyExists != null)
			{
				return alreadyExists;
			}

			var toAdd = new T()
			{
				Id = Guid.NewGuid(),
				Name = itemName,
			};

			_items.Add(toAdd);
			return toAdd;
		}

		public T Add(string itemName, Func<T, string, bool> existingComparer)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<T> Add(List<string> items)
		{
			var alreadyExists = _items.Where(x => items.Contains(x.Name)).ToList();

			var toAdd = items
				.Except(alreadyExists.Select(x => x.Name))
				.Select(x => new T()
				{
					Id = Guid.NewGuid(),
					Name = x,
				}).ToList();

			_items.AddRange(toAdd);
			return toAdd.Concat(alreadyExists);
		}

		public T Add(MetadataProperty property)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<T> Add(IEnumerable<MetadataProperty> properties)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<T> Add(List<string> items, Func<T, string, bool> existingComparer)
		{
			throw new NotImplementedException();
		}

		public void Add(IEnumerable<T> items)
		{
			throw new NotImplementedException();
		}

		public bool Remove(Guid id)
		{
			throw new NotImplementedException();
		}

		public bool Remove(IEnumerable<T> items)
		{
			throw new NotImplementedException();
		}

		public void Update(T item)
		{
			UpdateCount++;
		}

		public void Update(IEnumerable<T> items)
		{
			throw new NotImplementedException();
		}

		public IDisposable BufferedUpdate()
		{
			throw new NotImplementedException();
		}

		public void BeginBufferUpdate()
		{
			throw new NotImplementedException();
		}

		public void EndBufferUpdate()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<T> GetClone()
		{
			throw new NotImplementedException();
		}

		public T this[Guid id]
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public event EventHandler<ItemCollectionChangedEventArgs<T>> ItemCollectionChanged;

		public event EventHandler<ItemUpdatedEventArgs<T>> ItemUpdated;
	}
}