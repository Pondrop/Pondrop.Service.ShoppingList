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

namespace Pondrop.Service.ShoppingList.Application.Tests.Commands.StoreType.CreateStoreType;

public class GetStoreTypeByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<StoreTypeEntity>> _storeTypeCheckpointRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetStoreTypeByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetStoreTypeByIdQueryHandler>> _loggerMock;
    
    public GetStoreTypeByIdHandlerTests()
    {
        _storeTypeCheckpointRepositoryMock = new Mock<ICheckpointRepository<StoreTypeEntity>>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetStoreTypeByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetStoreTypeByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetStoreTypeByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetStoreTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreTypeFaker.GetStoreTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()),
            Times.Once());
    }
    
    [Fact]
    public async void GetStoreTypeByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetStoreTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreTypeFaker.GetStoreTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreTypeEntity?>(new StoreTypeEntity()));
        _mapperMock
            .Setup(x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetStoreTypeByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetStoreTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreTypeFaker.GetStoreTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreTypeEntity?>(null));
        _mapperMock
            .Setup(x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetStoreTypeByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetStoreTypeByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreTypeFaker.GetStoreTypeRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _storeTypeCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _storeTypeCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreTypeRecord>(It.IsAny<StoreTypeEntity>()),
            Times.Never());
    }
    
    private GetStoreTypeByIdQueryHandler GetQueryHandler() =>
        new GetStoreTypeByIdQueryHandler(
            _storeTypeCheckpointRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}