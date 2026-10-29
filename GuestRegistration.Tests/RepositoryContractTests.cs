using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public abstract class RepositoryContractTests<TDto> : IDisposable where TDto : EntityDto
{
    protected readonly TestDatabase _database = new();
    protected abstract IRepository<TDto> Repository { get; }
    protected abstract TDto NewRecord();
    protected abstract void ChangeRecord(TDto dto);
    protected abstract string ReadMarker(TDto dto);
    protected abstract string DirectUpdateSql { get; }

    [Fact]
    public void CreateAndRead_MapEveryEditableDtoField()
    {
        var dto = NewRecord();
        var saved = Repository.Create(dto);
        var read = Repository.GetById(saved.Id)!;
        foreach (var property in typeof(TDto).GetProperties())
        {
            if (property.Name is "Id" or "RowInsertTime" or "UpdateTime") continue;
            Assert.Equal(property.GetValue(dto), property.GetValue(saved));
            Assert.Equal(property.GetValue(dto), property.GetValue(read));
        }
    }

    [Fact]
    public void Create_GeneratesAuditTimesOnTheServer()
    {
        var dto = NewRecord();
        dto.RowInsertTime = DateTime.UnixEpoch;
        dto.UpdateTime = DateTime.UnixEpoch;
        DateTime earliest = DateTime.UtcNow.AddMinutes(-1);
        var saved = Repository.Create(dto);
        Assert.InRange(saved.RowInsertTime, earliest, DateTime.UtcNow.AddMinutes(1));
        Assert.InRange(saved.UpdateTime, saved.RowInsertTime, DateTime.UtcNow.AddMinutes(1));
        var read = Repository.GetById(saved.Id)!;
        Assert.Equal(saved.RowInsertTime, read.RowInsertTime);
        Assert.Equal(saved.UpdateTime, read.UpdateTime);
    }

    [Fact]
    public void Update_PreservesInsertTimeAndAdvancesUpdateTime()
    {
        var saved = Repository.Create(NewRecord());
        DateTime inserted = saved.RowInsertTime;
        DateTime updated = saved.UpdateTime;
        ChangeRecord(saved);
        saved.RowInsertTime = DateTime.UnixEpoch;
        saved.UpdateTime = DateTime.UnixEpoch;
        Assert.True(Repository.Update(saved));
        var read = Repository.GetById(saved.Id)!;
        Assert.Equal(inserted, read.RowInsertTime);
        Assert.True(read.UpdateTime > updated);
        Assert.Equal(ReadMarker(saved), ReadMarker(read));
    }

    [Fact]
    public void DirectSqlUpdate_AlsoMaintainsAuditTimes()
    {
        var saved = Repository.Create(NewRecord());
        _database.Context.Database.ExecuteSqlRaw(DirectUpdateSql, saved.Id);
        var read = Repository.GetById(saved.Id)!;
        Assert.Equal(saved.RowInsertTime, read.RowInsertTime);
        Assert.True(read.UpdateTime > saved.UpdateTime);
        Assert.NotEqual(ReadMarker(saved), ReadMarker(read));
    }

    [Fact]
    public void ReturnedDto_IsNotTrackedAsAnEntity()
    {
        var saved = Repository.Create(NewRecord());
        string marker = ReadMarker(saved);
        ChangeRecord(saved);
        _database.Context.SaveChanges();
        Assert.Equal(marker, ReadMarker(Repository.GetById(saved.Id)!));
    }

    public void Dispose() => _database.Dispose();
}
