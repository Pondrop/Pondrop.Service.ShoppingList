using Pondrop.Service.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Pondrop.Service.ShoppingList.Api.Services;

public class AddressService : IAddressService
{
    private readonly Regex _postcodeRegex =
        new Regex(
            "^(0[289][0-9]{2})|([1345689][0-9]{3})|(2[0-8][0-9]{2})|(290[0-9])|(291[0-4])|(7[0-4][0-9]{2})|(7[8-9][0-9]{2})$",
            RegexOptions.Compiled);
    
    private readonly ReadOnlyDictionary<string, string> _validStates = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
    {
        ["NSW"] = "New South Wales",
        ["VIC"] = "Victoria",
        ["QLD"] = "Queensland",
        ["WA"] = "Western Australia",
        ["SA"] = "South Australia",
        ["TAS"] = "Tasmania",
        ["ACT"] = "Australian Capital Territory",
        ["NT"] = "Northern Territory"
    });

    private readonly ReadOnlyDictionary<string, string> _validStatesReverse;
    
    private readonly ReadOnlyDictionary<string, string> _validCountries = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
    {
        ["AUS"] = "Australia",
    });
    
    private readonly ReadOnlyDictionary<string, string> _validCountriesReverse;

    public AddressService()
    {
        var statesReverse = new Dictionary<string, string>();
        foreach (var kv in _validStates)
            statesReverse.Add(kv.Value, kv.Key);
        _validStatesReverse = new ReadOnlyDictionary<string, string>(statesReverse);
        
        var countriesReverse = new Dictionary<string, string>();
        foreach (var kv in _validCountries)
            countriesReverse.Add(kv.Value, kv.Key);
        _validCountriesReverse = new ReadOnlyDictionary<string, string>(countriesReverse);
    }
    
    public bool IsValidAustralianPostcode(string postcode)
        => _postcodeRegex.IsMatch(postcode);

    public bool IsValidAustralianState(string state)
        => _validStates.ContainsKey(state) || _validStatesReverse.ContainsKey(state);

    public bool IsValidAustralianCountry(string country)
        => _validCountries.ContainsKey(country) || _validCountriesReverse.ContainsKey(country);
}