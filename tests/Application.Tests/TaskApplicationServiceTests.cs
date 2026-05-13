using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Application.Tasks;
using CursorAgenticWebApi.Domain.Entities;
using CursorAgenticWebApi.Domain.Enums;
using Moq;
using Xunit;

namespace CursorAgenticWebApi.Application.Tests;

public sealed class TaskApplicationServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new();

    private TaskApplicationService CreateSut() => new(_tasks.Object);

    [Fact]
    public async Task CreateAsync_returns_validation_for_empty_title()
    {
        var sut = CreateSut();
        var result = await sut.CreateAsync(Guid.NewGuid(), new CreateTaskRequest("  ", "d", TaskStatus.Pending, null)).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<TaskDto>.Failure>(result);
        Assert.Equal("validation", failure.Code);
    }

    [Fact]
    public async Task CreateAsync_inserts_task_when_valid()
    {
        var userId = Guid.NewGuid();
        WorkTask? inserted = null;
        _tasks.Setup(x => x.InsertAsync(It.IsAny<WorkTask>(), It.IsAny<CancellationToken>()))
            .Callback<WorkTask, CancellationToken>((t, _) => inserted = t)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var result = await sut.CreateAsync(userId, new CreateTaskRequest("Title", "Desc", TaskStatus.InProgress, null)).ConfigureAwait(false);

        var success = Assert.IsType<ServiceResult<TaskDto>.Success>(result);
        Assert.Equal("Title", success.Value.Title);
        Assert.Equal(TaskStatus.InProgress, success.Value.Status);
        Assert.NotNull(inserted);
        Assert.Equal(userId, inserted!.UserId);
    }

    [Fact]
    public async Task GetAsync_returns_not_found_for_other_user()
    {
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "T",
            Description = "D",
            Status = TaskStatus.Pending,
            DueDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        _tasks.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var sut = CreateSut();
        var result = await sut.GetAsync(Guid.NewGuid(), task.Id).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<TaskDto>.Failure>(result);
        Assert.Equal("not_found", failure.Code);
    }

    [Fact]
    public async Task DeleteAsync_returns_not_found_when_missing()
    {
        _tasks.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<bool>.Failure>(result);
        Assert.Equal("not_found", failure.Code);
    }
}
