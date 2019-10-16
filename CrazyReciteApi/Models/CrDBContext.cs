using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CrazyReciteApi.Models
{
    public class CrDBContext : DbContext
    {
        public DbSet<Books> BooksList { get; set; }
    }
}