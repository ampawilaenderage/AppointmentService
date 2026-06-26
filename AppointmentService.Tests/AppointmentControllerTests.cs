using AppointmentService.Controllers;
using AppointmentService.Models;
using AppointmentService.Tests.Helpers;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AppointmentService.Tests;

public class AppointmentControllerTests
{
    private static AppointmentController CreateController(string dbName)
    {
        var db = TestDbContext.Create(dbName);
        var publish = Substitute.For<IPublishEndpoint>();
        return new AppointmentController(db, publish);
    }

    private static Appointment SampleAppointment(int userId = 1, int doctorId = 10,
        DateTime? time = null) => new()
    {
        UserId = userId,
        DoctorId = doctorId,
        PatientName = "John Smith",
        ProblemType = "Heart Problems",
        Time = time ?? DateTime.UtcNow.AddDays(1),
        Status = "ShouldBeOverridden"
    };

    // ── Create ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidAppointment_Returns200()
    {
        var controller = CreateController(nameof(Create_ValidAppointment_Returns200));

        var result = await controller.Create(SampleAppointment());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_AlwaysSetsPendingStatus_IgnoresFrontendStatus()
    {
        var controller = CreateController(nameof(Create_AlwaysSetsPendingStatus_IgnoresFrontendStatus));
        var appt = SampleAppointment();
        appt.Status = "Confirmed"; // frontend tries to send a different status

        var result = await controller.Create(appt);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var created = ok.Value.Should().BeOfType<Appointment>().Subject;
        created.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Create_AssignsIdAfterSave()
    {
        var controller = CreateController(nameof(Create_AssignsIdAfterSave));

        var result = await controller.Create(SampleAppointment());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var created = ok.Value.Should().BeOfType<Appointment>().Subject;
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_StoresPatientNameAndProblemType()
    {
        var controller = CreateController(nameof(Create_StoresPatientNameAndProblemType));
        var appt = SampleAppointment();
        appt.PatientName = "Alice Wong";
        appt.ProblemType = "Skin Problems";

        var result = await controller.Create(appt);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var created = ok.Value.Should().BeOfType<Appointment>().Subject;
        created.PatientName.Should().Be("Alice Wong");
        created.ProblemType.Should().Be("Skin Problems");
    }

    // ── Approve ───────────────────────────────────────────────────

    [Fact]
    public async Task Approve_ExistingPendingAppointment_SetsConfirmed()
    {
        var controller = CreateController(nameof(Approve_ExistingPendingAppointment_SetsConfirmed));
        var created = (OkObjectResult)await controller.Create(SampleAppointment());
        var appt = (Appointment)created.Value!;

        var result = await controller.Approve(appt.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var updated = ok.Value.Should().BeOfType<Appointment>().Subject;
        updated.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Approve_NonExistingId_Returns404()
    {
        var controller = CreateController(nameof(Approve_NonExistingId_Returns404));

        var result = await controller.Approve(9999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Cancel ────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_ExistingAppointment_SetsCancelled()
    {
        var controller = CreateController(nameof(Cancel_ExistingAppointment_SetsCancelled));
        var created = (OkObjectResult)await controller.Create(SampleAppointment());
        var appt = (Appointment)created.Value!;

        var result = await controller.Cancel(appt.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var updated = ok.Value.Should().BeOfType<Appointment>().Subject;
        updated.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Cancel_NonExistingId_Returns404()
    {
        var controller = CreateController(nameof(Cancel_NonExistingId_Returns404));

        var result = await controller.Cancel(9999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Cancel_AfterApprove_CanStillCancel()
    {
        var controller = CreateController(nameof(Cancel_AfterApprove_CanStillCancel));
        var created = (OkObjectResult)await controller.Create(SampleAppointment());
        var appt = (Appointment)created.Value!;
        await controller.Approve(appt.Id);

        var result = await controller.Cancel(appt.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((Appointment)ok.Value!).Status.Should().Be("Cancelled");
    }

    // ── GetByDoctor ───────────────────────────────────────────────

    [Fact]
    public async Task GetByDoctor_ReturnsOnlyThatDoctorsAppointments()
    {
        var controller = CreateController(nameof(GetByDoctor_ReturnsOnlyThatDoctorsAppointments));
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddDays(1)));
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddDays(2)));
        await controller.Create(SampleAppointment(doctorId: 99, time: DateTime.UtcNow.AddDays(1)));

        var results = (await controller.GetByDoctor(10)).ToList();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(a => a.DoctorId == 10);
    }

    [Fact]
    public async Task GetByDoctor_ExcludesPastAppointments()
    {
        var controller = CreateController(nameof(GetByDoctor_ExcludesPastAppointments));
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddDays(-1)));  // past
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddDays(1)));   // future

        var results = (await controller.GetByDoctor(10)).ToList();

        results.Should().HaveCount(1);
        results.First().Time.Should().BeAfter(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task GetByDoctor_ExcludesAppointmentsBeyond3Months()
    {
        var controller = CreateController(nameof(GetByDoctor_ExcludesAppointmentsBeyond3Months));
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddMonths(4))); // too far
        await controller.Create(SampleAppointment(doctorId: 10, time: DateTime.UtcNow.AddDays(7)));   // within range

        var results = (await controller.GetByDoctor(10)).ToList();

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByDoctor_NoAppointments_ReturnsEmpty()
    {
        var controller = CreateController(nameof(GetByDoctor_NoAppointments_ReturnsEmpty));

        var results = await controller.GetByDoctor(42);

        results.Should().BeEmpty();
    }

    // ── Get (all) ─────────────────────────────────────────────────

    [Fact]
    public async Task Get_ReturnsAllAppointments()
    {
        var controller = CreateController(nameof(Get_ReturnsAllAppointments));
        await controller.Create(SampleAppointment(userId: 1));
        await controller.Create(SampleAppointment(userId: 2));
        await controller.Create(SampleAppointment(userId: 3));

        var results = (await controller.Get()).ToList();

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task Get_EmptyDatabase_ReturnsEmptyList()
    {
        var controller = CreateController(nameof(Get_EmptyDatabase_ReturnsEmptyList));

        var results = await controller.Get();

        results.Should().BeEmpty();
    }

    // ── Status transitions ─────────────────────────────────────────

    [Fact]
    public async Task StatusFlow_Pending_Confirmed_Cancelled()
    {
        var controller = CreateController(nameof(StatusFlow_Pending_Confirmed_Cancelled));
        var created = (OkObjectResult)await controller.Create(SampleAppointment());
        var appt = (Appointment)created.Value!;

        appt.Status.Should().Be("Pending");

        var approved = (OkObjectResult)await controller.Approve(appt.Id);
        ((Appointment)approved.Value!).Status.Should().Be("Confirmed");

        var cancelled = (OkObjectResult)await controller.Cancel(appt.Id);
        ((Appointment)cancelled.Value!).Status.Should().Be("Cancelled");
    }
}
