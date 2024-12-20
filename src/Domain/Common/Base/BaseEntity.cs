
namespace Domain.Common.Base;



public interface IAggregateRoot { }
public interface IEntity { }

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; }
}

public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    public virtual TKey Id { get; protected set; }
    public bool IsActive { get; private set; }

    public DateTime CreationDate { get; private set; }

    protected BaseEntity()
    {
        CreationDate = DateTime.Now;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}