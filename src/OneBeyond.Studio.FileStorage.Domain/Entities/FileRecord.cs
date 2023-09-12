using EnsureThat;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace OneBeyond.Studio.FileStorage.Domain.Entities;

public class FileRecord
{
    public FileRecord(string name, long size, string contentType)
        : this(new ID(Guid.NewGuid()), name, size, contentType)
    {
    }

    //We need this constructor in order to be able to create a file record with the Id explicitly specified
    public FileRecord(ID id, string name, long size, string contentType)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        EnsureArg.IsGte(size, 0, nameof(size));
        EnsureArg.IsNotNullOrWhiteSpace(contentType, nameof(contentType));

        Id = id;
        Name = name;
        Size = size;
        ContentType = contentType;
    }

    //This private constructor is needed for EF Core to construct an entity from database table row
#nullable disable
    private FileRecord()
    {
    }
#nullable restore

    public ID Id { get; }

    public string Name { get; private set; }

    public long Size { get; private set; }

    public string ContentType { get; private set; }

    public void UpdateContentInfo(long contentSize, string contentType)
    {
        EnsureArg.IsGte(contentSize, 0, nameof(contentSize));
        EnsureArg.IsNotNullOrWhiteSpace(contentType, nameof(contentType));

        Size = contentSize;
        ContentType = contentType;
    }

    /// <summary>
    /// Create a new copy of the file record (new Id will be generated)
    /// </summary>
    /// <param name="fileName">New file name</param>
    /// <returns></returns>
    public FileRecord Copy(string? fileName = null)
        => new FileRecord(
            string.IsNullOrWhiteSpace(fileName) ? Name : fileName,
            Size,
            ContentType);

    public readonly struct ID : IEquatable<ID>
    {
        public ID(Guid key)
        {
            EnsureArg.IsNotDefault(key, nameof(key));

            Key = key;
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public ID Id => this;

        public Guid Key { get; }

        public override bool Equals(object? obj)
            => this == obj as ID?;

        public static bool operator !=(ID first, ID second)
            => !(first == second);

        public static bool operator ==(ID first, ID second)
            => first.Equals(second);

        public bool Equals(ID other)
            => Key.Equals(other.Key);

        public override int GetHashCode()
            => Key.GetHashCode();

        public override string ToString()
            => Key.ToString();
    }
}
