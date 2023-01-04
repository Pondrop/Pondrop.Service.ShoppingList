using AutoMapper;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Domain.Mapper;

public class SharedListShopperProfile : Profile
{
    public SharedListShopperProfile()
    {
        CreateMap<SharedListShopperEntity, SharedListShopperRecord>();
    }
}
