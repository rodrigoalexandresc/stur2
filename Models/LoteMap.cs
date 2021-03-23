using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace STUR_mvc.Models
{
    public class LoteMap
    {
        public LoteMap(EntityTypeBuilder<Lote> entityBuilder)
        {
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.ToTable("lote");

            entityBuilder.Property(x => x.Id).HasColumnName("id");
            entityBuilder.Property(x => x.DataAtualizacao).HasColumnName("dataatualizacao");
            entityBuilder.Property(x => x.AreaConstruida).HasColumnName("areaconstruida");
            entityBuilder.Property(x => x.AreaTerreno).HasColumnName("areaterreno");
            entityBuilder.Property(x => x.InscricaoImovel).HasColumnName("inscricaoimovel");
        }
    }
}
