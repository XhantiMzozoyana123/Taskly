using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface IInstagramService
    {
        Task SearchAsync(SearchDto searchDto);

        Task<PostContentDto> GetAuthorPost(IPage page, string postUrl);

        Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto);

        Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto);
    }
}
