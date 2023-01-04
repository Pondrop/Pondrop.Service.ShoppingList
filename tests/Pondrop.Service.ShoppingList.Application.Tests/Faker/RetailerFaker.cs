using Bogus;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.ShoppingList.Tests.Faker;

public static class RetailerFaker
{
    private static readonly string[] Names = new[] { "Coles", "Woolworths", "Aldi", "Test" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<RetailerRecord> GetRetailerRecords(int count = 5)
    {
        var faker = new Faker<RetailerRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static CreateRetailerCommand GetCreateRetailerCommand()
    {
        var faker = new Faker<CreateRetailerCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString());

        return faker.Generate();
    }
    
    public static UpdateRetailerCommand GetUpdateRetailerCommand()
    {
        var faker = new Faker<UpdateRetailerCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names));

        return faker.Generate();
    }
    
    public static RetailerRecord GetRetailerRecord(CreateRetailerCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<RetailerRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => command.ExternalReferenceId)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static RetailerRecord GetRetailerRecord(UpdateRetailerCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<RetailerRecord>()
            .RuleFor(x => x.Id, f => command.Id)
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}