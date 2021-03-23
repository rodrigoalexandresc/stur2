using System;
using System.Collections.Generic;

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
        public IList<LoteProprietario> Proprietario { get; set; }
    }

    [Serializable]
    public class LoteProprietario {
        public int Id { get; set; }
        public int LoteId { get; set; }
        public Lote Lote { get; set; }
        public string CPFouCNPJ { get; set; }

    }
}
