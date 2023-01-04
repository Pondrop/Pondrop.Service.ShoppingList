using Bogus;
using Pondrop.Service.ShoppingList.Application.Commands;

using Pondrop.Service.ShoppingList.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.ShoppingList.Tests.Faker;

public static class StoreFaker
{
    private static readonly string[] Names = new[] { "The Local", "The Far Away", "The Just Right", "Test" };
    private static readonly string[] Statues = new[] { "Online", "Offline", "Unknown", };
    private static readonly string[] AddressLine1 = new[] { "123 Street", "123 Lane", "123 Court", };
    private static readonly string[] AddressLine2 = new[] { "" };
    private static readonly string[] Suburbs = new[] { "Lakes", "Rivers", "Seaside" };
    private static readonly string[] States = new[] { "WA", "NT", "SA", "QLD", "NSW", "ACT", "VIC", "TAS" };
    private static readonly string[] Postcodes = new[] { "6000", "5000", "4000", "2000", "3000", "7000" };
    private static readonly string[] Countries = new[] { "Australia" };
    private static readonly double[] Lats = new[] { 25.6091 };
    private static readonly double[] Lngs = new[] { 134.3619 };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<StoreRecord> GetStoreRecords(int count = 5)
    {
        var faker = new Faker<StoreRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Addresses, f => GetStoreAddressRecords(1))
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.StoreTypeId, f => Guid.NewGuid())
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static List<StoreViewRecord> GetStoreViewRecords(int count = 5)
    {
        var retailer = RetailerFaker.GetRetailerRecords(1).Single();
        var storeType = StoreTypeFaker.GetStoreTypeRecords(1).Single();
        
        var faker = new Faker<StoreViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => retailer.Id)
            .RuleFor(x => x.Retailer, f => retailer)
            .RuleFor(x => x.StoreTypeId, f => storeType.Id)
            .RuleFor(x => x.StoreType, f => storeType)
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static List<StoreAddressRecord> GetStoreAddressRecords(int count = 1)
    {
        var faker = new Faker<StoreAddressRecord>()
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static CreateStoreCommand GetCreateStoreCommand()
    {
        var faker = new Faker<CreateStoreCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Address, f => GetAddressRecord())
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.StoreTypeId, f => Guid.NewGuid());

        return faker.Generate();
    }
    
    public static UpdateStoreCommand GetUpdateStoreCommand()
    {
        var faker = new Faker<UpdateStoreCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Status, f => f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => Guid.NewGuid())
            .RuleFor(x => x.StoreTypeId, f => Guid.NewGuid());

        return faker.Generate();
    }
    
    public static AddAddressToStoreCommand GetAddAddressToStoreCommand()
    {
        var faker = new Faker<AddAddressToStoreCommand>()
            .RuleFor(x => x.StoreId, f => Guid.NewGuid())
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
    
    public static UpdateStoreAddressCommand GetUpdateStoreAddressCommand()
    {
        var faker = new Faker<UpdateStoreAddressCommand>()
            .RuleFor(x => x.StoreId, f => Guid.NewGuid())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
    
    public static StoreRecord GetStoreRecord(CreateStoreCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<StoreRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => command.Name)
            .RuleFor(x => x.ExternalReferenceId, f => command.ExternalReferenceId)
            .RuleFor(x => x.Addresses, f => command.Address is not null
                ? new List<StoreAddressRecord>(1) { GetStoreAddressRecord(command.Address!) }
                : GetStoreAddressRecords(1))
            .RuleFor(x => x.Status, f => command.Status)
            .RuleFor(x => x.RetailerId, f => command.RetailerId)
            .RuleFor(x => x.StoreTypeId, f => command.StoreTypeId)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static StoreRecord GetStoreRecord(UpdateStoreCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<StoreRecord>()
            .RuleFor(x => x.Id, f => command.Id)
            .RuleFor(x => x.Name, f => command.Name ?? f.PickRandom(Names))
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Addresses, f => GetStoreAddressRecords(1))
            .RuleFor(x => x.Status, f => command.Status ?? f.PickRandom(Statues))
            .RuleFor(x => x.RetailerId, f => command.RetailerId ?? Guid.NewGuid())
            .RuleFor(x => x.StoreTypeId, f => command.StoreTypeId ?? Guid.NewGuid())
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    private static StoreAddressRecord GetStoreAddressRecord(AddressRecord record)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<StoreAddressRecord>()
            .RuleFor(x => x.AddressLine1, f => record.AddressLine1)
            .RuleFor(x => x.AddressLine2, f => record.AddressLine2)
            .RuleFor(x => x.Suburb, f => record.Suburb)
            .RuleFor(x => x.State, f => record.State)
            .RuleFor(x => x.Postcode, f => record.Postcode)
            .RuleFor(x => x.Country, f => record.Country)
            .RuleFor(x => x.Latitude, f => record.Latitude)
            .RuleFor(x => x.Longitude, f => record.Longitude)
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    private static AddressRecord GetAddressRecord()
    {
        var faker = new Faker<AddressRecord>()
            .RuleFor(x => x.ExternalReferenceId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.AddressLine1, f => f.PickRandom(AddressLine1))
            .RuleFor(x => x.AddressLine2, f => f.PickRandom(AddressLine2))
            .RuleFor(x => x.Suburb, f => f.PickRandom(Suburbs))
            .RuleFor(x => x.State, f => f.PickRandom(States))
            .RuleFor(x => x.Postcode, f => f.PickRandom(Postcodes))
            .RuleFor(x => x.Country, f => f.PickRandom(Countries))
            .RuleFor(x => x.Latitude, f => f.PickRandom(Lats))
            .RuleFor(x => x.Longitude, f => f.PickRandom(Lngs));

        return faker.Generate();
    }
}