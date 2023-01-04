using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public abstract class RebuildCheckpointCommand : IRequest<Result<int>> 
{
}