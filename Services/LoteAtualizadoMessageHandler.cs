using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using STUR_mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace STUR_mvc.Services
{
    public class LoteAtualizadoMessageHandler : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        const string topico = "modgeo-lote-atualizado";

        public LoteAtualizadoMessageHandler(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scope = scopeFactory.CreateScope();
            var kafkaConfig = scope.ServiceProvider.GetRequiredService<IOptions<KafkaConfig>>();
            var conf = new ConsumerConfig
            {
                GroupId = "modgeo_lote_group",
                BootstrapServers = kafkaConfig.Value.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe(topico);
                var cts = new CancellationTokenSource();

                var dbContext = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<STURDBContext>();
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"Conectando ao tópico: ${topico}");
                        var message = c.Consume(cts.Token);
                        if (!string.IsNullOrEmpty(message.Message.Value))
                        {
                            Console.WriteLine($"KAFKA: {message.Message.Value}");
                            await CriarLote(message, dbContext);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Problema ao conectar no Kafka");
                }
            }
        }

        private async Task CriarLote(ConsumeResult<Ignore, string> message, STURDBContext dbContext)
        {
            try
            {
                var loteMessage = JsonSerializer.Deserialize<Lote>(message.Message.Value);
                var lote = await dbContext.Lotes.FirstOrDefaultAsync(w => w.InscricaoImovel == loteMessage.InscricaoImovel);
                if (lote != null)
                {
                    lote.AreaConstruida = loteMessage.AreaConstruida;
                    lote.AreaTerreno = loteMessage.AreaTerreno;
                    lote.DataAtualizacao = loteMessage.DataAtualizacao;
                    dbContext.Lotes.Update(lote);
                }                    
                else
                    dbContext.Lotes.Add(loteMessage);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gravar lote via mensagem: " + ex.Message);
            }

        }
    }
}
