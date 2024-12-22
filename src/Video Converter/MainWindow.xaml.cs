using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Video_Converter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window    {

    public ObservableCollection<VideoInfo> Videos { get; set; } = new ObservableCollection<VideoInfo>();
    private string _ffmpegPath = "ffmpeg.exe"; // Caminho para o ffmpeg.exe

    public MainWindow()
    {
        InitializeComponent();
        ListaVideos.ItemsSource = Videos;

        //Verifica se o ffmpeg existe na mesma pasta do executavel
        if (!File.Exists(_ffmpegPath))
        {
            MessageBox.Show("ffmpeg.exe não encontrado na pasta do aplicativo. A conversão não funcionará.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            ConverterButton.IsEnabled = false; //Desabilita o botão converter
        }
    }

    private void AdicionarVideos_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "Arquivos de Vídeo|*.mp4;*.avi;*.mkv;*.mpg;*.mpeg;*.wmv|Todos os Arquivos|*.*";

        if (openFileDialog.ShowDialog() == true)
        {
            foreach (string filename in openFileDialog.FileNames)
            {
                Videos.Add(new VideoInfo { NomeArquivo = System.IO.Path.GetFileName(filename), CaminhoArquivo = filename, Progresso = 0 });
            }
            StatusTextBlock.Text = "";
        }
    }

    private async void Converter_Click(object sender, RoutedEventArgs e)
    {
        if (Videos.Count == 0)
        {
            StatusTextBlock.Text = "Nenhum arquivo adicionado";
            return;
        }

        StatusTextBlock.Text = "Convertendo...";
        ConverterButton.IsEnabled = false; //Desabilita o botão durante a conversão
        AdicionarVideoButton.IsEnabled = false;

        foreach (var video in Videos)
        {
            await ConverterVideoAsync(video);
        }

        StatusTextBlock.Text = "Conversão concluída!";
        ConverterButton.IsEnabled = true; //Reabilita o botão após a conversão
        AdicionarVideoButton.IsEnabled = true;
    }

    private async Task ConverterVideoAsync(VideoInfo video)
    {
        try
        {
            string outputFileName = $"{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16)}.mp4";
            string outputPath = Path.Combine(Path.GetDirectoryName(video.CaminhoArquivo), outputFileName);

            Process process = new Process();
            process.StartInfo.FileName = _ffmpegPath;
            process.StartInfo.Arguments = $"-i \"{video.CaminhoArquivo}\" \"{outputPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true; //Captura erros do ffmpeg
            process.StartInfo.CreateNoWindow = true; //Não mostra a janela do cmd

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data) && args.Data.Contains("time="))
                {
                    //Extrai o tempo do output do ffmpeg para calcular o progresso.
                    //Esta parte é mais complexa e depende da saida do ffmpeg.
                    //Uma solução mais robusta seria usar uma biblioteca que parseia a saida do ffmpeg.
                    //Para este exemplo, vamos simplificar e apenas atualizar a cada evento.
                    Dispatcher.Invoke(() => video.Progresso = (int)Math.Min(video.Progresso + 10, 100)); //Atualiza o progresso na UI
                }
            };

            process.Start();
            process.BeginErrorReadLine(); //Inicia a leitura assíncrona dos erros

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new Exception($"Erro na conversão: {error}");
            }
            else
            {
                video.Progresso = 100; //Garante que o progresso chegue a 100%
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao converter {video.NomeArquivo}: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            video.Progresso = 0; //Reseta o progresso em caso de erro
        }
    }
}