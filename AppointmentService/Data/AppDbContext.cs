using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AppointmentService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
              : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
    }
}
