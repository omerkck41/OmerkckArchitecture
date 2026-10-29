// What a reader's Program.cs typically has in scope: the implicit usings of Microsoft.NET.Sdk.Web plus the Kck
// namespaces a README block relies on without spelling them out.
global using Kck.BackgroundJobs.Abstractions;
global using Kck.Caching.Abstractions;
global using Kck.Core.Abstractions.Paging;
global using Kck.Persistence.Abstractions.Repositories;
global using Kck.Persistence.Abstractions.UnitOfWork;
global using Mediator;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
