using Clinic.Application.Common;
using Clinic.Application.Features.Appointments.Commands.CreateAppointment;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Clinic.Application.Tests;

public class CreateAppointmentCommandHandlerTests
{
    private readonly TestApplicationDbContext _context;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly CreateAppointmentCommandHandler _handler;

    public CreateAppointmentCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // DB جديدة منعزلة لكل Test
            .Options;

        _context = new TestApplicationDbContext(options);
        _identityServiceMock = new Mock<IIdentityService>();
        _publisherMock = new Mock<IPublisher>();

        _handler = new CreateAppointmentCommandHandler(_context, _identityServiceMock.Object, _publisherMock.Object);

        SeedBasicData();
    }

    private void SeedBasicData()
    {
        _context.Doctors.Add(new Doctor { Id = 1, FullName = "Dr. Test", ApplicationUserId = "doc-1", SpecializationId = 1, ConsultationFee = 100 });
        _context.Patients.Add(new Patient { Id = 1, FullName = "Patient Test", ApplicationUserId = "pat-1", DateOfBirth = new DateTime(1990, 1, 1) });
        _context.DoctorWorkingHours.Add(new DoctorWorkingHour
        {
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(14, 0, 0),
            SlotDurationMinutes = 30
        });
        _context.SaveChanges();

        _identityServiceMock
            .Setup(s => s.GetUserEmailAsync("pat-1"))
            .ReturnsAsync("patient@test.com");
    }

    private static DateTime NextSaturdayAt(int hour, int minute = 0)
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        daysUntilSaturday = daysUntilSaturday == 0 ? 7 : daysUntilSaturday; // نضمن إنه يوم جاي مش النهاردة
        return today.AddDays(daysUntilSaturday).AddHours(hour).AddMinutes(minute);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesAppointmentSuccessfully()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            DoctorId = 1,
            PatientId = 1,
            AppointmentDateTime = NextSaturdayAt(10)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        var savedAppointment = await _context.Appointments.FindAsync(result.Value);
        savedAppointment.Should().NotBeNull();
        savedAppointment!.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task Handle_DoctorNotFound_ReturnsFailure()
    {
        var command = new CreateAppointmentCommand
        {
            DoctorId = 999, // مش موجود
            PatientId = 1,
            AppointmentDateTime = NextSaturdayAt(10)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Doctor not found.");
    }

    [Fact]
    public async Task Handle_PatientNotFound_ReturnsFailure()
    {
        var command = new CreateAppointmentCommand
        {
            DoctorId = 1,
            PatientId = 999,
            AppointmentDateTime = NextSaturdayAt(10)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Patient not found.");
    }

    [Fact]
    public async Task Handle_OutsideWorkingHours_ReturnsFailure()
    {
        var command = new CreateAppointmentCommand
        {
            DoctorId = 1,
            PatientId = 1,
            AppointmentDateTime = NextSaturdayAt(8) // برة 10-2
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("This time is outside the doctor's working hours.");
    }

    [Fact]
    public async Task Handle_InvalidSlotAlignment_ReturnsFailure()
    {
        var command = new CreateAppointmentCommand
        {
            DoctorId = 1,
            PatientId = 1,
            AppointmentDateTime = NextSaturdayAt(10, 15) // 10:15 مش Slot صحيح (كل Slot 30 دقيقة)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid appointment time slot.");
    }

    [Fact]
    public async Task Handle_SlotAlreadyBooked_ReturnsFailure()
    {
        // Arrange - احجز أول مرة
        var dateTime = NextSaturdayAt(11);
        var firstCommand = new CreateAppointmentCommand { DoctorId = 1, PatientId = 1, AppointmentDateTime = dateTime };
        await _handler.Handle(firstCommand, CancellationToken.None);

        // Act - حاول تحجز نفس المعاد تاني
        var secondCommand = new CreateAppointmentCommand { DoctorId = 1, PatientId = 1, AppointmentDateTime = dateTime };
        var result = await _handler.Handle(secondCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("This slot is already booked.");
    }

    [Fact]
    public async Task Handle_SuccessfulBooking_PublishesAppointmentCreatedEvent()
    {
        var command = new CreateAppointmentCommand
        {
            DoctorId = 1,
            PatientId = 1,
            AppointmentDateTime = NextSaturdayAt(12)
        };

        await _handler.Handle(command, CancellationToken.None);

        _publisherMock.Verify(p => p.Publish(
            It.IsAny<Clinic.Application.Features.Appointments.Events.AppointmentCreatedEvent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}