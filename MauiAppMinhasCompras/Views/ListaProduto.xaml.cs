using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

    public ListaProduto()
    {
        InitializeComponent();
        lst_produtos.ItemsSource = lista;
    }

    protected async override void OnAppearing()
    {
        await CarregarProdutos();
    }

    private async Task CarregarProdutos(string filtroCategoria = "Todos", string busca = "")
    {
        try
        {
            lista.Clear();
            List<Produto> tmp = await ((App)Application.Current).Db.GetAll();

            // Aplica filtro de busca dos produtos
            if (!string.IsNullOrWhiteSpace(busca))
                tmp = tmp.Where(p => p.Descricao.Contains(busca, StringComparison.OrdinalIgnoreCase)).ToList();

            // Aplica filtro de categoria (nova configuração criada)
            if (filtroCategoria != "Todos")
                tmp = tmp.Where(p => p.Categoria == filtroCategoria).ToList();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new NovoProduto());
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        await CarregarProdutos(pickerCategoria.SelectedItem?.ToString() ?? "Todos", e.NewTextValue);
    }

    private async void pickerCategoria_SelectedIndexChanged(object sender, EventArgs e)
    {
        await CarregarProdutos(pickerCategoria.SelectedItem?.ToString() ?? "Todos", txt_search.Text);
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double soma = lista.Sum(i => i.Total);
        string msg = $"O total é {soma:C}";
        DisplayAlert("Total dos Produtos", msg, "OK");
    }

    // Relatório por categoria
    private async void ToolbarItem_Clicked_Relatorio(object sender, EventArgs e)
    {
        try
        {
            var categorias = lista
                .GroupBy(p => p.Categoria)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(p => p.Total) })
                .ToList();

            string msg = string.Join(Environment.NewLine,
                categorias.Select(c => $"{c.Categoria}: {c.Total:C}"));

            await DisplayAlert("Relatório por Categoria", msg, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecinado = sender as MenuItem;
            Produto p = selecinado.BindingContext as Produto;

            bool confirm = await DisplayAlert("Tem certeza?", $"Remover {p.Descricao}?", "Sim", "Não");
            if (confirm)
            {
                await ((App)Application.Current).Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Produto p)
        {
            Navigation.PushAsync(new EditarProduto { BindingContext = p });
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        await CarregarProdutos(pickerCategoria.SelectedItem?.ToString() ?? "Todos", txt_search.Text);
        lst_produtos.IsRefreshing = false;
    }
}
