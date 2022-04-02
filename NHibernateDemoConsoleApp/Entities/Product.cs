namespace Examples.FirstProject.Entities
{
    public class Product
    {
        public virtual int Id { get; protected set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual double Price { get; set; }
        public virtual Location? Location { get; set; }
        public virtual IList<Store> StoresStockedIn { get; set; }
        public virtual DateTime ExpiryDate { get; set; }

        public Product()
        {
            StoresStockedIn = new List<Store>();
        }
    }

    public class Location
    {
        public virtual int Aisle { get; set; }
        public virtual int Shelf { get; set; }
    }
}