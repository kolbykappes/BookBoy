using System;
using System.Collections.Generic;

namespace BookBoy.Rules
{
    /// <summary>The things the player is holding: a capacity-limited list with one "current" item that can be cycled.
    /// The item picked up most recently becomes current.</summary>
    public sealed class CarryList<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private int _current;

        public CarryList(int capacity)
        {
            Capacity = Math.Max(0, capacity);
        }

        public int Capacity { get; private set; }
        public int Count => _items.Count;
        public bool IsFull => _items.Count >= Capacity;
        public bool IsEmpty => _items.Count == 0;
        public IReadOnlyList<T> Items => _items;
        public int CurrentIndex => _items.Count == 0 ? -1 : _current;
        public T Current => _items.Count == 0 ? null : _items[_current];

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public bool TryAdd(T item)
        {
            if (item == null || IsFull || _items.Contains(item))
            {
                return false;
            }

            _items.Add(item);
            _current = _items.Count - 1;
            return true;
        }

        public bool Remove(T item)
        {
            int index = _items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            _items.RemoveAt(index);
            if (_items.Count == 0)
            {
                _current = 0;
            }
            else if (index < _current)
            {
                _current--;
            }
            else if (_current >= _items.Count)
            {
                _current = 0;
            }

            return true;
        }

        public T RemoveCurrent()
        {
            T item = Current;
            if (item != null)
            {
                Remove(item);
            }

            return item;
        }

        /// <summary>Moves "current" forward (+1) or backward (-1), wrapping around. No-op with fewer than two items.</summary>
        public void Cycle(int direction)
        {
            if (_items.Count < 2)
            {
                return;
            }

            int step = direction >= 0 ? 1 : -1;
            _current = (_current + step + _items.Count) % _items.Count;
        }

        /// <summary>Changes the capacity. Held items are never dropped by shrinking; the list just cannot grow until it
        /// is below the new capacity.</summary>
        public void SetCapacity(int capacity)
        {
            Capacity = Math.Max(0, capacity);
        }
    }
}
