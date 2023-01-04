using AutoMapper;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Domain.Mapper;

public class ShoppingListProfile : Profile
{
    public ShoppingListProfile()
    {
        CreateMap<ShoppingListEntity, ShoppingListRecord>();
    }
}
