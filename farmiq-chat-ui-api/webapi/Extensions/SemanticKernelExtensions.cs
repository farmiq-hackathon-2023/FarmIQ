// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CopilotChat.WebApi.Hubs;
using CopilotChat.WebApi.Models.Storage;
using CopilotChat.WebApi.Options;
using CopilotChat.WebApi.Services;
using CopilotChat.WebApi.Skills.ChatSkills;
using CopilotChat.WebApi.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Connectors.Memory.Chroma;
using Microsoft.SemanticKernel.Connectors.Memory.Postgres;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Skills.Core;
using Newtonsoft.Json;
using Npgsql;
using Pgvector.Npgsql;
using SharpYaml.Tokens;
using static CopilotChat.WebApi.Options.MemoryStoreOptions;

namespace CopilotChat.WebApi.Extensions;

/// <summary>
/// Extension methods for registering Semantic Kernel related services.
/// </summary>
internal static class SemanticKernelExtensions
{
    /// <summary>
    /// Delegate to register skills with a Semantic Kernel
    /// </summary>
    public delegate Task RegisterSkillsWithKernel(IServiceProvider sp, IKernel kernel);

    /// <summary>
    /// Add Semantic Kernel services
    /// </summary>
    internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services)
    {
        // Semantic Kernel
        services.AddScoped<IKernel>(sp =>
        {
            IKernel kernel = Kernel.Builder
                .WithLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
                .WithCompletionBackend(sp.GetRequiredService<IOptions<AIServiceOptions>>().Value)
                .WithEmbeddingBackend(sp.GetRequiredService<IOptions<AIServiceOptions>>().Value)
                .Build();


            string json = @"[
    {
        ""State"": ""New York"",
        ""City"": ""New York"",
        ""Location"": ""40°40′N 73°56′W / 40.66°N 73.94°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""80"",
        ""Phosphorous"": ""50"",
        ""Potassium"": ""44"",
        ""Temperature"": ""61.77768706"",
        ""Humidity"": ""70"",
        ""Ph"": ""4"",
        ""RainFall"": ""146.8"",
        ""CropType"": ""grapes"",
        ""ID"": ""0""
    },
    {
        ""State"": ""California"",
        ""City"": ""Los Angeles"",
        ""Location"": ""34°01′N 118°25′W / 34.02°N 118.41°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""80"",
        ""Phosphorous"": ""85"",
        ""Potassium"": ""82"",
        ""Temperature"": ""59.32253725"",
        ""Humidity"": ""68"",
        ""Ph"": ""8"",
        ""RainFall"": ""118.1"",
        ""CropType"": ""chickpea"",
        ""ID"": ""1""
    },
    {
        ""State"": ""Illinois"",
        ""City"": ""Chicago"",
        ""Location"": ""41°50′N 87°41′W / 41.84°N 87.68°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""74"",
        ""Phosphorous"": ""50"",
        ""Potassium"": ""73"",
        ""Temperature"": ""56.59341347"",
        ""Humidity"": ""85"",
        ""Ph"": ""7"",
        ""RainFall"": ""132.5"",
        ""CropType"": ""pomegranate"",
        ""ID"": ""2""
    },
    {
        ""State"": ""Texas"",
        ""City"": ""Houston"",
        ""Location"": ""29°47′N 95°23′W / 29.79°N 95.39°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""86"",
        ""Phosphorous"": ""56"",
        ""Potassium"": ""47"",
        ""Temperature"": ""70.57621453"",
        ""Humidity"": ""72"",
        ""Ph"": ""7"",
        ""RainFall"": ""139.1"",
        ""CropType"": ""mungbean"",
        ""ID"": ""3""
    },
    {
        ""State"": ""Arizona"",
        ""City"": ""Phoenix"",
        ""Location"": ""33°34′N 112°05′W / 33.57°N 112.09°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""88"",
        ""Phosphorous"": ""72"",
        ""Potassium"": ""57"",
        ""Temperature"": ""76.06130826"",
        ""Humidity"": ""79"",
        ""Ph"": ""4"",
        ""RainFall"": ""85"",
        ""CropType"": ""kidneybeans"",
        ""ID"": ""4""
    },
    {
        ""State"": ""Pennsylvania"",
        ""City"": ""Philadelphia"",
        ""Location"": ""40°01′N 75°08′W / 40.01°N 75.13°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""91"",
        ""Phosphorous"": ""74"",
        ""Potassium"": ""79"",
        ""Temperature"": ""61.92053266"",
        ""Humidity"": ""77"",
        ""Ph"": ""8"",
        ""RainFall"": ""107.1"",
        ""CropType"": ""jute"",
        ""ID"": ""5""
    },
    {
        ""State"": ""Texas"",
        ""City"": ""San Antonio"",
        ""Location"": ""29°28′N 98°31′W / 29.46°N 98.52°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""69"",
        ""Phosphorous"": ""80"",
        ""Potassium"": ""68"",
        ""Temperature"": ""79.86933168"",
        ""Humidity"": ""76"",
        ""Ph"": ""6"",
        ""RainFall"": ""72.8"",
        ""CropType"": ""jute"",
        ""ID"": ""6""
    },
    {
        ""State"": ""California"",
        ""City"": ""San Diego"",
        ""Location"": ""32°49′N 117°08′W / 32.81°N 117.14°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""77"",
        ""Phosphorous"": ""77"",
        ""Potassium"": ""66"",
        ""Temperature"": ""62.3881933"",
        ""Humidity"": ""76"",
        ""Ph"": ""4"",
        ""RainFall"": ""63.8"",
        ""CropType"": ""papaya"",
        ""ID"": ""7""
    },
    {
        ""State"": ""Texas"",
        ""City"": ""Dallas"",
        ""Location"": ""32°47′N 96°46′W / 32.79°N 96.77°W"",
        ""Month"": ""January"",
        ""Nitrogen"": ""90"",
        ""Phosphorous"": ""60"",
        ""Potassium"": ""66"",
        ""Temperature"": ""75.81699631"",
        ""Humidity"": ""82"",
        ""Ph"": ""8"",
        ""RainFall"": ""55.1"",
        ""CropType"": ""lentil"",
        ""ID"": ""8""
    }
]";

            var farmDataList = JsonConvert.DeserializeObject<List<FarmData>>(json);

            // Convert the list to a dictionary with ID as the key and properties as the value
            Dictionary<string, string> farmDictionary = farmDataList.ToDictionary(
                farm => farm.ID,
                farm => GetRecordSummary(farm)
            );

            // Print the dictionary
            foreach (var entry in farmDictionary)
            {
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
                kernel.Memory.SaveReferenceAsync(
                collection: "FarmData",
                externalSourceName: "farmprediction",
                externalId: entry.Key,
                description: entry.Value,
                 text: entry.Value);

            }

            sp.GetRequiredService<RegisterSkillsWithKernel>()(sp, kernel);
            return kernel;
        });

        // Semantic memory
        services.AddSemanticTextMemory();

        // Azure Content Safety
        services.AddContentSafety();

        // Register skills
        services.AddScoped<RegisterSkillsWithKernel>(sp => RegisterSkillsAsync);

        return services;
    }


    public static string GetRecordSummary(FarmData farmdata)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Following are the details about the city " + farmdata.City + " which is present in the state " + farmdata.State + "  in the month of " + farmdata.Month);
        sb.AppendLine("1.The nitrogen content in soil is " + farmdata.Nitrogen);
        sb.AppendLine("2.The Phosporous content in soil is " + farmdata.Nitrogen);
        sb.AppendLine("3.Potassium content in soil is " + farmdata.Phosphorous);
        sb.AppendLine("4.The average temperature for city in farenheit " + farmdata.Temperature);
        sb.AppendLine("5.The pH value of water is " + farmdata.Ph);
        sb.AppendLine("By considering all the 5 points about the city " + farmdata.City + " which is present in the state " + farmdata.State + " in the month of " + farmdata.Month + " The prediction model says that the crop" + farmdata.CropType + " can be grown");
        return sb.ToString();
    }

    /// <summary>
    /// Add Planner services
    /// </summary>
    public static IServiceCollection AddPlannerServices(this IServiceCollection services)
    {
        IOptions<PlannerOptions>? plannerOptions = services.BuildServiceProvider().GetService<IOptions<PlannerOptions>>();
        services.AddScoped<CopilotChatPlanner>(sp =>
        {
            IKernel plannerKernel = Kernel.Builder
                .WithLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
                // TODO: [sk Issue #2046] verify planner has AI service configured
                .WithPlannerBackend(sp.GetRequiredService<IOptions<AIServiceOptions>>().Value)
                .Build();
            return new CopilotChatPlanner(plannerKernel, plannerOptions?.Value, sp.GetRequiredService<ILogger<CopilotChatPlanner>>());
        });

        // Register Planner skills (AI plugins) here.
        // TODO: [sk Issue #2046] Move planner skill registration from ChatController to this location.

        return services;
    }

    /// <summary>
    /// Register the chat skill with the kernel.
    /// </summary>
    public static IKernel RegisterChatSkill(this IKernel kernel, IServiceProvider sp)
    {
        // Chat skill
        kernel.ImportSkill(new ChatSkill(
                kernel: kernel,
                chatMessageRepository: sp.GetRequiredService<ChatMessageRepository>(),
                chatSessionRepository: sp.GetRequiredService<ChatSessionRepository>(),
                messageRelayHubContext: sp.GetRequiredService<IHubContext<MessageRelayHub>>(),
                promptOptions: sp.GetRequiredService<IOptions<PromptsOptions>>(),
                documentImportOptions: sp.GetRequiredService<IOptions<DocumentMemoryOptions>>(),
                contentSafety: sp.GetService<AzureContentSafety>(),
                planner: sp.GetRequiredService<CopilotChatPlanner>(),
                logger: sp.GetRequiredService<ILogger<ChatSkill>>()),
            nameof(ChatSkill));

        return kernel;
    }

    /// <summary>
    /// Propagate exception from within semantic function
    /// </summary>
    public static void ThrowIfFailed(this SKContext context)
    {
        if (context.ErrorOccurred)
        {
            var logger = context.LoggerFactory.CreateLogger(nameof(SKContext));
            logger.LogError(context.LastException, "{0}", context.LastException?.Message);
            throw context.LastException!;
        }
    }

    /// <summary>
    /// Register the skills with the kernel.
    /// </summary>
    private static Task RegisterSkillsAsync(IServiceProvider sp, IKernel kernel)
    {
        // Copilot chat skills
        kernel.RegisterChatSkill(sp);

        // Time skill
        kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));

        // Semantic skills
        ServiceOptions options = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(options.SemanticSkillsDirectory))
        {
            foreach (string subDir in System.IO.Directory.GetDirectories(options.SemanticSkillsDirectory))
            {
                try
                {
                    kernel.ImportSemanticSkillFromDirectory(options.SemanticSkillsDirectory, Path.GetFileName(subDir)!);
                }
                catch (SKException ex)
                {
                    var logger = kernel.LoggerFactory.CreateLogger(nameof(Kernel));
                    logger.LogError("Could not load skill from {Directory}: {Message}", subDir, ex.Message);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Add the semantic memory.
    /// </summary>
    private static void AddSemanticTextMemory(this IServiceCollection services)
    {
        MemoryStoreOptions config = services.BuildServiceProvider().GetRequiredService<IOptions<MemoryStoreOptions>>().Value;

        switch (config.Type)
        {
            case MemoryStoreType.Volatile:
                services.AddSingleton<IMemoryStore, VolatileMemoryStore>();
                break;

            case MemoryStoreType.Qdrant:
                if (config.Qdrant == null)
                {
                    throw new InvalidOperationException("MemoryStore type is Qdrant and Qdrant configuration is null.");
                }

                services.AddSingleton<IMemoryStore>(sp =>
                {
                    HttpClient httpClient = new(new HttpClientHandler { CheckCertificateRevocationList = true });
                    if (!string.IsNullOrWhiteSpace(config.Qdrant.Key))
                    {
                        httpClient.DefaultRequestHeaders.Add("api-key", config.Qdrant.Key);
                    }

                    var endPointBuilder = new UriBuilder(config.Qdrant.Host);
                    endPointBuilder.Port = config.Qdrant.Port;

                    return new QdrantMemoryStore(
                        httpClient: httpClient,
                        config.Qdrant.VectorSize,
                        endPointBuilder.ToString(),
                        loggerFactory: sp.GetRequiredService<ILoggerFactory>()
                    );
                });
                break;

            case MemoryStoreType.AzureCognitiveSearch:
                if (config.AzureCognitiveSearch == null)
                {
                    throw new InvalidOperationException("MemoryStore type is AzureCognitiveSearch and AzureCognitiveSearch configuration is null.");
                }
                Console.WriteLine("AzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStoreAzureCognitiveSearchMemoryStore####################################################");

                services.AddSingleton<IMemoryStore>(sp =>
                {
                    return new AzureCognitiveSearchMemoryStore(config.AzureCognitiveSearch.Endpoint, config.AzureCognitiveSearch.Key);
                });
                break;

            case MemoryStoreOptions.MemoryStoreType.Chroma:
                if (config.Chroma == null)
                {
                    throw new InvalidOperationException("MemoryStore type is Chroma and Chroma configuration is null.");
                }

                services.AddSingleton<IMemoryStore>(sp =>
                {
                    HttpClient httpClient = new(new HttpClientHandler { CheckCertificateRevocationList = true });
                    var endPointBuilder = new UriBuilder(config.Chroma.Host);
                    endPointBuilder.Port = config.Chroma.Port;

                    return new ChromaMemoryStore(
                        httpClient: httpClient,
                        endpoint: endPointBuilder.ToString(),
                        loggerFactory: sp.GetRequiredService<ILoggerFactory>()
                    );
                });
                break;

            case MemoryStoreOptions.MemoryStoreType.Postgres:
                if (config.Postgres == null)
                {
                    throw new InvalidOperationException("MemoryStore type is Postgres and Postgres configuration is null.");
                }

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(config.Postgres.ConnectionString);
                dataSourceBuilder.UseVector();

                services.AddSingleton<IMemoryStore>(sp =>
                {
                    return new PostgresMemoryStore(
                        dataSource: dataSourceBuilder.Build(),
                        vectorSize: config.Postgres.VectorSize
                    );
                });

                break;

            default:
                throw new InvalidOperationException($"Invalid 'MemoryStore' type '{config.Type}'.");
        }

        services.AddScoped<ISemanticTextMemory>(sp => new SemanticTextMemory(
            sp.GetRequiredService<IMemoryStore>(),
            sp.GetRequiredService<IOptions<AIServiceOptions>>().Value
                .ToTextEmbeddingsService(loggerFactory: sp.GetRequiredService<ILoggerFactory>())));
    }

    /// <summary>
    /// Adds Azure Content Safety
    /// </summary>
    internal static void AddContentSafety(this IServiceCollection services)
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        ContentSafetyOptions options = configuration.GetSection(ContentSafetyOptions.PropertyName).Get<ContentSafetyOptions>();

        if (options.Enabled)
        {
            services.AddSingleton<IContentSafetyService, AzureContentSafety>(sp => new AzureContentSafety(new Uri(options.Endpoint), options.Key, options));
        }
    }

    /// <summary>
    /// Add the completion backend to the kernel config
    /// </summary>
    private static KernelBuilder WithCompletionBackend(this KernelBuilder kernelBuilder, AIServiceOptions options)
    {
        return options.Type switch
        {
            AIServiceOptions.AIServiceType.AzureOpenAI
                => kernelBuilder.WithAzureChatCompletionService(options.Models.Completion, options.Endpoint, options.Key),
            AIServiceOptions.AIServiceType.OpenAI
                => kernelBuilder.WithOpenAIChatCompletionService(options.Models.Completion, options.Key),
            _
                => throw new ArgumentException($"Invalid {nameof(options.Type)} value in '{AIServiceOptions.PropertyName}' settings."),
        };
    }

    /// <summary>
    /// Add the embedding backend to the kernel config
    /// </summary>
    private static KernelBuilder WithEmbeddingBackend(this KernelBuilder kernelBuilder, AIServiceOptions options)
    {
        return options.Type switch
        {
            AIServiceOptions.AIServiceType.AzureOpenAI
                => kernelBuilder.WithAzureTextEmbeddingGenerationService(options.Models.Embedding, options.Endpoint, options.Key),
            AIServiceOptions.AIServiceType.OpenAI
                => kernelBuilder.WithOpenAITextEmbeddingGenerationService(options.Models.Embedding, options.Key),
            _
                => throw new ArgumentException($"Invalid {nameof(options.Type)} value in '{AIServiceOptions.PropertyName}' settings."),
        };
    }

    /// <summary>
    /// Add the completion backend to the kernel config for the planner.
    /// </summary>
    private static KernelBuilder WithPlannerBackend(this KernelBuilder kernelBuilder, AIServiceOptions options)
    {
        return options.Type switch
        {
            AIServiceOptions.AIServiceType.AzureOpenAI => kernelBuilder.WithAzureChatCompletionService(options.Models.Planner, options.Endpoint, options.Key),
            AIServiceOptions.AIServiceType.OpenAI => kernelBuilder.WithOpenAIChatCompletionService(options.Models.Planner, options.Key),
            _ => throw new ArgumentException($"Invalid {nameof(options.Type)} value in '{AIServiceOptions.PropertyName}' settings."),
        };
    }

    /// <summary>
    /// Construct IEmbeddingGeneration from <see cref="AIServiceOptions"/>
    /// </summary>
    /// <param name="options">The service configuration</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">Custom <see cref="ILoggerFactory"/> for logging.</param>
    private static ITextEmbeddingGeneration ToTextEmbeddingsService(this AIServiceOptions options,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        return options.Type switch
        {
            AIServiceOptions.AIServiceType.AzureOpenAI
                => new AzureTextEmbeddingGeneration(options.Models.Embedding, options.Endpoint, options.Key, httpClient: httpClient, loggerFactory: loggerFactory),
            AIServiceOptions.AIServiceType.OpenAI
                => new OpenAITextEmbeddingGeneration(options.Models.Embedding, options.Key, httpClient: httpClient, loggerFactory: loggerFactory),
            _
                => throw new ArgumentException("Invalid AIService value in embeddings backend settings"),
        };
    }
}
