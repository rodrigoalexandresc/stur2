using System.ComponentModel.DataAnnotations;

namespace STUR_mvc.Models
{
    public class IPTUCalculoViewModel
    {
        [Display(Name = "Ano Base")]
        public int AnoBase { get; set; }
        [Display(Name = "Inscrição Imóvel")]
        public string InscricaoImovel { get; set; }
        [Display(Name = "Reajuste")]
        public decimal Reajuste { get; set; }
    }
}
