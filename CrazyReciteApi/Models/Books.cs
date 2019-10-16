using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrazyReciteApi.Models
{
    public enum BookTypes
    {
        Default=0,
        Privates=1,
    }

    public class Books
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Remarks { get; set; }
        public BookTypes type { get; set; }
        public bool IsEnable { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}