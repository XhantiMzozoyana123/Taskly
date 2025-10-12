using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface ISenderService
    {
        Task AutomatedMessagingAsync(MessengerDto messengerDto);

        Task<bool> ManualMessagingAsync(MessengerDto messengerDto);
    }
}
