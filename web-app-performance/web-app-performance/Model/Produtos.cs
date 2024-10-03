namespace web_app_performance.Model
{
    public class Produtos
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quant_estoque { get; set; }
        public DateTime Data_criacao { get; set; } = DateTime.Now;
    }
}
