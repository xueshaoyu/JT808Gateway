﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JT808.Protocol;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using JT808.Gateway.TestHosting.Jobs;
using JT808.Gateway.Kafka;
using JT808.Gateway.InMemoryMQ;
using JT808.Gateway.ReplyMessage;
using JT808.Gateway.Client;

namespace JT808.Gateway.TestHosting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverHostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{ hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    Console.WriteLine($"Environment.OSVersion.Platform:{Environment.OSVersion.Platform.ToString()}");
                    NLog.LogManager.LoadConfiguration($"Configs/nlog.{Environment.OSVersion.Platform.ToString()}.config");
                    logging.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILoggerFactory, LoggerFactory>();
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddJT808Configure()
                            //.AddJT808Client()
                            //.AddJT808Gateway(options =>
                            //{
                            //    options.TcpPort = 808;
                            //    options.UdpPort = 808;
                            //})                            
                            .AddJT808Gateway(hostContext.Configuration)
                            .AddTcp()
                            .AddUdp()
                            .AddGrpc()
                            //InMemoryMQ
                            .AddJT808ServerInMemoryMQ()
                            .AddJT808InMemoryReplyMessage()
                            //kafka插件
                            //.AddJT808ServerKafkaMsgProducer(hostContext.Configuration)
                            //.AddJT808ServerKafkaMsgReplyConsumer(hostContext.Configuration)
                            //.AddJT808ServerKafkaSessionProducer(hostContext.Configuration)
                            ;
                    //grpc客户端调用
                    //services.AddHostedService<CallGrpcClientJob>();
                    //客户端测试
                    //services.AddHostedService<UpJob>();
                });

            await serverHostBuilder.RunConsoleAsync();
        }
    }
}