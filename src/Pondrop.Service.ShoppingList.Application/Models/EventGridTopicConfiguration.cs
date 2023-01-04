using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.ShoppingList.Application.Models;
public class EventGridTopicConfiguration
{
    public const string Key = nameof(EventGridTopicConfiguration);

    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
}

