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

namespace Pondrop.Service.ShoppingList.Application.Tests.Commands.Retailer.CreateRetailer;

public class GetRetailerByIdHandlerTests
{
    private readonly Mock<ICheckpointRepository<RetailerEntity>> _retailerCheckpointRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetRetailerByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetRetailerByIdQueryHandler>> _loggerMock;
    
    public GetRetailerByIdHandlerTests()
    {
        _retailerCheckpointRepositoryMock = new Mock<ICheckpointRepository<RetailerEntity>>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetRetailerByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetRetailerByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetRetailerByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetRetailerByIdQuery() { Id = Guid.NewGuid() };
        var item = RetailerFaker.GetRetailerRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _retailerCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
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
        _retailerCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Once());
    }
    
    [Fact]
    public async void GetRetailerByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetRetailerByIdQuery() { Id = Guid.NewGuid() };
        var item = RetailerFaker.GetRetailerRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _retailerCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<RetailerEntity?>(new RetailerEntity()));
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _retailerCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetRetailerByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetRetailerByIdQuery() { Id = Guid.NewGuid() };
        var item = RetailerFaker.GetRetailerRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _retailerCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<RetailerEntity?>(null));
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
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
        _retailerCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetRetailerByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetRetailerByIdQuery() { Id = Guid.NewGuid() };
        var item = RetailerFaker.GetRetailerRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _retailerCheckpointRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _retailerCheckpointRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never());
    }
    
    private GetRetailerByIdQueryHandler GetQueryHandler() =>
        new GetRetailerByIdQueryHandler(
            _retailerCheckpointRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}