using ZeroFormatter;

namespace ConsoleApp.Models
{
    [ZeroFormattable]
    public class LargeDataStructure
    {
        public string FirstName;

        public string LastName;

        public int Age;

        public string Country;

        public string City;

        public string Description;
    }
}