global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.AspNetCore.Authentication.JwtBearer;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using Microsoft.OpenApi;

global using Web.Api;
global using Web.Api.Extensions;
global using Web.Api.Infrastructure;
global using Web.Api.Infrastructure.Options;

// Common cross-layer namespaces used by Web.Api endpoints
global using Application.Abstractions.Messaging;
global using Application.Users.Register;
global using SharedKernel;
global using SharedKernel.Model.Responses;
global using System.Text.RegularExpressions;
global using Serilog.Enrichers.Sensitive;