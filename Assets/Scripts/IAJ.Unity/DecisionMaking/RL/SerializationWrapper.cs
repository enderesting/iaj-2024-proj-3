using System.Collections.Generic;

[System.Serializable]
public class SerializationWrapper<T>
{
    public List<T> data;

    public SerializationWrapper(List<T> data)
    {
        this.data = data;
    }
}
