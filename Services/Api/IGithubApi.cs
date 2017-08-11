﻿using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using RestEase;

namespace Fusonic.GitBackup.Services.Api
{
    [Header("User-Agent", "FusonicGitBackup")]
    public interface IGithubApi
    {
        [Header("Authorization")]
        AuthenticationHeaderValue Authorization { get; set; }
        
        [Get("user/repos")]
        Task<Response<List<GithubRepository>>> GetRepositoriesAsync([RawQueryString] string perPage);
    }
}