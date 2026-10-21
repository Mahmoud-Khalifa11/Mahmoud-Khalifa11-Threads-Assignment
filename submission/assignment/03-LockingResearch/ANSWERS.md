# Assignment 03 — Answers

Student Name: Mahmoud khalifa

Group / ID: Your ID

### 1. What is System.Threading.Monitor used for?

Monitor is used to make sure only one thread can access a shared resource at a time. It uses Enter and Exit to control access and Wait and Pulse to let threads communicate with each other. It works with reference-type objects.

### 2. What does the C# lock statement compile to?

The lock statement uses Monitor.Enter and Monitor.Exit internally. It uses try and finally to make sure the lock is released even if an exception happens.

### 3. Differences between lock and Monitor

- lock is easier and safer because it releases the lock automatically
- Monitor gives more control with TryEnter which allows a thread to try getting a lock for a limited time
- Monitor also supports Wait and Pulse for communication between threads

### 4. When to use TryEnter Wait or Pulse

- TryEnter is used when I want to try getting a lock without waiting too long
- Wait is used when a thread needs to wait for a condition and releases the lock while waiting
- Pulse wakes up one waiting thread while PulseAll wakes up all waiting threads

### 5. The same critical section twice

**Using lock**

```csharp
private readonly object _lock = new();

public void Book()
{
    lock (_lock)
    {
        if (TablesLeft <= 0) return;

        TablesLeft--;
        Booked++;
    }
}
```

**Using Monitor.Enter and Monitor.Exit**

```csharp
private readonly object _lock = new();

public void Book()
{
    bool lockTaken = false;

    try
    {
        Monitor.Enter(_lock, ref lockTaken);

        if (TablesLeft <= 0) return;

        TablesLeft--;
        Booked++;
    }
    finally
    {
        if (lockTaken)
            Monitor.Exit(_lock);
    }
}
```

### Links

- https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock
- https://learn.microsoft.com/en-US/dotnet/fundamentals/runtime-libraries/system-threading-monitor