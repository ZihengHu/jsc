using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JscServer.Models
{
    public class JscDbContext : DbContext
    {
        public JscDbContext(DbContextOptions<JscDbContext> options) : base(options)
        { }

        public DbSet<Injection> Injections { get; set; }
        public DbSet<Coverage> Coverages { get; set; }
    }

    
}
