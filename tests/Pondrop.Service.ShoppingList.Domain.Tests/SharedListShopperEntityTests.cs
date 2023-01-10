using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;
using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.ShoppingList.Domain.Tests;

public class SharedListShopperEntityTests
{
    private Guid UserId = Guid.NewGuid();
    private const ListPrivilegeType ListPrivilege = ListPrivilegeType.admin;
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    [Fact]
    public void SharedListShopper_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new SharedListShopperEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void SharedListShopper_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewSharedListShopper();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(UserId, entity.UserId);
        Assert.Equal(ListPrivilege, entity.ListPrivilege);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    [Fact]
    public void SharedListShopper_UpdateSharedListShopper_ShouldUpdate()
    {
        // arrange
        var newShopperId = Guid.NewGuid();
        var newListPrivilege = ListPrivilegeType.view;

        var entity = GetNewSharedListShopper();

        var updateEvent = new UpdateSharedListShopper(entity.Id, newShopperId, newListPrivilege, 0);

        // act
        entity.Apply(updateEvent, UpdatedBy);

        // assert
        Assert.NotNull(entity);
        Assert.Equal(updateEvent.UserId, newShopperId);
        Assert.Equal(updateEvent.ListPrivilege, newListPrivilege);
        Assert.Equal(UpdatedBy, entity.UpdatedBy);
        Assert.Equal(2, entity.EventsCount);
    }

    private SharedListShopperEntity GetNewSharedListShopper() => new SharedListShopperEntity(UserId, ListPrivilege, 0, CreatedBy);
}