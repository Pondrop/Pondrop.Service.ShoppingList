using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateListItemCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<ListItemRecord>>
{
}