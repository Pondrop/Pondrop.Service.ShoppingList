using AutoMapper;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Domain.Mapper;

public class ListItemProfile : Profile
{
    public ListItemProfile()
    {
        CreateMap<ListItemEntity, ListItemRecord>();
    }
}
