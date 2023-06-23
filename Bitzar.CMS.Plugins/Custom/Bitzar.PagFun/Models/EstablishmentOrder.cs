using System;

namespace Bitzar.PagFun.Models
{
    public class EstablishmentOrder
    {
        public int IdEstabelecimento { get; set; }
        public int NrPedido { get; set; }
        public string DataPedido { get; set; }
        public string HoraPedido { get; set; }
        public string DescricaoProduto { get; set; }
        public string Categoria { get; set; }
        public decimal Preco { get; set; }
        public int Quant { get; set; }
        public string Cliente { get; set; }
        public string Mesa { get; set; }
        public string StatusAtendimento { get; set; }
        public string HoraAtendimento { get; set; }
        public string TempoEntrega { get; set; }
        public string MotivoCancelamento { get; set; }
    }
}