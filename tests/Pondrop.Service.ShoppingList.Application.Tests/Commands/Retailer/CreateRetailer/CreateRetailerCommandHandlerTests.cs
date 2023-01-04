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

namespace Pondrop.Service.ShoppingList.Application.Tests.Commands.Retailer.CreateRetailer;

public class CreateRetailerCommandHandlerTests
{
    private readonly Mock<IOptions<RetailerUpdateConfiguration>> _retailerUpdateConfigMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDaprService> _daprServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateRetailerCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateRetailerCommandHandler>> _loggerMock;

    public CreateRetailerCommandHandlerTests()
    {
        _retailerUpdateConfigMock = new Mock<IOptions<RetailerUpdateConfiguration>>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _daprServiceMock = new Mock<IDaprService>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateRetailerCommand>>();
        _loggerMock = new Mock<ILogger<CreateRetailerCommandHandler>>();

        _retailerUpdateConfigMock
            .Setup(x => x.Value)
            .Returns(new RetailerUpdateConfiguration());
        _userServiceMock
            .Setup(x => x.CurrentUserName())
            .Returns("test/user");
    }

    [Fact]
    public async void CreateRetailerCommand_ShouldSucceed()
    {
        // arrange
        var cmd = RetailerFaker.GetCreateRetailerCommand();
        var item = RetailerFaker.GetRetailerRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(true));
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
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
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Once);
    }

    [Fact]
    public async void CreateRetailerCommand_WhenInvalid_ShouldFail()
    {
        // arrange
        var cmd = RetailerFaker.GetCreateRetailerCommand();
        var item = RetailerFaker.GetRetailerRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult(new[] { new ValidationFailure() }));
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
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateRetailerCommand_WhenAppendEventsFail_ShouldFail()
    {
        // arrange
        var cmd = RetailerFaker.GetCreateRetailerCommand();
        var item = RetailerFaker.GetRetailerRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Returns(Task.FromResult(false));
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
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
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never);
    }

    [Fact]
    public async void CreateRetailerCommand_WhenException_ShouldFail()
    {
        // arrange
        var cmd = RetailerFaker.GetCreateRetailerCommand();
        var item = RetailerFaker.GetRetailerRecord(cmd);
        _validatorMock
            .Setup(x => x.Validate(cmd))
            .Returns(new ValidationResult());
        _eventRepositoryMock
            .Setup(x => x.AppendEventsAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<IEnumerable<IEvent>>()))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()))
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
            x => x.Map<RetailerRecord>(It.IsAny<RetailerEntity>()),
            Times.Never);
    }

    private CreateRetailerCommandHandler GetCommandHandler() =>
        new CreateRetailerCommandHandler(
            _retailerUpdateConfigMock.Object,
            _eventRepositoryMock.Object,
            _daprServiceMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}