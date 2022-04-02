namespace Examples.FirstProject.Entities
{
    public class Employee
    {
        public virtual int Id { get; protected set; }
        public virtual string FirstName { get; set; } = string.Empty;
        public virtual string LastName { get; set; } = string.Empty;
        public virtual Store? Store { get; set; }
    }
}