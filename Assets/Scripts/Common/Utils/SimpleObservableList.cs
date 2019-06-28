using System;
using System.Collections;
using System.Collections.Generic;

public class SimpleObservableList<T> : IList<T>
{
    public delegate void ItemAddedHandler(object sender, object item);
    public delegate void ItemRemovedHandler(object sender, object item, int oldIndex);
    public event ItemAddedHandler ItemAdded;
    public event ItemRemovedHandler ItemRemoved;
    public event EventHandler ListCleared;
    List<T> m_list = new List<T>();

    #region IList[T] implementation
    public int IndexOf(T value)
    {
        return m_list.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        m_list.Insert(index, value);
    }

    public void RemoveAt(int index)
    {
        m_list.RemoveAt(index);
    }

    public T this[int index]
    {
        get
        {
            return m_list[index];
        }
        set
        {
            m_list[index] = value;
        }
    }
    #endregion

    #region IEnumerable implementation
    public IEnumerator<T> GetEnumerator()
    {
        return m_list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    #region ICollection[T] implementation
    public void Add(T item)
    {
        m_list.Add(item);

        if (ItemAdded != null)
        {
            ItemAdded(this, item);
        }
    }

    public void Clear()
    {
        m_list.Clear();

        if (ListCleared != null)
        {
            ListCleared(this, EventArgs.Empty);
        }
    }

    public bool Contains(T item)
    {
        return m_list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        m_list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        int oldIndex = m_list.IndexOf(item);

        bool res = m_list.Remove(item);
        if (ItemRemoved != null)
        {
            ItemRemoved(this, item, oldIndex);
        }

        return res;
    }

    public int Count
    {
        get
        {
            return m_list.Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }
    #endregion
}