using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface ITikTokService
    {
        Task SearchAsync(SearchDto searchDto);

        Task<IPage> GoToDiscoveryPageAsync(IPage page, SearchDto searchDto);

        Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto);

        Task<string> GetVideoDescription(IPage page);
    }
}
