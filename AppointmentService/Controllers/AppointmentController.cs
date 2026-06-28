using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using AppointmentService.Data;
using AppointmentService.Models;
using Shared.Contracts;

namespace AppointmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPublishEndpoint _publish;

        public AppointmentController(AppDbContext db, IPublishEndpoint publish)
        {
            _db = db;
            _publish = publish;
        }

        [HttpGet]
        public async Task<IEnumerable<Appointment>> Get() => await _db.Appointments.ToListAsync();

        [HttpGet("doctor/{doctorId:int}")]
        public async Task<IEnumerable<Appointment>> GetByDoctor(int doctorId)
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddMonths(3);
            return await _db.Appointments
                .Where(a => a.DoctorId == doctorId && a.Time >= from && a.Time < to)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment appointment)
        {
            appointment.Status = "Pending";
            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            await _publish.Publish(new AppointmentCreatedMessage
            {
                AppointmentId = appointment.Id,
                UserId = appointment.UserId,
                Time = appointment.Time
            });

            return Ok(appointment);
        }

        [HttpPatch("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();
            appointment.Status = "Confirmed";
            await _db.SaveChangesAsync();
            return Ok(appointment);
        }

        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();
            appointment.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return Ok(appointment);
        }
    }
}
