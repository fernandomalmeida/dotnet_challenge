using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ChatServer.Models;

namespace ChatServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
