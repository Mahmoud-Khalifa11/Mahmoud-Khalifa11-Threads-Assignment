using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrderProcessing.Shared.Exceptions;

namespace OrderProcessing.Shared.Storage;

public sealed class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates =
        new(StringComparer.OrdinalIgnoreCase);

    private SemaphoreSlim GateFor(string fileName) =>
        _gates.GetOrAdd(fileName, _ => new SemaphoreSlim(1, 1));

    // ---------------- async API ----------------

    public async Task<List<T>> ReadAllAsync<T>(string fileName, CancellationToken ct)
    {
        SemaphoreSlim gate = GateFor(fileName);
        await gate.WaitAsync(ct);
        try
        {
            return await ReadCoreAsync<T>(fileName, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task WriteAllAsync<T>(string fileName, List<T> items, CancellationToken ct)
    {
        SemaphoreSlim gate = GateFor(fileName);
        await gate.WaitAsync(ct);
        try
        {
            await WriteCoreAsync(fileName, items, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<TResult> UpdateAsync<T, TResult>(
        string fileName, Func<List<T>, TResult> update, CancellationToken ct)
    {
        SemaphoreSlim gate = GateFor(fileName);
        await gate.WaitAsync(ct);
        try
        {
            List<T> items = await ReadCoreAsync<T>(fileName, ct);
            TResult result = update(items);
            await WriteCoreAsync(fileName, items, ct);
            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public Task UpdateAsync<T>(string fileName, Action<List<T>> update, CancellationToken ct) =>
        UpdateAsync<T, bool>(fileName, items => { update(items); return true; }, ct);

    // ---------------- sync API ----------------

    public List<T> ReadAll<T>(string fileName)
    {
        SemaphoreSlim gate = GateFor(fileName);
        gate.Wait();
        try
        {
            return ReadCore<T>(fileName);
        }
        finally
        {
            gate.Release();
        }
    }

    public void WriteAll<T>(string fileName, List<T> items)
    {
        SemaphoreSlim gate = GateFor(fileName);
        gate.Wait();
        try
        {
            WriteCore(fileName, items);
        }
        finally
        {
            gate.Release();
        }
    }

    public TResult Update<T, TResult>(string fileName, Func<List<T>, TResult> update)
    {
        SemaphoreSlim gate = GateFor(fileName);
        gate.Wait();
        try
        {
            List<T> items = ReadCore<T>(fileName);
            TResult result = update(items);
            WriteCore(fileName, items);
            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Update<T>(string fileName, Action<List<T>> update) =>
        Update<T, bool>(fileName, items => { update(items); return true; });

    // ---------------- core read / write ----------------

    private static async Task<List<T>> ReadCoreAsync<T>(string fileName, CancellationToken ct)
    {
        string path = DataFiles.PathOf(fileName);
        if (!File.Exists(path))
            return new List<T>();

        try
        {
            await using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            return await JsonSerializer.DeserializeAsync<List<T>>(stream, Options, ct)
                   ?? throw new DataFileCorruptedException(fileName);
        }
        catch (JsonException ex)
        {
            throw new DataFileCorruptedException(fileName, ex);
        }
    }

    private static List<T> ReadCore<T>(string fileName)
    {
        string path = DataFiles.PathOf(fileName);
        if (!File.Exists(path))
            return new List<T>();

        try
        {
            using var stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return JsonSerializer.Deserialize<List<T>>(stream, Options)
                   ?? throw new DataFileCorruptedException(fileName);
        }
        catch (JsonException ex)
        {
            throw new DataFileCorruptedException(fileName, ex);
        }
    }

    private static async Task WriteCoreAsync<T>(string fileName, List<T> items, CancellationToken ct)
    {
        string path = DataFiles.PathOf(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp";

        try
        {
            await using (var stream = new FileStream(
                temp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, items, Options, ct);
                await stream.FlushAsync(ct);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temp, path, overwrite: true);
        }
        catch
        {
            TryDelete(temp);
            throw;
        }
    }

    private static void WriteCore<T>(string fileName, List<T> items)
    {
        string path = DataFiles.PathOf(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp";

        try
        {
            using (var stream = new FileStream(
                temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, items, Options);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temp, path, overwrite: true);
        }
        catch
        {
            TryDelete(temp);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
          catch {}
    }
}