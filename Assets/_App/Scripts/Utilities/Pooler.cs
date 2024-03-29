using System.Collections.Generic;

public class Pooler<T>
{
    private readonly List<T> _pooledObjects = new();
    private readonly List<T> _borrowedObjects = new();

    private System.Func<int, T> _createCallback;

    private int _objectCount;

    public int PooledCount => _pooledObjects.Count;

    public int BorrowedCount => _borrowedObjects.Count;

    public List<T> BorrowedObjects => _borrowedObjects;

    public void Initialize(System.Func<int, T> createCallback, int count)
    {
        this._createCallback = createCallback;

        for (int i = 0; i < count; i++)
        {
            T instance = createCallback(_objectCount++);
            _pooledObjects.Add(instance);
        }
    }

    public T BorrowItem()
    {
        if (_pooledObjects.Count > 0)
        {
            T borrowedObject = _pooledObjects[0];
            _pooledObjects.Remove(borrowedObject);
            _borrowedObjects.Add(borrowedObject);
            return borrowedObject;
        }
        else
        {
            T newObject = _createCallback(_objectCount++);
            _borrowedObjects.Add(newObject);
            return newObject;
        }
    }

    public void ReturnItem(T poolObject)
    {
        _borrowedObjects.Remove(poolObject);
        _pooledObjects.Add(poolObject);
    }

    public void ReturnAll(System.Action<T> returnCallback)
    {
        for (int i = _borrowedObjects.Count - 1; i >= 0; i--)
        {
            T borrowedObject = _borrowedObjects[i];

            returnCallback(borrowedObject);

            _borrowedObjects.Remove(borrowedObject);
            _pooledObjects.Add(borrowedObject);
        }
    }
}
