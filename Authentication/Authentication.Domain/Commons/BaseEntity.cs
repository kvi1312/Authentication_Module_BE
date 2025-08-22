namespace Authentication.Domain.Commons;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}