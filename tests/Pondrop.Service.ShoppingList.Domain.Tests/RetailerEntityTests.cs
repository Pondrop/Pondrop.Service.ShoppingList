using Pondrop.Service.ShoppingList.Domain.Events.Retailer;
using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.ShoppingList.Domain.Tests;

public class RetailerEntityTests
{
    private const string Name = "My Retailer";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";
    
    [Fact]
    public void Retailer_Ctor_ShouldCreateEmpty()
    {
        // arrange
        
        // act
        var entity = new RetailerEntity();
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }
    
    [Fact]
    public void Retailer_Ctor_ShouldCreateEvent()
    {
        // arrange
        
        // act
        var entity = GetNewRetailer();
        
        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }
    
    [Fact]
    public void Retailer_UpdateRetailer_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateRetailer("New Name");
        var entity = GetNewRetailer();
        
        // act
        entity.Apply(updateEvent, UpdatedBy);
        
        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }
    
    private RetailerEntity GetNewRetailer() => new RetailerEntity(Name, ExternalReferenceId, CreatedBy);
}