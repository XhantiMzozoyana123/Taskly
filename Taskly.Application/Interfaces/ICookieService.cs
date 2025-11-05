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
    public interface ICookieService
    {
        Task<(IPage page, IBrowser browser)> LoadCookieOnPageAsync(string cookiePath, bool hideBrowser);

        Task<string> IdentifyCookieSiteAsync(string cookiePath);

        Task<List<string>> GetCookieFilePathsAsync();

        Task<UploadResponseDto> UploadFileAsync(string filePath);
    }
}
