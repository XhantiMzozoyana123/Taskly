using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface IFacebookService
    {
        Task SearchAsync(SearchDto searchDto);

        Task<IPage> GoToFacebookGroupPage(IPage page, SearchDto searchDto);

        Task<IPage> SelectRandomFacebookGroup(IPage page, SearchDto searchDto);

        Task<string> AuthorProfileUrlExchangedUrlAsync(IPage page, string partialUrl);

        Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto);

        Task<List<string>> SelectAllFacebookFacebookGroups(IPage page, SearchDto searchDto);

        Task<IPage> ExtractSelectedProfileAsync(IPage page);

        Task<IPage> InjectMessenger(IPage page, string action);
    }
}