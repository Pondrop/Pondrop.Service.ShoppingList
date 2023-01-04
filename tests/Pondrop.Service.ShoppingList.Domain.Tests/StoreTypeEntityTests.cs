using Pondrop.Service.ShoppingList.Domain.Events.StoreType;
using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.ShoppingList.Domain.Tests;

public class StoreTypeEntityTests
{
    private const string Name = "My StoreType";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    [Fact]
    public void StoreType_Ctor_ShouldCreateEmpty()
    {
        // arrange
        
        // act
        var entity = new StoreTypeEntity();
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }
    
    [Fact]
    public void StoreType_Ctor_ShouldCreateEvent()
    {
        // arrange
        
        // act
        var entity = GetNewStoreType();
        
        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void StoreType_UpdateStoreType_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateStoreType("New Name");
        var entity = GetNewStoreType();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    private StoreTypeEntity GetNewStoreType() => new StoreTypeEntity(Name, ExternalReferenceId, CreatedBy);
}