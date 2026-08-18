using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface IAirbnbService
    {
        Task SearchAsync(SearchDto searchDto);

        Task<IPage> GoToSearchPageAsync(IPage page, SearchDto searchDto);

        Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto);
    }
}
