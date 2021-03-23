using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STUR_mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace STUR_mvc.Services
{
    public class IPTUCalculoService
    {
        readonly STURDBContext dbContext;
        readonly IOptions<KafkaConfig> kafkaConfig;
        public IPTUCalculoService(STURDBContext dbContext, IOptions<KafkaConfig> kafkaConfig)
        {
            this.dbContext = dbContext;
            this.kafkaConfig = kafkaConfig;
        }
        public async Task<IList<Imposto>> CalcularIPTU(int anoBase, string inscricaoImovel, decimal reajuste)
        {
            var impostosCalculados = new List<Imposto>();
            var dataCalculo = new DateTime(anoBase, 2, 20);
            var propriedadesCalculo = SelecionarPropriedades(anoBase, inscricaoImovel);

            foreach (var lote in propriedadesCalculo.GroupBy(g => g.InscricaoImovel))
            {
                var ultimaInfoGeo = lote.OrderByDescending(o => o.DataAtualizacao).FirstOrDefault();
                var chave = ImpostoChave.GerarChave(ultimaInfoGeo.InscricaoImovel, dataCalculo);

                var imposto = dbContext.Impostos.FirstOrDefault(w => w.Chave == chave) 
                    ?? new Imposto(ultimaInfoGeo.InscricaoImovel, dataCalculo);
                
                imposto.CPFOuCNPJ = ultimaInfoGeo.Proprietario.FirstOrDefault().CPFouCNPJ;
                imposto.AreaConstruida = ultimaInfoGeo.AreaConstruida;
                imposto.AreaTerreno = ultimaInfoGeo.AreaTerreno;
                imposto.Descricao = "IPTU ano base " + anoBase;
                imposto.CalcularValor();
                imposto.AplicarReajuste(reajuste);

                impostosCalculados.Add(imposto);

                if (imposto.Id == 0)
                    dbContext.Impostos.Add(imposto);
                else
                    dbContext.Impostos.Update(imposto);

                dbContext.SaveChanges();

                await NotificarImpostoCalculado(imposto);
            }

            return impostosCalculados;
        }

        private async Task NotificarImpostoCalculado(Imposto imposto)
        {
            var impostoJson = JsonSerializer.Serialize(imposto);
            var config = new ProducerConfig { BootstrapServers = kafkaConfig.Value.BootstrapServers };
            var producerBuilder = new ProducerBuilder<Null, string>(config);

            using (var producer = producerBuilder.Build())
            {
                try
                {
                    Console.WriteLine($"Produzindo mensagem stur_imposto_calculado: ${impostoJson}");
                    await producer.ProduceAsync("stur-imposto-calculado", new Message<Null, string>
                    {
                        Value = impostoJson
                    });
                    Console.WriteLine("$Mensagem enviada!");
                }
                catch (ProduceException<Null, Imposto> e)
                {
                    Console.WriteLine(e.Message);
                    throw e;
                }
            }
        }

        private IList<Lote> SelecionarPropriedades(int anoBase, string inscricaoImovel)
        {
            var propriedadesCalculo = new List<Lote>();
            if (!string.IsNullOrEmpty(inscricaoImovel))
            {
                return dbContext.Lotes.Include(i => i.Proprietario).Where(w => w.Proprietario != null && (w.InscricaoImovel == inscricaoImovel && w.DataAtualizacao.Year <= anoBase)).ToList();
            }
            
            return  dbContext.Lotes.Include(i => i.Proprietario).Where(w => w.Proprietario != null && (w.DataAtualizacao.Year <=  anoBase)).ToList();
        }
    }
}
