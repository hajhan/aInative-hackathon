using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SideReport.Application.Medications.Commands;
using SideReport.Domain.Exceptions;
using SideReport.Infrastructure.Persistence;
using SideReport.Infrastructure.Services;
using Xunit;

namespace SideReport.Tests.Medications;

public class MedicationServiceTests : IAsyncDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly MedicationService _sut;
    private const string TestUserId = "user-test-001";

    public MedicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _sut = new MedicationService(_db);
    }

    [Fact]
    public async Task CreateAsync_ValidCommand_ReturnsMedication()
    {
        var command = new CreateMedicationCommand("타이레놀", "500mg", "1일 3회", DateOnly.Parse("2026-01-01"), null);

        var result = await _sut.CreateAsync(command, TestUserId);

        result.DrugName.Should().Be("타이레놀");
        result.Dosage.Should().Be("500mg");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyUserMedications()
    {
        var cmd = new CreateMedicationCommand("아스피린", null, null, DateOnly.Parse("2026-01-01"), null);
        await _sut.CreateAsync(cmd, TestUserId);
        await _sut.CreateAsync(cmd, "other-user");

        var result = await _sut.GetByUserAsync(TestUserId);

        result.Should().HaveCount(1);
        result[0].DrugName.Should().Be("아스피린");
    }

    [Fact]
    public async Task UpdateAsync_ValidId_UpdatesMedication()
    {
        var created = await _sut.CreateAsync(
            new CreateMedicationCommand("이부프로펜", "200mg", null, DateOnly.Parse("2026-01-01"), null),
            TestUserId);

        var updated = await _sut.UpdateAsync(
            created.Id,
            new UpdateMedicationCommand("이부프로펜", "400mg", "1일 2회", DateOnly.Parse("2026-01-01"), null),
            TestUserId);

        updated.Dosage.Should().Be("400mg");
        updated.Frequency.Should().Be("1일 2회");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_RemovesMedication()
    {
        var created = await _sut.CreateAsync(
            new CreateMedicationCommand("삭제테스트약", null, null, DateOnly.Parse("2026-01-01"), null),
            TestUserId);

        await _sut.DeleteAsync(created.Id, TestUserId);

        var list = await _sut.GetByUserAsync(TestUserId);
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WrongUser_ThrowsNotFoundException()
    {
        var created = await _sut.CreateAsync(
            new CreateMedicationCommand("타약", null, null, DateOnly.Parse("2026-01-01"), null),
            TestUserId);

        var act = () => _sut.DeleteAsync(created.Id, "other-user");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public async ValueTask DisposeAsync() => await _db.DisposeAsync();
}
