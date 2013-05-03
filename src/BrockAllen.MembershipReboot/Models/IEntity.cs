namespace BrockAllen.MembershipReboot.Models
{
    public interface IEntity : IEntity<int>
    {
    }

    public interface IEntity<T>
    {
        T ID { get; set; }
    }
}
