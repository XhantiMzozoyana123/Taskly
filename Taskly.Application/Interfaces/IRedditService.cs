using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Taskly.Application.Interfaces
{
    public interface IRedditService
    {
        Task SearchAsync(SearchDto searchDto);

        Task ScrapeSocialLinks(IPage page, Leads lead, string profileUrl);

        Task<IPage> FindSubredditsUrl(IPage page, SearchDto searchDto);
    }
}
