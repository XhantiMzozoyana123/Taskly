using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Domain.Entities;

namespace Taskly.Application.Interfaces
{
    public interface ISenderService
    {
        Task MessagingSequenceAsync(MessengerDto messengerDto);
    }
}
