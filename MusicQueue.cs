namespace YukiMusicCSharp;

using System.Collections.Generic;

public class MusicQueue<T>
{
    private static readonly MusicQueue<T> instance = new MusicQueue<T>();
    private Dictionary<ulong, Queue<T>> queues;

    static MusicQueue()
    {
    }

    private MusicQueue()
    {
        queues = new Dictionary<ulong, Queue<T>>();
    }

    public static MusicQueue<T> Instance
    {
        get
        {
            return instance;
        }
    }

    public void Enqueue(ulong guildId, T item)
    {
        if (!queues.ContainsKey(guildId))
        {
            queues[guildId] = new Queue<T>();
        }

        queues[guildId].Enqueue(item);
    }

    public T Dequeue(ulong guildId)
    {
        return queues[guildId].Dequeue();
    }

    public T Peek(ulong guildId)
    {
        return queues[guildId].Peek();
    }

    public bool IsEmpty(ulong guildId)
    {
        return !queues.ContainsKey(guildId) || queues[guildId].Count == 0;
    }

    public int Size(ulong guildId)
    {
        if (!queues.ContainsKey(guildId))
        {
            return 0;
        }

        return queues[guildId].Count;
    }

    public void Clear(ulong guildid)
    {
        queues[guildid].Clear();
    }
}
