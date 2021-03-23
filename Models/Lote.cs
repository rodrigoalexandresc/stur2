using System;

namespace STUR_mvc.Models
{
    [Serializable]
    public class Lote
    {
        public int Id { get; set; }
        public string InscricaoImovel { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public decimal AreaConstruida { get; set; }
        public decimal AreaTerreno { get; set; }
    }
}
