using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.ShoppingList.Application.Commands;

using Pondrop.Service.ShoppingList.Application.Queries;
using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.ShoppingList.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Pondrop.Service.Interfaces;

namespace Pondrop.Service.ShoppingList.Application.Tests.Commands.ShoppingList.CreateStore;

public class GetStoreByIdHandlerTests
{
    private readonly Mock<IContainerRepository<StoreViewRecord>> _storeContainerRepositoryMock;
    private readonly Mock<IValidator<GetStoreByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetShoppingListByIdQueryHandler>> _loggerMock;
    
    public GetStoreByIdHandlerTests()
    {
        _storeContainerRepositoryMock = new Mock<IContainerRepository<StoreViewRecord>>();
        _validatorMock = new Mock<IValidator<GetStoreByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetShoppingListByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetStoreByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreViewRecord?>(new StoreViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreViewRecord?>(new StoreViewRecord()));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreViewRecord?>(null));
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreFaker.GetStoreRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeContainerRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeContainerRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
    }
    
    private GetShoppingListByIdQueryHandler GetQueryHandler() =>
        new GetStoreByIdQueryHandler(
            _storeContainerRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}