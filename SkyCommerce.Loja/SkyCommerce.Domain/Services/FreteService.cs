using Refit;
using SkyCommerce.Extensions;
using SkyCommerce.Interfaces;
using SkyCommerce.Models;
using SkyCommerce.ViewObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyCommerce.Services
{
    public class FreteService : IFreteService
    {
        private readonly IProdutoStore _produtoStore;

        public FreteService(IProdutoStore produtoStore)
        {
            _produtoStore = produtoStore;
        }


        public Task<IEnumerable<DetalhesFrete>> ObterModalidades(GeoCoordinate geo)
        {
            var freteApi = RestService.For<IFreteApi>("https://localhost:5007");
            return freteApi.Modalidades();
        }

        public Task<IEnumerable<Frete>> CalcularFrete(Embalagem embalagem, GeoCoordinate posicao, string token)
        {
            var freteApi = RestService.For<IFreteApi>("https://localhost:5007");
            return freteApi.Calcular($"Bearer {token}", posicao.Latitude, posicao.Longitude, embalagem);
        }

        public async Task<IEnumerable<Frete>> CalcularCarrinho(Carrinho carrinho, GeoCoordinate posicao, string token)
        {
            var freteApi = RestService.For<IFreteApi>("https://localhost:5007");
            var fretes = (await freteApi.Modalidades()).Select(Frete.FromViewModel).ToList();
            if (carrinho != null && posicao != null)
            {
                foreach (var carrinhoItem in carrinho.Items)
                {
                    var produto = await _produtoStore.ObterPorNome(carrinhoItem.NomeUnico);
                    var opcoesDeFrete = await freteApi.Calcular($"Bearer {token}", posicao.Latitude, posicao.Longitude, produto.Embalagem);
                    foreach (var frete in fretes)
                    {
                        frete.AtualizarValor(opcoesDeFrete.Modalidade(frete.Modalidade));
                    }
                }
            }

            return fretes;
        }
    }
}
