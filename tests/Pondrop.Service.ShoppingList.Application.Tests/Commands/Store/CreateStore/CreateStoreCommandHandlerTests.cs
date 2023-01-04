using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pondrop.Service.Events;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.ShoppingList.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.ShoppingList.Application.Tests.Commands.ShoppingList.CreateStore;

public class CreateStoreCommandHandlerTests
{
    private readonly Mock<IOptions<StoreUpdateConfiguration>> _StoreUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICheckpointRepository<RetailerEntity>> _retailerViewRepositoryMock;
    private readonly Mock<ICheckpointRepository<StoreTypeEntity>> _storeTypeViewRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateStoreCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateStoreCommandHandler>> _loggerMock;
    
    public CreateStoreCommandHandlerTests()
    {
        _StoreUpdateConfigMock = new Mock<IOptions<StoreUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _retailerViewRepositoryMock = new Mock<ICheckpointRepository<RetailerEntity>>();
        _storeTypeViewRepositoryMock = new Mock<ICheckpointRepository<StoreTypeEntity>>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateStoreCommand>>();
        _loggerMock = new Mock<ILogger<CreateStoreCommandHandler>>();

        _StoreUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new StoreUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }
    
    [Fact]
    public async void CreateStoreCommand_ShouldSucceed()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.StoreTypeId))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Once);
    }
    
    [Fact]
    public async void CreateStoreCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateStoreCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.StoreTypeId))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateStoreCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.StoreTypeId))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Never);
    }
    
    [Fact]
    public async void CreateStoreCommand_WhenRetailerNotFound_ShouldFail()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(null));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.StoreTypeId))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _retailerViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.RetailerId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void CreateStoreCommand_WhenStoreTypeNotFound_ShouldFail()
    {
        // arrange
        var cmd = StoreFaker.GetCreateStoreCommand();
        var item = StoreFaker.GetStoreRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _retailerViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.RetailerId))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _storeTypeViewRepositoryMock
            .Setup(x => x.GetByIdAsync(cmd.StoreTypeId))
            .Returns(Task.FromResult<StoreTypeEntity?>(null));
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()))
            .Returns(item);
        var handler = GetCommandHandler();
        
        // act
        var result = await handler.Handle(cmd, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(cmd),
            Times.Once());
        _storeTypeViewRepositoryMock.Verify(
            x =>x.GetByIdAsync(cmd.StoreTypeId),
            Times.Once());
        _eventRepositoryMock.Verify(
            x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<ShoppingListEntity>()),
            Times.Never());
    }
    
    private CreateStoreCommandHandler GetCommandHandler() =>
        new CreateStoreCommandHandler(
            _StoreUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _retailerViewRepositoryMock.Object,
            _storeTypeViewRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}