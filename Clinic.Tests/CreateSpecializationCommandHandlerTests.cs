using Clinic.Application.Features.Specializations.Commands.CreateSpecialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinic.Application.Tests;

public class CreateSpecializationCommandHandlerTests
{
    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_NewSpecialization_CreatesSuccessfully()
    {
        var context = CreateContext();
        var handler = new CreateSpecializationCommandHandler(context);

        var result = await handler.Handle(new CreateSpecializationCommand("Neurology"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await context.Specializations.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_DuplicateSpecialization_ReturnsFailure()
    {
        var context = CreateContext();
        var handler = new CreateSpecializationCommandHandler(context);

        await handler.Handle(new CreateSpecializationCommand("Cardiology"), CancellationToken.None);
        var result = await handler.Handle(new CreateSpecializationCommand("Cardiology"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Specialization already exists.");
    }
}