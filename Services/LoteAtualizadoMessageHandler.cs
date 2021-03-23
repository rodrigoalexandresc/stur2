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
        private IConsumer<Ignore, string> kafkaConsumer;        

        public LoteAtualizadoMessageHandler(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }

        protected void StartConsumerLoop(CancellationToken stoppingToken) {
            var scope = scopeFactory.CreateScope();
            var kafkaConfig = scope.ServiceProvider.GetRequiredService<IOptions<KafkaConfig>>();
            var dbContext = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<STURDBContext>();

            var conf = new ConsumerConfig
            {
                GroupId = "modgeo_lote_group",
                BootstrapServers = kafkaConfig.Value.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            this.kafkaConsumer = new ConsumerBuilder<Ignore, string>(conf).Build();

            kafkaConsumer.Subscribe(topico);
            Console.WriteLine($"    Conectando ao tópico: {topico}              " );

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"    Conectando ao tópico - while: {topico}              " );                    
                    var consumo = this.kafkaConsumer.Consume(stoppingToken);
                    
                    if (!string.IsNullOrEmpty(consumo.Message.Value)) {
                        Console.Write($"KAFKA: {consumo.Message.Value}");       
                        var lote = JsonSerializer.Deserialize<Lote>(consumo.Message.Value);             
                        CriarLote(lote, dbContext);                        
                    }                            
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Kafka operação cancelada");
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    Console.WriteLine($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e}");
                }                                
            }
            Console.WriteLine($" KAFKA SAIR DO TÓPICO!!!!!!!   " );
        }

        private void CriarLote(Lote lote, STURDBContext dbContext)
        {
            try
            {
                var loteGravado = dbContext.Lotes.FirstOrDefault(w => w.InscricaoImovel == lote.InscricaoImovel);
                if (loteGravado != null)
                {
                    loteGravado.AreaConstruida = lote.AreaConstruida;
                    loteGravado.AreaTerreno = lote.AreaTerreno;
                    loteGravado.DataAtualizacao = lote.DataAtualizacao;
                    dbContext.Lotes.Update(loteGravado);
                }                    
                else
                    dbContext.Lotes.Add(lote);

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gravar lote via mensagem: " + ex.Message);
            }

        }

        public override void Dispose()
        {
            Console.WriteLine($" KAFKA DISPOSE!!!!!!!   " );
            this.kafkaConsumer.Close(); // Commit offsets and leave the group cleanly.
            this.kafkaConsumer.Dispose();

            base.Dispose();
        }
    }
}
