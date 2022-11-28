using System.Text.Json;

namespace Mikrotik_DNS_Lookup.Models
{
    public class DNSRecords
    {
        public List<DNS> DNSRecordList;

        public DNSRecords()
        {
            DNSRecordList = new List<DNS>();
        }
    }

    public class DNS
    {
        public DNS(string name, string address, string comment)
        {
            Name = name;
            Address = address;
            Comment = comment;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<DNS>(this);
        }
    }

}
