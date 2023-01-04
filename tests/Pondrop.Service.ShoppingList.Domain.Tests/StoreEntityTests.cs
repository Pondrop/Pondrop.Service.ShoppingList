
using Pondrop.Service.ShoppingList.Domain.Events.Store;
using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.ShoppingList.Domain.Tests;

public class StoreEntityTests
{
    private const string Name = "My Store";
    private const string Status = "Online";
    private const string Email = "test@test.com";
    private const string Phone = "121323442";
    private const string OpenHours = "10am-6pm";
    private const string ExternalReferenceId = "dc9145d2-b108-482e-ba6e-a141e2fba16f";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    private const string RetailerName = "My Retailer";
    private const string StoreTypeName = "My StoreType";

    [Fact]
    public void Store_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new ShoppingListEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Store_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewStore();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(Email, entity.Email);
        Assert.Equal(Phone, entity.Phone);
        Assert.Equal(OpenHours, entity.OpenHours);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.NotEqual(Guid.Empty, entity.RetailerId);
        Assert.NotEqual(Guid.Empty, entity.StoreTypeId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void Store_UpdateStore_ShouldUpdate()
    {
        // arrange
        var updateEvent = new UpdateStore("New Name", null, null, null, null, null, null);
        var entity = GetNewStore();

        // act
        entity.Apply(updateEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.Name, updateEvent.Name);
        Assert.Equal(Status, entity.Status);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }

    [Fact]
    public void Store_AddStoreAddress_ShouldAddAddress()
    {
        // arrange
        var entity = GetNewStore();
        var addEvent = GetAddStoreAddress(entity.Id);

        // act
        entity.Apply(addEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(addEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }

    [Fact]
    public void Store_UpdateStoreAddress_ShouldUpdateAddress()
    {
        // arrange
        var entity = GetNewStore();
        var addEvent = GetAddStoreAddress(entity.Id);
        var updateEvent = new UpdateStoreAddress(
            addEvent.Id,
            entity.Id,
            addEvent.AddressLine1 + " Updated",
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        entity.Apply(addEvent, UpdatedBy);

        // act
        entity.Apply(updateEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(addEvent.Id, entity.Addresses.Single().Id);
        Assert.Equal(updateEvent.AddressLine1, entity.Addresses.Single().AddressLine1);
        Assert.Equal(addEvent.AddressLine2, entity.Addresses.Single().AddressLine2);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }

    [Fact]
    public void Store_RemoveAddressFromStore_ShouldRemoveAddress()
    {
        // arrange
        var entity = GetNewStore();
        var addEvent = GetAddStoreAddress(entity.Id);
        var removeEvent = new RemoveAddressFromStore(addEvent.Id, entity.Id);
        entity.Apply(addEvent, UpdatedBy);

        // act
        entity.Apply(removeEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Empty(entity.Addresses);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(3, entity.EventsCount);
    }

    private ShoppingListEntity GetNewStore() => new StoreEntity(
        Name,
        Status,
        ExternalReferenceId,
        Phone,
        Email,
        OpenHours,
        GetRetailerRecord().Id,
        GetStoreTypeRecord().Id,
        CreatedBy);
    private AddStoreAddress GetAddStoreAddress(Guid storeId) => new AddStoreAddress(
        Guid.NewGuid(),
        storeId,
        Guid.NewGuid().ToString(),
        nameof(AddStoreAddress.AddressLine1),
        nameof(AddStoreAddress.AddressLine2),
        nameof(AddStoreAddress.Suburb),
        nameof(AddStoreAddress.State),
        nameof(AddStoreAddress.Postcode),
        nameof(AddStoreAddress.Country),
        0,
        0);
    private RetailerRecord GetRetailerRecord() => new RetailerRecord(
        Guid.NewGuid(),
        Guid.NewGuid().ToString(),
        RetailerName,
        CreatedBy,
        UpdatedBy,
        DateTime.UtcNow.AddDays(-1),
        DateTime.UtcNow);
    private StoreTypeRecord GetStoreTypeRecord() => new StoreTypeRecord(
        Guid.NewGuid(),
        Guid.NewGuid().ToString(),
        StoreTypeName,
        CreatedBy,
        UpdatedBy,
        DateTime.UtcNow.AddDays(-1),
        DateTime.UtcNow);
}