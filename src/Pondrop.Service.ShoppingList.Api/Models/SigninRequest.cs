using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.ShoppingList.Api.Models;
public class SigninRequest
{
    public string Email { get; set; }


}
